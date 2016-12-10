using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ASTV.Models.Employee;
using ASTV.Models.Generic;
using ASTV.Services;
using ASTV.Extenstions;


namespace ASTV.Services {

    public interface IContactDataRepository: IEntityBaseRepositoryReadonly<ContactData, ContactDataContext> {
        void Update(ContactData entity);
        ContactData Get(string employeeId);
    }

    public class ContactDataRepository: EntityBaseRepositoryReadOnly<ContactData, ContactDataContext>, IContactDataRepository                               
    {
        protected IContactDataDefaultProvider _defaultProvider;
        protected IEmployeeRepository _employeeRepository;
        public ContactDataRepository(ContactDataContext context, IContactDataDefaultProvider defaultProvider, IEmployeeRepository employeeRepository) : base(context) {
            _defaultProvider = defaultProvider;           
            _employeeRepository = employeeRepository; 
        }
    
        // Get all latest versions
        // Actually we should not need to get this particular list because it is supposed always accessed thtough employee
        public override IEnumerable<ContactData> GetAll() {
            //return null;
            return _context.Set<ContactData>().Where(p => EF.Property<Boolean>(p, "IsCurrent") == true).ToList();            
        }                

        public override int Count() {
            return _context.Set<ContactData>().Where(p => EF.Property<Boolean>(p, "IsCurrent") == true).Count();
        }

        // return Contactdata if EMployee is existing, else return null. Or should throw?
        public ContactData Get(String employeeId) {
            var Employee = _employeeRepository.GetList(p => p.EmployeeId == employeeId).FirstOrDefault();
            if (Employee == null) {
                return null;
            }
            var cd = _context.Set<ContactData>().Where(p => EF.Property<Boolean>(p,"IsCurrent") == true && p.EmployeeId == employeeId).FirstOrDefault();
            if (cd == null) {
                cd = _defaultProvider.GetDefault(employeeId);
            }
            return cd;
        }

        public void Update(ContactData entity) {
            // check that employee exists.
            _context.Update(entity);
            _logger.LogInformation("Update invoked");            
        }
    }
}    