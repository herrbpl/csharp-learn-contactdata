using System.Collections.Generic;
using Newtonsoft.Json;

namespace ASTV.Extenstions {

    public static class EntitySerialize {
            public static string Serialize<T>(this T source, IList<string> exclude) {
                return Newtonsoft.Json.JsonConvert.SerializeObject(source,Formatting.Indented,
                            new JsonSerializerSettings {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                //, 
                                //PreserveReferencesHandling = PreserveReferencesHandling.Objects 
                            });
            }
            public static void DeSerialize<T, TU>(this T source, TU dest, string value, IList<string> exclude) {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                try {          
                    var Data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);

                    Data.CopyPropertiesTo(dest, new List<string> { "Serialized", "Id"});
                    
                } catch ( System.Exception e) {                  
                    throw(e);
                }
            }
    }
}