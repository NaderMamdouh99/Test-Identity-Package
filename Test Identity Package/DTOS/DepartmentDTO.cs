using System.ComponentModel.DataAnnotations;
using Test_Identity_Package.Models;

namespace Test_Identity_Package.DTOS
{
    public class DepartmentDTO
    {

        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        public List<Employee>? Employees { get; set; }
    }
}
