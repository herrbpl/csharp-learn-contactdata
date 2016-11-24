using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ASTV.Services {

    public interface IEducationRepository: IEntityBaseRepository<EducationLevel, ContactDataContext> {}

    public class EducationRepository: EntityBaseRepository<EducationLevel, ContactDataContext>, IEducationRepository                
        
    {        
        public EducationRepository(ContactDataContext context) 
            : base(context)
        {
                          
        }      
    }
}