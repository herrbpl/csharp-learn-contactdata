using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using ASTV.Models.Generic;

namespace ASTV.Services {

    /// <summary>
    /// Base repository for ASTV services, read only access
    ///  </summary>
    public interface IEntityBaseRepositoryReadonly<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : DbContext
    {
        IEnumerable<T> GetAll();
              
        /// <summary>
        /// Gets list from source, searched by where, including properties in navigationProperties        
        /// <see>Origin - <a href="https://blog.magnusmontin.net/2013/05/30/generic-dal-using-entity-framework/comment-page-1/">https://blog.magnusmontin.net/2013/05/30/generic-dal-using-entity-framework/comment-page-1/</a></see>                 
        /// <param name="where"> Lambda expression</param>
        /// <param name="Expression<Func<T"></param>
        /// <param name="navigationProperties"></param>
        /// <returns>List of objects T</returns> 
        /// </summary>        
        

        IEnumerable<T> GetList(Func<T, bool> where, params Expression<Func<T,object>>[] navigationProperties);
        int Count();
    }

    /// <summary>
    /// Base repository for ASTV services
    /// As we try to use stateless data, we are not keeping track of object state??
    /// So we do not also try to create queryable objects.
    /// </summary>
    public interface IEntityBaseRepository<T, TContext>: IEntityBaseRepositoryReadonly<T, TContext>
        where T: class, IEntityBase, new()  
        where TContext : DbContext
    {
        
        void Add(T entity);  
        void Update(T entity);
        void Delete(T entity);
    }
}