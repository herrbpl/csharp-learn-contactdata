
using ASTV.Models.Employee;
using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;


namespace ASTV.Services {
    public class EmployeeContext: DbContext {
        public DbSet<Employee> Employees {get; set;}
        public DbSet<ContactData> ContactData {get; set;}
        //public DbSet<Education> Education {get; set;}
        public DbSet<Language> Language {get; set;}
        //public DbSet<EducationLevel> EducationLevel {get; set;}
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
            builder.Entity<ContactData>().Property(p => p.Serialized).HasColumnName("Data");
            
        }
    }
}