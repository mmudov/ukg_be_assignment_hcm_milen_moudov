using System.Security.Claims;
using HCM.Data;
using HCM.Models;
using HCM.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCM.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var role = User.FindFirst(ClaimTypes.Role).Value;

            if (role == "admin")
            {
                return View(_context.Users.Include(u => u.Department).ToList());
            }

            if (role == "manager")
            {
                var currentUser = _context.Users.Find(userId);
                var employees = _context.Users
                    .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Role == "employee")
                    .ToList();
                return View(employees);
            }
            return Forbid();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role).Value;

            if (currentUserRole == "employee" && currentUserId != id)
                return Forbid();

            var user = _context.Users.Include(u => u.Department).FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create([Bind("FirstName, LastName, Email, JobTitle, Salary, Role, DepartmentId")] UserDTO userDTO, string password)
        {
            User user = UserMapper(userDTO);

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "admin, manager")]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            ViewBag.Departments = _context.Departments.ToList();
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "admin, manager")]
        public IActionResult Edit([Bind("Id", "FirstName, LastName, Email, JobTitle, Salary, Role, DepartmentId")] UserDTO userDTO, string password)
        {
            var userFound = _context.Users.Find(userDTO.Id);
            if (userFound == null)
                return NotFound();

            User user = UserMapper(userDTO, userFound);

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(user);
            }

            userFound.FirstName = user.FirstName;
            userFound.LastName = user.LastName;
            userFound.Email = user.Email;
            userFound.JobTitle = user.JobTitle;
            userFound.Salary = user.Salary;
            userFound.Role = user.Role;
            userFound.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            userFound.DepartmentId = user.DepartmentId;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public User UserMapper (UserDTO userDTO, User initialUser = null)
        {
            var user = initialUser != null ? initialUser : new User();
            user.Id = userDTO.Id;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Email = userDTO.Email;
            user.JobTitle = userDTO.JobTitle;
            user.Salary = userDTO.Salary;
            user.Role = userDTO.Role;
            user.DepartmentId = userDTO.DepartmentId;

            return user;
        }
    }
}
