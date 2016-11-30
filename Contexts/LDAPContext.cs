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
        IList<TEntity> Search(int scope, string filter, IDictionary<string, KeyValuePair<string, Type>> map);
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
        static public  IDictionary<string, KeyValuePair<string, Type>> DefaultMap() {
            Console.WriteLine("HERE!");
            Type t = typeof(TEntity);
            Console.WriteLine(typeof(TEntity).Name);
            var Dict = new Dictionary<string, KeyValuePair<string, Type>>();
            var sourceFields =  t.GetProperties();
            foreach(var field in sourceFields) {
                var kv = new KeyValuePair<string, Type>(field.Name, field.PropertyType);                
                Dict.Add(field.Name, kv);
                Console.WriteLine("kv {0}", kv.ToString());
            }
            return Dict;
        } 
        IDictionary<string, KeyValuePair<string, Type>> _attributeMap; // attribute to query -> mapped to property of type type in class.        
        public LDAPContext(LdapServerConfiguration configuration, ILoggerFactory loggerFactory):base(configuration, loggerFactory) {
            this._attributeMap = DefaultMap();
            foreach(var kv in _attributeMap) {
                Console.WriteLine("{0} => {1}:{2}", kv.Key, kv.Key, kv.Value.ToString());
            }
        }

        // we have POCO class. We need to map its parameters to attribs.

        private TEntity MapLdapEntry(LdapEntry entry,  IDictionary<string, KeyValuePair<string, Type>> map) {
            
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
                        
                        if (kv.Value == typeof(System.String)) {
                            string s = attribute.StringValue;       
                            p.SetValue(result, s, null);                
                        } else if (kv.Value == typeof(System.String[])) {
                            String[] s =   attribute.StringValueArray;
                            p.SetValue(result, s, null);
                        } 
                    }
                }

            }
            Console.WriteLine(result.Serialize(null));
            return result;
        }

        public virtual IList<TEntity> Search(int scope, string filter ) {            
            return Search(scope, filter, this._attributeMap);
        }


        public virtual IList<TEntity> Search(int scope, string filter, IDictionary<string, KeyValuePair<string, Type>> map ) {

            if (map == null) throw new ArgumentNullException();

            IList<TEntity> results = new List<TEntity>();
            
            // basic search
            string[] attrs = map.Keys.ToArray();
            Console.WriteLine("ATTRS {0}", attrs.Serialize(null));

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