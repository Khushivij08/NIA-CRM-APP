using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class CancellationController : ElephantController
    {
        private readonly NIACRMContext _context;

        public CancellationController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Cancellation
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? Members, string? SearchString, bool cancelled, string? actionButton,
                                              string sortDirection = "asc", string sortField = "Member")
        {
            PopulateDropdowns();
            string[] sortOptions = new[] { "Member", "Date" };

            var cancellations = _context.Cancellations.Include(c => c.Member).Where( c => c.IsCancelled == true).AsQueryable();

            int numberFilters = 0;

            if (!string.IsNullOrEmpty(SearchString))
            {
                cancellations = cancellations.Where(m =>
                    m.Member.MemberName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;

            }
            if (cancelled)
            {
                cancellations = cancellations.Where(c => c.IsCancelled);
                numberFilters++;
                ViewData["cancelledFilter"] = "Applied";

            }
            

            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            cancellations = sortField switch
            {
                "Member" => sortDirection == "asc"
                    ? cancellations.OrderBy(e => e.Member.MemberName)
                    : cancellations.OrderByDescending(e => e.Member.MemberName),

                "Date" => sortDirection == "asc"
                    ? cancellations.OrderBy(e => e.CancellationDate) // Assuming Address has City
                    : cancellations.OrderByDescending(e => e.CancellationDate),

                
                _ => cancellations
            };

            if (Members.HasValue)
            {
                // Assuming you have a Members entity or lookup to fetch the name by ID
                var member = _context.Members.FirstOrDefault(m => m.ID == Members.Value);

                if (member != null)
                {
                    cancellations = cancellations.Where(p => p.MemberID == Members.Value);
                    numberFilters++;
                    ViewData["MembersFilter"] = member.MemberName; // Set the member's name in ViewData
                }
            }
            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }

            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {cancellations.Count()}";
            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Cancellation>.CreateAsync(cancellations.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Cancellation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancellation = await _context.Cancellations
                .Include(c => c.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (cancellation == null)
            {
                return NotFound();
            }

            return View(cancellation);
        }

      
        // GET: Cancellation/Create
        public IActionResult Create(int? memberId)
        {
            if (memberId.HasValue)
            {
                var member = _context.Members
                    .Where(m => m.ID == memberId.Value)
                    .Select(m => new { m.ID, m.MemberName })
                    .FirstOrDefault();

                if (member != null)
                {
                    return Json(new
                    {
                        success = true,
                        memberId = member.ID,
                        memberName = member.MemberName
                    });
                }

            }

            return Json(new { success = false, message = "Member not found." });
        }



        // POST: Cancellation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Cancellation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CancellationDate,IsCancelled,CancellationNote,MemberID")] Cancellation cancellation)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (cancellation.MemberID == 0)
                {
                    return Json(new { success = false, message = "MemberID is not valid." });
                }

                var member = await _context.Members.Include(m => m.MemberContacts)
                    .ThenInclude(mc => mc.Contact)
                    .FirstOrDefaultAsync(m => m.ID == cancellation.MemberID);
                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found." });
                }

                if (ModelState.IsValid)
                {
                    _context.Add(cancellation); // Add the cancellation
                    await _context.SaveChangesAsync(); // Save changes to the database
                    TempData["Success"] = $"Member: {member.MemberName} Archived Successfully!";
                    return Json(new { success = true, message = "Cancellation archiving completed successfully!" });
                }

                
                string errorMessage = string.Join("|", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errorMessage });
            }

            if (ModelState.IsValid)
            {
                _context.Add(cancellation); // Add the cancellation to the context
                await _context.SaveChangesAsync(); // Save changes to the database
                TempData["Success"] = $"Member: {cancellation.Member.MemberName} Archived Successfully!";

                var member = await _context.Members.Include(m => m.MemberContacts)
                    .ThenInclude(mc => mc.Contact)
                    .FirstOrDefaultAsync(m => m.ID == cancellation.MemberID);

                if (member != null)
                {
                    foreach (var memberContact in member.MemberContacts)
                    {
                        var contactCancellation = await _context.ContactCancellations
                            .FirstOrDefaultAsync(cc => cc.ContactID == memberContact.ContactId);

                        if (contactCancellation != null)
                        {
                            contactCancellation.IsCancelled = true;
                            contactCancellation.CancellationDate = cancellation.CancellationDate;
                            contactCancellation.CancellationNote = "Cancelled due to member cancellation.";
                            _context.Update(contactCancellation);
                        }

                        // Archive the contact
                        var contact = await _context.Contacts.FindAsync(memberContact.ContactId);
                        if (contact != null)
                        {
                            contact.IsArchieved = true;
                            _context.Update(contact);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

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
            return View(cancellation);
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(Cancellation cancellation)
        {
            if (cancellation.MemberID == 0)
            {
                return BadRequest("Member ID is required.");
            }

            var member = await _context.Members.FindAsync(cancellation.MemberID);

            if (member == null)
            {
                return NotFound(); // Return 404 if member not found
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
            // Proceed with cancellation
            cancellation.CancellationDate = DateTime.UtcNow;
            cancellation.IsCancelled = true;
            cancellation.CancellationNote = "Archived via system.";

            _context.Cancellations.Add(cancellation);
            TempData["Success"] = $"Member: {member.MemberName} Archived Successfully!";

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Member archived successfully!" });
        }





        // GET: Cancellation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancellation = await _context.Cancellations.FindAsync(id);
            if (cancellation == null)
            {
                return NotFound();
            }
            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "MemberName", cancellation.MemberID);
            return View(cancellation);
        }

        // POST: Cancellation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            
            // Fetch the existing cancellation record from the database
            var cancellationToUpdate = await _context.Cancellations.FirstOrDefaultAsync(c => c.ID == id);

            if (cancellationToUpdate == null)
            {
                return NotFound();
            }

            // Attach the RowVersion for concurrency tracking
            _context.Entry(cancellationToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Cancellation>(cancellationToUpdate, "",
                c => c.CancellationDate, c => c.IsCancelled, c => c.CancellationNote, c => c.MemberID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    TempData["Success"] = $"Archived Member: {cancellationToUpdate.Member.MemberName} Updated Successfully!";

                    return RedirectToAction("Details", new { id = cancellationToUpdate.ID });

                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again later.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Cancellation)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "The record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Cancellation)databaseEntry.ToObject();

                        if (databaseValues.CancellationDate != clientValues.CancellationDate)
                            ModelState.AddModelError("CancellationDate", $"Current value: {databaseValues.CancellationDate:d}");
                        if (databaseValues.IsCancelled != clientValues.IsCancelled)
                            ModelState.AddModelError("Canceled", $"Current value: {databaseValues.IsCancelled}");
                        if (databaseValues.CancellationNote != clientValues.CancellationNote)
                            ModelState.AddModelError("CancellationNote", $"Current value: {databaseValues.CancellationNote}");
                        if (databaseValues.MemberID != clientValues.MemberID)
                        {
                            var databaseMember = await _context.Members
                                .FirstOrDefaultAsync(m => m.ID == databaseValues.MemberID);
                            ModelState.AddModelError("MemberID", $"Current value: {databaseMember?.MemberName}");
                        }

                        ModelState.AddModelError("", "The record was modified by another user after you started editing. " +
                            "If you still want to save your changes, click the Save button again.");

                        // Update RowVersion for the next attempt
                        cancellationToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    ModelState.AddModelError("", $"Unable to save changes: {message}");
                }
            }

            ViewData["MemberID"] = new SelectList(_context.Members, "ID", "MemberName", cancellationToUpdate.MemberID);
            return View(cancellationToUpdate);
        }


        // GET: Cancellation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancellation = await _context.Cancellations
                .Include(c => c.Member)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (cancellation == null)
            {
                return NotFound();
            }

            return View(cancellation);
        }

        // POST: Cancellation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cancellation = await _context.Cancellations.Include(m => m.Member).FirstOrDefaultAsync(a => a.ID == id);
            if (cancellation != null)
            {
                _context.Cancellations.Remove(cancellation);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Member: {cancellation.Member.MemberName} Activated Successfully!";

            return RedirectToAction(nameof(Index));
        }

        private bool CancellationExists(int id)
        {
            return _context.Cancellations.Any(e => e.ID == id);
        }

        private void PopulateDropdowns()
        {
            // Fetch Members for dropdown
            var members = _context.Members.ToList();
            ViewData["Members"] = new SelectList(members, "ID", "MemberName");

        }


        public async Task<IActionResult> GetMemberPreview(int id)
        {
            var member = await _context.Members
                .Include(m => m.Address) // Include the related Address
                .Include(m => m.MemberThumbnail)
                .Include(m => m.MemberMembershipTypes)
                .ThenInclude(mm => mm.MembershipType)
                .Include(m => m.MemberContacts).ThenInclude(m => m.Contact)
                .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                .FirstOrDefaultAsync(m => m.ID == id); // Use async version for better performance

            if (member == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_MemberPreview", member); // Ensure the partial view name matches
        }
    }
}
