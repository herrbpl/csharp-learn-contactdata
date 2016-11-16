using ASTV.Models.Generic;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

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