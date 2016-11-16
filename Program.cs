using System;
using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Extenstions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddDbContext<ContactDataContext>(options =>
               options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True"));                      
        }
    }
    public class Program
    {

        public static void Main(string[] args)
        {            
            Startup s = new Startup(new HostingEnvironment() { ContentRootPath = AppContext.BaseDirectory }); // is this line neccessary ?
            DbContextOptionsBuilder<EmployeeContext> optionsBuilder = new DbContextOptionsBuilder<EmployeeContext>();
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FuckYou;Trusted_Connection=True;MultipleActiveResultSets=True");
            
            

            using (var db = new EmployeeContext(optionsBuilder.Options))
            {
                
                var cdb = new ContactDataContext();

                LanguageRepository<ContactDataContext> lr = new LanguageRepository<ContactDataContext>(cdb);

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
                

                Employee EE = new Employee { Name = "Siim Aus", EmployeeId="0203" };
                ContactData cd = new ContactData();
                //EE.ContactData = cd;                                
                cd.Education.Add(edu);
                cd.FirstName = "Siim";
                cd.LastName = "Aus";
                cd.JobTitle = "IT Director";
                cd.ContactLanguage = ll; 
                cd.EmployeeId = EE.EmployeeId;
                cdrr.Add(cd); // should be add or update;
                // edu.ContactData = cd;
               // edu.ContactDataId = cd.Id;
                db.Employees.Add( EE);
                db.SaveChanges();
                cdb.SaveChanges();
                

                EmployeeRepository<EmployeeContext> er = new EmployeeRepository<EmployeeContext>(db);
                
                Employee ex2 = er.GetAll().Where( x => x.Id == 2).SingleOrDefault();

                //EntityBaseRepository<ContactData, EmployeeContext> cdr = new EntityBaseRepository<ContactData, EmployeeContext>(db);

                // should i get contact data ?
                //Console.WriteLine("Foreign Key is: {0}", db.Entry(ex2).Property("ContactDataId").CurrentValue);
                //ContactData ncd = cdb.ContactData.Find(db.Entry(ex2).Property("ContactDataId").CurrentValue);
                //ex2.ContactData = ncd;
                //ex2.ContactData.Serialized = "{\"FirstName\": \"Jaan\", \"ContactLanguage\": { \"Id\":23,\"Code\": \"FI\",\"Name\": \"Svenska\"}}";
                //ex2.ContactData.ContactLanguage = ll;
                db.SaveChanges();

                

                foreach(Employee ex in er.GetAll().Where( x => x.Id >= 12)) {
                    

                    
                    //Include(c => c.ContactData).ThenInclude(e => )
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(ex, Formatting.Indented, 
                        new JsonSerializerSettings {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                //, 
                                //PreserveReferencesHandling = PreserveReferencesHandling.Objects 
                        });                    
                    Console.WriteLine("{0} {1} {2}\n{3}", ex.Id, ex.Name, ex.EmployeeId, json);
                    er.Delete(ex);  // does not delete contactdata.
                }       
                db.SaveChanges();

                // how will it work when there is no connected data loaded.
                Employee ex3 = db.Employees.Find(2);
                if (ex3 != null)
                    Console.WriteLine("{0} {1} {2}\n{3}", ex3.Id, ex3.Name, ex3.EmployeeId, ex3.Serialize(null));

            }

           // testSerialize();
         
            

            Console.WriteLine("Hello World!");
        }
    }
}
