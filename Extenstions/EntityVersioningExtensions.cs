
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
                        .Property<int>("ChangeId").ValueGeneratedOnAdd();
                    
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

        internal static readonly MethodInfo PropertyMethod2
            = typeof(EntityEntry).GetTypeInfo().GetDeclaredMethods(nameof(Property)).
                    Where(
                            m => m.GetParameters().Where(
                                    p =>  p.Name == "propertyName" && p.ParameterType == typeof(string)  
                                    )
                                    .FirstOrDefault() != null //&& m.IsGenericMethod == true
                    ).First();

        // https://github.com/aspnet/EntityFramework/blob/1fa247b038927a7d7438f666dc11253f64e0432d/src/Microsoft.EntityFrameworkCore/Storage/ValueBuffer.cs
        internal static readonly MethodInfo GetValueMethod
            = typeof(ValueBuffer).GetRuntimeProperties().Single(p => p.GetIndexParameters().Any()).GetMethod;

        
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

        public static IKey GetVersionKeys<TEntity>(this DbSet<TEntity> set) where TEntity : class
        {
            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            
            //var pk = entityType.FindPrimaryKey();
            var keys = entityType.GetKeys();
            foreach(var key in keys) {
                if (key.Properties.Where(m => m.Name == "Version").Count() > 0) {
                    //Console.WriteLine("AK FOUND: '{0}': {1}", key.SqlServer().Name, key.Properties.Select(m => m.Name).ToArray().Join(", "));
                    return key;
                }                
            }
            return null;
        }


        public static int MaxVersion<TEntity>(this DbSet<TEntity> set, TEntity entity) where TEntity : class {
            try {

                return set.Versions(entity).Select( 
                            m => EF.Property<int>(m, "Version") ).Max();
            } catch (Exception e) {
                return 0;
            }

        }

         public static TEntity GetVersion<TEntity>(this DbSet<TEntity> set, TEntity entity, int version) where TEntity : class {

            if (entity == null ) throw new ArgumentNullException();               
            var x = set.Versions(entity).
                Where( m => EF.Property<int>(m, "Version") == version).FirstOrDefault();                              
            return x;
         }

         // change to DbSet based
        public static TEntity Latest<TEntity>(this IQueryable<TEntity> source, TEntity entity) where TEntity : class {
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;    
                int mv = set.MaxVersion(entity);
                if (mv == 0 ) return null;
                return set.GetVersion(entity, mv);
            }
            return null;
        }

        public static TEntity LatestSQL<TEntity>(this DbSet<TEntity> set, TEntity entity) where TEntity : class {

            if (entity == null ) throw new ArgumentNullException();
            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");

            var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            string schema = (entityType.Relational().Schema == ""? entityType.Relational().Schema+"." : "");
            //entityType.Relational().Schema
            string sql = @"SELECT * FROM {0}{1} as T WHERE Version = (SELECT MAX(Version) FROM {0}{1} WHERE #KEY#) AND #KEY#)";

            string ak = "";
            object[] sqlParams = {};

            foreach(var property in vk.Properties) {
                if (property.Name != "Version") {
                    // get value of entity of given name.
                    var oo = entity.GetType().GetTypeInfo().
                        GetProperties().Where(p => p.Name == property.Name).
                        Select(p => p.GetValue(entity,null)).FirstOrDefault();
                    
                    // value I'm comparing to, aka value of key
                    var value = Expression.Constant(oo, oo.GetType() );
                    
                    ak += sqlParams.Count()>0 ? " AND ":"";
                    ak += " \""+ property.Relational().
                        ColumnName.Replace("\"", "#quot;#quot").Replace("#quot;", "\"") +"\" = @p"+sqlParams.Count().ToString();
                    

                    sqlParams.Append( 
                        new SqlParameter {
                            ParameterName = "p"+sqlParams.Count().ToString() 
                            , Value = value
                        }

                    );

                }
            }
            sql = sql.Replace("#KEY#", "(1=1)");
            //sql = sql.Replace("#KEY#", ak);
            sql = String.Format(sql, schema, entityType.Relational().TableName);
            
            
            Console.WriteLine(sql);
            //var x = set.FromSql<TEntity>(sql, sqlParams).SingleOrDefault();
            var x = set.FromSql<TEntity>(sql).SingleOrDefault();
            

            return x;
        }
        

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
            if (entity == null ) throw new ArgumentNullException();
            var set = context.Set<TEntity>();

            var vk = set.GetVersionKeys();
            if (vk == null) throw new ArgumentException("Version keys are not defined for set");

            var properties  = vk.Properties.Where(p => p.Name != "Version").ToList();            
            var parameter = Expression.Parameter(typeof(EntityEntry<TEntity>), "x");
            var parameter2 = Expression.Parameter(typeof(TEntity), "y");            
            var vb = context.GetVersionKeyValues(entity);
            var predicate = BuildPredicate(properties, vb, parameter);
            
            Console.WriteLine(predicate.ToString());
            Console.WriteLine(parameter.ToString());
            var xp = new  ParameterExpression[2];
            xp[0] = parameter;
            xp[1] = parameter2;
            //var lambda = Expression.Lambda(predicate, xp) as Expression<Func<EntityEntry<TEntity>,TEntity, bool>>;
            var lambda = Expression.Lambda(predicate, parameter) as Expression<Func<EntityEntry<TEntity>, bool>>;
            Console.WriteLine("Lambda {0}", lambda);
            IQueryable<EntityEntry<TEntity>> ccc = context.ChangeTracker.Entries<TEntity>().AsQueryable();
            
            ccc.Where(lambda).ToList();
            Console.WriteLine("Build predicate is: {0}", predicate.ToString());

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
                var value = Expression.Constant(oo, oo.GetType() );

                keyValues[i] = value;
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
            var keyValuesConstant = Expression.Constant(keyValues);
            //Console.WriteLine("Type of parameter expression is \n{0}", ListObject(entityParameter));
            var xxx = entityParameter.Type.GenericTypeArguments.ToList().Select(x => x.FullName).Join(", ");
            var y = Expression.Parameter( entityParameter.Type.GenericTypeArguments[0], "y");
            
            Console.WriteLine(entityParameter.Type.GenericTypeArguments[0].GetMethods().Select(m => m.Name).ToList().Join("\n"));
            Console.WriteLine("BLAAAH");

            
            Type tt = typeof(PropertyEntry<,>);

            //Console.WriteLine(entityParameter.Type.GetMethods().Select(m => m.Name).ToList().Join("\n"));
            MethodInfo xxz = null;

            foreach(var mm in 
                    entityParameter.Type.GetMethods().Where(m => m.Name=="Property").
                    Where(
                            m => m.GetParameters().Where(
                                    p =>  p.Name == "propertyName" && p.ParameterType == typeof(string)  
                                    )
                                    .FirstOrDefault() != null && m.IsGenericMethod == true
                    )) {
                Console.WriteLine("IsGeneric: {0}, IsGenericMethodDefinition {1}", mm.IsGenericMethod, mm.IsGenericMethodDefinition);
                Console.WriteLine("Params {0}", mm.GetParameters().Select(m => (m.Name + ": "+m.ParameterType.Name)).ToList().Join("\n"));
                xxz = mm;
                //Console.WriteLine("Type of method is \n{0}", ListObject(mm));    
            }

            
            //ParameterExpression y = Expression.Parameter(entityParameter.)
            //MethodInfo mi = entityParameter.Type.GetMethods("Property").;
            //Console.WriteLine("Type of method is \n{0}", ListObject(mi));
            BinaryExpression predicate = null;
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];                
                
                var ttt = tt.MakeGenericType(
                    new Type[] {
                        entityParameter.Type.GenericTypeArguments[0]
                        , property.ClrType
                    }
                );

                Console.WriteLine(ttt.GetMethods().Select(m => m.Name).ToList().Join("\n"));
                var mmm = ttt.GetMethod("get_CurrentValue")
                /*.MakeGenericMethod(  new Type[] {
                                entityParameter.Type.GenericTypeArguments[0]
                                , property.ClrType
                            })
                
                */;
                
                var equalsExpression =
                    Expression.Equal(
                        Expression.Call(       

                        Expression.Call(
                            //entityParameter.Type.GenericTypeArguments[0].GetMethod("Property"),
                            //entityParameter.//.MakeGenericMethod(property.ClrType),
                            entityParameter,
                            xxz.MakeGenericMethod(property.ClrType),
                            //PropertyMethod2.MakeGenericMethod(property.ClrType),
                            Expression.Constant(property.Name, typeof(string))
                            )
                        //, //tt //entityParameter.Type                                                                         
                        , mmm
                        //, "CurrentValue"
                        )
                        
                            
                            ,
                        Expression.Convert(
                           // Expression.Constant(keyValues[i], typeof(object)),
                             Expression.Call(
                                keyValuesConstant,
                                GetValueMethod,
                                Expression.Constant(i)), 
                            property.ClrType)
                            );
            if (predicate == null)  {
                predicate = equalsExpression;
            } else {
                predicate = Expression.And(predicate, equalsExpression);
            }
                //predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }
            
            return predicate;
        }

       
        // TODO: might have to change this to method of DbSet, not queryable..

        public static IQueryable<TEntity> Versions<TEntity>(this IQueryable<TEntity> source,  TEntity entity) where TEntity : class
        {            
            if (entity == null ) throw new ArgumentNullException();
            //-ValueBuffer
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;                
                var exp = BuildVersionQueryPredicate<TEntity>(set, entity);
                //return set.AsNoTracking().Where(exp);
                var context =  set.GetService<IDbContextServices>().CurrentContext.Context; 
                //Expression<Func<EntityEntry<TEntity>, bool>> 
                //context.ChangeTracker.Entries<TEntity>().Where(p => p.Property<int>("ss").CurrentValue == 1) 
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

        public static IQueryable<TEntity> IsCurrent<TEntity>(this IQueryable<TEntity> source,  TEntity entity) where TEntity : class
        {
            if (typeof(DbSet<TEntity>).IsAssignableFrom(source.GetType())) {
                var set = (DbSet<TEntity>)source;                                
                                
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
                        parameter) as Expression<Func<TEntity, bool>>;
                return source.Where(expression);
            }        
            return source;
        }

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

    }
}