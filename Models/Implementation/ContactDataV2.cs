using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using ASTV.Models.Generic;
using ASTV.Extenstions;
using Newtonsoft.Json;



namespace ASTV.Models.Employee {
    public class ContactData: IEntityBase, IEntityVersioning {

         public ContactData() {
           Education = new List<Education>();
         }
         //public int Id { get; set; }

         //public int EmployeeId { get; set; }

         [MaxLength(6)]  
         public string EmployeeId { get; set; }      

         [MaxLength(30)]
         public string FirstName {get; set; }  
         
         [MaxLength(50)]
         public string LastName {get; set; }

         [MaxLength(100)]
         public string JobTitle {get; set; }
         public Language ContactLanguage { get; set; } // should also try with many languages.. how does that works?

         [MaxLength(255)]
         public string Address1 {get; set; }
         [MaxLength(255)]
         public string Address2 {get; set; }

         [MaxLength(100)]
         public string CityOrCommune {get; set; }

         [MaxLength(30)]
         public string County {get; set; }
         
         [MaxLength(6)]
         public string ZipCode {get; set; }

         [MaxLength(50)]
         public string EmailPersonal {get; set; }

         [MaxLength(50)]
         public string EmailBusiness {get; set; }

         [MaxLength(15)]
         public string PhonePersonal {get; set; }
         [MaxLength(15)]
         public string MobilePersonal {get; set; }

         [MaxLength(15)]
         public string PhoneBusiness {get; set; }
         [MaxLength(15)]
         public string MobileBusiness {get; set; }

         [MaxLength(3)]
         public string QuickDialBusiness {get; set; }
         
         [MaxLength(50)]
         public string ContactPerson {get; set; }  
          [MaxLength(15)]
         public string PhoneContactPerson {get; set; }

         public List<Education> Education { get; set; }

         [MaxLength(255)]
         public string UrlIdCard { get; set;}

         [MaxLength(255)]
         public string UrlDriverLicense { get; set;}

         [MaxLength(255)]
         public string UrlLivingPermit { get; set;}

         [JsonIgnore]
         public string Serialized {
           get {   return this.Serialize(null); }
           set { this.DeSerialize(this, value,new List<string> { "Serialized", "Id"} );  }
         }
    }
}