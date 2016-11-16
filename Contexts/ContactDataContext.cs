using ASTV.Models.Employee;
using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;

namespace ASTV.Services {

    public class ContactDataContext: DbContext {
        
        public DbSet<ContactData> ContactData {get; set;}
        //public DbSet<Education> Education {get; set;}
        public DbSet<Language> Language {get; set;}
        public DbSet<EducationLevel> EducationLevel {get; set;}
        public ContactDataContext(DbContextOptions<ContactDataContext> options)
            : base(options)
        {

        }

        public ContactDataContext(): base() {

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
            builder.Entity<ContactData>().Ignore(p => p.ContactLanguage); 
            builder.Entity<ContactData>().Ignore(p => p.Education);
            
        }
    }
}    