using BlueProject.Models;
using BlueProject.Models.DTO;
using BlueProject.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlueProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            _logger.LogInformation("Fetching all transactions.");
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all transactions.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving transactions.");
            }
        }

        /// <summary>
        /// Retrieves a specific transaction by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            _logger.LogInformation("Fetching transaction with ID: {TransactionId}", id);
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found.", id);
                    return NotFound();
                }
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transaction with ID: {TransactionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the transaction.");
            }
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                _logger.LogWarning("Attempted to create a null transaction.");
                return BadRequest("Transaction cannot be null.");
            }

            _logger.LogInformation("Creating a new transaction: {@Transaction}", transaction);
            try
            {
                await _transactionService.AddTransactionAsync(transaction);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating transaction: {@Transaction}", transaction);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the transaction.");
            }
        }

        /// <summary>
        /// Retrieves transactions for a specific user by their ID.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserId(int userId)
        {
            _logger.LogInformation("Fetching transactions for user ID: {UserId}", userId);
            try
            {
                var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found for user ID: {UserId}", userId);
                    return NotFound();
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions for user ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user transactions.");
            }
        }

        /// <summary>
        /// Retrieves filtered transactions based on specified criteria.
        /// </summary>
        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions([FromBody] TransactionFilterDTO filter)
        {
            if (filter == null)
            {
                _logger.LogWarning("Filter cannot be null.");
                return BadRequest("Filter cannot be null.");
            }

            _logger.LogInformation("Fetching filtered transactions with criteria: {@Filter}", filter);
            try
            {
                var transactions = await _transactionService.GetFilteredTransactionsAsync(filter);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found for the provided filter: {@Filter}", filter);
                    return NotFound();
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered transactions with criteria: {@Filter}", filter);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving filtered transactions.");
            }
        }

        /// <summary>
        /// Retrieves transactions by their type.
        /// </summary>
        [HttpGet("type/{transactionType}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByType(string transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                _logger.LogWarning("Transaction type cannot be empty.");
                return BadRequest("Transaction type cannot be empty.");
            }

            _logger.LogInformation("Fetching transactions of type: {TransactionType}", transactionType);
            try
            {
                var transactions = await _transactionService.GetTransactionsByTypeAsync(transactionType);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found for type: {TransactionType}", transactionType);
                    return NotFound();
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions of type: {TransactionType}", transactionType);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving transactions by type.");
            }
        }

        /// <summary>
        /// Retrieves transactions within a specified date range.
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                _logger.LogWarning("Start date and end date must be provided.");
                return BadRequest("Start date and end date must be provided.");
            }

            var dateRangeFilter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
            _logger.LogInformation("Fetching transactions between {StartDate} and {EndDate}", startDate, endDate);
            try
            {
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(dateRangeFilter);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found between {StartDate} and {EndDate}", startDate, endDate);
                    return NotFound();
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching transactions between {StartDate} and {EndDate}", startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving transactions by date range.");
            }
        }

        /// <summary>
        /// Retrieves filtered transactions based on date range and type.
        /// </summary>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string transactionType)
        {
            if (startDate == default || endDate == default || string.IsNullOrWhiteSpace(transactionType))
            {
                _logger.LogWarning("Invalid filter parameters provided.");
                return BadRequest("Invalid filter parameters.");
            }

            var filter = new DateRangeAndTypeFilterDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = transactionType
            };

            _logger.LogInformation("Fetching filtered transactions with criteria: {@Filter}", filter);
            try
            {
                var transactions = await _transactionService.GetTransactionsByDateRangeAndTypeAsync(filter);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found for the filter: {@Filter}", filter);
                    return NotFound();
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered transactions with criteria: {@Filter}", filter);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving filtered transactions.");
            }
        }
    }
}
