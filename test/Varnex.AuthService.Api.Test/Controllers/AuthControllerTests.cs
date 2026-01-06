// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthControllerTests.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Varnex.AuthService.Abstractions.DTOs;
using Varnex.AuthService.Abstractions.Models;
using Varnex.AuthService.Api.Controllers;
using Varnex.AuthService.Core.Business;
using Varnex.AuthService.Core.Data;
using Ep.Platform.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Varnex.AuthService.Api.Test.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> authService;
        private readonly Mock<IJwtTokenGenerator> jwtTokenGenerator;
        private readonly Mock<ILogger<AuthController>> logger;
        private readonly Mock<IHttpClientFactory> httpClientFactory;
        private readonly AppDbContext dbContext;

        public AuthControllerTests()
        {
            this.authService = new Mock<IAuthService>();
            this.jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            this.logger = new Mock<ILogger<AuthController>>();
            this.httpClientFactory = new Mock<IHttpClientFactory>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            this.dbContext = new AppDbContext(options);
        }

        /// <summary>
        /// Verifies that the AuthController constructor initializes successfully when all dependencies are provided.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenAllSpecified_ThenInitializes()
        {
            // act
            var actual = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // assert
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Verifies that the AuthController constructor throws ArgumentNullException when authService is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenAuthServiceNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AuthController(
                    null!,
                    this.jwtTokenGenerator.Object,
                    this.logger.Object,
                    this.httpClientFactory.Object,
                    this.dbContext));
        }

        /// <summary>
        /// Verifies that the AuthController constructor throws ArgumentNullException when jwtTokenGenerator is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenJwtTokenGeneratorNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AuthController(
                    this.authService.Object,
                    null!,
                    this.logger.Object,
                    this.httpClientFactory.Object,
                    this.dbContext));
        }

        /// <summary>
        /// Verifies that the AuthController constructor throws ArgumentNullException when logger is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenLoggerNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AuthController(
                    this.authService.Object,
                    this.jwtTokenGenerator.Object,
                    null!,
                    this.httpClientFactory.Object,
                    this.dbContext));
        }

        /// <summary>
        /// Verifies that the AuthController constructor throws ArgumentNullException when httpClientFactory is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenHttpClientFactoryNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AuthController(
                    this.authService.Object,
                    this.jwtTokenGenerator.Object,
                    this.logger.Object,
                    null!,
                    this.dbContext));
        }

        /// <summary>
        /// Verifies that the AuthController constructor throws ArgumentNullException when dbContext is null.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenDbContextNull_ThenThrows()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AuthController(
                    this.authService.Object,
                    this.jwtTokenGenerator.Object,
                    this.logger.Object,
                    this.httpClientFactory.Object,
                    null!));
        }

        /// <summary>
        /// Verifies that Login returns OK (200) with a token when valid credentials are provided.
        /// </summary>
        [Fact]
        public async Task GivenValidData_WhenLogin_ThenReturnOk()
        {
            // arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "Test User",
            };

            this.authService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((user, (string?)null));

            this.jwtTokenGenerator
                .Setup(x => x.GenerateToken(It.IsAny<Dictionary<string, string>>(), It.IsAny<TimeSpan?>()))
                .Returns("test-token");

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.Login(loginDto);

            // assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            // verify
            this.authService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
            this.jwtTokenGenerator.Verify(x => x.GenerateToken(It.IsAny<Dictionary<string, string>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        /// <summary>
        /// Verifies that Login returns Unauthorized (401) when invalid credentials are provided.
        /// </summary>
        [Fact]
        public async Task GivenInvalidCredentials_WhenLogin_ThenReturnUnauthorized()
        {
            // arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };

            this.authService
                .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(((User?)null, "Invalid credentials"));

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.Login(loginDto);

            // assert
            Assert.NotNull(result);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);

            // verify
            this.authService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Once);
            this.jwtTokenGenerator.Verify(x => x.GenerateToken(It.IsAny<Dictionary<string, string>>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        /// <summary>
        /// Verifies that Login returns BadRequest (400) when email is empty.
        /// </summary>
        [Fact]
        public async Task GivenEmptyEmail_WhenLogin_ThenReturnBadRequest()
        {
            // arrange
            var loginDto = new LoginDto { Email = string.Empty, Password = "Password123!" };

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.Login(loginDto);

            // assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);

            // verify
            this.authService.Verify(x => x.LoginAsync(It.IsAny<LoginDto>()), Times.Never);
        }

        /// <summary>
        /// Verifies that Me endpoint returns OK (200) with user details when a valid authenticated user ID is provided.
        /// </summary>
        [Fact]
        public async Task GivenValidUserId_WhenMe_ThenReturnOk()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
            };

            this.authService
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // Setup User claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            // act
            var result = await controller.Me();

            // assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            // verify
            this.authService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        /// <summary>
        /// Verifies that Me endpoint returns Unauthorized (401) when no user ID is present in claims.
        /// </summary>
        [Fact]
        public async Task GivenNoUserId_WhenMe_ThenReturnUnauthorized()
        {
            // arrange
            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() },
            };

            // act
            var result = await controller.Me();

            // assert
            Assert.NotNull(result);
            Assert.IsType<UnauthorizedResult>(result);

            // verify
            this.authService.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        /// <summary>
        /// Verifies that ResetPassword returns OK (200) when valid email and new password are provided.
        /// </summary>
        [Fact]
        public async Task GivenValidData_WhenResetPassword_ThenReturnOk()
        {
            // arrange
            var resetDto = new ResetPasswordDto { Email = "test@example.com", NewPassword = "NewPassword123!" };

            this.authService
                .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                .ReturnsAsync(true);

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.ResetPassword(resetDto);

            // assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            // verify
            this.authService.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()), Times.Once);
        }

        /// <summary>
        /// Verifies that ResetPassword returns NotFound (404) when the user email does not exist.
        /// </summary>
        [Fact]
        public async Task GivenNonExistentUser_WhenResetPassword_ThenReturnNotFound()
        {
            // arrange
            var resetDto = new ResetPasswordDto { Email = "nonexistent@example.com", NewPassword = "NewPassword123!" };

            this.authService
                .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                .ReturnsAsync(false);

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.ResetPassword(resetDto);

            // assert
            Assert.NotNull(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);

            // verify
            this.authService.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()), Times.Once);
        }

        /// <summary>
        /// Verifies that ResetPassword returns BadRequest (400) when email is empty.
        /// </summary>
        [Fact]
        public async Task GivenEmptyEmail_WhenResetPassword_ThenReturnBadRequest()
        {
            // arrange
            var resetDto = new ResetPasswordDto { Email = string.Empty, NewPassword = "NewPassword123!" };

            var controller = new AuthController(
                this.authService.Object,
                this.jwtTokenGenerator.Object,
                this.logger.Object,
                this.httpClientFactory.Object,
                this.dbContext);

            // act
            var result = await controller.ResetPassword(resetDto);

            // assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);

            // verify
            this.authService.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()), Times.Never);
        }
    }
}




