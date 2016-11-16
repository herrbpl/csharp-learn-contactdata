using System.Collections.Generic;
using ASTV.Models.Generic;
namespace ASTV.Services
{    
    /// <summary>
    /// Interface to process entity hierarcy    
    /// </summary>
    public interface IEntityHierachy<T> where T: class , IEntityBase {
        /// <summary>
        /// Gets children of entity T        
        /// </summary>
        /// <param name="entity">Entity for which children are being searched</param>
        /// <returns>Collection of entities T</returns>
        IEnumerable<T> GetChildren(T entity);                
    }
}