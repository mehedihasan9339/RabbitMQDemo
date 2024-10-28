using Microsoft.EntityFrameworkCore;

namespace PayrollManagement.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Payroll> Payrolls { get; set; }
    }
}
