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
            var vi = this.Set<TEntity>().Versions(entity).Select( m => 
                    new VersionInfo { 
                            IsCurrent = EF.Property<Boolean>(m, "IsCurrent"), 
                            ChangeId = EF.Property<int>(m, "ChangeId"),
                            Version = EF.Property<int>(m, "Version")
                            
                            } ).FirstOrDefault();
            Expression<Func<TEntity, bool>> p = this.Set<TEntity>().BuildVersionQueryPredicate<TEntity>(entity);
            var v2 = this.Set<TEntity>().Versions(entity).Select( m => 
                    new VersionInfo { 
                            IsCurrent = EF.Property<Boolean>(m, "IsCurrent"), 
                            ChangeId = EF.Property<int>(m, "ChangeId"),
                            Version = EF.Property<int>(m, "Version")
                            
                            } ).FirstOrDefault();                            
            if (vi == null) {
                vi = new VersionInfo();
            }  
            return vi;
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {

            
            // get version info.
            VersionInfo x = GetVersionInfo<TEntity>(entity);
            // update proprty information. Why is this not working?

            
            Console.WriteLine("Version info before: \n{0}\n",  x.Serialize( new List<string>() { "aa" }));
            
            Console.WriteLine("x.Version {0} {1}", x.Version, x.Version+1);
            
            
            
            
            Entry(entity).Property<int>("ChangeId").CurrentValue = x.ChangeId+1;
            Entry(entity).Property<int>("Version").CurrentValue = x.Version+1;
            Entry(entity).Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
            var entry = base.Add(entity);   
            

                    

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