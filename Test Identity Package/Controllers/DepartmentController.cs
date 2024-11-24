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
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public DepartmentController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetDepartment()
        {
            var _departments = await (from emp in _context.Departments.AsNoTracking()
                                        where !emp.isDeleted
                                        select new DepartmentDTO
                                        {
                                            Id = emp.Id,
                                            Name = emp.Name, 
                                            Description = emp.Description,
                                            Employees  = emp.Employees.ToList(),
                                        }).ToListAsync();
            if (_departments.Any())
                return Ok(_departments);
            return NotFound("Not Found Department !!");
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            // Validate the ID parameter
            if (id <= 0)
                return BadRequest("Invalid Department ID.");

            try
            {
                // Query the database with null checks
                var _departments = await (from emp in _context.Departments.AsNoTracking()
                                            where emp.Id == id && !emp.isDeleted
                                            select new DepartmentDTO
                                            {
                                                Id = emp.Id,
                                                Name = emp.Name,
                                                Description = emp.Description,
                                                Employees = emp.Employees.ToList(),
                                            }).FirstOrDefaultAsync();

                // Return result or not found message
                if (_departments != null)
                    return Ok(_departments);

                return NotFound($"Departments with ID {id} not found.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving departments: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddDepartment(DepartmentDTO departmentDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _departments = new Department()
                    {
                        Name = departmentDTO.Name,
                        Description = departmentDTO.Description,  
                    };
                    await _context.AddAsync(_departments);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest(departmentDTO);

            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving department: {ex.Message}");
            }

        }

        [HttpPut]
        public async Task<IActionResult> EditDepartment(DepartmentDTO departmentDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {  
                    var _department = await _context.Departments.Where(x=>x.Id == departmentDTO.Id && !x.isDeleted)
                                                                .ExecuteUpdateAsync(x => x.SetProperty(s => s.Name, departmentDTO.Name)
                                                                                          .SetProperty(s => s.Description, departmentDTO.Description));
                    if (_department > 0)
                        return Ok(); 
                }
                return BadRequest(departmentDTO);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving department: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid Department ID.");
                var _department = await _context.Departments.Where(x => x.Id == id && !x.isDeleted)
                                                            .ExecuteUpdateAsync(x => x.SetProperty(s => s.isDeleted, true));
                if (_department > 0)
                    return Ok();
                return NotFound($"Department with ID {id} not found.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving department: {ex.Message}");
            }
        }
    }
}
