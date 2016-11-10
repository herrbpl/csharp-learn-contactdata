using ASTV.Models.Employee;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace ASTV.Services {
    
    public class EmployeeRepository<TContext>: EntityBaseRepository<Employee, TContext>                
        where TContext : EmployeeContext       
    {
        
        public EmployeeRepository(TContext context) 
            : base(context)
        {
                          
        } 
        public override IQueryable<Employee> GetAll() {
            _context.Language.ToList();
            _context.EducationLevel.ToList();
            
            return _context.Employees.
                Include(c => c.ContactData).
                ThenInclude(e => e.Education).                
                AsQueryable();
        }
        
    }
}