using System;
using System.Collections;
using System.Collections.Generic;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;
using Microsoft.Extensions.Logging;

namespace ASTV.Services {

    // should add queryable support to search using LINQ, however this is huge work and I have no time.
    public interface ILDAPContext {
        LdapConnection GetConnection(bool connect);
        LdapConnection GetConnection();
        IList<IDictionary<string, object[]>> Search(int scope, string filter, string[] attrs );
        
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


}