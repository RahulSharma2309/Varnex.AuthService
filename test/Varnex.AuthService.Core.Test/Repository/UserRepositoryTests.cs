// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepositoryTests.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Core.Data;
using Varnex.AuthService.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Varnex.AuthService.Core.Test.Repository
{
    public class UserRepositoryTests
    {
        private readonly AppDbContext dbContext;
        private readonly Mock<ILogger<UserRepository>> logger;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            this.dbContext = new AppDbContext(options);
            this.logger = new Mock<ILogger<UserRepository>>();
        }

        /// <summary>
        /// Verifies that the UserRepository constructor initializes successfully when all dependencies are provided.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenAllSpecified_ThenInitializes()
        {
            // act
            var actual = new UserRepository(this.dbContext, this.logger.Object);

            // assert
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Verifies that the UserRepository constructor throws ArgumentNullException when dbContext is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenDbContextNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserRepository(null!, this.logger.Object));
        }

        /// <summary>
        /// Verifies that the UserRepository constructor throws ArgumentNullException when logger is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenLoggerNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserRepository(this.dbContext, null!));
        }

        /// <summary>
        /// Verifies that AddAsync successfully adds a user to the database.
        /// </summary>
        [Fact]
        public async Task GivenValidUser_WhenAddAsync_ThenAddsUser()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            await repository.AddAsync(user);

            // assert
            var addedUser = await this.dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(addedUser);
            Assert.Equal(user.Email, addedUser.Email);
            Assert.Equal(user.FullName, addedUser.FullName);
        }

        /// <summary>
        /// Verifies that AddAsync throws ArgumentNullException when the user is null.
        /// </summary>
        [Fact]
        public async Task GivenNullUser_WhenAddAsync_ThenThrows()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.AddAsync(null!));
        }

        /// <summary>
        /// Verifies that FindByEmailAsync returns the user when an existing email is provided.
        /// </summary>
        [Fact]
        public async Task GivenExistingEmail_WhenFindByEmailAsync_ThenReturnsUser()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            await this.dbContext.Users.AddAsync(user);
            await this.dbContext.SaveChangesAsync();

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.FindByEmailAsync(user.Email);

            // assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FullName, result.FullName);
        }

        /// <summary>
        /// Verifies that FindByEmailAsync returns null when the email does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentEmail_WhenFindByEmailAsync_ThenReturnsNull()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.FindByEmailAsync("nonexistent@example.com");

            // assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that FindByEmailAsync throws ArgumentException when email is null.
        /// </summary>
        [Fact]
        public async Task GivenNullEmail_WhenFindByEmailAsync_ThenThrows()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.FindByEmailAsync(null!));
        }

        /// <summary>
        /// Verifies that FindByEmailAsync throws ArgumentException when email is empty.
        /// </summary>
        [Fact]
        public async Task GivenEmptyEmail_WhenFindByEmailAsync_ThenThrows()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.FindByEmailAsync(string.Empty));
        }

        /// <summary>
        /// Verifies that FindByIdAsync returns the user when an existing ID is provided.
        /// </summary>
        [Fact]
        public async Task GivenExistingId_WhenFindByIdAsync_ThenReturnsUser()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            await this.dbContext.Users.AddAsync(user);
            await this.dbContext.SaveChangesAsync();

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.FindByIdAsync(user.Id);

            // assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        /// <summary>
        /// Verifies that FindByIdAsync returns null when the ID does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentId_WhenFindByIdAsync_ThenReturnsNull()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.FindByIdAsync(Guid.NewGuid());

            // assert
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that EmailExistsAsync returns true when the email exists in the database.
        /// </summary>
        [Fact]
        public async Task GivenExistingEmail_WhenEmailExistsAsync_ThenReturnsTrue()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            await this.dbContext.Users.AddAsync(user);
            await this.dbContext.SaveChangesAsync();

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.EmailExistsAsync(user.Email);

            // assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifies that EmailExistsAsync returns false when the email does not exist in the database.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentEmail_WhenEmailExistsAsync_ThenReturnsFalse()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            var result = await repository.EmailExistsAsync("nonexistent@example.com");

            // assert
            Assert.False(result);
        }

        /// <summary>
        /// Verifies that EmailExistsAsync throws ArgumentException when email is null.
        /// </summary>
        [Fact]
        public async Task GivenNullEmail_WhenEmailExistsAsync_ThenThrows()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.EmailExistsAsync(null!));
        }

        /// <summary>
        /// Verifies that UpdateAsync successfully updates the user in the database.
        /// </summary>
        [Fact]
        public async Task GivenValidUser_WhenUpdateAsync_ThenUpdatesUser()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "old_password",
                FullName = "Test User",
            };

            await this.dbContext.Users.AddAsync(user);
            await this.dbContext.SaveChangesAsync();

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            user.PasswordHash = "new_password";
            user.FullName = "Updated User";
            await repository.UpdateAsync(user);

            // assert
            var updatedUser = await this.dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("new_password", updatedUser.PasswordHash);
            Assert.Equal("Updated User", updatedUser.FullName);
        }

        /// <summary>
        /// Verifies that UpdateAsync throws ArgumentNullException when the user is null.
        /// </summary>
        [Fact]
        public async Task GivenNullUser_WhenUpdateAsync_ThenThrows()
        {
            // arrange
            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.UpdateAsync(null!));
        }

        /// <summary>
        /// Verifies that SaveChangesAsync successfully persists changes to the database.
        /// </summary>
        [Fact]
        public async Task GivenValidContext_WhenSaveChangesAsync_ThenSavesChanges()
        {
            // arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            this.dbContext.Users.Add(user);

            var repository = new UserRepository(this.dbContext, this.logger.Object);

            // act
            await repository.SaveChangesAsync();

            // assert
            var savedUser = await this.dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(savedUser);
        }
    }
}



