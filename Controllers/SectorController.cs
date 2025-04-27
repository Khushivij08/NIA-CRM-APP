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
    public class SectorController : Controller
    {
        private readonly NIACRMContext _context;

        public SectorController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Sector
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sectors.ToListAsync());
        }

        // GET: Sector/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sector = await _context.Sectors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sector == null)
            {
                return NotFound();
            }

            return View(sector);
        }

        // GET: Sector/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sector/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SectorName,SectorDescription")] Sector sector)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sector);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sector Created Successfully";

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

            return View(sector);
        }

        // GET: Sector/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sector = await _context.Sectors.FindAsync(id);
            if (sector == null)
            {
                return NotFound();
            }
            return View(sector);
        }

        // POST: Sector/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var sectorToUpdate = await _context.Sectors
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sectorToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(sectorToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Sector>(
                    sectorToUpdate, "",
                    s => s.SectorName, s => s.SectorDescription))
                {
                    try
                    {
                        // Update the sector record in the database
                        _context.Update(sectorToUpdate);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Sector Updated Successfully";

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (Sector)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The sector was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (Sector)databaseEntry.ToObject();
                            // Compare each field and provide feedback on changes
                            if (databaseValues.SectorName != clientValues.SectorName)
                                ModelState.AddModelError("SectorName", $"Current value: {databaseValues.SectorName}");
                            if (databaseValues.SectorDescription != clientValues.SectorDescription)
                                ModelState.AddModelError("SectorDescription", $"Current value: {databaseValues.SectorDescription}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            sectorToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
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

            return View(sectorToUpdate);
        }


        // GET: Sector/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sector = await _context.Sectors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sector == null)
            {
                return NotFound();
            }

            return View(sector);
        }

        // POST: Sector/Delete/5
        // POST: Sector/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sector = await _context.Sectors.FindAsync(id);
                if (sector != null)
                {
                    _context.Sectors.Remove(sector);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Sector deleted successfully!";
                    return Json(new
                    {
                        success = true,
                        message = "Sector deleted successfully!",
                        deletedId = id
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Sector not found.",
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
                        message = "This sector is currently assigned to one or more records and cannot be deleted."
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
                    message = "Error deleting sector: " + ex.Message,
                    deletedId = id
                });
            }
        }


        private bool SectorExists(int id)
        {
            return _context.Sectors.Any(e => e.Id == id);
        }

        // For Adding Sector
        private SelectList SectorSelectList(string skip)
        {
            var sectorQuery = _context.Sectors
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                // Convert the string to an array of integers
                // so we can make sure we leave them out of the data we download
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                sectorQuery = sectorQuery
                    .Where(s => !skipKeys.Contains(s.Id));
            }
            return new SelectList(sectorQuery.OrderBy(d => d.SectorName), "Id", "SectorName");
        }

        [HttpGet]
        public JsonResult GetSectors(string skip)
        {
            return Json(SectorSelectList(skip));
        }

    }
}
