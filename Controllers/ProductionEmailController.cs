using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using NIA_CRM.ViewModels;

namespace NIA_CRM.Controllers
{
    public class ProductionEmailController : LookupsController
    {
        //for sending email
        //private readonly IMyEmailSender _emailSender;
        private readonly NIACRMContext _context;

        public ProductionEmailController(NIACRMContext context/*, IMyEmailSender emailSender*/)
        {
            _context = context;
            //_emailSender = emailSender;
        }

        // GET: ProductionEmail
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? EmailTypeID, string? actionButton, string sortDirection = "asc", string sortField = "Template Name")
        {
            // Populate the dropdown list
            ViewData["EmailTypeID"] = ProductionEmailTypeSelectList(EmailTypeID);
            int numberFilters = 0;
            string[] sortOptions = new[] { "Template Name", "Email Type", "Subject" };

            // Declare the email list to be used in the view
            var emailsQuery = _context.ProductionEmails.AsQueryable();

            // Filter by EmailTypeID if provided
            if (EmailTypeID.HasValue)
            {
                var emailType = emailsQuery.FirstOrDefault(et => et.Id == EmailTypeID.Value);

                if (emailType != null)
                {
                    emailsQuery = emailsQuery.Where(e => e.Id == EmailTypeID.Value);
                    numberFilters++;
                    ViewData["EmailTypeIDFilter"] = emailType.EmailType; // Set the email type name in ViewData
                }


            }

            // Handle sorting
            if (!string.IsNullOrEmpty(actionButton)) // Form submitted!
            {
                page = 1; // Reset to the first page
                if (sortOptions.Contains(actionButton)) // Sort requested
                {
                    if (actionButton == sortField) // Reverse sort direction for the same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton; // Update the sort field
                }
            }

            // Apply sorting
            emailsQuery = sortField switch
            {
                "Template Name" => sortDirection == "asc"
                    ? emailsQuery.OrderBy(e => e.TemplateName)
                    : emailsQuery.OrderByDescending(e => e.TemplateName),
                "Email Type" => sortDirection == "asc"
                    ? emailsQuery.OrderBy(e => e.EmailType)
                    : emailsQuery.OrderByDescending(e => e.EmailType),
                "Subject" => sortDirection == "asc"
                    ? emailsQuery.OrderBy(e => e.Subject)
                    : emailsQuery.OrderByDescending(e => e.Subject),
                _ => emailsQuery
            };
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

            // Get all non-cancelled contacts
            var allContacts = _context.Contacts
                .Include(c => c.ContactCancellations)
                .Where(c => !c.ContactCancellations.Any(cc => cc.IsCancelled))
                .ToList();

            // Create ListOptionVM entries
            var available = allContacts.Select(contact => new ListOptionVM
            {
                ID = contact.Id,
                DisplayText = (contact.Summary ?? "") + " (" + (contact.Email ?? "") + ")"
            })
            .OrderBy(c => c.DisplayText)
            .ToList();

            // Assign to ViewData
            ViewData["availContacts"] = new MultiSelectList(available, "ID", "DisplayText");


            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<ProductionEmail>.CreateAsync(emailsQuery.AsNoTracking(), page ?? 1, pageSize);

            // Pass sorting info to the view
            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {emailsQuery.Count()}";

            // Return the paginated result
            return View(pagedData);
        }

        // GET: ProductionEmail/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionEmail = await _context.ProductionEmails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionEmail == null)
            {
                return NotFound();
            }

            return View(productionEmail);
        }

        // GET: ProductionEmail/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductionEmail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TemplateName,EmailType,Subject,Body")] ProductionEmail productionEmail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productionEmail);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Production Email Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(productionEmail);
        }

        // GET: ProductionEmail/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionEmail = await _context.ProductionEmails.FindAsync(id);
            if (productionEmail == null)
            {
                return NotFound();
            }
            return View(productionEmail);
        }

        // POST: ProductionEmail/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var EmailToUpdate = await _context.ProductionEmails.FirstOrDefaultAsync(e => e.Id == id);
            if (EmailToUpdate == null)
            {
                return NotFound();
            }
            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(EmailToUpdate).Property("RowVersion").OriginalValue = RowVersion;


            if (await TryUpdateModelAsync<ProductionEmail>(EmailToUpdate, "", e => e.TemplateName,  e => e.EmailType, e => e.Subject, e => e.Body))
            {
                try
                {
                    _context.Update(EmailToUpdate);
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    TempData["Success"] = "Email Updated Successfully";
                    return RedirectToAction("Details", new { id = EmailToUpdate.Id});




                }
                catch (DbUpdateConcurrencyException ex) // Adddeed For Concurency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (ProductionEmail)exceptionEntry.Entity;
                    var databaseEntry = await exceptionEntry.GetDatabaseValuesAsync();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The email record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (ProductionEmail)databaseEntry.ToObject();

                        // Compare client and database values
                        if (databaseValues.EmailType != clientValues.EmailType)
                            ModelState.AddModelError("EmailType", $"Current value: {databaseValues.EmailType}");
                        if (databaseValues.Subject != clientValues.Subject)
                            ModelState.AddModelError("Subject", $"Current value: {databaseValues.Subject}");
                        if (databaseValues.Body != clientValues.Body)
                            ModelState.AddModelError("Body", $"Current value: {databaseValues.Body}");

                        ModelState.AddModelError(string.Empty,
                            "The record you attempted to edit was modified by another user after you received your values. "
                            + "The edit operation was canceled, and the current values in the database have been displayed. "
                            + "If you still want to save your version of this record, click the Save button again.");

                        // Update RowVersion for the current instance
                        EmailToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }


                }

                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    if (message.Contains("UNIQUE") && message.Contains("ProductionEmails.EmailType"))
                    {
                        ModelState.AddModelError("EmailType", "Unable to save changes. Remember, " +
                            "you cannot have duplicate EmailType.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }

                }
            }
            return View(EmailToUpdate);

        }

        // GET: ProductionEmail/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionEmail = await _context.ProductionEmails
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionEmail == null)
            {
                return NotFound();
            }

            return View(productionEmail);
        }

        // POST: ProductionEmail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productionEmail = await _context.ProductionEmails.FindAsync(id);
            if (productionEmail != null)
            {
                _context.ProductionEmails.Remove(productionEmail);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Email Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        

        private bool ProductionEmailExists(int id)
        {
            return _context.ProductionEmails.Any(e => e.Id == id);
        }

        private SelectList ProductionEmailTypeSelectList(int? selectedId)
        {
            // Query to fetch the email types ordered alphabetically
            var qry = _context.ProductionEmails
                               .OrderBy(e => e.EmailType)
                               .Select(e => new { e.Id, e.EmailType })
                               .AsNoTracking();

            // Return SelectList, passing the selectedId to indicate the pre-selected value
            return new SelectList(qry, "Id", "EmailType", selectedId);
        }
    }
}
