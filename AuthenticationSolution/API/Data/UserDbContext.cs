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
    public DbSet<ClientSecret> ClientSecrets { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Project> Projects { get; set; }
}
