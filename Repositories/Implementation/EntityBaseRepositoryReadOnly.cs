using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

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
        protected ILogger _logger;
        public EntityBaseRepositoryReadOnly(TContext context) {
            this._context = context;
            ILoggerFactory lf = new LoggerFactory();
            lf.AddConsole().AddDebug();
            this._logger = lf.CreateLogger(this.GetType().Name);
        }
        public EntityBaseRepositoryReadOnly(TContext context, ILoggerFactory loggerFactory) {
            this._context = context;
            this._logger = loggerFactory.CreateLogger(this.GetType().Name);
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
        public virtual int Count() {
            return _context.Set<T>().Count();
        }
    }
}