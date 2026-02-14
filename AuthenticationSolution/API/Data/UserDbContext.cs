using API.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
}
