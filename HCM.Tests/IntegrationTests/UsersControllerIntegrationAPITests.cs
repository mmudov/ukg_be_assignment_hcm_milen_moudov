using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HCM.Tests.IntegrationTests
{
    public class UsersControllerIntegrationAPITests : IClassFixture<CustomWebApplicationFactory>
    {
        private CustomWebApplicationFactory _factory;
        public UsersControllerIntegrationAPITests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

        }

        private WebApplicationFactory<Program> GetFactoryForRole(string role)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(_ => role);
                });
            });
        }

        [Fact]
        public async Task Index_ReturnsOk_WithAdminRole()
        {
            var client = GetFactoryForRole("admin").CreateClient();

            // Act
            var response = await client.GetAsync("/Users");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Index_ReturnsForbidden_ForManagerRole()
        {
            var client = GetFactoryForRole("manager").CreateClient();

            // Act
            var response = await client.GetAsync("/Users");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_ReturnsView_WithAdminRole()
        {
            var client = GetFactoryForRole("admin").CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "true");

            // Act
            var response = await client.GetAsync("/Users/Create");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_ReturnsForbidden_ForManagerRole()
        {
            var client = GetFactoryForRole("manager").CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await client.GetAsync("/Users/Create");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        
        [Fact]
        public async Task Edit_ReturnsOk_WithManagerRole()
        {
            var client = GetFactoryForRole("manager").CreateClient();
            var userId = 1;

            // Act
            var response = await client.GetAsync($"/Users/Edit/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WithAdminRole()
        {
            var client = GetFactoryForRole("admin").CreateClient();
            var userId = 1;

            // Act
            var response = await client.GetAsync($"/Users/Delete/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
