// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HealthControllerTests.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Net;
using Varnex.AuthService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Varnex.AuthService.Api.Test.Controllers
{
    public class HealthControllerTests
    {
        /// <summary>
        /// Verifies that the HealthController can be instantiated successfully.
        /// </summary>
        [Fact]
        public void GivenCtor_WhenInitialized_ThenCreatesInstance()
        {
            // act
            var controller = new HealthController();

            // assert
            Assert.NotNull(controller);
        }

        /// <summary>
        /// Verifies that the health check endpoint returns OK (200) status.
        /// </summary>
        [Fact]
        public void GivenHealthCheck_WhenGet_ThenReturnsOk()
        {
            // arrange
            var controller = new HealthController();

            // act
            var result = controller.Get();

            // assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        }

        /// <summary>
        /// Verifies that the health check endpoint returns the correct status and service name.
        /// </summary>
        [Fact]
        public void GivenHealthCheck_WhenGet_ThenReturnsHealthyStatus()
        {
            // arrange
            var controller = new HealthController();

            // act
            var result = controller.Get();

            // assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var statusProperty = value.GetType().GetProperty("status");
            var serviceProperty = value.GetType().GetProperty("service");

            Assert.NotNull(statusProperty);
            Assert.NotNull(serviceProperty);

            Assert.Equal("healthy", statusProperty.GetValue(value));
            Assert.Equal("auth-service", serviceProperty.GetValue(value));
        }
    }
}



