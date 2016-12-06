using System;
using System.Collections;
using System.Collections.Generic;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Principal;
using System.Linq;
using ASTV.Extenstions;

namespace ASTV.Services {

    // should add queryable support to search using LINQ, however this is huge work and I have no time.

    
    public interface ILDAPContext {
        IList<IDictionary<string, object[]>> Search(int scope, string filter, string[] attrs );        
    }

    public interface ILDAPContext<TEntity> {
        IList<TEntity> Search(int scope, string filter );                
        IList<TEntity> Search(int scope, string filter, IDictionary<string, KeyValuePair<string, MethodInfo>> map );
    }

    public class LdapServerConfiguration
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string BaseDn { get; set; }
        public string RootUserDn { get; set; }
        public string RootUserPassword { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
    }

    public class LDAPContext: ILDAPContext {
        protected LdapServerConfiguration _configuration;
        protected ILogger _logger;
        
        public LDAPContext(LdapServerConfiguration configuration, ILoggerFactory loggerFactory) {
            _configuration = configuration; 
            _logger = loggerFactory.CreateLogger(this.GetType().Name);                                                
        }

        public LdapConnection GetConnection(bool connect) {
            
            var ldapConnection = new LdapConnection();
            if (connect) {
                try {
                     ldapConnection.Connect(_configuration.ServerAddress, _configuration.ServerPort);
                     ldapConnection.Bind(_configuration.RootUserDn, _configuration.RootUserPassword);
                } catch (Exception e) {
                    _logger.LogError(e.StackTrace);
                    throw e;                    
                }
            }   
            return ldapConnection;            
        }

        public LdapConnection GetConnection() { return GetConnection(false); }

        // TODO, add maxResults in configuration, search attribs.
        public virtual IList<IDictionary<string, object[]>> Search(int scope, string filter, string[] attrs ) {

            IList<IDictionary<string, object[]>> results = new List<IDictionary<string, object[]>>();
            
            // basic search

            using (var ldapConnection = new LdapConnection()) {
                 try {
                     LdapConstraints lr = new LdapConstraints();
                     
                     ldapConnection.Connect(_configuration.ServerAddress, _configuration.ServerPort);
                     ldapConnection.Bind(_configuration.RootUserDn, _configuration.RootUserPassword);
                     
                     // search.                     
                     LdapSearchConstraints lsconstr = new LdapSearchConstraints();
                     lsconstr.MaxResults = 1000; // Actual limit imposed by LDAP server.  
                     
                     LdapSearchResults lsc=ldapConnection.Search(	_configuration.SearchBase,
												scope,
												filter,
												attrs,
												false,
                                                lsconstr);
                    
                     while (lsc.hasMore()) {
                        LdapEntry nextEntry = null;
                        
                        try 
                        {
                            nextEntry = lsc.next();
                        }
                        catch(LdapException e1) 
                        {                         
                            // If limit is exceeded then should get last value and start search from next value. However, its too complicated to solve now..   
                            _logger.LogError(e1.Message + ": "+ e1.StackTrace);
                            
                            // Exception is thrown, go for next entry
                            continue;
                        }
                        
                        LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
				        IEnumerator ienum=attributeSet.GetEnumerator();

                        // create result object
                        IDictionary<string, object[]> result = new Dictionary<string, object[]>();

                        // iterate trhough attributes..
                        while(ienum.MoveNext()) {
                            LdapAttribute attribute=(LdapAttribute)ienum.Current;
                            string attributeName = attribute.Name;
                            result.Add(attribute.Name, attribute.ByteValueArray);
                        }

                        results.Add(result);

                     }

                     ldapConnection.Disconnect();
                } catch (Exception e) {
                    _logger.LogError(e.StackTrace);                    
                }
            }               
            return results;
        }
        
    }

    public class LDAPContext<TEntity> : LDAPContext, ILDAPContext<TEntity> where TEntity : class, new() 
    {
        static public  IDictionary<string, KeyValuePair<string, MethodInfo>> DefaultMap() {            
            Type t = typeof(TEntity);            
            var Dict = new Dictionary<string, KeyValuePair<string, MethodInfo>>();
            var sourceFields =  t.GetProperties();
            foreach(var field in sourceFields) {
                var kv = new KeyValuePair<string, MethodInfo>(field.Name, null);                
                Dict.Add(field.Name, kv);
                
            }
            return Dict;
        } 
        IDictionary<string, KeyValuePair<string, MethodInfo>> _attributeMap; // attribute to query -> mapped to property of type type in class.        
        public LDAPContext(LdapServerConfiguration configuration, ILoggerFactory loggerFactory):base(configuration, loggerFactory) {
            this._attributeMap = DefaultMap();           
        }

