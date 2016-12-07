using System;
using System.Reflection;
namespace ASTV.Helpers {

     public static class DebugHelpers {
        ///<summary>
        /// Converts LDAP sbyte[] to SID string.
        /// https://blogs.msdn.microsoft.com/alextch/2006/03/04/how-to-convert-objectsid-value-in-active-directory-from-binary-form-to-string-sddl-representation/
        /// </summary>
        public static string ListObject(object O) {
            string info = "";

            Type t = O.GetType();
            info += "Type: "+t.FullName;
            info += "Members:\n";
            if (O is string) {
                info += "Value: '"+(string)O+"'\n";
            } else {
            foreach( var s in t.GetProperties()) {
                info += s.Name + ":"; //+s.GetType().FullName;
                if (s.GetType().FullName == "System.Reflection.RuntimePropertyInfo") {
                    
                    try {
                        info += ":"+s.PropertyType.FullName;
                        var oo = s.GetValue(O, null);
                        if (oo == null) {
                            info += "= (null)";
                        } else {
                            info += "= "+oo.ToString();
                        }
                    } catch (Exception e) {
                        info += "Exception: "+e.Message;
                    }
                }
                info += "\n";
            }
            }

            return info;
        }
    }

}