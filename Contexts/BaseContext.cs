using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ASTV.Models.Generic;

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

            // Versioning information. Need to create index on previous key and version number?
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IEntityVersioning).IsAssignableFrom(e.ClrType)))
            {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("ValidFrom");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("ValidUntil");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<Boolean>("IsCurrent");
 
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<Boolean>("IsDeleted");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<int>("Version");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<int>("ChangeId");
                    
                    // need to create alternate key
                    var akey = modelBuilder.Entity(entityType.ClrType).Metadata.FindPrimaryKey();
                    if (akey != null) {
                        
                        // remove old primary key

                        var oldkey = modelBuilder.Entity(entityType.ClrType).Metadata.RemoveKey(akey.Properties);
                        
                        
                        IList<string> pnames = new List<string>();
                        
                        foreach(var p in oldkey.Properties ) {
                            pnames.Add(p.Name);
                        }

                        pnames.Add("Version");
                        modelBuilder.Entity(entityType.ClrType).HasKey("ChangeId");
                        modelBuilder.Entity(entityType.ClrType).HasAlternateKey(pnames.ToArray()); 
                        
                    } else {
                        modelBuilder.Entity(entityType.ClrType).HasKey("ChangeId");
                    }
                     
                    foreach(var key in modelBuilder.Entity(entityType.ClrType).Metadata.GetKeys()) {
                        string s ="";
                        foreach(var p in key.Properties ) {
                            s += " "+p.Name;
                        }
                        Console.WriteLine("Key is {0}", s);
                        
                    }
            }
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

                // Get latest changeId 
                
                var en =  this.Model.FindEntityType(entry.Metadata.Name);
                // how the fuck i query for values when I do not have Type??
                entry.GetType().GetTypeInfo().GetDeclaredMethod("set");
                 
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
            this.RecordsVersioning();
 
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