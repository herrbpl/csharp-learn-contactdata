using ASTV.Models.Employee;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace ASTV.Services {
    
    public class EmployeeRepository<TContext>: EntityBaseRepositoryReadOnly<Employee, TContext>                
        where TContext : EmployeeContext       
    {
        
        public EmployeeRepository(TContext context) 
            : base(context)
        {
                          
        } 
        public override IEnumerable<Employee> GetAll() {            
            return _context.Employees.AsNoTracking().ToList().AsEnumerable();                                                
        }
        
    }
}