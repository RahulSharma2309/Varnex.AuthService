// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthServiceTests.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Varnex.AuthService.Abstractions.DTOs;
using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Core.Business;
using Varnex.AuthService.Core.Repository;
using Ep.Platform.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using VarnexAuthServiceBusiness = Varnex.AuthService.Core.Business.AuthService;

namespace Varnex.AuthService.Core.Test.Business
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> userRepository;
        private readonly Mock<IPasswordHasher> passwordHasher;
        private readonly Mock<ILogger<VarnexAuthServiceBusiness>> logger;

        public AuthServiceTests()
        {
            this.userRepository = new Mock<IUserRepository>();
            this.passwordHasher = new Mock<IPasswordHasher>();
            this.logger = new Mock<ILogger<VarnexAuthServiceBusiness>>();
        }

        /// <summary>
        /// Verifies that the AuthService constructor initializes successfully when all dependencies are provided.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenAllSpecified_ThenInitializes()
        {
            // act
            var actual = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // assert
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Verifies that the AuthService constructor throws ArgumentNullException when userRepository is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenUserRepositoryNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new VarnexAuthServiceBusiness(
                    null!,
                    this.passwordHasher.Object,
                    this.logger.Object));
        }

        /// <summary>
        /// Verifies that the AuthService constructor throws ArgumentNullException when passwordHasher is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenPasswordHasherNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new VarnexAuthServiceBusiness(
                    this.userRepository.Object,
                    null!,
                    this.logger.Object));
        }

        /// <summary>
        /// Verifies that the AuthService constructor throws ArgumentNullException when logger is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenLoggerNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new VarnexAuthServiceBusiness(
                    this.userRepository.Object,
                    this.passwordHasher.Object,
                    null!));
        }

        /// <summary>
        /// Verifies that RegisterAsync returns a user with hashed password when valid registration data is provided.
        /// </summary>
        [Fact]
        public async Task GivenValidData_WhenRegisterAsync_ThenReturnsUser()
        {
            // arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "Test User",
                PhoneNumber = "+1234567890",
                Address = "123 Test St",
            };

            this.passwordHasher
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            this.userRepository
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var result = await authService.RegisterAsync(registerDto);

            // assert
            Assert.NotNull(result);
            Assert.Equal(registerDto.Email, result.Email);
            Assert.Equal(registerDto.FullName, result.FullName);
            Assert.Equal("hashed_password", result.PasswordHash);

            // verify
            this.passwordHasher.Verify(x => x.HashPassword(registerDto.Password), Times.Once);
            this.userRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Verifies that RegisterAsync throws ArgumentNullException when the DTO is null.
        /// </summary>
        [Fact]
        public async Task GivenNullDto_WhenRegisterAsync_ThenThrows()
        {
            // arrange
            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                authService.RegisterAsync(null!));
        }

        /// <summary>
        /// Verifies that LoginAsync returns the user when valid credentials are provided.
        /// </summary>
        [Fact]
        public async Task GivenValidCredentials_WhenLoginAsync_ThenReturnsUser()
        {
            // arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            this.userRepository
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            this.passwordHasher
                .Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(true);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var (resultUser, error) = await authService.LoginAsync(loginDto);

            // assert
            Assert.NotNull(resultUser);
            Assert.Null(error);
            Assert.Equal(user.Id, resultUser.Id);
            Assert.Equal(user.Email, resultUser.Email);

            // verify
            this.userRepository.Verify(x => x.FindByEmailAsync(loginDto.Email), Times.Once);
            this.passwordHasher.Verify(x => x.VerifyPassword(loginDto.Password, user.PasswordHash), Times.Once);
        }

        /// <summary>
        /// Verifies that LoginAsync returns an error when the user does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentUser_WhenLoginAsync_ThenReturnsError()
        {
            // arrange
            var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "Password123!" };

            this.userRepository
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var (resultUser, error) = await authService.LoginAsync(loginDto);

            // assert
            Assert.Null(resultUser);
            Assert.NotNull(error);
            Assert.Equal("Invalid credentials", error);

            // verify
            this.userRepository.Verify(x => x.FindByEmailAsync(loginDto.Email), Times.Once);
            this.passwordHasher.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that LoginAsync returns an error when an invalid password is provided.
        /// </summary>
        [Fact]
        public async Task GivenInvalidPassword_WhenLoginAsync_ThenReturnsError()
        {
            // arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            this.userRepository
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            this.passwordHasher
                .Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(false);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var (resultUser, error) = await authService.LoginAsync(loginDto);

            // assert
            Assert.Null(resultUser);
            Assert.NotNull(error);
            Assert.Equal("Invalid credentials", error);

            // verify
            this.userRepository.Verify(x => x.FindByEmailAsync(loginDto.Email), Times.Once);
            this.passwordHasher.Verify(x => x.VerifyPassword(loginDto.Password, user.PasswordHash), Times.Once);
        }

        /// <summary>
        /// Verifies that LoginAsync throws ArgumentNullException when the DTO is null.
        /// </summary>
        [Fact]
        public async Task GivenNullDto_WhenLoginAsync_ThenThrows()
        {
            // arrange
            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                authService.LoginAsync(null!));
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync returns true and updates the password when valid data is provided.
        /// </summary>
        [Fact]
        public async Task GivenValidData_WhenResetPasswordAsync_ThenReturnsTrue()
        {
            // arrange
            var resetDto = new ResetPasswordDto { Email = "test@example.com", NewPassword = "NewPassword123!" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "old_hashed_password",
                FullName = "Test User",
            };

            this.userRepository
                .Setup(x => x.FindByEmailAsync(resetDto.Email))
                .ReturnsAsync(user);

            this.passwordHasher
                .Setup(x => x.HashPassword(resetDto.NewPassword))
                .Returns("new_hashed_password");

            this.userRepository
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var result = await authService.ResetPasswordAsync(resetDto);

            // assert
            Assert.True(result);
            Assert.Equal("new_hashed_password", user.PasswordHash);

            // verify
            this.userRepository.Verify(x => x.FindByEmailAsync(resetDto.Email), Times.Once);
            this.passwordHasher.Verify(x => x.HashPassword(resetDto.NewPassword), Times.Once);
            this.userRepository.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync returns false when the user does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentUser_WhenResetPasswordAsync_ThenReturnsFalse()
        {
            // arrange
            var resetDto = new ResetPasswordDto { Email = "nonexistent@example.com", NewPassword = "NewPassword123!" };

            this.userRepository
                .Setup(x => x.FindByEmailAsync(resetDto.Email))
                .ReturnsAsync((User?)null);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var result = await authService.ResetPasswordAsync(resetDto);

            // assert
            Assert.False(result);

            // verify
            this.userRepository.Verify(x => x.FindByEmailAsync(resetDto.Email), Times.Once);
            this.passwordHasher.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
            this.userRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync throws ArgumentNullException when the DTO is null.
        /// </summary>
        [Fact]
        public async Task GivenNullDto_WhenResetPasswordAsync_ThenThrows()
        {
            // arrange
            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                authService.ResetPasswordAsync(null!));
        }

        /// <summary>
        /// Verifies that GetUserByIdAsync returns the user when a valid user ID is provided.
        /// </summary>
        [Fact]
        public async Task GivenValidId_WhenGetUserByIdAsync_ThenReturnsUser()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                FullName = "Test User",
            };

            this.userRepository
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var result = await authService.GetUserByIdAsync(userId);

            // assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.Email, result.Email);

            // verify
            this.userRepository.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// Verifies that GetUserByIdAsync returns null when the user ID does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentId_WhenGetUserByIdAsync_ThenReturnsNull()
        {
            // arrange
            var userId = Guid.NewGuid();

            this.userRepository
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act
            var result = await authService.GetUserByIdAsync(userId);

            // assert
            Assert.Null(result);

            // verify
            this.userRepository.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// Verifies that RegisterAsync propagates exceptions from the repository layer.
        /// </summary>
        [Fact]
        public async Task GivenException_WhenRegisterAsync_ThenThrows()
        {
            // arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "Test User",
                PhoneNumber = "+1234567890",
                Address = "123 Test St",
            };

            this.passwordHasher
                .Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            this.userRepository
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database error"));

            var authService = new VarnexAuthServiceBusiness(
                this.userRepository.Object,
                this.passwordHasher.Object,
                this.logger.Object);

            // act & assert
            await Assert.ThrowsAsync<Exception>(() =>
                authService.RegisterAsync(registerDto));
        }
    }
}




