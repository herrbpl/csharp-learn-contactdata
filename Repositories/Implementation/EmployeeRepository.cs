
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using ASTV.Models.Employee;
using ASTV.Services;

namespace ASTV.Services {

    public interface IEmployeeRepository: IEntityBaseRepositoryReadonly<Employee, EmployeeContext> {}
    
    public class EmployeeRepository: EntityBaseRepositoryReadOnly<Employee, EmployeeContext>                            
    {
        // It is highly unusual, if employee list changes between lifetime of repository object as repo is shortlived.
        protected IList<Employee> _cache; 
        public EmployeeRepository(EmployeeContext context) 
            : base(context)
        {
                        
        } 

        protected void refreshCache() {
            try {                
                this._cache = _context.Employees.FromSql(
                  "select cast(employeeid as int) as Id, name as Name, ltrim(rtrim(employeeid)) as EmployeeId from v_sync_iscala_ad_userdata"
                   ).AsNoTracking().ToList();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                this._cache = new List<Employee>();
            }                
             
        }

        public  IEnumerable<Employee> GetAll(bool forceRefresh) {      
            
            // TODO: Add actual SQL string to appSettings.json
            try {

                if (forceRefresh || _cache == null) { refreshCache(); }
                return this._cache.AsEnumerable();
                
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                if (_cache != null) {
                    return _cache.AsEnumerable();
                } else {
                    return new List<Employee>().AsEnumerable();
                }                                
            }

            //return _context.Employees.AsNoTracking().ToList().AsEnumerable();                                                
        }

        public override IEnumerable<Employee> GetAll() {      
           return GetAll(false);
            //return _context.Employees.AsNoTracking().ToList().AsEnumerable();                                                
        }

        public override IEnumerable<Employee> GetList(Func<Employee, bool> where, params Expression<Func<Employee,object>>[] navigationProperties) {

            if (_cache == null) { refreshCache(); } 
            try {                
                
                IQueryable<Employee> dbQuery = _cache.AsQueryable();                
                return dbQuery.Where(where).ToList<Employee>().AsEnumerable();                            

            } catch (Exception e) {
                Console.WriteLine(e.Message);
                //throw(e);
                return new List<Employee>().AsEnumerable();                
            }
        }
        public override int Count() {
            if (_cache == null) { refreshCache(); }             
            return _cache.Count;
        }
    }
}