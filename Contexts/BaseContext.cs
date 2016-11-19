using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using ASTV.Models.Generic;
using ASTV.Extenstions;

namespace ASTV.Services {
    public class BaseContext: DbContext {
        public BaseContext(DbContextOptions  options)
            : base(options)
        {
                        
        }

        public BaseContext(): base()
        {
                        
        }

        /// <summary>
        /// We overrride OnModelCreating to map the audit properties for every entity marked with the 
        /// IAuditable interface.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {                      
            this.AddVersioningAttributes(modelBuilder);           
            base.OnModelCreating(modelBuilder);
        }


        public virtual VersionInfo GetVersionInfo<TEntity>(TEntity entity)  where TEntity : class {
            VersionInfo vi;            
            var vil = this.Set<TEntity>().Versions(entity).Select( m => 
                    new VersionInfo { 
                            IsCurrent = EF.Property<Boolean>(m, "IsCurrent"), 
                            ChangeId = EF.Property<int>(m, "ChangeId"),
                            Version = EF.Property<int>(m, "Version")
                            
                            } ).ToList();
                                 
            if (vil.Count == 0) {
                vi = new VersionInfo();
            }  else {
                vi = vil.Aggregate(
                                (v1, v2) => v1.Version > v2.Version ? v1 : v2
                            );
            }
            return vi;
        }

        

        
        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
           // this.printChangeTracker<TEntity>();
           //  this.printSet<TEntity>();
           
            // get version info. Actually, saving back should occur as quickly as possible.
            // otherwise some other process might grab version no.
            // ideally this should be executed as transaction

            VersionInfo x = GetVersionInfo<TEntity>(entity);
            int mv = Set<TEntity>().MaxVersion(entity);
             Console.WriteLine("Previous version: {0} ", mv);
            TEntity previous;
            if (x.Version > 0) {
                previous = Set<TEntity>().GetVersion(entity, x.Version);
                if (previous != null) {
                    Console.WriteLine("Previous version: {0}", previous.Serialize(null));
                }
            } 
            
            // need to retrieve previous latest entry 
            
            Console.WriteLine("Version info before: \n{0}\n",  x.Serialize( new List<string>() { "aa" }));
            
            Console.WriteLine("x.Version {0} {1}", x.Version, x.Version+1);

            
            //Entry(entity).Property<int>("ChangeId").CurrentValue = x.ChangeId+1;
            Entry(entity).Property<int>("Version").CurrentValue = x.Version+1;
            Entry(entity).Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
            var entry = base.Add(entity);                                   
            Console.WriteLine("-----------------------------------------------------");
            return entry;
        }


        public override int SaveChanges()
        {            
 
            return base.SaveChanges();
        }            

    }

    public class XBaseContext: BaseContext {
        public XBaseContext(DbContextOptions<XBaseContext> options)
            : base(options)
        {

        }
    }    

}