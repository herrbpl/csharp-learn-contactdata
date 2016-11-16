using System.ComponentModel.DataAnnotations;
using ASTV.Models.Generic;
namespace ASTV.Models.Employee
{
    public class Employee: IEntityBase  {        
        public int Id { get; set; }
        public string Name { get; set; }
        [MaxLength(6)]  
        public string EmployeeId { get; set; }        
        //public ContactData ContactData {get; set; }
    }    
}
 