using System.ComponentModel.DataAnnotations;
using ASTV.Models.Generic;
using Newtonsoft.Json;
namespace ASTV.Models.Employee {
    public class Education: IEntityBase {
        
        //[JsonIgnore]
         //public int Id { get; set; }
         public EducationLevel Level { get; set; }

         [MaxLength(100)]
         public string SchoolName { get; set; }
         [MaxLength(255)]
         public string Specification { get; set; }

         [MaxLength(255)]
         public string NameOfDegree { get; set; }

         public int YearStarted { get; set; }
         public int? YearCompleted { get; set; }

         [MaxLength(255)]
         public string UrlDiploma { get; set;}

         // reference to ContactData
         //public int ContactDataId { get; set; }
         //public ContactData ContactData { get; set; } // should there be link back to ContactData?         
    }
}