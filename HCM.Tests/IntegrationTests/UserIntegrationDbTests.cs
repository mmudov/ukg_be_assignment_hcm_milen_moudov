using HCM.Data;
using HCM.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using FluentAssertions;

namespace HCM.Tests.IntegrationTests
{
    public class UserIntegrationDbTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UserIntegrationDbTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("HCMTestDb")
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task Can_Add_User_To_Database()
        {
            var context = CreateInMemoryDbContext();
            var department = new Department { Name = "HR" };
            context.Departments.Add(department);
            context.SaveChanges();

            var user = new User
            {
                FirstName = "Kotse",
                LastName = "Kolev",
                Email = "kotse.kolev@hcm.com",
                JobTitle = "Software Developer",
                Salary = 75000,
                Role = "Admin",
                PasswordHash = "hashedpassword123",
                DepartmentId = department.Id
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var savedUser = await context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == "kotse.kolev@hcm.com");

            // Assert
            savedUser.Should().NotBeNull();
            savedUser.FirstName.Should().Be("Kotse");
            savedUser.LastName.Should().Be("Kolev");
            savedUser.Email.Should().Be("kotse.kolev@hcm.com");
            savedUser.Department.Name.Should().Be("HR");
        }

        [Fact]
        public async Task Can_Get_All_Users_From_Database()
        {
            var context = CreateInMemoryDbContext();
            var department = new Department { Name = "HR" };
            context.Departments.Add(department);
            context.SaveChanges();

            var user1 = new User
            {
                FirstName = "Elka",
                LastName = "Tomova",
                Email = "elka.tomova@hcm.com",
                JobTitle = "Project Manager",
                Salary = 80000,
                Role = "Manager",
                PasswordHash = "hashedpassword123",
                DepartmentId = department.Id
            };

            var user2 = new User
            {
                FirstName = "Ben",
                LastName = "Aflek",
                Email = "ben.aflek@hcm.com",
                JobTitle = "QA Engineer",
                Salary = 65000,
                Role = "Employee",
                PasswordHash = "hashedpassword123",
                DepartmentId = department.Id
            };

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            // Act
            var users = await context.Users.Include(u => u.Department).ToListAsync();

            // Assert
            users.Should().HaveCount(2);
            users[0].FirstName.Should().Be("Elka");
            users[1].FirstName.Should().Be("Ben");
        }
    }
}
