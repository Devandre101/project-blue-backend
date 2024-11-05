using BlueProject.Data;
using BlueProject.Models;
using BlueProject.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueProject.Service
{
    /// <summary>
    /// Service implementation for managing transactions.
    /// </summary>
    public class TransactionServiceImpl : ITransactionService
    {
        private readonly ApiContext _context;
        private readonly ILogger<TransactionServiceImpl> _logger;
        private ApiContext context;

        public TransactionServiceImpl(ApiContext context, ILogger<TransactionServiceImpl> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public TransactionServiceImpl(ApiContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adds a new transaction to the database.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        public async Task AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                _logger.LogWarning("Attempted to add a null transaction.");
                throw new ArgumentNullException(nameof(transaction));
            }

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction added successfully: {@Transaction}", transaction);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while adding transaction: {@Transaction}", transaction);
                throw new Exception("Could not add transaction to the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves all transactions, including associated user data.
        /// </summary>
        /// <returns>A list of all transactions.</returns>
        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            _logger.LogInformation("Fetching all transactions.");
            try
            {
                return await _context.Transactions.Include(t => t.User).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all transactions.");
                throw new Exception("Could not retrieve transactions from the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves a transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction.</param>
        /// <returns>The transaction, or null if not found.</returns>
        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction by ID: {TransactionId}", id);
            try
            {
                return await _context.Transactions.Include(t => t.User).FirstOrDefaultAsync(t => t.TransactionId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transaction ID: {TransactionId}", id);
                throw new Exception($"Could not retrieve transaction with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Retrieves transactions for a specific user by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of transactions for the specified user.</returns>
        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(int userId)
        {
            _logger.LogInformation("Fetching transactions for user ID: {UserId}", userId);
            try
            {
                return await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions for user ID: {UserId}", userId);
                throw new Exception($"Could not retrieve transactions for user ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves filtered transactions based on specified criteria.
        /// </summary>
        /// <param name="filter">The filter criteria.</param>
        /// <returns>A list of filtered transactions.</returns>
        public async Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(TransactionFilterDTO filter)
        {
            if (filter == null)
            {
                _logger.LogWarning("Attempted to filter transactions with a null filter.");
                throw new ArgumentNullException(nameof(filter));
            }

            _logger.LogInformation("Fetching filtered transactions with criteria: {@Filter}", filter);
            try
            {
                return await _context.Transactions
                    .Include(t => t.User)
                    .Where(t => t.UserId == filter.UserId &&
                                t.TransactionDate >= filter.StartDate &&
                                t.TransactionDate <= filter.EndDate &&
                                t.TransactionType == filter.TransactionType)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip(filter.Offset)
                    .Take(filter.Limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered transactions with criteria: {@Filter}", filter);
                throw new Exception("Could not retrieve filtered transactions from the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves transactions by their type.
        /// </summary>
        /// <param name="transactionType">The type of transaction to filter by.</param>
        /// <returns>A list of transactions of the specified type.</returns>
        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(string transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                _logger.LogWarning("Transaction type is null or whitespace. Returning empty list.");
                return Enumerable.Empty<Transaction>();
            }

            var lowerCaseTransactionType = transactionType.ToLower();
            _logger.LogInformation("Fetching transactions of type: {TransactionType}", lowerCaseTransactionType);

            try
            {
                return await _context.Transactions
                    .Include(t => t.User)
                    .Where(t => t.TransactionType.ToLower() == lowerCaseTransactionType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions of type: {TransactionType}", lowerCaseTransactionType);
                throw new Exception("Could not retrieve transactions by type from the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves transactions within a specified date range.
        /// </summary>
        /// <param name="dateRangeFilter">The date range filter criteria.</param>
        /// <returns>A list of transactions within the specified date range.</returns>
        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateRangeFilterDTO dateRangeFilter)
        {
            // Ensure dates are provided
            if (dateRangeFilter == null || dateRangeFilter.StartDate == default || dateRangeFilter.EndDate == default)
            {
                _logger.LogWarning("Invalid date range provided.");
                throw new ArgumentException("Invalid date range");
            }

            _logger.LogInformation("Fetching transactions between {StartDate} and {EndDate}", dateRangeFilter.StartDate, dateRangeFilter.EndDate);
            try
            {
                return await _context.Transactions
                    .Include(t => t.User)
                    .Where(t => t.TransactionDate >= dateRangeFilter.StartDate && t.TransactionDate <= dateRangeFilter.EndDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions between {StartDate} and {EndDate}", dateRangeFilter.StartDate, dateRangeFilter.EndDate);
                throw new Exception("Could not retrieve transactions within the specified date range.", ex);
            }
        }

        /// <summary>
        /// Retrieves transactions within a specified date range and of a specified type.
        /// </summary>
        /// <param name="filter">The filter criteria including date range and transaction type.</param>
        /// <returns>A list of transactions matching the specified criteria.</returns>
        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAndTypeAsync(DateRangeAndTypeFilterDTO filter)
        {
            // Ensure dates are provided
            if (filter == null || filter.StartDate == default || filter.EndDate == default)
            {
                _logger.LogWarning("Invalid date range provided.");
                throw new ArgumentException("Invalid date range");
            }

            _logger.LogInformation("Fetching transactions between {StartDate} and {EndDate} for type: {TransactionType}", filter.StartDate, filter.EndDate, filter.TransactionType);
            try
            {
                return await _context.Transactions
                    .Where(t => t.TransactionDate >= filter.StartDate &&
                                t.TransactionDate <= filter.EndDate &&
                                t.TransactionType == filter.TransactionType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions between {StartDate} and {EndDate} for type: {TransactionType}", filter.StartDate, filter.EndDate, filter.TransactionType);
                throw new Exception("Could not retrieve transactions within the specified date range and type.", ex);
            }
        }
    }
}
