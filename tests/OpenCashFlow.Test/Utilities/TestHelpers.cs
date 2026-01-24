using global::Shared.Data;
using global::Shared.Models;
using global::Shared.Models.Identity;

namespace  OpenCashFlow.Test.Utilities
{
    public static class TestHelpers
    {
        // --- Creating Asp Net Roles
        public static List<AspNetRole> CreateAspNetRoles()
        {
            return [
                new AspNetRole
                {
                    RoleID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    RoleName = "Administrator",
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new AspNetRole
                {
                    RoleID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    RoleName = "Employee",
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                }
            ];
        }

        // --- Creating Asp Net Users
        public static List<AspNetUser> CreateAspNetUsers()
        {
            return [
                new AspNetUser
                {
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserName = "BaronLuca",
                    UserAvatar = null,
                    Language = "IT",
                    Country = "IT",
                    Timezone = null,
                    UserTitle = null,
                    UserFirstName = "Luca",
                    UserMiddleName = null,
                    UserLastName = "Baron",
                    Email = "baron_luca@nestia.local",
                    EmailConfirmed = true,
                    PhoneNumberPrefix = "+39",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Gender = "Male",
                    Pronouns = null,
                    DoB = null,
                    PoB = null,
                    SoB = null,
                    CoB = null,
                    Nationality = "Italian",
                    PrivacyPolicyAcepted = false,
                    PrivacyPolicyAcceptedDate = null,
                    PrivacyPolicyVersion = null,
                    PasswordHash = "1J9y+7vb6zYOykos49K6UIWBs6yTIR52f6yJVE55N18=",
                    PasswordSalt = "4cB1NmkERk/TiMqrc2DONA==",
                    MobilePin = null,
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PasswordQuestion = "abcdefg",
                    PasswordAnswer = "dewafev[pi[w",
                    TwoFactorEnabled = false,
                    AccountValidUntil = null,
                    PasswordValidUntil = null,
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    IsApproved = true,
                    AccessFailedCount = 0,
                    FailedPasswordAnswerAttemptCount = 0,
                    LastLoginDate = null,
                    LastAppLoginDate = null,
                    IpAddress = null,
                    LastKnownLocation = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    QuickLoginPinHash = "lRpzr9szDAtETNymgtm7JJQT3PRIfmnjllPASChPxHf=",
                    UserMustChangePassword = false,
                    QuickLoginPinValidUntil = null
                },
                new AspNetUser
                {
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserName = "UserNameTest",
                    UserAvatar = null,
                    Language = "IT",
                    Country = "IT",
                    Timezone = null,
                    UserTitle = null,
                    UserFirstName = "UserName",
                    UserMiddleName = null,
                    UserLastName = "UserSurname",
                    Email = "usernametest@nestia.local",
                    EmailConfirmed = true,
                    PhoneNumberPrefix = "+39",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Gender = "Male",
                    Pronouns = null,
                    DoB = null,
                    PoB = null,
                    SoB = null,
                    CoB = null,
                    Nationality = "Italian",
                    PrivacyPolicyAcepted = false,
                    PrivacyPolicyAcceptedDate = null,
                    PrivacyPolicyVersion = null,
                    PasswordHash = "1J9y+7vb6zYOykos49K6UIWBs6yTIR52f6yJVE55N48=",
                    PasswordSalt = "4cB1NmkERk/TiMqrc2DONA==",
                    MobilePin = null,
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PasswordQuestion = "a",
                    PasswordAnswer = "dewafev[pi[w",
                    TwoFactorEnabled = false,
                    AccountValidUntil = null,
                    PasswordValidUntil = null,
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    IsApproved = true,
                    AccessFailedCount = 0,
                    FailedPasswordAnswerAttemptCount = 0,
                    LastLoginDate = null,
                    LastAppLoginDate = null,
                    IpAddress = null,
                    LastKnownLocation = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    QuickLoginPinHash = "edjJX2/CU8gLFoHEWzgNBH5848+Z+OLGTczG7PrEujI=",
                    UserMustChangePassword = false,
                    QuickLoginPinValidUntil = null
                },
                new AspNetUser
                {
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    UserName = "TPignatta",
                    UserAvatar = null,
                    Language = "IT",
                    Country = "IT",
                    Timezone = null,
                    UserTitle = null,
                    UserFirstName = "Toni",
                    UserMiddleName = null,
                    UserLastName = "Pignatta",
                    Email = "ciccioamante63@aol.com", // Homer Simpson reference
                    EmailConfirmed = true,
                    PhoneNumberPrefix = "+39",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Gender = "Male",
                    Pronouns = null,
                    DoB = null,
                    PoB = null,
                    SoB = null,
                    CoB = null,
                    Nationality = "Italian",
                    PrivacyPolicyAcepted = false,
                    PrivacyPolicyAcceptedDate = null,
                    PrivacyPolicyVersion = null,
                    PasswordHash = "1J9y+7vb6zYOykos49K6UIWBs6yTIR52f6yJVE55N18=",
                    PasswordSalt = "4cB1NmkERk/TiMqrc2DONA==",
                    MobilePin = null,
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PasswordQuestion = "a",
                    PasswordAnswer = "dewafev[pi[w",
                    TwoFactorEnabled = false,
                    AccountValidUntil = null,
                    PasswordValidUntil = null,
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    IsApproved = true,
                    AccessFailedCount = 0,
                    FailedPasswordAnswerAttemptCount = 0,
                    LastLoginDate = null,
                    LastAppLoginDate = null,
                    IpAddress = null,
                    LastKnownLocation = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    QuickLoginPinHash = "edjJX2/CU8gLFoHEWzgNBH5848+Z+OLGTczG7PrEujI=",
                    UserMustChangePassword = false,
                    QuickLoginPinValidUntil = null
                }
            ];
        }

        // --- Creating Companies
        public static List<Company> CreateTestCompanies()
        {
            return [
                new Company
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    CompanyName = "Company SRL",
                    MaxUsers = 50,
                    VATRates = 22,
                    StartingContract = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EndingContract = new DateTime(2035, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DefaultCurrency = "&euro;",
                    IsActive = true,
                    IsDeleted = false,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    CompanySecret = "bS8gmD_6L7zsADdQ17Q-MeeWB1yB5G4k0Q2Wy72yaFdEEJVzy2DhcinmWR3Tx45e68Bn8_b1t-1F35Co9uf_Bh"
                },
                new Company
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    CompanyName = "Secondary Company LLC",
                    MaxUsers = 50,
                    VATRates = 22,
                    StartingContract = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EndingContract = new DateTime(2035, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DefaultCurrency = "&euro;",
                    IsActive = true,
                    IsDeleted = false,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    CompanySecret = "bS8gmD_6L7zsADdQ17Q-MeeWB1yB5G4k0Q2Wy72yaFdEEJVzy2DhcinmWR3Tx45e68Bn8_b1t-1F35Co9uf_Bh"
                }
            ];
        }

        // --- Adding Users in Company
        public static List<Company_Staff> CreateCompanyStaff()
        {
            return [
                new Company_Staff
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    TimeCost = null,
                    BadgeID = null,
                    OutOfReports = false,
                    RequireShiftCheckIn = true,
                    LastCheckIn = null,
                    LastCheckOut = null,
                    Role = null,
                    Department = null,
                    WorkLocation = null,
                    ContractStartDate = null,
                    ContractEndDate = null,
                    MonthlySalary = null,
                    Bonuses = null,
                    Allowances = null,
                    EmploymentType = null,
                    OvertimeRate = null,
                    Skills = null,
                    SupervisorID = null,
                    AccessLevel = null,
                    AuthorizedAreas = null,
                    InternalNotes = null,
                    PublicNotes = null,
                    ExternalSystemReference = null,
                    SyncStatus = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Company_Staff
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    TimeCost = null,
                    BadgeID = null,
                    OutOfReports = false,
                    RequireShiftCheckIn = true,
                    LastCheckIn = null,
                    LastCheckOut = null,
                    Role = null,
                    Department = null,
                    WorkLocation = null,
                    ContractStartDate = null,
                    ContractEndDate = null,
                    MonthlySalary = null,
                    Bonuses = null,
                    Allowances = null,
                    EmploymentType = null,
                    OvertimeRate = null,
                    Skills = null,
                    SupervisorID = null,
                    AccessLevel = null,
                    AuthorizedAreas = null,
                    InternalNotes = null,
                    PublicNotes = null,
                    ExternalSystemReference = null,
                    SyncStatus = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Company_Staff
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    TimeCost = null,
                    BadgeID = null,
                    OutOfReports = false,
                    RequireShiftCheckIn = true,
                    LastCheckIn = null,
                    LastCheckOut = null,
                    Role = null,
                    Department = null,
                    WorkLocation = null,
                    ContractStartDate = null,
                    ContractEndDate = null,
                    MonthlySalary = null,
                    Bonuses = null,
                    Allowances = null,
                    EmploymentType = null,
                    OvertimeRate = null,
                    Skills = null,
                    SupervisorID = null,
                    AccessLevel = null,
                    AuthorizedAreas = null,
                    InternalNotes = null,
                    PublicNotes = null,
                    ExternalSystemReference = null,
                    SyncStatus = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                }
            ];
        }

