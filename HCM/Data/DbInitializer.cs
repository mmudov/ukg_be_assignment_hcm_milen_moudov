using HCM.Models;

namespace HCM.Data
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Departments.Any())
                return;

            var departments = new Department[]
            {
                new Department { Name = "-" },
                new Department { Name = "Software Development" },
                new Department { Name = "Sales" }
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();

            var users = new User[]
            {
                new User {
                    FirstName = "AdminName",
                    LastName = "AdminFamily",
                    Email = "admin@hcm.com",
                    JobTitle = "Admin",
                    Salary = 10000,
                    Role = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    DepartmentId = 1
                },
                new User {
                    FirstName = "Mariya",
                    LastName = "Georgieva",
                    Email = "manager@hcm.com",
                    JobTitle = "Manager",
                    Salary = 5000,
                    Role = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                    DepartmentId = 2
                },
                new User {
                    FirstName = "Donka",
                    LastName = "Koeva",
                    Email = "dk@hcm.com",
                    JobTitle = "Manager assistant",
                    Salary = 2600,
                    Role = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dkman123?"),
                    DepartmentId = 3
                },
                new User {
                    FirstName = "Ivan",
                    LastName = "Petrov",
                    Email = "ivan@hcm.com",
                    JobTitle = "Developer",
                    Salary = 3000,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ivan123?"),
                    DepartmentId = 2
                },
                new User {
                    FirstName = "Petko",
                    LastName = "Asenov",
                    Email = "pa@hcm.com",
                    JobTitle = "Employee 1",
                    Salary = 3000,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pa123?"),
                    DepartmentId = 2
                },
                new User {
                    FirstName = "Grigor",
                    LastName = "Tinev",
                    Email = "gt@hcm.com",
                    JobTitle = "Employee 2",
                    Salary = 3100,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Gt123?"),
                    DepartmentId = 3
                },
                new User {
                    FirstName = "Petra",
                    LastName = "Rosenova",
                    Email = "pr@hcm.com",
                    JobTitle = "Employee 3",
                    Salary = 3200,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pr123?"),
                    DepartmentId = 3
                },
                new User {
                    FirstName = "Yavor",
                    LastName = "Emilov",
                    Email = "ye@hcm.com",
                    JobTitle = "Employee 4",
                    Salary = 3300,
                    Role = "employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ye123?"),
                    DepartmentId = 3
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
