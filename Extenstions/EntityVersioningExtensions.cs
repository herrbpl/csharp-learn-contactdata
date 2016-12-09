
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;

using ASTV.Models.Generic;
using ASTV.Helpers;

namespace ASTV.Extenstions {
    
    public static class EntityVersioningExtensions {           

        // TODO: Add versioned entries to separate tables?    
        
        public static void AddVersioningAttributes(this DbContext context, ModelBuilder modelBuilder) {
            // Versioning information. Need to create index on previous key and version number?
            // TODO: save original key to context cache?
            // TODO: Add support for other defined unique keys 
            

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
                        .Property<int>("ChangeId");//.Has ValueGeneratedOnAdd().UseSqlServerIdentityColumn().HasDefaultValueSql("IDENTITY");
                    
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
                                        
            }
        }

        /// <summary>
        /// Add versioning data to EntityEntry&lt;TEntity&gt;
        /// <br/>TODO: Should also add opportunity to differ if change is between saves and if addition changes anything
        /// TODO: Add current/max version info to change tracker itself and keep it updated to gain performance 
        /// </summary>
        public static EntityEntry<TEntity> AddEntityVersion<TEntity>(this DbContext context, TEntity entity) where TEntity : class {

            var current = context.Entry(entity);
            if (!typeof(IEntityVersioning).IsAssignableFrom(entity.GetType())) return current;

            // find previous version of this entity
            var previous = context.ChangeTracker.Entries<TEntity>().AsQueryable().Latest(entity);

            Console.WriteLine("Entity state: {0}", current.State.ToString());
            
            int version = 0;
            
            // if not attached, add
            // if updated, copy entity and add to latest
            // if deleted, copy entity and add to latest, set deleted
            // if added, check if same entity already exist, add copy if not latest.

            // entity is not yet tracked
            if (current.State == EntityState.Detached) {
                
                
                // no previous entry found
                if (previous == null) {
                    current.Property<int>("Version").CurrentValue = 1;
                    current.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                    current.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                    current.Property<Boolean>("IsCurrent").CurrentValue = true;
                } 
                // previous entry found
                else {

                    // get latest version number
                    version =  previous.Property<int>("Version").CurrentValue;                
                    previous.Property<DateTime>("ValidUntil").CurrentValue = DateTime.Now;
                    previous.Property<Boolean>("IsCurrent").CurrentValue = false;

                    //                    
                    if (previous.State == EntityState.Unchanged) {
                        previous.State = EntityState.Modified;
                    }

                    // update version number
                    version++;

                    // update current item (not yet tracked)
                    current.Property<int>("Version").CurrentValue = version;
                    current.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                    current.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                    current.Property<Boolean>("IsCurrent").CurrentValue = true;                                                      
                }
            } else if (current.State == EntityState.Added || current.State == EntityState.Modified) {
                // entity has already been added.
                // no previous entry found 
                if (previous == null ) {  
                    // Guess its latest. Do not change version. Update other meta
                    current.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                    current.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                    current.Property<Boolean>("IsCurrent").CurrentValue = true;    
                } 
                // previous entry found
                else {
                    // if previous does not equals to current, add extra copy
                    if (!previous.Entity.Equals(entity)) {
                        

                        version =  previous.Property<int>("Version").CurrentValue;  

                        // This is probably quite inefficient
                        var serialized = entity.Serialize(null);
                        var newCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<TEntity>(serialized);
                        
                        

                        var newversion = context.Entry(newCopy);

                        previous.Property<DateTime>("ValidUntil").CurrentValue = DateTime.Now;
                        previous.Property<Boolean>("IsCurrent").CurrentValue = false;
                        
                        if (previous.State == EntityState.Unchanged) {
                            Console.WriteLine("Here");
                            previous.State = EntityState.Modified;
                        }
                        

                        version++;

                        newversion.Property<int>("Version").CurrentValue = version;
                        newversion.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                        newversion.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                        newversion.Property<Boolean>("IsCurrent").CurrentValue = true;
                        newversion.State = EntityState.Added;

                        // change current object change to not tracked
                        //current.State = EntityState.Detached;

                    } else {
                        Console.WriteLine("Previous is  equal to current, just setting iscurrent");
                        current.Property<Boolean>("IsCurrent").CurrentValue = true;
                    }
                }
            } else if (current.State == EntityState.Deleted ) {
                // entity has already been added.
                // no previous entry found 
                if (previous == null ) {  
                    // Guess its latest. Do not change version. Update other meta
                    current.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                    current.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                    current.Property<Boolean>("IsDeleted").CurrentValue = true;
                    current.Property<Boolean>("IsCurrent").CurrentValue = true;    
                } 
                // previous entry found
                else {
                    // if previous does not equals to current, add extra copy
                    if (!previous.Entity.Equals(entity)) {
                        

                        version =  previous.Property<int>("Version").CurrentValue;  

                        // This is probably quite inefficient
                        var serialized = entity.Serialize(null);
                        var newCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<TEntity>(serialized);
                        
                        

                        var newversion = context.Entry(newCopy);

                        previous.Property<DateTime>("ValidUntil").CurrentValue = DateTime.Now;
                        previous.Property<Boolean>("IsCurrent").CurrentValue = false;
                        previous.State = EntityState.Modified;

                        version++;

                        newversion.Property<int>("Version").CurrentValue = version;
                        newversion.Property<DateTime>("ValidFrom").CurrentValue = DateTime.Now;
                        newversion.Property<DateTime>("ValidUntil").CurrentValue = DateTime.MaxValue;
                        newversion.Property<Boolean>("IsCurrent").CurrentValue = true;
                        newversion.Property<Boolean>("IsDeleted").CurrentValue = true;
                        newversion.State = EntityState.Added;

                        // change current object change to not tracked
                        current.State = EntityState.Unchanged;

                    } else {
                        Console.WriteLine("Previous is  equal to current, just setting iscurrent");
                        current.Property<Boolean>("IsCurrent").CurrentValue = true;
                        current.Property<Boolean>("IsDeleted").CurrentValue = true;
                        current.State = EntityState.Modified;
                    }
                }

            }

            
            return current;            
        }

