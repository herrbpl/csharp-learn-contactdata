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

        
        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {           
            var entry = this.AddEntityVersion(entity);
           // entry = base.Add(entity);                                                                
            return entry;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {                                  
            var entry = this.UpdateEntityVersion(entity);                                                                         
            return entry;
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {   
           
            var entry = this.RemoveEntityVersion(entity);                                                                         
            return entry;
        }
        

        public override int SaveChanges()
        {            
 
            return base.SaveChanges();
        }            

    }


}