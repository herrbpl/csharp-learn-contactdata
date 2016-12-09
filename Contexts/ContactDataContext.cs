using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Extenstions;
using Microsoft.EntityFrameworkCore;
using System;
namespace ASTV.Services {

    public class ContactDataContext: BaseContext {
        
        public DbSet<ContactData> ContactData {get; set;}    
        public DbSet<Language> Language {get; set;}
        public DbSet<EducationLevel> EducationLevel {get; set;}
        public ContactDataContext(DbContextOptions<ContactDataContext> options)
            : base(options)
        {

        }

        public ContactDataContext(): base() {

        }
        /*
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {            
           // builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;");
        }
        */
        protected override void OnModelCreating(ModelBuilder builder)
        {            
            builder.Entity<ContactData>().HasKey(p => p.EmployeeId);
            builder.Entity<ContactData>().Property(p => p.Serialized).HasColumnName("Data");
            builder.Entity<ContactData>().Ignore(p => p.ContactLanguage); 
            builder.Entity<ContactData>().Ignore(p => p.Education);         
            base.OnModelCreating(builder);

        }
            
    }
}    