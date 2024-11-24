using System.ComponentModel;

namespace Test_Identity_Package.DTOS
{
    public class ApplicationUserClaimsDTO
    {
        public string UserId { get; set; }
        public string? ClaimsType { get; set; }
        public string? ClaimsValue { get; set; } 
    }
}
