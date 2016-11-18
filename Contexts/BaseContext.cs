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


        


        public virtual VersionInfo GetVersionInfo<TEntity>(TEntity entity) where TEntity : class {
            this.Set<TEntity>().Latest(entity);
            return null;
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