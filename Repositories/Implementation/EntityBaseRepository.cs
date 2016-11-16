using ASTV.Models.Generic;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace ASTV.Services {
    /// <summary>
    /// Base repository
    ///
    /// Origin <see><a href="https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/">https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/</a></see>
    ///</summary>
    public class EntityBaseRepository<T, TContext> : 
        IEntityBaseRepository<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : DbContext
     {
        protected TContext _context;
        public EntityBaseRepository(TContext context) {
            this._context = context;
        }
        public virtual IList<T> GetAll()
        {                        
            return _context.Set<T>().ToList();             
        }
        public virtual void Add(T entity)
        {
            EntityEntry dbEntityEntry = _context.Entry<T>(entity);            
            _context.Set<T>().Add(entity);                          
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