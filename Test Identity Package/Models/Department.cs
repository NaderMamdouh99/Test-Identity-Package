using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Test_Identity_Package.Models
{
    public class Department
    {
        
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [DefaultValue(false)] 
        public bool isDeleted { get; set; }
        public List<Employee> Employees { get; set; }

    }
}