        // =============================================================================================


        // https://github.com/aspnet/EntityFramework/blob/3a10927c849002777fc656fba063ffde3f8d3938/src/Microsoft.EntityFrameworkCore/EF.cs 
        internal static readonly MethodInfo PropertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));
        
        

        /// <summary>
        /// Returns keys that are related to Versioning
        /// </summary>
        public static IKey GetVersionKeys<TEntity>(this DbSet<TEntity> set) where TEntity : class
        {
            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));                        
            var keys = entityType.GetKeys();
            foreach(var key in keys) {
                if (key.Properties.Where(m => m.Name == "Version").Count() > 0) {                    
                    return key;
                }                
            }
            return null;
        }


        /// <summary>
        /// Returns maximum version number for given entity
        /// </summary>
        public static int MaxVersion<TEntity>(this DbSet<TEntity> set, TEntity entity) where TEntity : class {
            try {

                return set.Versions(entity).Select( 
                            m => EF.Property<int>(m, "Version") ).Max();
            } catch (Exception e) {
                return 0;
            }

        }

        /// <summary>
        /// Returns maximum version number for given entity
        /// </summary>
        public static int MaxVersion<TEntity>(this IQueryable<EntityEntry<TEntity>> source, TEntity entity) where TEntity : class {
            try {
                var l = source.Versions(entity).ToList();               
                return source.Versions(entity).Select( 
                            m => m.Property<int>("Version").CurrentValue ).Max();
                
            } catch (Exception e) {
                return 0;
            }

        }

        /// <summary>
        /// Returns specific version of entity. If version is not found, null is returned
        /// </summary>
        public static TEntity GetVersion<TEntity>(this DbSet<TEntity> set, TEntity entity, int version) where TEntity : class {

            if (entity == null ) throw new ArgumentNullException();               
            var x = set.Versions(entity).
                Where( m => EF.Property<int>(m, "Version") == version).FirstOrDefault();                              
            return x;
        }

        /// <summary>
        /// Returns specific version of entity. If version is not found, null is returned
        /// </summary>
        public static EntityEntry<TEntity> GetVersion<TEntity>(this IQueryable<EntityEntry<TEntity>> source, TEntity entity, int version) where TEntity : class {

            if (entity == null ) throw new ArgumentNullException();               
            var x = source.Versions(entity).
                Where( m => m.Property<int>("Version").CurrentValue == version).FirstOrDefault();                              
            return x;
        }

        /// <summary>
        /// Returns latest version of given entity
        /// </summary>
        public static TEntity Latest<TEntity>(this IQueryable<TEntity> source, TEntity entity) where TEntity : class {
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;    
                int mv = set.MaxVersion(entity);
                if (mv == 0 ) return null;
                return set.GetVersion(entity, mv);
            }
            return null;
        }

        /// <summary>
        /// /// Returns latest version of given entity
        /// </summary>
        public static EntityEntry<TEntity> Latest<TEntity>(this IQueryable<EntityEntry<TEntity>> source, TEntity entity) where TEntity : class {                                        
            int mv = source.MaxVersion(entity);
            if (mv == 0 ) return null;
            return source.GetVersion(entity, mv);            
        }
        
        /// <summary>
        /// Returns all versions of given entity
        /// </summary>

        public static IQueryable<TEntity> Versions<TEntity>(this IQueryable<TEntity> source,  TEntity entity) where TEntity : class
        {            
            if (entity == null ) throw new ArgumentNullException();
            
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {

                var set = (DbSet<TEntity>)source;                
                var exp = BuildVersionQueryPredicate<TEntity>(set, entity);            
                var context =  set.GetService<IDbContextServices>().CurrentContext.Context;              
                return set.Where(exp);
            } 
            return source;
        }

        public static IQueryable<EntityEntry<TEntity>> Versions<TEntity>(this IQueryable<EntityEntry<TEntity>> source, TEntity entity) where TEntity : class
        {
            if (entity == null ) throw new ArgumentNullException();
            if (typeof(EntityEntry<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (EntityEntry<TEntity>)source;                
                var pred = set.EntityVersions();              
                return source.Where(t => pred(t, entity));
            } 
            return source;
        }


        // should check this out
        // https://github.com/aspnet/EntityFramework/blob/f9adcb64fdf668163377beb14251e67d17f60fa0/src/Microsoft.EntityFrameworkCore/Internal/EntityFinder.cs
       
        public static MethodCallExpression BuildCallExpression<TEntity>( ParameterExpression parameter, string propertyName, Type propertyType) {
            return Expression.Call(
                                PropertyMethod.MakeGenericMethod(propertyType)
                                , parameter
                                , Expression.Constant(propertyName, typeof(string))
                            );
        }
        
        /// <summary>
        /// Returns predicate expression to find all versions for given entity.
        /// </summary>
        public static Expression<Func<TEntity, bool>> BuildVersionQueryPredicate<TEntity>(this DbSet<TEntity> set, TEntity entity) where TEntity : class
        {   
            if (entity == null ) throw new ArgumentNullException();
            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");

            var vvk = vk.Properties.Where(p => p.Name != "Version").ToList();

            BinaryExpression exbody = null;
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            foreach(var property in vk.Properties) {
                
                if (property.Name != "Version") {
                    // get value of entity of given name.
                    var oo = entity.GetType().GetTypeInfo().
                        GetProperties().Where(p => p.Name == property.Name).
                        Select(p => p.GetValue(entity,null)).FirstOrDefault();

                    // value I'm comparing to, aka value of key
                    var value = Expression.Constant(oo, oo.GetType() );
                    var subExpression = Expression.Equal(
                        BuildCallExpression<TEntity>(parameter, property.Name, oo.GetType())
                        , value
                    );

                    if (exbody == null) {
                        exbody = subExpression;
                    } else {
                        exbody = Expression.And(
                            exbody, subExpression
                        );
                    }                        
                }                    
            }

            var exp = Expression.Lambda(exbody, parameter) as Expression<Func<TEntity, bool>>;

            return exp;
        }


        
        // ================================================================================

        // All this stuff is so inefficient. Linq is death to performance! 

        /// <summary>
        /// Stores cache of compiled functions
        /// </summary>
        /// <returns></returns>
        internal static IDictionary<Type, object> _ChangeTrackerFinderCache = new Dictionary<Type, object>();


        public static Func< EntityEntry<TEntity>, TEntity, bool> EntityVersions<TEntity>(this EntityEntry<TEntity> entry) 
            where TEntity : class 
        {
            Func< EntityEntry<TEntity>, TEntity, bool> f;
            if (_ChangeTrackerFinderCache.ContainsKey(typeof(TEntity))) {
                f = (Func< EntityEntry<TEntity>, TEntity, bool>)_ChangeTrackerFinderCache[typeof(TEntity)];                
            } else {
                f = entry.Context.VersionKeyFunction<TEntity>()().Compile();                
                _ChangeTrackerFinderCache.Add(typeof(TEntity),f);                
            }
            
            return f;
        }

        /// <summary>
        /// Returns function(TEntity) which returns expression to search version keys
        /// </summary>
        public static Func< Expression< Func< EntityEntry<TEntity>, TEntity, bool>>> VersionKeyFunction<TEntity>(this DbContext context) where TEntity : class {
            // return parameter 
            var entityEntryParameter = Expression.Parameter(typeof(EntityEntry<TEntity>), "x");
            // entity searched for
            var entityParameter = Expression.Parameter(typeof(TEntity), "y"); 
            // return type
            var returnType = typeof(Expression< Func< EntityEntry<TEntity>, TEntity, bool>>);

            Type propertyParameterType = typeof(PropertyEntry<,>);

            // Signature to find correct property method 
            var methodSignature = new Type[] { typeof(string) };

            // Method info which helps to invoke 
            MethodInfo propertyMethodInfo= typeof(EntityEntry<>)
                        .MakeGenericType(entityEntryParameter.Type.GenericTypeArguments[0])  // resolves to EntityEntry<TEntity>
                        .GetMethod("Property", methodSignature);  // resolves to EntityEntry<TEntity>.Property<Propertytype>(string propertyName);            

            // TEntity information, keys.
            var set = context.Set<TEntity>();
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");
            var properties  = vk.Properties.Where(p => p.Name != "Version").ToList();

            BinaryExpression predicate = null;

            foreach( var prop in properties ) {
                
                // find property accessor
                // Tentity.property. There *should* not be any properties with multiple types..
                var pi = typeof(TEntity).GetProperty(prop.Name);                
                var me = Expression.Property( entityParameter, pi.GetGetMethod());

                // create type of PropertyType<TEntity, property.ClrType>
                var propertyEntryType = propertyParameterType.MakeGenericType(
                    new Type[] {
                        entityEntryParameter.Type.GenericTypeArguments[0]  
                        , pi.PropertyType
                    }
                );

                var currentValuePropertyInfo = propertyEntryType.GetProperty("CurrentValue", pi.PropertyType);


                var equalsExpression =
                    Expression.Equal( // start of one expression
                        Expression.Property( // get property of PropertyEntity

                            Expression.Call( // Calling for x.Property<propertype>(propertyname)                             
                                entityEntryParameter, // typeof(EntityEntry<TEntity>)
                                propertyMethodInfo.MakeGenericMethod(prop.ClrType),                            
                                Expression.Constant(prop.Name, typeof(string))
                                )                                                                                               
                            , currentValuePropertyInfo  // Calling for x.Property<propertype>(propertyname).CurrentValue                     
                        )                                                    
                        ,    
                        
                        me
                    ); // end of one expression
                if (predicate == null)  {
                    predicate = equalsExpression;
                } else {
                    predicate = Expression.AndAlso(predicate, equalsExpression);
                }      
            }
            
            IList<ParameterExpression> parameters = new List<ParameterExpression>();
            parameters.Add(entityEntryParameter);
            parameters.Add(entityParameter );            

            Expression< Func< EntityEntry<TEntity>, TEntity, bool>> exp = ( Expression< Func< EntityEntry<TEntity>, TEntity, bool>>) Expression.Lambda(predicate,parameters);
            
            var xc = Expression.Constant(exp, typeof(Expression< Func< EntityEntry<TEntity>, TEntity, bool>>));
            var ll =  Expression.Lambda(xc,null);
            
            return (Func< Expression< Func< EntityEntry<TEntity>, TEntity, bool>>>) ll.Compile();
        }


        
        // ================================================================================

        public static string KeyValueString<TEntity>(this EntityEntry<TEntity> entry) where TEntity: class {
            var key = entry.Metadata.GetKeys().Where(k => k.Properties.Any(p => p.Name ==  "Version")).FirstOrDefault();
            
            string s = "";
            if (key != null) {
                foreach(var prop in key.Properties) {
                    if (prop.Name != "Version") {
                        s += string.Format("{0}='{1}'", prop.Name, entry.Property(prop.Name).CurrentValue);
                    }
                }

            }
            return s;
        }


        public static string ToVersionString<TEntity>(this EntityEntry<TEntity> entry) where TEntity: class {
             int version = entry.Property<int>("Version").CurrentValue;
             bool iscurrent = entry.Property<bool>("IsCurrent").CurrentValue;
             //string s = entry.Property<string>("EmployeeId").CurrentValue;
             DateTime d = entry.Property<DateTime>("ValidFrom").CurrentValue;
             DateTime d2 = entry.Property<DateTime>("ValidUntil").CurrentValue;
                
             return string.Format("{0} {1} {2} {3} {4} {5} {6}", entry.Property<int>("ChangeId").CurrentValue, iscurrent, version,
                    d, d2, entry.State.ToString(), entry.KeyValueString());
        } 

        public static void printChangeTracker<TEntity>(this DbContext context, string tracing) where TEntity : class {
            Console.WriteLine("[{0}] ChangeTracker has {1} entries", tracing, context.ChangeTracker.Entries<TEntity>().Count());
            foreach(var xx in context.ChangeTracker.Entries<TEntity>()) {
                //xx.Metadata.
                /*
                int version = xx.Property<int>("Version").CurrentValue;
                string s = xx.Property<string>("EmployeeId").CurrentValue;
                DateTime d = xx.Property<DateTime>("ValidFrom").CurrentValue;
                DateTime d2 = xx.Property<DateTime>("ValidUntil").CurrentValue;
                
                Console.WriteLine("{0} {1} {2} {3} {4}", xx.Property<int>("ChangeId").CurrentValue, s, version,
                    d, d2);
                */
                Console.WriteLine("{0}", xx.ToVersionString());
                                
            }
        }         
         public static void printSet<TEntity>(this DbContext context, string tracing) where TEntity : class {
            Console.WriteLine("[{0}] Set<> has {1} entries", tracing, context.Set<TEntity>().Count());             
            foreach(var xxy in context.Set<TEntity>()) {
                //xx.Metadata.
                 var xx = context.Entry(xxy);
                int version = xx.Property<int>("Version").CurrentValue;
                string s = xx.Property<string>("EmployeeId").CurrentValue;
                DateTime d = xx.Property<DateTime>("ValidFrom").CurrentValue;
                DateTime d2 = xx.Property<DateTime>("ValidUntil").CurrentValue;

                Console.WriteLine("{0} {1} {2} {3} {4}", xx.Property<int>("ChangeId").CurrentValue, s, version,
                    d, d2);
            }
        }              

    }
}