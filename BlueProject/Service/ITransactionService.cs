using BlueProject.Models;
using BlueProject.Models.DTO;

namespace BlueProject.Service
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction> GetTransactionByIdAsync(int id);
        Task AddTransactionAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(TransactionFilterDTO filter);
        Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(string transactionType);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateRangeFilterDTO dateRangeFilter);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAndTypeAsync(DateRangeAndTypeFilterDTO filter);
    }
}
