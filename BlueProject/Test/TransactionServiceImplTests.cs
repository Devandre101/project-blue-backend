using BlueProject.Data;
using BlueProject.Models;
using BlueProject.Models.DTO;
using BlueProject.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace BlueProject.Test
{
    public class TransactionServiceImplTests : IAsyncLifetime
    {
        private readonly ApiContext _context;
        private readonly TransactionServiceImpl _service;

        public TransactionServiceImplTests()
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique database for each test
                .Options;
            _context = new ApiContext(options);
            _service = new TransactionServiceImpl(_context);
        }

        public async Task InitializeAsync()
        {
            // Clear the database before each test
            _context.Transactions.RemoveRange(_context.Transactions);
            await _context.SaveChangesAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;



        [Fact]
        public async Task AddTransactionAsync_ShouldAddTransaction()
        {
            var transaction = new Transaction { TransactionId = 1, TransactionType ="deposit" };

            await _service.AddTransactionAsync(transaction);

            Assert.Contains(transaction, _context.Transactions);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnAllTransactions()
        {
            // Arrange
            var user = new User { UserId = 1, Username = "Test User", Email ="jy@mm.com", PasswordHash ="jskdkf" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var transaction1 = new Transaction { TransactionType = "withdrawal", User = user };
            var transaction2 = new Transaction { TransactionType = "withdrawal", User = user };
            _context.Transactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTransactionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }


        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnTransaction_WhenTransactionExists()
        {
            // Arrange
            var user = new User { UserId = 1, Username = "Test User", Email= "test@test.com", PasswordHash = "xvhsdjek" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var transaction = new Transaction { TransactionId = 10, TransactionType = "deposit", User = user };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTransactionByIdAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.TransactionId);
            Assert.Equal("deposit", result.TransactionType);
            Assert.Equal(user.UserId, result.User.UserId);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Act
            var result = await _service.GetTransactionByIdAsync(999); // Use a non-existent ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTransactionsByUserIdAsync_ShouldReturnTransactions_WhenTransactionsExistForUserId()
        {
            // Arrange
            var user1 = new User { UserId = 1, Username = "Test User1", Email = "test@test.com", PasswordHash = "xvhsdjek" };
            var user2 = new User { UserId = 2, Username = "Test User2", Email = "test@test.com", PasswordHash = "xvhsdjek" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var transaction1 = new Transaction { TransactionId = 1, TransactionType = "deposit", UserId = user1.UserId };
            var transaction2 = new Transaction { TransactionId = 2, TransactionType = "withdrawal", UserId = user1.UserId };
            var transaction3 = new Transaction { TransactionId = 3, TransactionType = "deposit", UserId = user2.UserId };
            _context.Transactions.AddRange(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTransactionsByUserIdAsync(user1.UserId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(user1.UserId, t.UserId));
        }


        [Fact]
        public async Task GetTransactionsByUserIdAsync_ShouldReturnEmpty_WhenNoTransactionsExistForUserId()
        {
            // Arrange
            var user1 = new User { UserId = 1, Username = "Test User1", Email = "test@test.com", PasswordHash = "xvhsdjek" };
            var user2 = new User { UserId = 2, Username = "Test User2", Email = "test@test.com", PasswordHash = "xvhsdjek" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var transaction1 = new Transaction { TransactionId = 1, TransactionType = "deposit", UserId = user1.UserId };
            var transaction2 = new Transaction { TransactionId = 2, TransactionType = "withdrawal", UserId = user1.UserId };
            _context.Transactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTransactionsByUserIdAsync(user2.UserId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTransactionsByTypeAsync_ShouldReturnTransactions_WhenValidTransactionTypeExists()
        {
            // Arrange
            var user = new User { UserId = 1,
                Username = "Test User1",
                Email = "test@test.com",
                PasswordHash = "xvhsdjek" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var transaction1 = new Transaction { TransactionId = 1, TransactionType = "deposit", UserId = user.UserId };
            var transaction2 = new Transaction { TransactionId = 2, TransactionType = "deposit", UserId = user.UserId };
            var transaction3 = new Transaction { TransactionId = 3, TransactionType = "withdrawal", UserId = user.UserId };
            _context.Transactions.AddRange(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTransactionsByTypeAsync("deposit");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal("deposit", t.TransactionType.ToLower()));
        }

        [Fact]
        public async Task GetTransactionsByTypeAsync_ShouldReturnEmpty_WhenTransactionTypeIsNullOrWhiteSpace()
        {
            // Act - Test with null
            var resultNull = await _service.GetTransactionsByTypeAsync(null);
            Assert.Empty(resultNull);

            // Act - Test with empty string
            var resultEmpty = await _service.GetTransactionsByTypeAsync(string.Empty);
            Assert.Empty(resultEmpty);

            // Act - Test with whitespace
            var resultWhitespace = await _service.GetTransactionsByTypeAsync("   ");
            Assert.Empty(resultWhitespace);
        }

        [Fact]
        public async Task GetTransactionsByDateRangeAsync_ShouldReturnTransactions_WhenValidDateRangeProvided()
        {
            // Arrange
            var user = new User { UserId = 1,
                Username = "Test User1",
                Email = "test@test.com",
                PasswordHash = "xvhsdjek" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var transaction1 = new Transaction
            {
                TransactionId = 1,
                TransactionType = "deposit",
                TransactionDate = new DateTime(2023, 11, 1),
                UserId = user.UserId
            };
            var transaction2 = new Transaction
            {
                TransactionId = 2,
                TransactionType = "withdrawal",
                TransactionDate = new DateTime(2023, 11, 5),
                UserId = user.UserId
            };
            var transaction3 = new Transaction
            {
                TransactionId = 3,
                TransactionType = "deposit",
                TransactionDate = new DateTime(2023, 10, 30),
                UserId = user.UserId
            };
            _context.Transactions.AddRange(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();

            var dateRangeFilter = new DateRangeFilterDTO
            {
                StartDate = new DateTime(2023, 11, 1),
                EndDate = new DateTime(2023, 11, 5)
            };

            // Act
            var result = await _service.GetTransactionsByDateRangeAsync(dateRangeFilter);

            // Assert
            Assert.Equal(2, result.Count()); // transaction1 and transaction2 should be returned
            Assert.Contains(result, t => t.TransactionId == transaction1.TransactionId);
            Assert.Contains(result, t => t.TransactionId == transaction2.TransactionId);
        }


    }
}
