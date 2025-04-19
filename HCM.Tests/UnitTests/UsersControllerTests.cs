using System.Security.Claims;
using HCM.Controllers;
using HCM.Data;
using HCM.Models;
using HCM.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HCM.Tests.UnitTests;

public class UsersControllerUnitTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly UsersController _controller;

    public UsersControllerUnitTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        var mockUsers = new Mock<DbSet<User>>();
        var usersList = new List<User>
        {
            new User { Id = 1, FirstName = "Petko", LastName = "Asenov", Role = "admin", DepartmentId = 1 },
            new User { Id = 2, FirstName = "Grigor", LastName = "Tinev", Role = "employee", DepartmentId = 1 },
            new User { Id = 3, FirstName = "Donka", LastName = "Koeva", Role = "employee", DepartmentId = 2 }
        }.AsQueryable();

        mockUsers.As<IQueryable<User>>().Setup(m => m.Provider).Returns(usersList.Provider);
        mockUsers.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersList.Expression);
        mockUsers.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersList.ElementType);
        mockUsers.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(usersList.GetEnumerator());

        _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);

        var departmentsList = new List<Department>
        {
            new Department { Id = 1, Name = "HR" },
            new Department { Id = 2, Name = "IT" }
        }.AsQueryable();

        var mockDepartments = new Mock<DbSet<Department>>();
        mockDepartments.As<IQueryable<Department>>().Setup(m => m.Provider).Returns(departmentsList.Provider);
        mockDepartments.As<IQueryable<Department>>().Setup(m => m.Expression).Returns(departmentsList.Expression);
        mockDepartments.As<IQueryable<Department>>().Setup(m => m.ElementType).Returns(departmentsList.ElementType);
        mockDepartments.As<IQueryable<Department>>().Setup(m => m.GetEnumerator()).Returns(departmentsList.GetEnumerator());

        _mockContext.Setup(c => c.Departments).Returns(mockDepartments.Object);


        _controller = new UsersController(_mockContext.Object);
    }

    [Fact]
    public void Index_AdminRole_ReturnsAllUsers()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = _controller.Index() as ViewResult;

        var model = result?.Model as List<User>;
        Assert.NotNull(model);
        Assert.Equal(3, model.Count);
    }

    [Fact]
    public void Details_EmployeeCannotAccessOtherUsers_ReturnsForbidden()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "2"),
        new Claim(ClaimTypes.Role, "employee")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = _controller.Details(1) as ForbidResult;

        Assert.NotNull(result);
    }

    [Fact]
    public void Create_GET_ReturnsViewWithDepartments()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var result = _controller.Create() as ViewResult;

        Assert.NotNull(result);
        Assert.NotNull(result.ViewData["Departments"]);
    }

    [Fact]
    public void Create_POST_ValidUser_AddsUserAndRedirects()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var newUserDTO = new UserDTO
        {
            FirstName = "Pepa",
            LastName = "Nikolova",
            Email = "pepa@hcm.com",
            Role = "employee",
            DepartmentId = 1
        };

        var password = "password123";

        var result = _controller.Create(newUserDTO, password) as RedirectToActionResult;

        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public void Edit_GET_ExistingUser_ReturnsViewWithUserData()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Lazar",
            LastName = "Angelov",
            Role = "employee",
            DepartmentId = 1
        };

        _mockContext.Setup(c => c.Users.Find(userId)).Returns(existingUser);

        var result = _controller.Edit(userId) as ViewResult;

        Assert.NotNull(result);
        var model = result.Model as User;
        Assert.NotNull(model);
        Assert.Equal(userId, model.Id);
    }

    [Fact]
    public void Edit_POST_ValidUser_UpdatesUserAndRedirects()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var updatedUserDTO = new UserDTO
        {
            Id = 1,
            FirstName = "Petra",
            LastName = "Rosenova",
            Email = "petra.rosenova@hcm.com",
            Role = "employee",
            DepartmentId = 1
        };

        var userFound = new User
        {
            Id = 1,
            FirstName = "Yavor",
            LastName = "Emilov",
            Role = "employee",
            DepartmentId = 1
        };
        _mockContext.Setup(c => c.Users.Find(updatedUserDTO.Id)).Returns(userFound);

        var result = _controller.Edit(updatedUserDTO, "newpassword123") as RedirectToActionResult;

        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public void Delete_GET_ExistingUser_ReturnsViewWithUserData()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Trayana",
            LastName = "Yana",
            Role = "employee",
            DepartmentId = 1
        };

        _mockContext.Setup(c => c.Users.Find(userId)).Returns(existingUser);

        var result = _controller.Delete(userId) as ViewResult;

        Assert.NotNull(result);
        var model = result.Model as User;
        Assert.NotNull(model);
        Assert.Equal(userId, model.Id);
    }

    [Fact]
    public void Delete_POST_ExistingUser_RemovesUserAndRedirects()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "admin")
    };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var userId = 1;
        var userToDelete = new User
        {
            Id = userId,
            FirstName = "Pepi",
            LastName = "Yurukov",
            Role = "employee",
            DepartmentId = 1
        };

        _mockContext.Setup(c => c.Users.Find(userId)).Returns(userToDelete);

        var result = _controller.DeleteConfirmed(userId) as RedirectToActionResult;

        _mockContext.Verify(m => m.Users.Remove(It.IsAny<User>()), Times.Once);
        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }
}
