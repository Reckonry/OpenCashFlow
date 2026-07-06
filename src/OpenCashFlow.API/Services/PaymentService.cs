using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Enums;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;
        private readonly ICashService _cashService;
        private readonly IAuditLogService? _auditLogService;
        private readonly ILogger<PaymentService> _logger;
        private readonly ApplicationDbContext _context;

        private static readonly Guid SystemCashPaymentMethodId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly string[] CashMethodAliases = new[] { "Cash", "Contanti" };

        public PaymentService(IPaymentRepository PaymentRepository, IAuthenticationService authenticationService, IMapper mapper, ICashService cashService, ILogger<PaymentService> logger, ApplicationDbContext context, IAuditLogService? auditLogService = null)
        {
            _paymentRepository = PaymentRepository;
            _authenticationService = authenticationService;
            _mapper = mapper;
            _cashService = cashService;
            _logger = logger;
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<Payment_List_DTO>?> GetAllPaymentsAsync(Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
            return _mapper.Map<IEnumerable<Payment_List_DTO>>(
                await _paymentRepository.GetAllPaymentsAsync(_authenticationService.GetTenantID(), filters, cancellationToken));
        }

        public async Task<Payment_Detail_DTO?> GetPaymentByIdAsync(Guid PaymentID, CancellationToken cancellationToken)
        {
            return _mapper.Map<Payment_Detail_DTO>(await _paymentRepository.GetPaymentByIdAsync(PaymentID, _authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Payment_Create_DTO?> AddPaymentAsync(Payment_Create_DTO payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;
            ValidatePaymentInput(payment.Amount, payment.EntryType, payment.PaymentMethodID, payment.DocumentTypeID, payment.DateIns);
            payment.TenantID = _authenticationService.GetTenantID();
            payment.UserID = _authenticationService.GetUserID();
            payment.EntryType = NormalizeEntryType(payment.EntryType);
            payment.DateIns = NormalizeDate(payment.DateIns);

            // ✅ IDEMPOTENCY CHECK: Check if payment with this RequestId already exists
            var existingPayment = await _paymentRepository.GetPaymentByRequestIdAsync(payment.RequestId, payment.TenantID, cancellationToken);
            if (existingPayment != null)
            {
                _logger.LogInformation("Payment with RequestId {RequestId} already exists (PaymentID: {PaymentID}). Returning existing payment.",
                    payment.RequestId, existingPayment.PaymentID);
                return _mapper.Map<Payment_Create_DTO>(existingPayment);
            }

            // ✅ ATOMIC TRANSACTION: Create payment + ledger in single transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Insert payment
                var createdPayment = await _paymentRepository.AddPaymentAsync(_mapper.Map<Payment>(payment), cancellationToken);
                if (createdPayment == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return null;
                }

                _logger.LogInformation("Payment created with PaymentID {PaymentID} and RequestId {RequestId}",
                    createdPayment.PaymentID, payment.RequestId);

                // 2. Update daily payment
                await UpdateDailyPaymentAsync(payment.TenantID, payment.DateIns, payment.Amount, payment.EntryType, cancellationToken);

                // 3. Apply to cash balance if cash-like method (within same transaction)
                if (payment.PaymentMethodID.HasValue)
                {
                    var method = await _paymentRepository.GetPaymentMethodByIdAsync(payment.PaymentMethodID.Value, payment.TenantID, cancellationToken);
                    if (IsCashMethod(payment.PaymentMethodID, method))
                    {
                        var userIdStr = payment.UserID.ToString();
                        var isOutcome = string.Equals(payment.EntryType, nameof(EntryTypeEnum.Outcome), StringComparison.OrdinalIgnoreCase);
                        var delta = Convert.ToDecimal(payment.Amount) * (isOutcome ? -1 : 1);

                        // CashService will detect the existing transaction and use it
                        await _cashService.ApplyPaymentAsync(payment.TenantID, createdPayment.PaymentID, delta, userIdStr, cancellationToken);

                        _logger.LogInformation("Cash ledger updated for PaymentID {PaymentID} with delta {Delta}",
                            createdPayment.PaymentID, delta);
                    }
                }

                // 4. Commit all changes
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Payment transaction committed successfully for PaymentID {PaymentID}", createdPayment.PaymentID);
                if (_auditLogService != null)
                {
                    await _auditLogService.LogEventAsync(
                    AuditEventType.PaymentCreated,
                    "Payment",
                    "Create",
                    createdPayment.PaymentID.ToString(),
                    new { createdPayment.Amount, createdPayment.EntryType, createdPayment.PaymentMethodID, createdPayment.DocumentTypeID },
                    cancellationToken: cancellationToken);
                }

                return _mapper.Map<Payment_Create_DTO>(createdPayment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment with RequestId {RequestId}. Rolling back transaction.", payment.RequestId);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Payment_Update_DTO?> UpdatePaymentAsync(Payment_Detail_DTO payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;
            ValidatePaymentInput(payment.Amount, payment.EntryType, payment.PaymentMethodID, payment.DocumentTypeID, payment.DateIns);
            payment.EntryType = NormalizeEntryType(payment.EntryType);
            payment.DateIns = NormalizeDate(payment.DateIns);

            // Get payment for reading original values (with navigation properties)
            var paymentRead = await _paymentRepository.GetPaymentByIdAsync(payment.PaymentID, _authenticationService.GetTenantID(), cancellationToken);
            if (paymentRead == null) return null;

            // Store original values before update
            var originalAmount = paymentRead.Amount;
            var originalEntryType = paymentRead.EntryType;
            var originalDate = paymentRead.DateIns;
            var originalPaymentMethodID = paymentRead.PaymentMethodID;
            var originalPaymentMethod = paymentRead.PaymentMethod;

            // ✅ ATOMIC TRANSACTION: Update payment + daily payment + cash ledger in single transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Get payment for update (WITH tracking, NO navigation properties)
                var paymentTBE = await _paymentRepository.GetPaymentByIdForUpdateAsync(payment.PaymentID, _authenticationService.GetTenantID(), cancellationToken);
                if (paymentTBE == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return null;
                }

                // 1. Update payment - modify tracked entity directly
                paymentTBE.Amount = payment.Amount;
                paymentTBE.PaymentMethodID = (Guid)payment.PaymentMethodID;
                paymentTBE.DocumentTypeID = (Guid)payment.DocumentTypeID;
                paymentTBE.Description = payment.Description;
                paymentTBE.EntryType = payment.EntryType;
                paymentTBE.DateIns = payment.DateIns;
                paymentTBE.DateEdit = DateTime.UtcNow;
                paymentTBE.EditedBy = _authenticationService.GetUserID();

                // Save changes (EF will track and update only modified fields)
                var updatePayment = await _paymentRepository.UpdatePaymentAsync(paymentTBE, cancellationToken);

                if (updatePayment == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return null;
                }

                _logger.LogInformation("Payment {PaymentID} updated", payment.PaymentID);

                // 2. Update daily payments
                await DeleteDailyPaymentAsync(paymentTBE.TenantID, originalAmount, originalDate, originalEntryType, cancellationToken);
                await UpdateDailyPaymentAsync(paymentTBE.TenantID, paymentTBE.DateIns, paymentTBE.Amount, paymentTBE.EntryType, cancellationToken);

                // 3. Handle cash balance updates (within same transaction)
                var wasOriginalCash = IsCashMethod(originalPaymentMethodID, originalPaymentMethod);
                var isNewCash = false;
                Payment_Method_LookUps? newMethod = null;

                if (payment.PaymentMethodID.HasValue)
                {
                    newMethod = await _paymentRepository.GetPaymentMethodByIdAsync(payment.PaymentMethodID.Value, paymentTBE.TenantID, cancellationToken);
                    isNewCash = IsCashMethod(payment.PaymentMethodID, newMethod);
                }

                var userIdStr = _authenticationService.GetUserID().ToString();

                // Check if this payment currently has a non-zero net balance in cash ledger
                // Use OriginalPaymentId to find all related entries
                var netBalance = await _context.CashLedgers
                    .Where(x => x.CompanyId == paymentTBE.TenantID &&
                               (x.RefId == payment.PaymentID || x.OriginalPaymentId == payment.PaymentID))
                    .SumAsync(x => (decimal?)x.Delta, cancellationToken) ?? 0m;

                var hasActiveCashEntry = netBalance != 0m;

                // If was cash but no longer cash, void the original
                if (wasOriginalCash && !isNewCash)
                {
                    await _cashService.VoidAsync(paymentTBE.TenantID, payment.PaymentID, Convert.ToDecimal(originalAmount), userIdStr, cancellationToken);
                    _logger.LogInformation("Cash voided for payment {PaymentID} (changed from cash to non-cash)", payment.PaymentID);
                }
                // If wasn't cash but now is cash, apply new amount
                else if (!wasOriginalCash && isNewCash)
                {
                    var isOutcome = string.Equals(paymentTBE.EntryType, nameof(EntryTypeEnum.Outcome), StringComparison.OrdinalIgnoreCase);
                    var delta = Convert.ToDecimal(paymentTBE.Amount) * (isOutcome ? -1 : 1);

                    // Check if there's a voided entry (payment was cash before)
                    if (hasActiveCashEntry)
                    {
                        // Should not happen but handle it
                        await _cashService.ApplyPaymentAsync(paymentTBE.TenantID, payment.PaymentID, delta, userIdStr, cancellationToken);
                        _logger.LogInformation("Cash applied for payment {PaymentID} (changed from non-cash to cash)", payment.PaymentID);
                    }
                    else
                    {
                        // Check if this payment was cash before (has Void entries)
                        // Use OriginalPaymentId to check if payment was voided before
                        var wasVoided = await _context.CashLedgers
                            .AnyAsync(x => x.CompanyId == paymentTBE.TenantID &&
                                         x.RefType == "Void" &&
                                         (x.RefId == payment.PaymentID || x.OriginalPaymentId == payment.PaymentID),
                                    cancellationToken);

                        if (wasVoided)
                        {
                            // Use ReapplyPaymentAsync to avoid idempotency block
                            await _cashService.ReapplyPaymentAsync(paymentTBE.TenantID, payment.PaymentID, delta, userIdStr, cancellationToken);
                            _logger.LogInformation("Cash re-applied for payment {PaymentID} (was voided, now cash again with delta {Delta})",
                                payment.PaymentID, delta);
                        }
                        else
                        {
                            // First time as cash
                            await _cashService.ApplyPaymentAsync(paymentTBE.TenantID, payment.PaymentID, delta, userIdStr, cancellationToken);
                            _logger.LogInformation("Cash applied for payment {PaymentID} (changed from non-cash to cash)", payment.PaymentID);
                        }
                    }
                }
                // If both are cash, check if we need to update or create new entry
                else if (wasOriginalCash && isNewCash)
                {
                    // Calculate original delta
                    var wasOutcome = string.Equals(originalEntryType, nameof(EntryTypeEnum.Outcome), StringComparison.OrdinalIgnoreCase);
                    var originalDelta = Convert.ToDecimal(originalAmount) * (wasOutcome ? -1 : 1);

                    // Calculate new delta
                    var isOutcome = string.Equals(paymentTBE.EntryType, nameof(EntryTypeEnum.Outcome), StringComparison.OrdinalIgnoreCase);
                    var newDelta = Convert.ToDecimal(paymentTBE.Amount) * (isOutcome ? -1 : 1);

                    // If there's an active entry, update it; otherwise create a new one
                    if (hasActiveCashEntry)
                    {
                        await _cashService.UpdatePaymentAsync(paymentTBE.TenantID, payment.PaymentID, originalDelta, newDelta, userIdStr, cancellationToken);
                        _logger.LogInformation("Cash updated for payment {PaymentID} (delta changed from {OldDelta} to {NewDelta})",
                            payment.PaymentID, originalDelta, newDelta);
                    }
                    else
                    {
                        // No active entry (was voided before), use ReapplyPaymentAsync
                        await _cashService.ReapplyPaymentAsync(paymentTBE.TenantID, payment.PaymentID, newDelta, userIdStr, cancellationToken);
                        _logger.LogInformation("Cash re-applied for payment {PaymentID} (was voided, now cash again with delta {Delta})",
                            payment.PaymentID, newDelta);
                    }
                }

                // 4. Commit all changes
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Payment update transaction committed successfully for PaymentID {PaymentID}", payment.PaymentID);
                if (_auditLogService != null)
                {
                    await _auditLogService.LogEventAsync(
                    AuditEventType.PaymentUpdated,
                    "Payment",
                    "Update",
                    payment.PaymentID.ToString(),
                    new
                    {
                        Amount = new { Before = originalAmount, After = paymentTBE.Amount },
                        EntryType = new { Before = originalEntryType, After = paymentTBE.EntryType },
                        PaymentMethodID = new { Before = originalPaymentMethodID, After = paymentTBE.PaymentMethodID }
                    },
                    cancellationToken: cancellationToken);
                }

                return _mapper.Map<Payment_Update_DTO>(updatePayment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment {PaymentID}. Rolling back transaction.", payment.PaymentID);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeletePaymentAsync(Guid PaymentID, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(PaymentID, _authenticationService.GetTenantID(), cancellationToken);
            if (payment == null) return false;

            // ✅ ATOMIC TRANSACTION: Delete payment + daily payment + cash ledger in single transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Delete payment
                var deletedPayment = await _paymentRepository.DeletePaymentAsync(PaymentID, _authenticationService.GetTenantID(), cancellationToken);
                if (!deletedPayment)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }

                _logger.LogInformation("Payment {PaymentID} deleted", PaymentID);

                // 2. Delete daily payment
                await DeleteDailyPaymentAsync(payment.TenantID, payment.Amount, payment.DateIns, payment.EntryType, cancellationToken);

                // 3. For cash-like payments, treat delete as a void (reverse original amount)
                if (IsCashMethod(payment.PaymentMethodID, payment.PaymentMethod))
                {
                    var userIdStr = _authenticationService.GetUserID().ToString();
                    await _cashService.VoidAsync(payment.TenantID, payment.PaymentID, Convert.ToDecimal(payment.Amount), userIdStr, cancellationToken);
                    _logger.LogInformation("Cash voided for deleted payment {PaymentID}", PaymentID);
                }

                // 4. Commit all changes
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Payment delete transaction committed successfully for PaymentID {PaymentID}", PaymentID);
                if (_auditLogService != null)
                {
                    await _auditLogService.LogEventAsync(
                    AuditEventType.PaymentDeleted,
                    "Payment",
                    "SoftDelete",
                    PaymentID.ToString(),
                    new { payment.Amount, payment.EntryType, payment.PaymentMethodID },
                    cancellationToken: cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment {PaymentID}. Rolling back transaction.", PaymentID);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static bool IsCashMethod(Guid? paymentMethodId, Payment_Method_LookUps? method)
        {
            if (!paymentMethodId.HasValue)
            {
                return false;
            }

            if (paymentMethodId.Value == SystemCashPaymentMethodId)
            {
                return true;
            }

            if (method == null)
            {
                return false;
            }

            if (method.PaymentMethodID == SystemCashPaymentMethodId)
            {
                return true;
            }

            var name = method.PaymentMethodName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            foreach (var alias in CashMethodAliases)
            {
                if (string.Equals(name, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidatePaymentInput(double amount, string? entryType, Guid? paymentMethodId, Guid? documentTypeId, DateTime dateIns)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            }

            if (paymentMethodId == null || paymentMethodId == Guid.Empty)
            {
                throw new ArgumentException("Payment method is required", nameof(paymentMethodId));
            }

            if (documentTypeId == null || documentTypeId == Guid.Empty)
            {
                throw new ArgumentException("Document type is required", nameof(documentTypeId));
            }

            if (!Enum.TryParse<EntryTypeEnum>(entryType, ignoreCase: true, out _))
            {
                throw new ArgumentException("Entry type must be Income or Outcome", nameof(entryType));
            }

            if (dateIns == default)
            {
                throw new ArgumentException("Payment date is required", nameof(dateIns));
            }
        }

        private static string NormalizeEntryType(string entryType)
        {
            return Enum.Parse<EntryTypeEnum>(entryType, ignoreCase: true).ToString();
        }

        private static DateTime NormalizeDate(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
