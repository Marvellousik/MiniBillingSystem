namespace MiniBillingSystem.Models
{
    public class CustomerDto
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal AccountBalance { get; set; }
        public decimal TotalOwed { get; set; }
    }

    public class CreateCustomerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateCustomerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CustomerDetailDto
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal AccountBalance { get; set; }
        public decimal TotalOwed { get; set; }
        public List<BillDto> Bills { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class BillDto
    {
        public int BillID { get; set; }
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }

    public class CreateBillRequest
    {
        public int CustomerID { get; set; }
        public decimal AmountDue { get; set; }
        public string DueDate { get; set; } = string.Empty;
    }

    public class BillCreationResultDto
    {
        public string Message { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public decimal FinalAmountDue { get; set; }
        public bool IsPaid { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentID { get; set; }
        public int BillID { get; set; }
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentDate { get; set; } = string.Empty;
    }

    public class RecordPaymentRequest
    {
        public int BillID { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentResultDto
    {
        public string Message { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public decimal RemainingAmountDue { get; set; }
    }

    public class SummaryDto
    {
        public int TotalCustomers { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int BillsDueThisWeek { get; set; }
        public decimal TotalCollected { get; set; }
    }
}
