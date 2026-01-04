using global::Shared.Models;

namespace OpenCashFlow.App.ViewModels
{
    public class Company_Billing_Plans_ViewModel
    {
        public IEnumerable<Company_Invoice>? Invoices { get; set; }

        public Company_Billing_Plans_ViewModel(IEnumerable<Company_Invoice>? Invoices)
        {
            this.Invoices = Invoices ?? [];
        }
    }
}
