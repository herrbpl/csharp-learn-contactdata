using Microsoft.EntityFrameworkCore;
using ASTV.Models.Generic;

namespace ASTV.Services {
    public class LanguageRepository<TContext>: EntityBaseRepository<Language, TContext>                
        where TContext : EmployeeContext       
    {        
        public LanguageRepository(TContext context) 
            : base(context)
        {
                          
        } 
    }
}