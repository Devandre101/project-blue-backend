namespace BlueProject.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string TransactionType { get; set; } // "deposit" or "withdrawal"
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } // Navigation property
    }
}
