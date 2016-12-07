
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
    public class VersionInfo {
        public int ChangeId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public Boolean IsCurrent { get; set; }
        public Boolean IsDeleted { get; set; }
        public int Version { get; set; }

    }
    public static class EntityVersioningExtensions {               

        public static void AddVersioningAttributes(this DbContext context, ModelBuilder modelBuilder) {
            // Versioning information. Need to create index on previous key and version number?
            // TODO: save original key to context cache?
            
            //modelBuilder.ForSqlServerHasSequence<int>("DBSequence")
            //                  .StartsAt(1000).IncrementsBy(2);

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
        /// Returns specific version of entity. If version is not found, null is returned
        /// </summary>
        public static TEntity GetVersion<TEntity>(this DbSet<TEntity> set, TEntity entity, int version) where TEntity : class {

            if (entity == null ) throw new ArgumentNullException();               
            var x = set.Versions(entity).
                Where( m => EF.Property<int>(m, "Version") == version).FirstOrDefault();                              
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

        
        public static void GetChangeTrackerPredicate<TEntity>(this DbContext context, TEntity entity) where TEntity : class 
        {
            var xx = context.VersionKeyFunction<TEntity>();
            var pred = xx().Compile();
            Console.WriteLine("XCNXX: {0}", context.ChangeTracker.Entries<TEntity>().Where(t => pred(t, entity)).Count());
            if (entity == null ) throw new ArgumentNullException();
            var set = context.Set<TEntity>();
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            // EntityEntry<TEntity> x.Property<propertytype>("propertyname").CurrentValue
           /* var entry = context.Entry<TEntity>(entity);

            var mi1 = GetFunc<TEntity>();

            var mi2 = mi1(entry);

            Console.WriteLine("MI2 \n{0}\n/MI2", mi2 != null? DebugHelpers.ListObject(mi2): "(null)");

            var Ni1 = GetFunc2<TEntity>();

            var Ni2 = Ni1(entry);

            Console.WriteLine("NI2 \n{0}\n/NI2", Ni2 != null? DebugHelpers.ListObject(Ni2): "(null)");
            */
            
            
            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");
            // exclude version property from lambda
            var properties  = vk.Properties.Where(p => p.Name != "Version").ToList();
            // map parameter for lambda call            
            var parameter = Expression.Parameter(typeof(EntityEntry<TEntity>), "x");

            // get key values for entity            
            var vb = context.GetVersionKeyValues(entity);

            var predicate = BuildPredicate(properties, vb, parameter);
            
            var lambda = Expression.Lambda(predicate, parameter) as Expression<Func<EntityEntry<TEntity>, bool>>;
            Console.WriteLine("Lambda {0}", lambda);
            
            IQueryable<EntityEntry<TEntity>> ccc = context.ChangeTracker.Entries<TEntity>().AsQueryable();

            string xs = ccc.Where(t => t.Property<string>("EmployeeId").CurrentValue == "0203").ToList().Select(m =>  
                m.Property<int>("Version").CurrentValue.ToString() + " " + m.Property<string>("EmployeeId").CurrentValue ).
                ToList().Join(" \n");
            Console.WriteLine("BODY OF LAMBDA \n{0}\nEND OF BODY", lambda.Body);

          

            string ss = ccc.Where(lambda).Where( p => p.Property<int>("Version").CurrentValue > 0).ToList().Select(m =>  
                m.Property<int>("Version").CurrentValue.ToString() + " " + m.Property<string>("EmployeeId").CurrentValue ).
                ToList().Join(" \n");

            Console.WriteLine("VALUES FOUND {0}", ccc.Where(lambda).ToList().Count());
            Console.WriteLine("VALUES FOUND {0}", xs);
            Console.WriteLine("Build predicate is: {0}\n{1}", predicate.ToString(), ss);

        }
        
        public static ValueBuffer GetVersionKeyValues<TEntity>(this DbContext context, TEntity entity) where TEntity : class 
        {

            if (entity == null ) throw new ArgumentNullException();
            var set = context.Set<TEntity>();

            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");

            var properties  = vk.Properties.Where(p => p.Name != "Version").ToList();            

            var keyValues = new object[properties.Count];
            for (var i = 0; i < keyValues.Length; i++)
            {
                var oo = entity.GetType().GetTypeInfo().
                    GetProperties().Where(p => p.Name == properties[i].Name).
                    Select(p => p.GetValue(entity,null)).FirstOrDefault();
                //var value = Expression.Constant(oo, oo.GetType() );

                keyValues[i] = oo;
                if (keyValues[i] == null)
                {
                    throw new ArgumentNullException("One of composite key values is null!");
                }
            }
            return new ValueBuffer(keyValues);
        }
        

        
         /// <summary>
         ///   Builds predicate for comparison. Properties that are going to be used, will be given.
         /// <see><a href="https://github.com/aspnet/EntityFramework/blob/f9adcb64fdf668163377beb14251e67d17f60fa0/src/Microsoft.EntityFrameworkCore/Internal/EntityFinder.cs">https://github.com/aspnet/EntityFramework/blob/f9adcb64fdf668163377beb14251e67d17f60fa0/src/Microsoft.EntityFrameworkCore/Internal/EntityFinder.cs</a></see>
         ///  </summary>
         /// <param name="keyProperties"></param>
         /// <param name="keyValues"></param>
         /// <param name="entityParameter"></param>
         /// <returns></returns>
         
         private static BinaryExpression BuildPredicate(
            IReadOnlyList<IProperty> keyProperties,
            ValueBuffer keyValues,
            ParameterExpression entityParameter            
            )
        {   
            // entityParameter is   Expression.Parameter(typeof(EntityEntry<TEntity>), "x");
            // should i be looking for func func<TEntity, Expression<Func<EntityEntry<TEntity>, bool>>> ?
            // Object type for PropertyEntry which will be returned when ChangeTracker.Entries<TEntity>() is called            
            Type propertyParameterType = typeof(PropertyEntry<,>);
            
            // Values that make up key.
            var keyValuesConstant = Expression.Constant(keyValues);                                    
            
            // Signature to find correct property method 
            var methodSignature = new Type[] { typeof(string) };

            // Method info which helps to invoke 
            MethodInfo propertyMethodInfo= typeof(EntityEntry<>)
                        .MakeGenericType(entityParameter.Type.GenericTypeArguments[0])  // resolves to EntityEntry<TEntity>
                        .GetMethod("Property", methodSignature);  // resolves to EntityEntry<TEntity>.Property<Propertytype>(string propertyName);
            
            
            BinaryExpression predicate = null;

            // Build predicate expression on given key properties.            
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];                
                
                // create type of PropertyType<TEntity, property.ClrType>
                var propertyEntryType = propertyParameterType.MakeGenericType(
                    new Type[] {
                        entityParameter.Type.GenericTypeArguments[0]  
                        , property.ClrType
                    }
                );

                // property that contains value to be compared against     
                // that is EntityEntry<TEntity>(entity).Property<propertytype>(propertyName).CurrentValue     
                var currentValuePropertyInfo = propertyEntryType.GetProperty("CurrentValue", property.ClrType);
                
                // build equation
                var equalsExpression =
                    Expression.Equal( // start of one expression
                        Expression.Property( // get property of PropertyEntity

                            Expression.Call( // Calling for x.Property<propertype>(propertyname)                             
                                entityParameter, // typeof(EntityEntry<TEntity>)
                                propertyMethodInfo.MakeGenericMethod(property.ClrType),                            
                                Expression.Constant(property.Name, typeof(string))
                                )                                                                                               
                            , currentValuePropertyInfo  // Calling for x.Property<propertype>(propertyname).CurrentValue                     
                        )                                                    
                        ,    
                        Expression.Constant(keyValues[i],typeof(string)) // value of tentity.propertyname
                    ); // end of one expression
            if (predicate == null)  {
                predicate = equalsExpression;
            } else {
                predicate = Expression.AndAlso(predicate, equalsExpression);
            }                
            }
            
            return predicate;
        }

        // ================================================================================
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

            
            Console.WriteLine("Entity type name: {0} '{1}'", entityType.Name, entityType.ClrType.FullName);
            
            // accessor to entityParameter property N
            IList<MemberExpression> pl = new List<MemberExpression>();

            BinaryExpression predicate = null;

            foreach( var prop in properties ) {
                Console.WriteLine("Prop name: {0}", prop.Name);
                // find property accessor
                // Tentity.property. There *should* not be any properties with multiple types..
                var pi = typeof(TEntity).GetProperty(prop.Name);
                Console.WriteLine(pi.GetGetMethod().Name);
                var me = Expression.Property( entityParameter, pi.GetGetMethod());
                pl.Add(me);


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
                        //Expression.Constant(keyValues[i],typeof(string)) // value of tentity.propertyname
                        me
                    ); // end of one expression
                if (predicate == null)  {
                    predicate = equalsExpression;
                } else {
                    predicate = Expression.AndAlso(predicate, equalsExpression);
                }      
            }
            Console.WriteLine("Predicate is: '{0}'", predicate.ToString());
            IList<ParameterExpression> parameters = new List<ParameterExpression>();
            parameters.Add(entityEntryParameter);
            parameters.Add(entityParameter );
            

            Expression< Func< EntityEntry<TEntity>, TEntity, bool>> exp = ( Expression< Func< EntityEntry<TEntity>, TEntity, bool>>) Expression.Lambda(predicate,parameters);
            Console.WriteLine("Lambda is: '{0}'", exp.ToString());
            var xc = Expression.Constant(exp, typeof(Expression< Func< EntityEntry<TEntity>, TEntity, bool>>));
            var ll =  Expression.Lambda(xc,null);
            Console.WriteLine("Lambda is: '{0}'", ll.ToString());
            return (Func< Expression< Func< EntityEntry<TEntity>, TEntity, bool>>>) ll.Compile();
