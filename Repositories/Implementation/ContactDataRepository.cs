using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using ASTV.Models.Employee;

namespace ASTV.Services {
    public class ContactDataRepository<TContext>: EntityBaseRepositoryReadOnly<ContactData, TContext>                
        where TContext : ContactDataContext       
    {
        public ContactDataRepository(TContext context) : base(context) {            
        }
        // add - if exists, update, else add
        // update - if exist, create old version
        // delete - mark deleted

        // Get all latest versions
        public override IEnumerable<ContactData> GetAll() {
            return null;
        }
    }
}    