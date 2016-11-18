using System;
using System.Linq;
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
            Console.WriteLine("Entering Base OnModelCreating");
            // Add audit information.
            /*
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IEntityAuditable).IsAssignableFrom(e.ClrType)))
            {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("UpdatedAt");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("CreatedBy");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("UpdatedBy");
            }
            */
            
            this.AddVersioningAttributes(modelBuilder);
            Console.WriteLine("Chaining model creating");
            base.OnModelCreating(modelBuilder);
        }

        protected void RecordsVersioning() {
            

            //Console.WriteLine("Entity types defined: {0}",this.Model.GetEntityTypes().ToString());
            // update speed depends on how many rows are to changed.
            foreach (var entityType in this.Model.GetEntityTypes()) {
                
                Console.WriteLine("Entity type defined: {0}", entityType.Name);
                if ( typeof(IEntityVersioning).IsAssignableFrom(entityType.ClrType)) {
                    Console.WriteLine("IEntityVersioning enabled");
                    // should get latest versions for those objects
                    
                    
                }
                
            }
            foreach (EntityEntry<IEntityVersioning> entry in ChangeTracker.Entries<IEntityVersioning>())
            {
                // add - if exists previous, 
                // delete
                // should i load data from db?               
                Console.WriteLine("{0} {1} {2} {3}", entry.GetType(),  entry.State, entry.IsKeySet, entry.Metadata.Name);
                // this goes so damn crazy, cannot do it with EF :(
                // Get latest changeId 
                
                
                
                var type = entry.Metadata.ClrType;
                //this.Set(type). 
                 
                //.MakeGenericType(entry.Metadata.ClrType);
                //MethodInfo SetMethod = typeof(DbSet<>).GetTypeInfo().GetDeclaredMethod("AsQueryable");
                //SetMethod.MakeGenericMethod(type).Invoke(entry.Context.GetSet(type),null);
                var aSet = Activator.CreateInstance(type);
                foreach(var prop in this.GetType().GetTypeInfo().GetProperties()) {
                    Console.WriteLine("Found proprty: {0} {1}", prop.Name, prop.PropertyType.Name);
                }
                //typeof(aSet).MakeGenericType(type);
                
                
                
                
                
                

                // if previous record does not exist, this is the firsy
                // if previous record exist, 
                // check if current record is latest. If yes, create 
                // If the entity was added.
                /*
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedBy").CurrentValue = userName;
                    entry.Property("CreatedAt").CurrentValue = now;
                }
                else if (entry.State == EntityState.Modified) // If the entity was updated
                {
                    entry.Property("UpdatedBy").CurrentValue = userName;
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
                */
            }
        }
        public override int SaveChanges()
        {
            //this.RecordsVersioning();
 
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