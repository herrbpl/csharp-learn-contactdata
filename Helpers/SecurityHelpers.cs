using System.Security.Principal;
namespace ASTV.Helpers {

     public static class SecurityHelpers {
        ///<summary>
        /// Converts LDAP sbyte[] to SID string.
        /// https://blogs.msdn.microsoft.com/alextch/2006/03/04/how-to-convert-objectsid-value-in-active-directory-from-binary-form-to-string-sddl-representation/
        /// </summary>
        public static string LdapSIDToString(sbyte[] input) {
            byte[] byteData = new byte[input.Length];

            for(var i = 0; i<input.Length;i++) { 
                byteData[i] = (byte)input[i];
            }
            SecurityIdentifier sid = new SecurityIdentifier(byteData, 0);            
            return sid.ToString();
        }
    }

}    