        // --- Adding Payment Methods Type
        public static List<Payment_Method_LookUps> CreatePaymentMethodsLoookUp()
        {
            return [
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    PaymentMethodName = "Cash",
                    PaymentMethodDescription = "Cash payments",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    PaymentMethodName = "Credit Card",
                    PaymentMethodDescription = "Payment By Credit Card",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 1,
                    TenantID = null,
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    PaymentMethodName = "Custom method",
                    PaymentMethodDescription = "method that can be deleted from company",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    PaymentMethodName = "Cash",
                    PaymentMethodDescription = "Cash payments",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"), // secondary company
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                }
            ];
        }

        // --- Adding Document Types
        public static List<Payment_DocumentType_LookUp> CreateDocumentTypesLoookUp()
        {
            return [
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    DocumentTypeName = "Invoice",
                    DocumentTypeDescription = "Invoice",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    DocumentTypeName = "Second receipt type",
                    DocumentTypeDescription = "receipt that can be deleted",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    DocumentTypeName = "third receipt type",
                    DocumentTypeDescription = "receipt that cannot be deleted",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                }
            ];
        }

        // --- Adding Payment
        public static List<Payment> CreateTestPayment()
        {
            return [
                new Payment
                {
                    PaymentID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    EntryType = nameof(EntryTypeEnum.Income),
                    Amount = 1000,
                    Description = "Payment that can be deleted",
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = DateTime.UtcNow,
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment
                {
                    PaymentID = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    EntryType = nameof(EntryTypeEnum.Income),
                    Amount = 1000,
                    Description = "Payment that cannot be deleted",
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = DateTime.UtcNow,
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment
                {
                    PaymentID = Guid.Parse("00000000-0000-0000-0000-000000000999"),
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    EntryType = nameof(EntryTypeEnum.Income),
                    Amount = 1000,
                    Description = "Payment that will be deleted ...",
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = DateTime.UtcNow,
                    DateEdit = null,
                    EditedBy = null
                }
            ];
        }

        public static void SeedAllTestData(ApplicationDbContext context)
        {
            foreach (var role in CreateAspNetRoles())
                if (!context.AspNetRole_DS.Any(r => r.RoleID == role.RoleID))
                    context.AspNetRole_DS.Add(role);

            foreach (var user in CreateAspNetUsers())
                if (!context.AspNetUser_DS.Any(u => u.UserID == user.UserID))
                    context.AspNetUser_DS.Add(user);

            foreach (var company in CreateTestCompanies())
                if (!context.Company_DS.Any(c => c.TenantID == company.TenantID))
                    context.Company_DS.Add(company);

            foreach (var staff in CreateCompanyStaff())
                if (!context.Company_Staff_DS.Any(s => s.TenantID == staff.TenantID && s.UserID == staff.UserID))
                    context.Company_Staff_DS.Add(staff);

            foreach (var method in CreatePaymentMethodsLoookUp())
                if (!context.PaymentMethod_DS.Any(m => m.PaymentMethodID == method.PaymentMethodID))
                    context.PaymentMethod_DS.Add(method);

            foreach (var doc in CreateDocumentTypesLoookUp())
                if (!context.Payment_DocumentType_DS.Any(d => d.DocumentTypeID == doc.DocumentTypeID))
                    context.Payment_DocumentType_DS.Add(doc);

            foreach (var payment in CreateTestPayment())
                if (!context.Payment_DS.Any(p => p.PaymentID == payment.PaymentID))
                    context.Payment_DS.Add(payment);

            context.SaveChanges();
        }
    }
}
