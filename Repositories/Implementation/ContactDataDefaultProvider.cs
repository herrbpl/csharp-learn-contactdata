using System;
using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Services;


namespace ASTV.Services {
    public interface IContactDataDefaultProvider: IEntityDefaultProvider<ContactData, System.String> {}
    
    public class ContactDataTestProvider: IContactDataDefaultProvider {

        public ContactData GetDefault() {
            return GetDefault(""); 
        }
        public  ContactData GetDefault(string key) {
            var cd = new ContactData();
            cd.EmployeeId = key;
            cd.FirstName = "Test";
            cd.LastName = "Person";
            cd.Address1 = "Test address '" + key + "'";
            cd.Address2 = "Address Line 2";
            cd.EmailBusiness = "test.person@test.com";
            cd.CityOrCommune = "Test City";
            cd.County = "Test County";
            return cd;
        }
    }

}