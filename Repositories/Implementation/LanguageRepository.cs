using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ASTV.Services {

    public interface ILanguageRepository: IEntityBaseRepository<Language, ContactDataContext> {}

    public class LanguageRepository: EntityBaseRepository<Language, ContactDataContext>, ILanguageRepository                
        
    {        
        public LanguageRepository(ContactDataContext context) 
            : base(context)
        {
                          
        }      
    }
}