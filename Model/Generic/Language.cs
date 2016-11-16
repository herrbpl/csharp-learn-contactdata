using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace ASTV.Models.Generic {
    public class Language: IEntityBase {
        //[JsonIgnore]
        public int Id { get; set; }
        [MaxLength(3)]
        public string Code { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
    }
}