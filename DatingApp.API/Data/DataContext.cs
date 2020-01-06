using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        // Pluralize the entity name, it will be the table created in the database.
        public DbSet<Value> Values { get; set; }
    }
}