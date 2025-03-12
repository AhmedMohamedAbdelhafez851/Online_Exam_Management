using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Models.UserDTO;

namespace OnlineExamSystem.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: ManageRoles
        public async Task<IActionResult> ManageRoles(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID cannot be empty.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                UserRoles = userRoles.ToList(),
                Roles = roles.Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    IsAssigned = userRoles.Contains(role.Name!)
                }).ToList()
            };

            return View(model);
        }

        // POST: ManageRoles
        [HttpPost]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || model.Roles == null)
            {
                return BadRequest("User ID or Roles cannot be null.");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToList();

            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError("", $"Failed to add role '{error.Description}'.");
                }
                return View(model);
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError("", $"Failed to remove role '{error.Description}'.");
                }
                return View(model);
            }

            return RedirectToAction("Index", "Users");
        }

        // GET: CreateRole
        public IActionResult CreateRole()
        {
            var model = new ManageRolesViewModel
            {
                Roles = _roleManager.Roles.Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!
                }).ToList()
            };
            return View(model);
        }

        // POST: CreateRole
        [HttpPost]
        public async Task<IActionResult> CreateRole(ManageRolesViewModel model)
        {
            model.Roles ??= new List<RoleDto>();

            if (!string.IsNullOrWhiteSpace(model.NewRoleName))
            {
                var newRole = new IdentityRole(model.NewRoleName);
                var result = await _roleManager.CreateAsync(newRole);

                if (result.Succeeded)
                {
                    model.Roles.Add(new RoleDto
                    {
                        Id = newRole.Id,
                        Name = newRole.Name!
                    });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            model.Roles = await _roleManager.Roles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name!
            }).ToListAsync();

            return View(model);
        }

        // GET: EditRole
        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Role ID cannot be empty.");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var model = new RoleDto
            {
                Id = role.Id,
                Name = role.Name!
            };

            return View(model);
        }

        // POST: EditRole
        [HttpPost]
        public async Task<IActionResult> EditRole(RoleDto model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    return NotFound("Role not found.");
                }

                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("CreateRole");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // POST: DeleteRole
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new { success = false, message = "Role ID cannot be empty." });
                }

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Json(new { success = false, message = "Role not found." });
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{role.Name}' deleted successfully.");
                    return Json(new { success = true, message = $"Role '{role.Name}' deleted successfully." });
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"Failed to delete role '{role.Name}': {errors}");
                return Json(new { success = false, message = $"Failed to delete role: {errors}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting role with ID {id}");
                return Json(new { success = false, message = "An unexpected error occurred while deleting the role." });
            }
        }
    }
}