
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ASTV.Models.Employee;
using ASTV.Services;
using ASTV.Helpers;

using Novell.Directory.Ldap;

namespace ASTV.Services {

    public interface IEmployeeRepository: IEntityBaseRepositoryReadonly<Employee, EmployeeContext> {}
    
    // TODO: Add logger DI
    public class EmployeeRepository: EntityBaseRepositoryReadOnly<Employee, EmployeeContext>, IEmployeeRepository                            
    {
        // It is highly unusual, if employee list changes between lifetime of repository object as repo is shortlived.
        protected IDictionary<string,Employee> _cache; 
        protected ILDAPContext<Employee> _ldap;
        
        private IDictionary<string, KeyValuePair<string, MethodInfo>> _map;
        public EmployeeRepository(EmployeeContext context, ILDAPContext<Employee> ldapcontext, ILoggerFactory loggerFactory) 
            : base(context)
        {
            _ldap = ldapcontext;            
            _logger = loggerFactory.CreateLogger(this.GetType().FullName);
            _logger.LogInformation("Employee repository created!");
            createMap();  
        } 

         public EmployeeRepository(EmployeeContext context, ILDAPContext<Employee> ldapcontext) 
            : base(context)
        {
            var  loggerFactory = new LoggerFactory()
            .AddConsole()
            .AddDebug();

            _ldap = ldapcontext;            
            _logger = loggerFactory.CreateLogger(this.GetType().FullName);
            _logger.LogInformation("Employee repository created!");  
            createMap();            
        } 

        private void createMap() {
            _map = new Dictionary<string, KeyValuePair<string, MethodInfo>>();            
            _map.Add("employeeID", new KeyValuePair<string, MethodInfo>("EmployeeId", null));
            _map.Add("displayName", new KeyValuePair<string, MethodInfo>("Name", null));
            _map.Add("sAMAccountName", new KeyValuePair<string, MethodInfo>("sAMAccountName", null));
            _map.Add("objectSid", new KeyValuePair<string, MethodInfo>("SID", typeof(SecurityHelpers).GetMethod("LdapSIDToString")));
            _map.Add("extensionAttribute1", new KeyValuePair<string, MethodInfo>("RegistrationCode", null));
        }

        protected void refreshCache() {
            try {                
                var elSQL = _context.Employees.FromSql(
                  "select distinct cast(employeeid as int) as Id, name as Name, ltrim(rtrim(employeeid)) as EmployeeId from v_sync_iscala_ad_userdata"
                   ).AsNoTracking().ToList().ToDictionary(p => p.EmployeeId);
                   

                // get ldap information
                var elLDAP = _ldap.Search(LdapConnection.SCOPE_SUB, "(&(objectCategory=user)(objectClass=user)(employeeID=*))", _map);

                foreach(var emp in elLDAP) {
                    if (elSQL.ContainsKey(emp.EmployeeId)) {
                        elSQL[emp.EmployeeId].sAMAccountName = emp.sAMAccountName;
                        elSQL[emp.EmployeeId].RegistrationCode = emp.RegistrationCode;
                        elSQL[emp.EmployeeId].SID = emp.SID;
                    }
                }
                
                this._cache = elSQL;

            } catch (Exception e) {
                Console.WriteLine(e.Message);
                this._cache = new Dictionary<string,Employee>();
            }                
             
        }

        public  IEnumerable<Employee> GetAll(bool forceRefresh) {      
            
            // TODO: Add actual SQL string to appSettings.json
            try {

                if (forceRefresh || _cache == null) { refreshCache(); }
                return this._cache.Values.AsEnumerable();
                
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                if (_cache != null) {
                    return _cache.Values.AsEnumerable();
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
                
                IQueryable<Employee> dbQuery = _cache.Values.AsQueryable();                
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