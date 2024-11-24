using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Identity_Package.DTOS;
using Test_Identity_Package.Models;

namespace Test_Identity_Package.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public EmployeeController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetEmployee()
        {
            var _employees = await (from emp in _context.Employees.AsNoTracking()
                                    where !emp.isDeleted
                                    select new EmployeeDTO
                                    {
                                        Id = emp.Id,
                                        Name = emp.Name,
                                        Address = emp.Address,
                                        Age = emp.Age,
                                        DepartmnetName = emp.Department.Name
                                    }).ToListAsync();
            if (_employees.Any())
                return Ok(_employees);
            return NotFound("Not Found Employees !!");
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            // Validate the ID parameter
            if (id <= 0)
                return BadRequest("Invalid Employee ID.");

            try
            {
                // Query the database with null checks
                var employee = await (from emp in _context.Employees.AsNoTracking()
                                      where emp.Id == id && !emp.isDeleted
                                      select new EmployeeDTO
                                      {
                                          Id = emp.Id,
                                          Name = emp.Name,
                                          Address = emp.Address,
                                          Age = emp.Age,
                                          DepartmnetName = emp.Department.Name
                                      }).FirstOrDefaultAsync();

                // Return result or not found message
                if (employee != null)
                    return Ok(employee);

                return NotFound($"Employee with ID {id} not found.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving employee: {ex.Message}");
            }
        }
         
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddEmployee(EmployeeDTO employeeDTO)
        {
            try
            {
                // Remove 'DepartmnetName' from ModelState validation
                ModelState.Remove(nameof(EmployeeDTO.DepartmnetName));

                if (ModelState.IsValid)
                {
                    var _employee = new Employee()
                    {
                        Name = employeeDTO.Name,
                        Address = employeeDTO.Address,
                        Age = employeeDTO.Age,
                        DepartmentId = employeeDTO?.DepartmentId,
                    };
                    await _context.AddAsync(_employee);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest(employeeDTO);

            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving employee: {ex.Message}");
            }

        }

        [HttpPut]
        public async Task<IActionResult> EditEmployee(EmployeeDTO employeeDTO)
        {
            try
            {
                // Remove 'DepartmnetName' from ModelState validation
                ModelState.Remove(nameof(EmployeeDTO.DepartmnetName));

                if (ModelState.IsValid)
                { 
                    var _employee = await _context.Employees.Where(x=>x.Id == employeeDTO.Id && !x.isDeleted)
                                                            .ExecuteUpdateAsync(x => x.SetProperty(s => s.Name, employeeDTO.Name)
                                                                                      .SetProperty(s => s.Address, employeeDTO.Address)
                                                                                      .SetProperty(s => s.Age, employeeDTO.Age)
                                                                                      .SetProperty(s => s.DepartmentId, employeeDTO.DepartmentId));
                    if (_employee > 0)
                        return Ok(); 
                }
                return BadRequest(employeeDTO);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving employee: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid Employee ID.");
                var _employee = await _context.Employees.Where(x => x.Id == id && !x.isDeleted)
                                                        .ExecuteUpdateAsync(x => x.SetProperty(s => s.isDeleted, true));
                if (_employee > 0)
                    return Ok();
                return NotFound($"Employee with ID {id} not found.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving employee: {ex.Message}");
            }
        }
    }
}
