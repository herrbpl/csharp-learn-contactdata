using ASTV.Models.Generic;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ASTV.Services {
    public class EntityBaseRepository<T, TContext> : 
        IEntityBaseRepository<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : class
     {
        protected TContext _context;
        public EntityBaseRepository(TContext context) {
            this._context = context;
        }
        public virtual IQueryable<T> GetAll()
        {            
            //throw new NotImplementedException("Not implemented!");            
            //return _context.Set<T>().AsEnumerable();
            if (_context is DbContext) {
                DbContext c = (DbContext)(object) _context;

                return c.Set<T>().AsQueryable();
            }
            if (_context is IList<T>) {
                 IList<T> c = (IList<T>)(object) _context;
                return c.AsQueryable();
            }
            // should throw exception.
            IEnumerable<T> a = new List<T>();
            a.Append(new T { Id = 1 });
            a.Append(new T { Id = 2 });
            a.Append(new T { Id = 3 });
            return a.AsQueryable();

        }
    }
}