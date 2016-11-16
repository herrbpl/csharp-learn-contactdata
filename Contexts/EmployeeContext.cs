
using ASTV.Models.Employee;
using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;


namespace ASTV.Services {
    public class EmployeeContext: DbContext {
        public DbSet<Employee> Employees {get; set;}        
        public EmployeeContext(DbContextOptions<EmployeeContext> options)
            : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            //builder.UseSqlServer(@"Server=TISCALA.NTSERVER2.SISE;Database=scalaDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Employee>().Ignore(p => p.sAMAccountName);
            builder.Entity<Employee>().Ignore(p => p.SID);                        
        }
    }
}