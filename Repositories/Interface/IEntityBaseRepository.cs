using ASTV.Models.Generic;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ASTV.Services {

    /// <summary>
    /// Base repository for ASTV services, read only access
    /// /// </summary>
    public interface IEntityBaseRepositoryReadonly<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : DbContext
    {
        IEnumerable<T> GetAll(); 
    }

    /// <summary>
    /// Base repository for ASTV services
    /// As we try to use stateless data, we are not keeping track of object state??
    /// So we do not also try to create queryable objects.
    /// /// </summary>
    public interface IEntityBaseRepository<T, TContext>: IEntityBaseRepositoryReadonly<T, TContext>
        where T: class, IEntityBase, new()  
        where TContext : DbContext
    {
        
        void Add(T entity);  
        void Update(T entity);
        void Delete(T entity);
    }
}