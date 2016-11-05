using ASTV.Models.Generic;
using System.Collections.Generic;

namespace ASTV.Services {
    public interface IEntityBaseRepository<T, TContext> 
        where T: class, IEntityBase, new()  
        where TContext : class
    {
        IEnumerable<T> GetAll();        
    }
}