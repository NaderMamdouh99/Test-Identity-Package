using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Test_Identity_Package.Models;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Test_Identity_Package.DTOS
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string? Address { get; set; }
        public double Age { get; set; } 
        public int? DepartmentId { get; set; } 
        public string? DepartmnetName { get; set; }
    }
}
