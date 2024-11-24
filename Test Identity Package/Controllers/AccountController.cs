using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Test_Identity_Package.DTOS;
using Test_Identity_Package.Models;

namespace Test_Identity_Package.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ApplicationContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,IConfiguration config ,ApplicationContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
        }

        // Register 
        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration(RegisterDTO registerDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _userCheck = await _context.Users.Where(x=>x.UserName == registerDTO.UserName ||  x.Email == registerDTO.Email ).FirstOrDefaultAsync();
                    if (_userCheck != null)
                        return BadRequest("User Alerady Exist ..!");
                    ApplicationUser user = new ApplicationUser();
                    user.Id = Guid.NewGuid().ToString();
                    user.Email = registerDTO.Email;
                    user.UserName = registerDTO.UserName;
                    user.Firstname = registerDTO.FirstName;
                    user.LastName = registerDTO.LastName;
                    user.Address  = registerDTO.Address;
                    IdentityResult result =  await _userManager.CreateAsync(user,registerDTO.Password);
                    if (result.Succeeded) 
                    {
                        return Ok("Account Created Successfuly");                                           
                    }
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                } 
                return BadRequest(ModelState);
            }
            catch (Exception ex) 
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving Account: {ex.Message}");
            } 
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(loginDTO.UserName);
                    if (user != null)
                    {
                        bool found =  await _userManager.CheckPasswordAsync(user, loginDTO.Password);
                        if (found) 
                        {
                            // Claims
                            #region Claims 
                            var _claims = new List<Claim>();
                            _claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                            _claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                            _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                            var userRoles = await _userManager.GetRolesAsync(user);
                            foreach (var role in userRoles)
                            {
                                _claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                            var userCalims = await (from c in _context.UserClaims.AsNoTracking()
                                                    where user.Id == c.UserId
                                                    select c).ToListAsync();
                            foreach (var item in userCalims)
                            {
                                _claims.Add(new Claim(ClaimTypes.UserData,$"Key:{item.ClaimType},Value:{item.ClaimValue}")); // That is Key and Value 
                                //_claims.Add(new Claim(ClaimTypes.UserData,item.ClaimType)); // that is Key Only 
                            }
                            #endregion
                            //Security Key Bytes 
                            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
                            // Signture For Token
                            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                            // create Token 
                            JwtSecurityToken token = new JwtSecurityToken(
                            issuer: _config["JWT:ValidIssuer"],
                            audience: _config["JWT:ValidAudiance"],
                            expires: loginDTO.RemmberMe ? DateTime.Now.AddDays(15): DateTime.Now.AddDays(1),
                            claims: _claims,
                            signingCredentials: credentials
                            );
                            return Ok(new
                            {
                                Token = new JwtSecurityTokenHandler().WriteToken(token),
                                expiration = token.ValidTo
                            });
                        }
                    }
                    ModelState.AddModelError("", "User Name Or Password Not Valid !");
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex) 
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving Login: {ex.Message}");
            } 
        }

        [HttpPost]
        [Route("AddClaims")]
        [Authorize]
        public async Task<IActionResult> AddClaims(AddClaimsDTO addClaimsDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userClaims = await(from c in _context.UserClaims
                                           where c.UserId == addClaimsDTO.UserId 
                                           select new ApplicationUserClaimsDTO
                                           {
                                              UserId = c.UserId,
                                              ClaimsType = c.ClaimType,
                                              ClaimsValue = c.ClaimValue
                                           }).ToListAsync();
                    var userId = addClaimsDTO.UserId;
                    if (userClaims == null)
                        return BadRequest("User No Found !!");
                    // The User Dosen't Have Claims and I Will Add the First Claim
                    if (userClaims.Count == 0)
                    {
                        foreach (var i in addClaimsDTO.Claims)
                        {
                            var _newUserClaims = new ApplicationUserClaims()
                            {
                                UserId = userId,
                                ClaimType = i,
                            };
                            await _context.UserClaims.AddAsync(_newUserClaims);
                        }
                    }
                    // The User Has Claims  
                    else
                    {
                        foreach (var i in addClaimsDTO.Claims)
                        {
                            var check = userClaims.Where(X => X.ClaimsType == i).FirstOrDefault();
                            if (check == null)
                            {
                                var _newUserClaims = new ApplicationUserClaims()
                                {
                                    UserId = userId,
                                    ClaimType = i,
                                };
                                await _context.UserClaims.AddAsync(_newUserClaims);
                            }
                        }
                    } 
                    var _save =  await _context.SaveChangesAsync();
                    // Create New Token With New Claims 
                    if(_save > 0)
                    {
                        ApplicationUser user = await _userManager.FindByIdAsync(userId); 
                        // Claims
                        #region Claims 
                        var _claims = new List<Claim>();
                        _claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        _claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var userRoles = await _userManager.GetRolesAsync(user);
                        foreach (var role in userRoles)
                        {
                            _claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var userCalims = await (from c in _context.UserClaims.AsNoTracking()
                                                where user.Id == c.UserId
                                                select c).ToListAsync();
                        foreach (var item in userCalims)
                        {
                            _claims.Add(new Claim(ClaimTypes.UserData, $"Key:{item.ClaimType},Value:{item.ClaimValue}")); // That is Key and Value 
                           //_claims.Add(new Claim(ClaimTypes.UserData,item.ClaimType)); // that is Key Only 
                        }
                        #endregion
                        //Security Key Bytes 
                        SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
                        // Signture For Token
                        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        // create Token 
                        JwtSecurityToken token = new JwtSecurityToken(
                        issuer: _config["JWT:ValidIssuer"],
                        audience: _config["JWT:ValidAudiance"],
                        expires: addClaimsDTO.RemmberMe ? DateTime.Now.AddDays(15) : DateTime.Now.AddDays(1),
                        claims: _claims,
                        signingCredentials: credentials
                        );
                        return Ok(new
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }  
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex) 
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving AddClaims: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("DeleteClaims")]
        [Authorize]
        public async Task<IActionResult> DeleteClaims(AddClaimsDTO addClaimsDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get Claims From DataBase By UserId  
                    var userClaims = await (from c in _context.UserClaims
                                            where c.UserId == addClaimsDTO.UserId
                                            select new ApplicationUserClaimsDTO
                                            {
                                                UserId = c.UserId,
                                                ClaimsType = c.ClaimType,
                                                ClaimsValue = c.ClaimValue
                                            }).ToListAsync();
                    if (userClaims.Count == 0)
                        return Ok("The Use Already Not Has Claims !!");
                    var userId = addClaimsDTO.UserId;
                    // Loop In Claims To Delete Claims
                    foreach (var claim in addClaimsDTO.Claims)
                    {
                        var check = userClaims.Where(X => X.ClaimsType == claim).FirstOrDefault();
                        if (check != null)
                        { 
                           var _oldClaims =  await _context.UserClaims.Where(x=>x.UserId == userId && x.ClaimType == claim).FirstOrDefaultAsync();
                           _context.UserClaims.Remove(_oldClaims);
                        }
                    }
                    var _save = await _context.SaveChangesAsync();
                    // Create New Token With New Claims 
                    if (_save > 0)
                    {
                        ApplicationUser user = await _userManager.FindByIdAsync(userId);
                        // Claims
                        #region Claims 
                        var _claims = new List<Claim>();
                        _claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        _claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var userRoles = await _userManager.GetRolesAsync(user);
                        foreach (var role in userRoles)
                        {
                            _claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var userCalims = await (from c in _context.UserClaims.AsNoTracking()
                                                where user.Id == c.UserId
                                                select c).ToListAsync();
                        foreach (var item in userCalims)
                        {
                            _claims.Add(new Claim(ClaimTypes.UserData, $"Key:{item.ClaimType},Value:{item.ClaimValue}")); // That is Key and Value 
                                                                                                                          //_claims.Add(new Claim(ClaimTypes.UserData,item.ClaimType)); // that is Key Only 
                        }
                        #endregion
                        //Security Key Bytes 
                        SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
                        // Signture For Token
                        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        // create Token 
                        JwtSecurityToken token = new JwtSecurityToken(
                        issuer: _config["JWT:ValidIssuer"],
                        audience: _config["JWT:ValidAudiance"],
                        expires: addClaimsDTO.RemmberMe ? DateTime.Now.AddDays(15) : DateTime.Now.AddDays(1),
                        claims: _claims,
                        signingCredentials: credentials
                        );
                        return Ok(new
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"Error retrieving RevokeClaims: {ex.Message}");
            }
        }
    }
}