        // we have POCO class. We need to map its parameters to attribs.
        // TODO: create mapping upon creating of ldap search so it is compiled and thus faster.
        // TODO: cache compiled mappings in static class cache
        private TEntity MapLdapEntry(LdapEntry entry,  IDictionary<string, KeyValuePair<string, MethodInfo>> map) {
            // If method is defined, use this to convert value to required type.
            if (entry == null) return null;
            if (map == null) throw new ArgumentNullException();

            LdapAttributeSet attributeSet = entry.getAttributeSet();
            IEnumerator ienum=attributeSet.GetEnumerator();
            
            // create result object
            TEntity result = new TEntity();

            var destProps = typeof(TEntity).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            // iterate trhough attributes..
            while(ienum.MoveNext()) {
                LdapAttribute attribute=(LdapAttribute)ienum.Current;
                string attributeName = attribute.Name;
                               
                // look up if we have this mapped
                if (map.ContainsKey(attributeName)) {
                    var kv = map[attributeName];
                    if (destProps.Any(x => x.Name == kv.Key)) {
                        var p = destProps.First(x => x.Name == kv.Key);
                        // get destination property type
                        var tt = p.PropertyType;

                        var mi = kv.Value;
                        if (mi == null) {
                            if (p.PropertyType.IsAssignableFrom(typeof(System.String))) {
                                string s = attribute.StringValue;       
                                p.SetValue(result, s, null);
                            } else if (p.PropertyType.IsAssignableFrom(typeof(System.String[]))) {
                                string[] s = attribute.StringValueArray;       
                                p.SetValue(result, s, null);
                            } else {
                                string Error = string.Format("Cannot convert source attribute {0} type to {1}", attribute.Name, tt.FullName);                                 
                                throw new Exception(Error);
                            }
                        } else {
                            // we have some method to invoke.
                            if (!p.PropertyType.IsAssignableFrom(mi.ReturnType) ) {
                                string Error = string.Format("Cannot invoke method with unassignable return type {0}({1}):{2} type to {3}", 
                                    mi.Name, attribute.Name, mi.ReturnType.FullName, tt.FullName); 
                                
                                throw new Exception(Error);
                            }

                            // invoke method.
                            // Find suitable signature.
                            // first parameter of method must be one we can use
                            // there should be no other parameters..
                            if (mi.GetParameters().Count() != 1) {
                                string Error = string.Format("Parameter count mismatch, expecting function with one input!");                                 
                                throw new Exception(Error);
                            }

                            var pi = mi.GetParameters().First();
                            // parameter type can be either String, String[], ByteValue or ByteValueArray
                            object[] ip = new object[1];

                            if (pi.ParameterType == typeof(System.String)) {
                                ip[0] = attribute.StringValue;                                
                            } else if (pi.ParameterType == typeof(System.String[])) {
                                 ip[0] = attribute.StringValueArray;
                            } else if (pi.ParameterType == typeof(System.SByte[])) {
                                ip[0] = attribute.ByteValue;
                            } else if (pi.ParameterType == typeof(System.SByte[][])) {
                                ip[0] = attribute.ByteValueArray;
                            } else {
                                string Error = string.Format("Unable to find suitable input parameter."); 
                                
                                throw new Exception(Error);
                            }
                            try {
                                var v = mi.Invoke(null, ip);
                                p.SetValue(result, v, null);
                            } catch (Exception e) {                                
                                throw e;
                            }
                        }
                         
                    }
                }

            }           
            return result;
            
        }
       

        public virtual IList<TEntity> Search(int scope, string filter ) {            
            return Search(scope, filter, this._attributeMap);
        }
       
         public virtual IList<TEntity> Search(int scope, string filter, IDictionary<string, KeyValuePair<string, MethodInfo>> map ) {

            if (map == null) throw new ArgumentNullException();

            IList<TEntity> results = new List<TEntity>();
            
            // basic search
            string[] attrs = map.Keys.ToArray();            

            using (var ldapConnection = new LdapConnection()) {
                 try {
                     LdapConstraints lr = new LdapConstraints();
                     
                     ldapConnection.Connect(_configuration.ServerAddress, _configuration.ServerPort);
                     ldapConnection.Bind(_configuration.RootUserDn, _configuration.RootUserPassword);
                     
                     // search.                     
                     LdapSearchConstraints lsconstr = new LdapSearchConstraints();
                     lsconstr.MaxResults = 1000; // Actual limit imposed by LDAP server.  
                     
                     LdapSearchResults lsc=ldapConnection.Search(	_configuration.SearchBase,
												scope,
												filter,
												attrs,
												false,
                                                lsconstr);
                    
                     while (lsc.hasMore()) {
                        LdapEntry nextEntry = null;
                        
                        try 
                        {
                            nextEntry = lsc.next();
                        }
                        catch(LdapException e1) 
                        {                         
                            // If limit is exceeded then should get last value and start search from next value. However, its too complicated to solve now..   
                            _logger.LogError(e1.Message + ": "+ e1.StackTrace);
                            
                            // Exception is thrown, go for next entry
                            continue;
                        }
                        
                        try {
                            TEntity result = MapLdapEntry(nextEntry, map);
                            results.Add(result);                                
                        } catch ( Exception e) {
                            _logger.LogError(e.Message + ": "+ e.StackTrace);
                            throw e;
                        }
                    
                     }

                     ldapConnection.Disconnect();
                } catch (Exception e) {
                    _logger.LogError(e.StackTrace);                    
                }
            }               
            return results;
        }


    }
}