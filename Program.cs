using System;
using ASTV.Models.Employee;
using ASTV.Models.Generic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Internal;
using ASTV.Services;

using Newtonsoft.Json;

namespace ConsoleApplication
{

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
          
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<EmployeeContext>(options =>
               options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True"));                      
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {            
            Startup s = new Startup(new HostingEnvironment() { ContentRootPath = AppContext.BaseDirectory });
            DbContextOptionsBuilder<EmployeeContext> optionsBuilder = new DbContextOptionsBuilder<EmployeeContext>();
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True");
            
            using (var db = new EmployeeContext(optionsBuilder.Options))
            {
                Language ll = new Language { Name="Eesti", Code="EST" };
                db.Language.Add(ll);
                db.SaveChanges();

                EducationLevel el = new EducationLevel { Name="Algharidus", Code="ah"};

                Education edu = new Education();
                edu.SchoolName = "Uus kool";
                edu.YearCompleted = 2015;
                edu.NameOfDegree = "uu";
                edu.Level = el;
                

                Employee EE = new Employee { Name = "Siim Aus", EmployeeId="0203" };
                ContactData cd = new ContactData();
                EE.ContactData = cd;                
                cd.Employee = EE; // should this come automatically ?
                cd.Education.Add(edu);
                cd.FirstName = "Siim";
                cd.LastName = "Aus";
                cd.JobTitle = "IT Director";
                cd.ContactLanguage = ll; 
                edu.ContactData = cd;
                edu.ContactDataId = cd.Id;
                //db.Employees.Add( EE);
                //db.SaveChanges();

                EmployeeRepository<EmployeeContext> er = new EmployeeRepository<EmployeeContext>(db);
                foreach(Employee ex in er.GetAll()) {
                    

                    
                    //Include(c => c.ContactData).ThenInclude(e => )
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(ex, Formatting.Indented, 
                        new JsonSerializerSettings {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects 
                        });                    
                    Console.WriteLine("{0} {1} {2}\n{3}", ex.Id, ex.Name, ex.EmployeeId, json);
                }
            }

            


            Console.WriteLine("Hello World!");
        }
    }
}
