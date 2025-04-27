﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.ViewModels;

namespace NIA_CRM.Controllers
{

    [Authorize(Roles = "Admin")]
    public class UserRoleController : CognizantController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserRoleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await (from u in _context.Users
                               .OrderBy(u => u.UserName)
                               select new UserVM
                               {
                                   Id = u.Id,
                                   UserName = u.UserName
                               }).ToListAsync();
            foreach (var u in users)
            {
                var user = await _userManager.FindByIdAsync(u.Id);
                u.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
                //Note: we needed the explicit cast above because GetRolesAsync() returns an IList<string>
            }
            ;
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            var _user = await _userManager.FindByIdAsync(id);//IdentityRole
            if (_user == null)
            {
                return NotFound();
            }
            UserVM user = new UserVM
            {
                Id = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            };
            PopulateAssignedRoleData(user);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, string[] selectedRoles)
        {
            var _user = await _userManager.FindByIdAsync(Id);
            if (_user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // If the current user is not found, return an error to avoid crashes
            if (currentUser == null)
            {
                ModelState.AddModelError(string.Empty, "Error retrieving the current user.");
                PopulateAssignedRoleData(new UserVM
                {
                    Id = _user.Id,
                    UserName = _user.UserName,
                    UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
                });
                return View();
            }

            var isAdmin = await _userManager.IsInRoleAsync(_user, "Admin");

            // Prevent an admin from removing their own "Admin" role
            if (_user.Id == currentUser.Id && isAdmin && !selectedRoles.Contains("Admin"))
            {
                ModelState.AddModelError(string.Empty, "You cannot remove the Admin role from your own account.");
                PopulateAssignedRoleData(new UserVM
                {
                    Id = _user.Id,
                    UserName = _user.UserName,
                    UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
                });
                return View();
            }

            try
            {
                await UpdateUserRoles(selectedRoles, new UserVM
                {
                    Id = _user.Id,
                    UserName = _user.UserName,
                    UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
                });
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes.");
            }

            PopulateAssignedRoleData(new UserVM
            {
                Id = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            });

            return View();
        }


        private void PopulateAssignedRoleData(UserVM user)
        {//Prepare checkboxes for all Roles
            var allRoles = _context.Roles;
            var currentRoles = user.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        private async Task UpdateUserRoles(string[] selectedRoles, UserVM userToUpdate)
        {
            var UserRoles = userToUpdate.UserRoles;//Current roles use is in
            var _user = await _userManager.FindByIdAsync(userToUpdate.Id);//IdentityUser

            if (selectedRoles == null)
            {
                //No roles selected so just remove any currently assigned
                foreach (var r in UserRoles)
                {
                    await _userManager.RemoveFromRoleAsync(_user, r);
                }
            }
            else
            {
                //At least one role checked so loop through all the roles
                //and add or remove as required

                //We need to do this next line because foreach loops don't always work well
                //for data returned by EF when working async.  Pulling it into an IList<>
                //first means we can safely loop over the colleciton making async calls and avoid
                //the error 'New transaction is not allowed because there are other threads running in the session'
                IList<IdentityRole> allRoles = _context.Roles.ToList<IdentityRole>();

                foreach (var r in allRoles)
                {
                    if (selectedRoles.Contains(r.Name))
                    {
                        if (!UserRoles.Contains(r.Name))
                        {
                            await _userManager.AddToRoleAsync(_user, r.Name);
                        }
                    }
                    else
                    {
                        if (UserRoles.Contains(r.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(_user, r.Name);
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
