using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.Data;
using NIA_CRM.Models;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class MembershipTypeController : Controller
    {
        private readonly NIACRMContext _context;

        public MembershipTypeController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: MembershipType
        public async Task<IActionResult> Index()
        {
            return View(await _context.MembershipTypes.ToListAsync());
        }

        // GET: MembershipType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipType == null)
            {
                return NotFound();
            }

            return View(membershipType);
        }

        // GET: MembershipType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MembershipType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TypeName,TypeDescr")] MembershipType membershipType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //Decide if we need to send the Validaiton Errors directly to the client
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                //Was an AJAX request so build a message with all validation errors
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                //Note: returning a BadRequest results in HTTP Status code 400
                return BadRequest(errorMessage);
            }

            return View(membershipType);
        }

        // GET: MembershipType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipTypes.FindAsync(id);
            if (membershipType == null)
            {
                return NotFound();
            }
            return View(membershipType);
        }

        // POST: MembershipType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var membershipTypeToUpdate = await _context.MembershipTypes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membershipTypeToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(membershipTypeToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<MembershipType>(
                    membershipTypeToUpdate, "",
                    m => m.TypeName, m => m.TypeDescr))
                {
                    try
                    {
                        // Update the MembershipType record in the database
                        _context.Update(membershipTypeToUpdate);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (MembershipType)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The MembershipType was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (MembershipType)databaseEntry.ToObject();
                            // Compare each field and provide feedback on changes
                            if (databaseValues.TypeName != clientValues.TypeName)
                                ModelState.AddModelError("TypeName", $"Current value: {databaseValues.TypeName}");
                            if (databaseValues.TypeDescr != clientValues.TypeDescr)
                                ModelState.AddModelError("TypeDescr", $"Current value: {databaseValues.TypeDescr}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            membershipTypeToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                            ModelState.Remove("RowVersion");
                        }
                    }
                    catch (DbUpdateException dex)
                    {
                        string message = dex.GetBaseException().Message;
                        ModelState.AddModelError("", $"Unable to save changes: {message}");
                    }
                }
            }

            return View(membershipTypeToUpdate);
        }


        // GET: MembershipType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membershipType = await _context.MembershipTypes
                .FirstOrDefaultAsync(m => m.Id == id);


            if (membershipType == null)
            {
                return NotFound();
            }

            return View(membershipType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var membershipType = await _context.MembershipTypes.FindAsync(id);
                if (membershipType != null)
                {
                    _context.MembershipTypes.Remove(membershipType);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Membership type deleted successfully!";
                    return Json(new
                    {
                        success = true,
                        message = "Membership type deleted successfully!",
                        deletedId = id
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Membership type not found.",
                    deletedId = id
                });
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message;

                if (innerMessage != null && innerMessage.Contains("FOREIGN KEY constraint failed"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "This membership type is currently assigned to one or more members and cannot be deleted."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "An error occurred while deleting. Details: " + innerMessage ?? dbEx.Message
                });
            }

            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error deleting membership type: " + ex.Message,
                    deletedId = id
                });
            }
        }


        private bool MembershipTypeExists(int id)
        {
            return _context.MembershipTypes.Any(e => e.Id == id);
        }

        // For Adding MembershipType
        private SelectList MembershipTypeSelectList(string skip)
        {
            var membershipTypeQuery = _context.MembershipTypes
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                // Convert the string to an array of integers
                // so we can make sure we leave them out of the data we download
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                membershipTypeQuery = membershipTypeQuery
                    .Where(m => !skipKeys.Contains(m.Id));
            }
            return new SelectList(membershipTypeQuery.OrderBy(d => d.TypeName), "Id", "TypeName");
        }

        [HttpGet]
        public JsonResult GetMembershipTypes(string skip)
        {
            return Json(MembershipTypeSelectList(skip));
        }

        


    }
}
