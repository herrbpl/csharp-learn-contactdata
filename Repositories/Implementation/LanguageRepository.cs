using ASTV.Models.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ASTV.Services {
    public class LanguageRepository<TContext>: EntityBaseRepository<Language, TContext>                
        where TContext : ContactDataContext       
    {        
        public LanguageRepository(TContext context) 
            : base(context)
        {
                          
        } 
        public override IEnumerable<Language> GetAll() { 
            return _context.Language.AsNoTracking().ToList();
        }
    }
}