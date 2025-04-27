using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class ContactCancellationController : ElephantController
    {
        private readonly NIACRMContext _context;

        public ContactCancellationController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: ContactCancellation
        public async Task<IActionResult> Index(int? page, int? pageSizeID, DateTime? dateFrom, DateTime? dateTo, string? actionButton,
                                              string sortDirection = "asc", string sortField = "Contact")
        {
            string[] sortOptions = new[] { "Contact", "Date" };  // You can add more sort options if needed

            int numberFilters = 0;
            var contactCancellations = _context.ContactCancellations.Include(c => c.Contact).AsQueryable();

            // Filtering by date range (CancellationDate between dateFrom and dateTo)
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                contactCancellations = contactCancellations
                    .Where(c => c.CancellationDate.Date >= dateFrom.Value.Date && c.CancellationDate.Date <= dateTo.Value.Date);
                numberFilters++;
                ViewData["DateFilterFrom"] = dateFrom.Value.ToString("yyyy-MM-dd");
                ViewData["DateFilterTo"] = dateTo.Value.ToString("yyyy-MM-dd");
            }
            else if (dateFrom.HasValue) // If only 'From' date is selected
            {
                contactCancellations = contactCancellations.Where(c => c.CancellationDate.Date >= dateFrom.Value.Date);
                numberFilters++;
                ViewData["DateFilterFrom"] = dateFrom.Value.ToString("yyyy-MM-dd");
            }
            else if (dateTo.HasValue) // If only 'To' date is selected
            {
                contactCancellations = contactCancellations.Where(c => c.CancellationDate.Date <= dateTo.Value.Date);
                numberFilters++;
                ViewData["DateFilterTo"] = dateTo.Value.ToString("yyyy-MM-dd");
            }

            if (!String.IsNullOrEmpty(actionButton)) // Form Submitted!
            {
                page = 1; // Reset page to start

                if (sortOptions.Contains(actionButton)) // Change of sort is requested
                {
                    if (actionButton == sortField) // Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton; // Sort by the button clicked
                }
            }

            contactCancellations = sortField switch
            {
                "Contact" => sortDirection == "asc"
                    ? contactCancellations.OrderBy(e => e.Contact.FirstName).ThenBy(e => e.Contact.LastName)
                    : contactCancellations.OrderByDescending(e => e.Contact.FirstName).ThenByDescending(e => e.Contact.LastName),

                "Date" => sortDirection == "asc"
                    ? contactCancellations.OrderBy(e => e.CancellationDate) // Assuming Address has City
                    : contactCancellations.OrderByDescending(e => e.CancellationDate),

                
                _ => contactCancellations
            };

            // Give feedback about applied filters
            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = " show";
            }

            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {contactCancellations.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<ContactCancellation>.CreateAsync(contactCancellations.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // GET: ContactCancellation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactCancellation = await _context.ContactCancellations
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (contactCancellation == null)
            {
                return NotFound();
            }

            return View(contactCancellation);
        }

        // GET: ContactCancellation/Create
        public IActionResult Create(int? ContactID)
        {
            if (ContactID.HasValue)
            {
                // Find the contact based on the ContactID
                var contact = _context.Contacts
                    .Where(c => c.Id == ContactID.Value)
                    .Select(c => new { c.Id, c.FirstName, c.LastName })
                    .FirstOrDefault();

                // If contact is found, return success with member data
                if (contact != null)
                {
                    return Json(new
                    {
                        success = true,
                        contactId = contact.Id,
                        contactName = $"{contact.FirstName} {contact.LastName}"  // Concatenate First and Last Name
                    });
                }

                // If no contact is found, return failure
                return Json(new { success = false, message = "Contact not found." });
            }
            else
            {
                // If ContactID is not provided, show the select list of contacts
                ViewData["ContactID"] = new SelectList(_context.Contacts, "Id", "FirstName");
                return View();
            }
        }


        // POST: ContactCancellation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: ContactCancellation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CancellationDate,CancellationNote,IsCancelled,ContactID")] ContactCancellation contactCancellation)
        {
            // If the request is an AJAX request (check for the presence of X-Requested-With header)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Check if ContactID is valid
                if (contactCancellation.ContactID == 0)
                {
                    return Json(new { success = false, message = "ContactID is not valid." });
                }

                // Retrieve the Contact by ContactID
                var contact = await _context.Contacts.FindAsync(contactCancellation.ContactID);
                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found." });
                }

                // Proceed if the model is valid
                if (ModelState.IsValid)
                {
                    // Add the ContactCancellation to the database
                    _context.Add(contactCancellation);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Contact: {contact.FirstName} {contact.LastName} Archived Successfully!";

                    // Return success message as JSON
                    return Json(new { success = true, message = "Cancellation created successfully!" });
                }

                // Return error messages if the model is invalid (for AJAX requests)
                string errorMessage = string.Join("|", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errorMessage });
            }

            // If the request is not AJAX, process the form normally (standard post request)
            if (ModelState.IsValid)
            {
                // Add the ContactCancellation to the database
                _context.Add(contactCancellation);
                await _context.SaveChangesAsync();

                // Redirect to the index page
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
            // If the model is invalid, re-populate the select list and return the view
            ViewData["ContactID"] = new SelectList(_context.Contacts, "Id", "FirstName", contactCancellation.ContactID);
            return View(contactCancellation);
        }


        // GET: ContactCancellation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactCancellation = await _context.ContactCancellations.Include(c => c.Contact).FirstOrDefaultAsync(c => c.ID == id);
            if (contactCancellation == null)
            {
                return NotFound();
            }

            var Contact = _context.Contacts.Include(c => c.ContactCancellations)
               .Where(m => m.Id == contactCancellation.ContactID)
               .Select(m => new { m.Id, m.FirstName, m.LastName })
               .FirstOrDefault();

            if (Contact != null)
            {
                ViewBag.MemberName = Contact.FirstName + " " + Contact.LastName;
                ViewBag.ContactID = Contact.Id;
            }

            ViewData["ContactID"] = new SelectList(_context.Contacts, "Id", "FirstName", contactCancellation);
            return View(contactCancellation);
        }

        // POST: ContactCancellation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {

            // Fetch the existing ContactCancellation record from the database
            var contactCancellationToUpdate = await _context.ContactCancellations.Include(c => c.Contact)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (contactCancellationToUpdate == null)
            {
                return NotFound();
            }


            // Attach the RowVersion for concurrency tracking
            _context.Entry(contactCancellationToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<ContactCancellation>(contactCancellationToUpdate, "",
                c => c.CancellationDate, c => c.CancellationNote, c => c.IsCancelled, c => c.ContactID))
            {
                try
                {

                    _context.Update(contactCancellationToUpdate);
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    TempData["Success"] = $"Contact: {contactCancellationToUpdate.Contact.FirstName} {contactCancellationToUpdate.Contact.LastName} Updated Successfully!";

                    return RedirectToAction("Details", new { id = contactCancellationToUpdate.ID });

                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please try again later.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (ContactCancellation)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "The record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (ContactCancellation)databaseEntry.ToObject();

                        if (databaseValues.CancellationDate != clientValues.CancellationDate)
                            ModelState.AddModelError("CancellationDate", $"Current value: {databaseValues.CancellationDate:d}");
                        if (databaseValues.CancellationNote != clientValues.CancellationNote)
                            ModelState.AddModelError("CancellationNote", $"Current value: {databaseValues.CancellationNote}");
                        if (databaseValues.IsCancelled != clientValues.IsCancelled)
                            ModelState.AddModelError("IsCancelled", $"Current value: {databaseValues.IsCancelled}");
                        if (databaseValues.ContactID != clientValues.ContactID)
                        {
                            var databaseContact = await _context.Contacts
                                .FirstOrDefaultAsync(c => c.Id == databaseValues.ContactID);
                            ModelState.AddModelError("ContactID", $"Current value: {databaseContact?.FirstName}");
                        }

                        ModelState.AddModelError("", "The record was modified by another user after you started editing. " +
                            "If you still want to save your changes, click the Save button again.");

                        // Update RowVersion for the next attempt
                        contactCancellationToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    ModelState.AddModelError("", $"Unable to save changes: {message}");
                }
            }

            ViewData["ContactID"] = new SelectList(_context.Contacts, "Id", "FirstName", contactCancellationToUpdate);
            return View(contactCancellationToUpdate);
        }



        // GET: ContactCancellation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactCancellation = await _context.ContactCancellations
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (contactCancellation == null)
            {
                return NotFound();
            }

            return View(contactCancellation);
        }

        // POST: ContactCancellation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contactCancellation = await _context.ContactCancellations.Include(m => m.Contact).FirstOrDefaultAsync(a => a.ID == id);
            if (contactCancellation != null)
            {
                _context.ContactCancellations.Remove(contactCancellation);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Contact: {contactCancellation.Contact.FirstName} Activated Successfully!";

            return RedirectToAction(nameof(Index));
        }

        private bool ContactCancellationExists(int id)
        {
            return _context.ContactCancellations.Any(e => e.ID == id);

        }

        public async Task<IActionResult> GetCancellationPreview(int id)
        {
            var member = await _context.ContactCancellations
                .Include(m => m.Contact).ThenInclude(m => m.MemberContacts).ThenInclude(m => m.Member).FirstOrDefaultAsync(m => m.ID == id); // Use async version for better performance

            if (member == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_ContactCancellationPreview", member); // Ensure the partial view name matches
        }

        [HttpPost]
        public async Task<IActionResult> SaveCancellationNote(int id, string note)
        {
            var memberToUpdate = await _context.ContactCancellations.FirstOrDefaultAsync(m => m.ID == id);

            if (memberToUpdate == null)
            {
                return Json(new { success = false, message = "Cancellation Contact not found." });
            }

            // Update MemberNote
            memberToUpdate.CancellationNote = note;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Note saved successfully!";

                return Json(new { success = true, message = "Note saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


    }
}
