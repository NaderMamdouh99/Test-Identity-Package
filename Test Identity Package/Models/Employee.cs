using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test_Identity_Package.Models
{
    public class Employee
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Address { get; set; }
        public double Age { get; set; }
        [DefaultValue(false)]
        public bool isDeleted { get; set; }
        [ForeignKey("Department")]
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

    }
}
