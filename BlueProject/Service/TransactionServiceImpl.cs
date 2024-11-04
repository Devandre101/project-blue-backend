using BlueProject.Data;
using BlueProject.Models;
using BlueProject.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlueProject.Service
{
    public class TransactionServiceImpl : ITransactionService
    {
        private readonly ApiContext _context;

        public TransactionServiceImpl(ApiContext context)
        {
            _context = context;
        }
        public async Task AddTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.Include(t => t.User).ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.Include(t => t.User).FirstOrDefaultAsync(t => t.TransactionId == id);
        }


        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(int userId)
        {
            return await _context.Transactions
                                 .Where(t => t.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(TransactionFilterDTO filter)
        {
            return await _context.Transactions
                                 .Include(t => t.User) // Include the User data here
                                 .Where(t => t.UserId == filter.UserId &&
                                             t.TransactionDate >= filter.StartDate &&
                                             t.TransactionDate <= filter.EndDate &&
                                             t.TransactionType == filter.TransactionType)
                                 .OrderByDescending(t => t.TransactionDate)
                                 .Skip(filter.Offset)
                                 .Take(filter.Limit)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(string transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                return Enumerable.Empty<Transaction>();
            }

            var lowerCaseTransactionType = transactionType.ToLower();

            return await _context.Transactions
                                 .Include(t => t.User) // Include the User data here
                                 .Where(t => t.TransactionType.ToLower() == lowerCaseTransactionType)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateRangeFilterDTO dateRangeFilter)
        {
            // Ensure dates are provided
            if (dateRangeFilter.StartDate == default || dateRangeFilter.EndDate == default)
            {
                throw new ArgumentException("Invalid date range");
            }

            // Fetch transactions within the specified date range
            return await _context.Transactions
                .Where(t => t.TransactionDate >= dateRangeFilter.StartDate && t.TransactionDate <= dateRangeFilter.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAndTypeAsync(DateRangeAndTypeFilterDTO filter)
        {
            // Ensure dates are provided
            if (filter.StartDate == default || filter.EndDate == default)
            {
                throw new ArgumentException("Invalid date range");
            }

            // Fetch transactions within the specified date range and type
            return await _context.Transactions
                .Where(t => t.TransactionDate >= filter.StartDate &&
                            t.TransactionDate <= filter.EndDate &&
                            t.TransactionType == filter.TransactionType)
                .ToListAsync();
        }




    }
}
