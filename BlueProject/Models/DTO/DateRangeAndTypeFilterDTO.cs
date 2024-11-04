namespace BlueProject.Models.DTO
{
    public class DateRangeAndTypeFilterDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TransactionType { get; set; }
    }
}
