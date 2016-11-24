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
using Microsoft.EntityFrameworkCore.Storage;
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
           
            if (typeof(IEntityVersioning).IsAssignableFrom(entity.GetType())) {

            
            
                this.GetChangeTrackerPredicate(entity);
            
            
                VersionInfo x = GetVersionInfo<TEntity>(entity);
                var entityType = Model.FindEntityType(typeof(TEntity));
                
                var previous = Set<TEntity>().Latest(entity);
                // If entity with same key is given and is 
                int version = 0;
                
                if (previous != null) {
                    version =  Entry(previous).Property<int>("Version").CurrentValue;
                //  Console.WriteLine("Previous version: {0}", previous.Serialize(null));
                    Entry(previous).Property<DateTime>("ValidUntil").CurrentValue = DateTime.Now;
                    Entry(previous).Property<Boolean>("IsCurrent").CurrentValue = false;
                    Entry(previous).State = EntityState.Modified;
                } 
                
                version++;
                // need to retrieve previous latest entry 
                
                //Console.WriteLine("Version info before: \n{0}\n",  x.Serialize( new List<string>() { "aa" }));
                
                //Console.WriteLine("x.Version {0} {1}", x.Version, x.Version+1);
                
                Entry(entity).Property<int>("Version").CurrentValue = version;
                Entry(entity).Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                Entry(entity).Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                Entry(entity).Property<Boolean>("IsCurrent").CurrentValue = true;
            }
            
            var entry = base.Add(entity);
            
            // Temporary fix, actually should get max version from change tracker.
            // To get that, need to build predicate for that. Its PITA     
            this.SaveChanges();                              
            Console.WriteLine("-----------------------------------------------------");
            return entry;
        }


        public override int SaveChanges()
        {            
 
            return base.SaveChanges();
        }            

    }


}