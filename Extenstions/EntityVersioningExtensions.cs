
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
    public class VersionInfo {
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public Boolean IsCurrent { get; set; }
        public Boolean IsDeleted { get; set; }
        public int Version { get; set; }

    }
    public static class EntityVersioningExtensions {

        public static readonly Dictionary<string, Type> VersionProperties = new Dictionary<string, Type>(){
            { "ValidFrom", typeof(System.DateTime)}
        };         

        public static void AddVersioningAttributes(this DbContext context, ModelBuilder modelBuilder) {
            // Versioning information. Need to create index on previous key and version number?
            // TODO: save original key to context cache?
             
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
        
        public static string ListObject(object O) {
            string info = "";
            Type t = O.GetType();
            info += "Type: "+t.FullName;
            info += "Members:\n";
            foreach( var s in t.GetProperties()) {
                info += s.Name + ":"; //+s.GetType().FullName;
                if (s.GetType().FullName == "System.Reflection.RuntimePropertyInfo") {
                    
                    info += ":"+s.PropertyType.FullName;
                    var oo = s.GetValue(O, null);
                    if (oo == null) {
                        info += "= (null)";
                    } else {
                        info += "= "+oo.ToString();
                    }
                    
                }
                info += "\n";
            }

            return info;
        }



        public static IKey VK<TEntity>(this DbSet<TEntity> set) where TEntity : class
        {
            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var pk = entityType.FindPrimaryKey();
            var keys = entityType.GetKeys();
            foreach(var key in keys) {
                if (key.Properties.Where(m => m.Name == "Version").Count() > 0) {
                    Console.WriteLine("AK FOUND: '{0}': {1}", key.SqlServer().Name, key.Properties.Select(m => m.Name).ToArray().Join(", "));
                    return key;
                }                
            }
            return null;
        }

        public static void BuildVersionExpression<TEntity>(this DbSet<TEntity> set, TEntity entity) where TEntity : class
        {   
            var vk = set.VK();
            if (vk == null) return;            
            // enumerate values of key properties            
            // I need to get latest version no from db by AK and increase it by 1. 
            // I also need to mark latest as not current, current as current and update dates


        }

        /// <summary>
        /// Returns latest versioninfo for entity 
        /// </summary>
        public static VersionInfo Latest<TEntity>(this IQueryable<TEntity> source,  TEntity entity) where TEntity : class
        {            
            if (entity == null ) return null;
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;  
                var ak = set.VK();
                if (ak == null) { return null; } // no version keys
                string kv = "";
                foreach(var property in ak.Properties) {
                    if (property.Name != "Version") {
                        // get value of entity of given name.
                        var oo = entity.GetType().GetTypeInfo().GetProperties().Where(p => p.Name == property.Name).Select(p => p.GetValue(entity,null)).FirstOrDefault();
                        kv  += property.Name+ "=";
                        //var oo = s.GetValue(O, null);
                        if (oo == null) {
                            kv += "(null)";
                        } else {
                            kv += oo.ToString();
                        }
                        kv += " ";
                    }                    
                }
                kv = "Search Key: '"+kv+"'";
                Console.WriteLine(kv);
            }
            return null;
        }

        public static IQueryable<TEntity> IsCurrent<TEntity>(this IQueryable<TEntity> source,  TEntity entity) where TEntity : class
        {
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;                                
                Console.WriteLine("Well, well, Yes yes, it is!");
                set.VK();
                var parameter = Expression.Parameter(typeof(TEntity), "x");
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
                return source.Where(expression);
            }        
            return source;
        }
        public static void PK<TEntity>(this DbSet<TEntity> set, params object[] keyValues) where TEntity : class
        {
            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var pk = entityType.FindPrimaryKey();
            //entityType.GetProperties()
            //var xxx = entityType.GetProperties().ToList().AsReadOnly();

            
            //var ak = entityType.FindKey(xxx);
            var keys = entityType.GetKeys();
            if (pk != null) {
                Console.WriteLine("PK: {0}", pk.Properties.Select(m => m.Name).ToArray().Join(", "));
            }

            var ak = entityType.FindKey(entityType.GetProperties().Where(m => m.Name == "Version" ).ToList().AsReadOnly());

            if (ak != null) {
                Console.WriteLine("AK: '{0}': {1}", ak.SqlServer().Name, ak.Properties.Select(m => m.Name).ToArray().Join(", "));
            }

            foreach(var key in keys) {
                Console.WriteLine("key: '{0}': {1}", key.SqlServer().Name, key.Properties.Select(m => m.Name).ToArray().Join(", "));
            }

        }

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
//            IList<TEntity> x =set.Where(cdd).ToList();
//             Console.WriteLine("CNN is : {0}", x.Count);
            foreach(var nn  in set.AsNoTracking().Where(expression).ToList()) {
                Console.WriteLine("NN is : {0}", nn.ToString());
                Console.WriteLine("NN info: \n{0}", ListObject(nn));
            }
            //set.AsNoTracking().Where(p => p.Equals())
        }



        
        public static void GetLatestVersion<TEntity>(this DbSet<TEntity> set, params object[] keyValues) where TEntity : class
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
//            IList<TEntity> x =set.Where(cdd).ToList();
//             Console.WriteLine("CNN is : {0}", x.Count);
            foreach(var nn  in set.Where(expression).ToList()) {
                Console.WriteLine("NN is : {0}", nn.ToString());
                
            }
            //set.AsNoTracking().Where(p => p.Equals())
        }

    }
}