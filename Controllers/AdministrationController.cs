using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Data;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Controllers
{
    [Authorize(Roles = "ChiefAccountant, Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext context;
        public AdministrationController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.context = context;
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // We just need to specify a unique role name to create a new role
                ApplicationRole identityRole = new ApplicationRole
                {
                    Name = model.RoleName
                };

                // Saves the role in the underlying AspNetRoles table
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            //var users = from user in userManager.Users
            //            where user.IsActive == true
            //            select user;
            var users = userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await roleManager.Roles.FirstOrDefaultAsync(u => u.Id == id);
            if (role == null)
            {
                return RedirectToAction("ListRoles");
            }
            else
            {
                await roleManager.DeleteAsync(role);
                return RedirectToAction("ListRoles");
            }

        }
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {

                user.IsActive = false;
                await userManager.UpdateAsync(user);
                return RedirectToAction("ListUsers");
            }

        }
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {

                user.IsActive = true;
                await userManager.UpdateAsync(user);
                return RedirectToAction("ListUsers");
            }

        }

        [HttpGet]
        public async Task<IActionResult> EditRole(int id)
        {
            // Find the role by Role ID
            var role = await roleManager.Roles.FirstOrDefaultAsync(u => u.Id == id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };


            foreach (var user in userManager.Users.ToList())
            {

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.Roles.FirstOrDefaultAsync(u => u.Id == model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;

                // Update the Role using UpdateAsync
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(int roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.Roles.FirstOrDefaultAsync(u => u.Id == roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, int roleId)
        {
            var role = await roleManager.Roles.FirstOrDefaultAsync(u => u.Id == roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == model[i].UserId);

                IdentityResult result = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        public async Task<IActionResult> CreateParticular()
        {
            var particulars = await context.Particulars.ToListAsync();
            return View(new CreateParticularViewModel { particular = particulars.OrderBy(x => x.Id).ToList() });
        }
        [HttpPost]
        public async Task<IActionResult> CreateParticular(CreateParticularViewModel particulars)
        {
            if (particulars.ParticularName != null)
            {
                var particular = new Particular
                {
                    Name = particulars.ParticularName
                };
                await context.AddAsync(particular);
                await context.SaveChangesAsync();

                return Json("1|Particular Created successfully!");
                
            }
            else
                return Json("2|Error creating Particular.");
        }
        
        public async Task<IActionResult> DeleteParticular(int Id)
        {
            var particular = await context.Particulars.FindAsync(Id);
            context.Particulars.Remove(particular);
            await context.SaveChangesAsync();
            return RedirectToAction("CreateParticular");
        }
    }

}
