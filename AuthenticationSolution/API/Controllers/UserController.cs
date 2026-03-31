using API.Data;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class UserController : Controller
{
    private readonly UserDbContext _dbContext;

    public UserController(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _dbContext.Users
            .AsNoTracking()   
            .Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .ToListAsync();

        return View(users);
    }
}