/*

            // create lambda with two parameters
            var c = Expression.Constant(true, typeof(System.Boolean));
            IList<ParameterExpression> paramss = new List<ParameterExpression>();

            paramss.Add(entityParameter );
            paramss.Add(entityEntryParameter);
            var exp = Expression.Lambda(c,paramss);
            Console.WriteLine("FLambda \n{0}\n/FLambda", exp.ToString());

            IList<ParameterExpression> paramss2 = new List<ParameterExpression>();

            paramss2.Add(entityEntryParameter );
            //paramss.Add(entityEntryParameter);
            Expression< Func< EntityEntry<TEntity>, bool>> exp1 = ( Expression< Func< EntityEntry<TEntity>, bool>>)Expression.Lambda(c, paramss2);
            
            Console.WriteLine("EXP1FLambda \n{0}\n/FLambda", exp1.ToString());

            var xc = Expression.Constant(exp1, typeof(Expression< Func< EntityEntry<TEntity>, bool>>));
            Console.WriteLine("FLambda \n{0}\n/FLambda", exp1.ToString());
            var vv = Expression.Variable(typeof(Expression< Func< EntityEntry<TEntity>, bool>>));            
            var exp2 = Expression.Assign(vv, xc);
            



            Console.WriteLine("FLambda \n{0}\n/FLambda", vv.ToString());
            Console.WriteLine("FLambda \n{0}\n/FLambda", exp2.ToString());
*/
            // return Expression< Func< EntityEntry<TEntity>, bool>>
           // return null;
        }


        
        // ================================================================================

        public static void printChangeTracker<TEntity>(this DbContext context, string tracing) where TEntity : class {
            Console.WriteLine("[{0}] ChangeTracker has {1} entries", tracing, context.ChangeTracker.Entries<TEntity>().Count());
            foreach(var xx in context.ChangeTracker.Entries<TEntity>()) {
                //xx.Metadata.
                
                int version = xx.Property<int>("Version").CurrentValue;
                string s = xx.Property<string>("EmployeeId").CurrentValue;
                DateTime d = xx.Property<DateTime>("ValidFrom").CurrentValue;
                DateTime d2 = xx.Property<DateTime>("ValidUntil").CurrentValue;

                Console.WriteLine("{0} {1} {2} {3} {4}", xx.Property<int>("ChangeId").CurrentValue, s, version,
                    d, d2);
                                
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

        // gets value of employeeid
        public  static Func<EntityEntry<TEntity>, string> GetFunc2<TEntity>() where TEntity : class {
            var sourceParameter = Expression.Parameter(typeof(EntityEntry<TEntity>), "source");            
            
            var str = new Type[] { typeof(string) };
            
            var returnTarget = Expression.Label(typeof(string));

            var lambda = Expression.Lambda<Func<EntityEntry<TEntity>,  string>>(                                
                Expression.Property(

                Expression.Call(
                    sourceParameter
                    , typeof(EntityEntry<TEntity>).GetMethod("Property", str).MakeGenericMethod(typeof(string))
                    , Expression.Constant("EmployeeId", typeof(string))
                )
                    ,  typeof(PropertyEntry<TEntity, string>).GetProperty("CurrentValue", typeof(string))

                )                
                , sourceParameter
                            
            );
            Console.WriteLine("Lambda \n{0}\n/dLambda", lambda.ToString());
            return lambda.Compile();                        
        }


        public  static Func<EntityEntry<TEntity>, PropertyEntry<TEntity, string>> GetFunc<TEntity>() where TEntity : class {
            var sourceParameter = Expression.Parameter(typeof(EntityEntry<TEntity>), "source");
                        
            var str = new Type[] { typeof(string) };
            
            var lambda = Expression.Lambda<Func<EntityEntry<TEntity>,  PropertyEntry<TEntity, string>>>(
                Expression.Call(
                    sourceParameter
                    , typeof(EntityEntry<TEntity>).GetMethod("Property", str).MakeGenericMethod(typeof(string))
                    , Expression.Constant("EmployeeId", typeof(string))
                )           
                , sourceParameter                
            );
            Console.WriteLine("Lambda \n{0}\n/dLambda", lambda.ToString());
            return lambda.Compile();            
            
        }
              

    }
}