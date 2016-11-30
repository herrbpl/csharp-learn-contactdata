using System;
using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Extenstions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Hosting.Internal;
using ASTV.Services;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleApplication
{

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder () 
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
          
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            string eDbConfig = Configuration.GetConnectionString("EmployeeDatabase") == null?
                                    Configuration.GetConnectionString("EmployeeDatabase"):
                                    @"Server=TISCALA.NTSERVER2.SISE;Database=scalaDB;Trusted_Connection=True;MultipleActiveResultSets=true";
            string cDbConfig = Configuration.GetConnectionString("ContactDataDatabase") == null?
                                    Configuration.GetConnectionString("ContactDataDatabase"):
                                    @"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True";

            // Add framework services.
            services.AddDbContext<EmployeeContext>(options =>  options.UseSqlServer(eDbConfig));
               
            services.AddDbContext<ContactDataContext>(options => options.UseSqlServer(cDbConfig));                      
        }
    }
    public class Program
    {

        public static void Main(string[] args)
        {            
        
            Startup s = new Startup(new HostingEnvironment() { ContentRootPath = AppContext.BaseDirectory, EnvironmentName = "Development" }); // is this line neccessary ?

            string eDbConfig = s.Configuration.GetConnectionString("EmployeeDatabase") != null?
                                    s.Configuration.GetConnectionString("EmployeeDatabase"):
                                    @"Server=TISCALA.NTSERVER2.SISE;Database=scalaDB;Trusted_Connection=True;MultipleActiveResultSets=true";
            string cDbConfig = s.Configuration.GetConnectionString("ContactDataDatabase") != null?
                                    s.Configuration.GetConnectionString("ContactDataDatabase"):
                                    @"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True";
            
            
            DbContextOptionsBuilder<EmployeeContext> optionsBuilder = new DbContextOptionsBuilder<EmployeeContext>();
           
            optionsBuilder.UseSqlServer(eDbConfig);
            
            DbContextOptionsBuilder<ContactDataContext> optionsBuilder2 = new DbContextOptionsBuilder<ContactDataContext>();            
            optionsBuilder2.UseSqlServer(cDbConfig);

            using (var db = new EmployeeContext(optionsBuilder.Options))
            {
                
                var cdb = new ContactDataContext(optionsBuilder2.Options);
                

                //LanguageRepository<ContactDataContext> lr = new LanguageRepository<ContactDataContext>(cdb);
                LanguageRepository lr = new LanguageRepository(cdb);

                // add language;
                if (!lr.GetAll().Any(l => l.Code=="EE")) {
                     lr.Add( new Language { Name="Eesti", Code="EE" });
                }


                Language ll = lr.GetAll().Where(l => l.Code=="EE").FirstOrDefault();
            

                EntityBaseRepository<EducationLevel, ContactDataContext> elr = new EntityBaseRepository<EducationLevel, ContactDataContext>(cdb);
                EntityBaseRepository<ContactData, ContactDataContext> cdrr = new EntityBaseRepository<ContactData, ContactDataContext>(cdb);
                 // add language;
                if (!elr.GetAll().Any(l => l.Code=="ah")) {
                     elr.Add(  new EducationLevel { Name="Algharidus", Code="ah"});
                }

                EducationLevel el = elr.GetAll().Where(l => l.Code=="ah").FirstOrDefault();

                Education edu = new Education();
                edu.SchoolName = "Uus kool";
                edu.YearCompleted = 2015;
                edu.NameOfDegree = "uu";
                edu.Level = el;
                
                ContactData cd = new ContactData();
                                
                cd.Education.Add(edu);
                cd.FirstName = "Siim";
                cd.LastName = "Aus";
                cd.JobTitle = "IT Director";
                cd.ContactLanguage = ll; 
                cd.EmployeeId = "0203";
                cdrr.Add(cd); // should be add or update;
                // edu.ContactData = cd;
               // edu.ContactDataId = cd.Id;                
                cdb.SaveChanges();
                

                EmployeeRepository er = new EmployeeRepository(db, null);                               
                

                foreach(Employee ex in er.GetAll().Where( x => x.Id >= 12)) {
                    

                    
                    //Include(c => c.ContactData).ThenInclude(e => )
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(ex, Formatting.Indented, 
                        new JsonSerializerSettings {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                //, 
                                //PreserveReferencesHandling = PreserveReferencesHandling.Objects 
                        });                    
                    Console.WriteLine("{0} {1} {2}\n{3}", ex.Id, ex.Name, ex.EmployeeId, json);
                    //er.Delete(ex);  // does not delete contactdata.
                }       
                db.SaveChanges();

                // how will it work when there is no connected data loaded.
                //Employee ex3 = db.Employees.Find(2);
                Employee ex3 = er.GetList(e => e.EmployeeId == "0203").SingleOrDefault();
                if (ex3 != null)
                    Console.WriteLine("{0} {1} {2}\n{3}", ex3.Id, ex3.Name, ex3.EmployeeId, ex3.Serialize(null));

            }

           // testSerialize();
         
            

            Console.WriteLine("Hello World!");
        }
    }
}
