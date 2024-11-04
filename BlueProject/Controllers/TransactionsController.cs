using BlueProject.Models;
using BlueProject.Models.DTO;
using BlueProject.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlueProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            return Ok(await _transactionService.GetAllTransactionsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null) return NotFound();
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTransaction([FromBody] Transaction transaction)
        {
            await _transactionService.AddTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
        }

        // New endpoint to get transactions by UserId
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserId(int userId)
        {
            var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);

            if (transactions == null)
            {
                return NotFound(); // 404 Not Found if no transactions are found
            }

            return Ok(transactions); // 200 OK with the list of transactions
        }



        // New endpoint to get filtered transactions
        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions([FromBody] TransactionFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest("Filter cannot be null");
            }

            var transactions = await _transactionService.GetFilteredTransactionsAsync(filter);

            if (transactions == null || !transactions.Any())
            {
                return NotFound(); // Return 404 if no transactions found
            }

            return Ok(transactions); // Return 200 with the list of transactions
        }

        // New endpoint to get transactions by TransactionType
        [HttpGet("type/{transactionType}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByType(string transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                return BadRequest("Transaction type cannot be empty.");
            }

            var transactions = await _transactionService.GetTransactionsByTypeAsync(transactionType);

            if (transactions == null || !transactions.Any())
            {
                return NotFound(); // Return 404 if no transactions found
            }

            return Ok(transactions); // Return 200 with the list of transactions
        }


        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest("Start date and end date must be provided");
            }

            var dateRangeFilter = new DateRangeFilterDTO { StartDate = startDate, EndDate = endDate };
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(dateRangeFilter);

            if (transactions == null || !transactions.Any())
            {
                return NotFound(); // Return 404 if no transactions found
            }

            return Ok(transactions); // Return 200 with the list of transactions
        }


        // New endpoint to get filtered transactions by date range and type
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string transactionType)
        {
            if (startDate == default || endDate == default || string.IsNullOrWhiteSpace(transactionType))
            {
                return BadRequest("Invalid filter parameters.");
            }

            var filter = new DateRangeAndTypeFilterDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = transactionType
            };

            var transactions = await _transactionService.GetTransactionsByDateRangeAndTypeAsync(filter);

            if (transactions == null || !transactions.Any())
            {
                return NotFound(); // Return 404 if no transactions found
            }

            return Ok(transactions); // Return 200 with the list of transactions
        }


    }
}
