using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ASTV.Services {
    public class LanguageRepository<TContext>: EntityBaseRepository<Language, TContext>                
        where TContext : EmployeeContext       
    {        
        public LanguageRepository(TContext context) 
            : base(context)
        {
                          
        } 
        public override IList<Language> GetAll() { 
            return _context.Language.AsNoTracking().ToList();
        }
    }
}