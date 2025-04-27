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
    public class MTagController : Controller
    {
        private readonly NIACRMContext _context;

        public MTagController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: MTag
        public async Task<IActionResult> Index()
        {
            return View(await _context.MTag.ToListAsync());
        }

        // GET: MTag/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mTag = await _context.MTag
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mTag == null)
            {
                return NotFound();
            }

            return View(mTag);
        }

        // GET: MTag/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MTag/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MTagName,MTagDescription")] MTag mTag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mTag);
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

            return View(mTag);
        }

        // GET: MTag/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mTag = await _context.MTag.FindAsync(id);
            if (mTag == null)
            {
                return NotFound();
            }
            return View(mTag);
        }

        // POST: MTag/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var mTagToUpdate = await _context.MTags
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mTagToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(mTagToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<MTag>(
                    mTagToUpdate, "",
                    m => m.MTagName, m => m.MTagDescription))
                {
                    try
                    {
                        // Update the MTag record in the database
                        _context.Update(mTagToUpdate);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Tag Updated Successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (MTag)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The MTag was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (MTag)databaseEntry.ToObject();
                            // Compare each field and provide feedback on changes
                            if (databaseValues.MTagName != clientValues.MTagName)
                                ModelState.AddModelError("MTagName", $"Current value: {databaseValues.MTagName}");
                            if (databaseValues.MTagDescription != clientValues.MTagDescription)
                                ModelState.AddModelError("MTagDescription", $"Current value: {databaseValues.MTagDescription}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            mTagToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
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

            return View(mTagToUpdate);
        }


        // GET: MTag/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mTag = await _context.MTag
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mTag == null)
            {
                return NotFound();
            }

            return View(mTag);
        }

        // POST: MTag/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var mTag = await _context.MTag.FindAsync(id);
                if (mTag != null)
                {
                    _context.MTag.Remove(mTag);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Tag deleted successfully!";
                    return Json(new
                    {
                        success = true,
                        message = "Tag deleted successfully!",
                        deletedId = id
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Tag not found.",
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
                        message = "This tag is currently assigned to one or more records and cannot be deleted."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "An error occurred while deleting. Details: " + (innerMessage ?? dbEx.Message)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error deleting tag: " + ex.Message,
                    deletedId = id
                });
            }
        }

        private bool MTagExists(int id)
        {
            return _context.MTag.Any(e => e.Id == id);
        }

        // For Adding MTag
        private SelectList MTagSelectList(string skip)
        {
            var mTagQuery = _context.MTags
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                // Convert the string to an array of integers
                // so we can make sure we leave them out of the data we download
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                mTagQuery = mTagQuery
                    .Where(m => !skipKeys.Contains(m.Id));
            }
            return new SelectList(mTagQuery.OrderBy(d => d.MTagName), "Id", "MTagName");
        }

        [HttpGet]
        public JsonResult GetMTags(string skip)
        {
            return Json(MTagSelectList(skip));
        }

    }
}
