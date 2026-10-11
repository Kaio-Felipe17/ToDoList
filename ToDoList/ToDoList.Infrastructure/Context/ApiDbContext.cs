using Microsoft.EntityFrameworkCore;
using ToDoList.Application.Entities;

namespace ToDoList.Infrastructure.Context;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    public DbSet<Item> Items { get; set; }
}
