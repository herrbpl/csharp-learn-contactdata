
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using ASTV.Models.Generic;

namespace ASTV.Extenstions {
    public static class EntityVersioningExtensions {

        public static void AddVersioningAttributes(this DbContext context, ModelBuilder modelBuilder) {
            // Versioning information. Need to create index on previous key and version number?
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IEntityVersioning).IsAssignableFrom(e.ClrType)))
            {
                    Console.WriteLine("Updating type {0}", entityType.Name);
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
        }


        internal static readonly MethodInfo PropertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));
        
        public static void AddVersion<TEntity>(this DbSet<TEntity> set, params object[] keyValues) where TEntity : class
        {
            // get latest information from database
            MethodInfo PropertyMethodx
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));
            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var keys = entityType.GetKeys();
            var entries = context.ChangeTracker.Entries<TEntity>();
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            //EF.Property<Boolean>(parameter, "IsCurrent");
        
            /*
            Expression.Call(
                            EF.PropertyMethod.MakeGenericMethod(property.ClrType),
                            entityParameter,
                            Expression.Constant(property.Name, typeof(string))
            */
            // should extract keys from key which has "Version" defined and get its latest value
             var expression = Expression.Lambda(
                    //Expression.And(  
                        Expression.Equal(
                            Expression.Call(
                                PropertyMethod.MakeGenericMethod(typeof(Boolean) )
                                , parameter
                                , Expression.Constant("IsCurrent", typeof(string))
                            )
                            ,
                            Expression.Constant(true)),
                            /*,
                        Expression.Equal(
                            Expression.Property(parameter, "Version"),
                            Expression.Constant(1))
                            ),*/
                        parameter) as Expression<Func<TEntity, bool>>;
            Console.WriteLine("Linq Expression: {0}",  expression.ToString());
            var cdd = expression.Compile();
            //set.AsNoTracking().Where(expression).Max();
            IList<TEntity> x =set.Where(cdd).ToList();
             Console.WriteLine("CNN is : {0}", x.Count);
            foreach(var nn  in set.Where(expression).ToList()) {
                Console.WriteLine("NN is : {0}", nn.ToString());
            }
            //set.AsNoTracking().Where(p => p.Equals())
        }
    }
}