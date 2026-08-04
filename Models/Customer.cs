namespace MiniBillingSystem.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }

        public Customer(int id, string name, string address, string phone, string email, DateTime? dateAdded = null)
        {
            CustomerID = id;
            FullName = name ?? string.Empty;
            Address = address ?? string.Empty;
            PhoneNumber = phone ?? string.Empty;
            Email = email ?? string.Empty;
            DateAdded = dateAdded ?? DateTime.Now;
        }

        public override string ToString()
        {
            return $"{CustomerID,-5} {FullName,-25} {PhoneNumber,-16} {Email}";
        }
    }
}
