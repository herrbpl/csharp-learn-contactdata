using System.ComponentModel.DataAnnotations;
namespace ASTV.Models.Generic {
    public class EducationLevel : IEntityBase {
        public int Id { get; set; }
        [MaxLength(3)]
        public string Code { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
    }
}