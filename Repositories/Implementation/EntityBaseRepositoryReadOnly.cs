using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;

using ASTV.Models.Generic;

namespace ASTV.Services {
    /// <summary>
    /// Base repository
    ///
    /// Origin <see><a href="https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/">https://chsakell.com/2016/06/23/rest-apis-using-asp-net-core-and-entity-framework-core/</a></see>
    ///</summary>
    public class EntityBaseRepositoryReadOnly<T, TContext> : 
        IEntityBaseRepositoryReadonly<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : DbContext
     {
        protected TContext _context;
        public EntityBaseRepositoryReadOnly(TContext context) {
            this._context = context;
        }
        public virtual IEnumerable<T> GetAll()
        {                        
            return _context.Set<T>().ToList();             
        }
        public virtual IEnumerable<T> GetList(Func<T, bool> where, params Expression<Func<T,object>>[] navigationProperties) {

            IQueryable<T> dbQuery = _context.Set<T>();
                 
            //Apply eager loading
            if (navigationProperties != null) {
                foreach (Expression<Func<T, object>> navigationProperty in navigationProperties)
                    dbQuery = dbQuery.Include<T, object>(navigationProperty);
            }
            return dbQuery
                .AsNoTracking() // Might need to remove that
                .Where(where)
                .ToList<T>().AsEnumerable();
        }
    }
}