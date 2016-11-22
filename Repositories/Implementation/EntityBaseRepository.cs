using ASTV.Models.Generic;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

using ASTV.Extenstions;

namespace ASTV.Services {
    /// <summary>
    /// Base repository with modification capabilities
    ///
    /// Origin <see><a href="https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/">https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/</a></see>
    ///</summary>
    public class EntityBaseRepository<T, TContext> : 
        EntityBaseRepositoryReadOnly<T, TContext>, IEntityBaseRepository<T, TContext>
        where T: class, IEntityBase, new()  
        where TContext : DbContext
     {
        
        public EntityBaseRepository(TContext context) : base(context) {
            //this._context = context;
        }

        public virtual void Add(T entity)
        {

            /*
            Console.WriteLine("==========================================================\n");
            Console.WriteLine("Entity to be saved: \n{0}", EntityVersioningExtensions.ListObject(entity));
            Console.WriteLine("==========================================================\n");
            
            IQueryable<T> qq = _context.Set<T>();
            Console.WriteLine("CCOUNT IS:{0}", qq.IsCurrent(entity).Count());

           // VersionInfo ll = qq.Latest(entity);

            foreach(var xx in _context.Set<T>().AsNoTracking().Where(
                e => EF.Property<Boolean>(e, "IsCurrent") == true).Select( m => 
                    new VersionInfo { 
                            IsCurrent = EF.Property<Boolean>(m, "IsCurrent"), 
                            ChangeId = EF.Property<int>(m, "ChangeId"),
                            Version = EF.Property<int>(m, "Version")
                            
                            } )) {
                Console.WriteLine(xx.ToString());
                
                Console.WriteLine("XX info: \n{0}", EntityVersioningExtensions.ListObject(xx));
            }
            _context.Set<T>().PK("");
            var k = _context.Set<T>().GetVersionKeys();
            if (k != null) {
                Console.WriteLine("Versioning key found: ");
            }

            */
            // this line is probably creating it in changetrcker?
          //  _context.printChangeTracker<T>("BeforeCreatEntry");
          //  _context.printSet<T>("BeforeCreatEntry");
            EntityEntry dbEntityEntry = _context.Entry<T>(entity);
          //  _context.printChangeTracker<T>("BeforeAdd");
          //  _context.printSet<T>("BeforeAdd");     
            //dbEntityEntry.Property("Version").CurrentValue = (int)dbEntityEntry.Property("Version").CurrentValue+1;
            _context.Set<T>().Add(entity);
          //  _context.printChangeTracker<T>("AfterAdd");
          //  _context.printSet<T>("AfterAdd");        
           // _context.SaveChanges();          
          //  _context.printChangeTracker<T>("AfterSave");
          //  _context.printSet<T>("AfterSave");
            Console.WriteLine("======================================================="); 

            /*            
            Console.WriteLine("Entity added");
            IList<T> o = _context.Set<T>().ToList();
            int i = 0;
            foreach(var x in o) {
                i++;
                Console.WriteLine("Line: {0}:{1}", i, x.ToString());
            }
            */
        }
        public virtual void Update(T entity) {
            EntityEntry dbEntityEntry = _context.Entry<T>(entity);
            dbEntityEntry.State = EntityState.Modified;
        }
        public virtual void Delete(T entity) {
            EntityEntry dbEntityEntry = _context.Entry<T>(entity);
            dbEntityEntry.State = EntityState.Deleted;
        }  
    }
}