namespace BlueProject.Models.DTO
{
    public class TransactionFilterDTO
    {
        public int UserId { get; set; }           // ID of the user to filter transactions
        public DateTime StartDate { get; set; }   // Start date for the transaction date range
        public DateTime EndDate { get; set; }     // End date for the transaction date range
        public string TransactionType { get; set; } // Type of transaction (e.g., deposit, withdrawal)
        public int Offset { get; set; }            // Number of records to skip (for pagination)
        public int Limit { get; set; }             // Maximum number of records to return (for pagination)
    }
}
