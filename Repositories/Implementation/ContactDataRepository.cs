using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Services;


namespace ASTV.Services {

    public interface IContactDataRepository: IEntityBaseRepositoryReadonly<ContactData, ContactDataContext> {
        void Update(ContactData entity);
    }
    public class ContactDataRepository: EntityBaseRepositoryReadOnly<ContactData, ContactDataContext>, IContactDataRepository                               
    {
        public ContactDataRepository(ContactDataContext context) : base(context) {            
        }
        // add - if exists, update, else add
        // update - if exist, create old version
        // delete - mark deleted

        // Get all latest versions
        public override IEnumerable<ContactData> GetAll() {
             ContactData cd = new ContactData();
                // should it be here??
                var lr = new LanguageRepository(this._context);
                var er = new EducationRepository(this._context);

                Language ll = lr.GetAll().Where(l => l.Code=="EE").FirstOrDefault();
                EducationLevel el = er.GetAll().Where(l => l.Code=="ah").FirstOrDefault();

                Education edu = new Education();
                edu.SchoolName = "Uus kool";
                edu.YearCompleted = 2015;
                edu.NameOfDegree = "uu";
                edu.Level = el;


                cd.FirstName = "Siim";
                cd.LastName = "Aus";
                cd.JobTitle = "IT Director";
                cd.ContactLanguage = ll;
                cd.Education.Add(edu);

                cd.EmployeeId = "0203";
            IList<ContactData> a = new List<ContactData>();
            a.Add(cd);
            return a;
        }
        
        public void Update(ContactData entity) {
            _logger.LogInformation("Update invoked");            
        }
    }
}    