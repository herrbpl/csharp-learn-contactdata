using ASTV.Models.Generic;
using System.Collections.Generic;
using System.Linq;

namespace ASTV.Services {
    public interface IEntityBaseRepository<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : class
    {
        IQueryable<T> GetAll();        
    }
}