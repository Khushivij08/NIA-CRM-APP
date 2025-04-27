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
    public class NAICSCodesController : Controller
    {
        private readonly NIACRMContext _context;

        public NAICSCodesController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: NAICSCodes
        public async Task<IActionResult> Index()
        {
            var naicsCodes = await _context.NAICSCodes.ToListAsync();
            ViewData["records"] = $"Records Found: {naicsCodes.Count()}";

            return View(naicsCodes);
        }

        // GET: NAICSCodes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nAICSCode = await _context.NAICSCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nAICSCode == null)
            {
                return NotFound();
            }

            return View(nAICSCode);
        }

        // GET: NAICSCodes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NAICSCodes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Description")] NAICSCode nAICSCode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nAICSCode);
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

            return View(nAICSCode);
        }

        // GET: NAICSCodes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nAICSCode = await _context.NAICSCodes.FindAsync(id);
            if (nAICSCode == null)
            {
                return NotFound();
            }
            return View(nAICSCode);
        }

        // POST: NAICSCodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Description")] NAICSCode nAICSCode)
        {
            if (id != nAICSCode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nAICSCode);
                    TempData["Success"] = "NAICS Code Updated Successfully";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NAICSCodeExists(nAICSCode.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(nAICSCode);
        }

        // GET: NAICSCodes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nAICSCode = await _context.NAICSCodes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nAICSCode == null)
            {
                return NotFound();
            }

            return View(nAICSCode);
        }

        // POST: NAICSCodes/Delete/5
        // POST: NaicsCode/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var naicsCode = await _context.NAICSCodes.FindAsync(id);
                if (naicsCode != null)
                {
                    _context.NAICSCodes.Remove(naicsCode);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "NAICS Code deleted successfully!";
                    return Json(new
                    {
                        success = true,
                        message = "NAICS Code deleted successfully!",
                        deletedId = id
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "NAICS Code not found.",
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
                        message = "This NAICS Code is currently assigned to one or more records and cannot be deleted."
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
                    message = "Error deleting NAICS Code: " + ex.Message,
                    deletedId = id
                });
            }
        }


        private bool NAICSCodeExists(int id)
        {
            return _context.NAICSCodes.Any(e => e.Id == id);
        }

        // For Adding NAICSCode
        private SelectList NAICSCodeSelectList(string skip)
        {
            var naicsCodeQuery = _context.NAICSCodes
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                // Convert the string to an array of integers
                // so we can make sure we leave them out of the data we download
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                naicsCodeQuery = naicsCodeQuery
                    .Where(n => !skipKeys.Contains(n.Id));
            }
            return new SelectList(naicsCodeQuery.OrderBy(d => d.Code), "Id", "Code");
        }

        [HttpGet]
        public JsonResult GetNAICSCode(string skip)
        {
            return Json(NAICSCodeSelectList(skip));
        }

    }
}
