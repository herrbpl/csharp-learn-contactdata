using ASTV.Models.Employee;

namespace ASTV.Services {
    
    public class EmployeeRepository<TContext>: EntityBaseRepository<Employee, TContext>                
        where TContext : class       
    {
        
        public EmployeeRepository(TContext context) 
            : base(context)
        {
                                  
        }        
        
    }
}