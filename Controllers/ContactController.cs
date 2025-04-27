using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using NIA_CRM.ViewModels;
using OfficeOpenXml;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class ContactController : ElephantController
    {
        //for sending email
        private readonly IMyEmailSender _emailSender;
        private readonly NIACRMContext _context;

        public ContactController(NIACRMContext context, IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Contact
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? Departments, string? Titles, bool IsVIP, string? SearchString, string? MemberNameSearchString, string? actionButton,
                                              string sortDirection = "asc", string sortField = "Contact Name")
        {
            PopulateDropdownLists();
            string[] sortOptions = new[] { "Contact Name", "Member Name", "VIP", "Phone", "Email" };  // You can add more sort options if needed

            int numberFilters = 0;



            var contacts = _context.Contacts
                                     .Include(c => c.MemberContacts)
                                     .ThenInclude(mc => mc.Member)
                                     .Include(m => m.ContactCancellations)
                                     .Where(c => !c.ContactCancellations.Any(cc => cc.IsCancelled))  // Only include contacts with no cancellations or 
                                     .AsQueryable();

            if (Departments != null)
            {
                contacts = contacts.Where(c => c.Department == Departments);
                numberFilters++;
                ViewData["DepartmentsFilter"] = Departments;
            }
            if (Titles != null)
            {
                contacts = contacts.Where(c => c.Title == Titles);
                numberFilters++;
                ViewData["TitlesFilter"] = Titles;

            }
            if (IsVIP)
            {
                contacts = contacts.Where(c => c.IsVip);
                numberFilters++;
                ViewData["IsVIPFilter"] = "VIP"; // Set filter value for VIP

            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                contacts = contacts.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;

            }

            if (!String.IsNullOrEmpty(MemberNameSearchString))
            {
                contacts = contacts.Where(m => m.MemberContacts.Any(mc => mc.Member.MemberName.ToUpper().Contains(MemberNameSearchString.ToUpper())));
                numberFilters++;
                ViewData["MemberNameSearchString"] = MemberNameSearchString; // Store the member name search string in ViewData
            }

            // Check if the actionButton is "ExportExcel" BEFORE applying filters
            if (!string.IsNullOrEmpty(actionButton) && actionButton == "ExportExcel")
            {
                var exportData = await contacts.AsNoTracking().ToListAsync();
                return ExportContactsToExcel(exportData);
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

            contacts = sortField switch
            {
                "Member Name" => sortDirection == "asc"
                    ? contacts.OrderBy(e => e.MemberContacts.FirstOrDefault().Member.MemberName)
                    : contacts.OrderByDescending(e => e.MemberContacts.FirstOrDefault().Member.MemberName),

                "VIP" => sortDirection == "asc"
                    ? contacts.OrderBy(e => e.IsVip) // Assuming Address has City
                    : contacts.OrderByDescending(e => e.IsVip),

                "Phone" => sortDirection == "asc"
                    ? contacts.OrderBy(e => e.Phone) // Assuming the MembershipType has a Name
                    : contacts.OrderByDescending(e => e.Phone),

                "Email" => sortDirection == "asc"
                    ? contacts.OrderBy(e => e.Email) // Assuming NAICSCode has Sector
                    : contacts.OrderByDescending(e => e.Email),

                "Contact Name" => sortDirection == "asc"
                    ? contacts.OrderBy(e => e.FirstName).ThenBy(e => e.LastName) // Assuming Contact has Name
                    : contacts.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName),

                _ => contacts
            };


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


            if (!string.IsNullOrEmpty(actionButton) && actionButton == "ExportExcel")
            {

                return ExportContactsToExcel(contacts.ToList());
            }


            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {contacts.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Contact>.CreateAsync(contacts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);



        }
        private IActionResult ExportContactsToExcel(List<Contact> contacts)
        {
            //ExcelPackage = LicenseContext;

            var package = new ExcelPackage(); // No 'using' block to avoid disposal
            var worksheet = package.Workbook.Worksheets.Add("Contacts");

            // Adding headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Title";
            worksheet.Cells[1, 3].Value = "Department";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "Phone";
            worksheet.Cells[1, 6].Value = "LinkedIn URL";
            worksheet.Cells[1, 7].Value = "Is VIP";
            worksheet.Cells[1, 8].Value = "Member Name";

            // Populating data
            int row = 2;
            foreach (var contact in contacts)
            {
                worksheet.Cells[row, 1].Value = contact.Summary;
                worksheet.Cells[row, 2].Value = contact.Title;
                worksheet.Cells[row, 3].Value = contact.Department;
                worksheet.Cells[row, 4].Value = contact.Email;
                worksheet.Cells[row, 5].Value = contact.PhoneFormatted;
                worksheet.Cells[row, 6].Value = contact.LinkedInUrl;
                worksheet.Cells[row, 7].Value = contact.IsVip ? "Yes" : "No";
                //worksheet.Cells[row, 8].Value = contact.Member?.MemberName;
                row++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0; // Reset position before returning

            string excelName = $"Contacts.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }




        // GET: Contact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.MemberContacts).ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contact/Create
        public IActionResult Create(int? memberId)
        {
            bool isNewMember = TempData["IsNewMember"] as bool? ?? false;
            ViewData["IsNewMember"] = isNewMember;
            //TempData.Keep("IsNewMember"); // Keeps the value for the next request


            if (memberId.HasValue)
            {
                var member = _context.Members
                    .Where(m => m.ID == memberId.Value)
                    .Select(m => new { m.ID, m.MemberName })
                    .FirstOrDefault();

                if (member != null)
                {
                    ViewBag.MemberName = member.MemberName; // Store MemberName in ViewBag
                    ViewBag.MemberId = member.ID; // Store MemberId for hidden input
                }
            }

            // If no member is preselected, show a dropdown for selecting a member
            ViewData["Members"] = new SelectList(_context.Members, "ID", "MemberName", memberId);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("Id,FirstName,MiddleName,LastName,Title,Department,Email,Phone,LinkedInUrl,IsVip,ContactNote")] Contact contact,
    int? memberId,
    int? selectedMemberId, bool isNewMember = false)
        {
            if (ModelState.IsValid)
            {
                // Add the Contact to the database
                _context.Add(contact);
                await _context.SaveChangesAsync(); // Save the contact first to get its ID

                // Determine the correct memberId to use (from step-by-step or dropdown)
                int? finalMemberId = memberId ?? selectedMemberId;

                if (finalMemberId.HasValue)
                {
                    // Create a MemberContact relationship
                    var memberContact = new MemberContact
                    {
                        ContactId = contact.Id,
                        MemberId = finalMemberId.Value
                    };

                    _context.MemberContacts.Add(memberContact);
                    await _context.SaveChangesAsync();
                }

                // Send email only if NO member was selected (meaning a new member is being created)
                if (isNewMember)
                {
                    SendWelcomeEmail(contact.Id);
                    TempData["IsNewMember"] = false;
                    TempData["Success"] = $"Welcome Email sent to New Member successfully!";

                }

                TempData["Success"] = $"Contact: {contact.FirstName} {contact.LastName} added successfully!";

                return RedirectToAction(nameof(Details), "Member", new { id = finalMemberId });
            }

            // Retrieve member info again to display banner if necessary
            var member = _context.Members
                .Where(m => m.ID == memberId)
                .Select(m => new { m.ID, m.MemberName })
                .FirstOrDefault();

            if (member != null)
            {
                ViewBag.MemberName = member.MemberName;
                ViewBag.MemberId = member.ID;
            }

            // If no member is preselected, show a dropdown for selecting a member
            ViewData["Members"] = new SelectList(_context.Members, "ID", "MemberName", memberId);

            return View(contact);
        }





        // GET: Contact/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.MemberContacts)
                .ThenInclude(mc => mc.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            // Get the member name based on the contact's MemberContacts
            var memberContact = contact.MemberContacts.FirstOrDefault();
            if (memberContact != null)
            {
                var member = await _context.Members.FindAsync(memberContact.MemberId);
                if (member != null)
                {
                    ViewBag.MemberName = member.MemberName; // Set the member's name
                }
                else
                {
                    ViewBag.MemberName = "No member name provided"; // Handle null or missing member
                }
            }
            else
            {
                ViewBag.MemberName = "No member associated"; // Handle case where no member is associated
            }

            return View(contact);
        }


        // POST: Contact/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {

            // Try updating the model with user input
            // Fetch the existing Contact record from the database
            var contactToUpdate = await _context.Contacts
                .Include(c => c.MemberContacts)
                .FirstOrDefaultAsync(m => m.Id == id);



            if (contactToUpdate == null)
            {
                return NotFound();
            }

            // Attach the RowVersion for concurrency tracking
            _context.Entry(contactToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (await TryUpdateModelAsync<Contact>(contactToUpdate, "",
                c => c.FirstName, c => c.MiddleName, c => c.LastName,
                c => c.Title, c => c.Department, c => c.Email,
                c => c.Phone, c => c.LinkedInUrl, c => c.IsVip, c => c.IsArchieved))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Contact: {contactToUpdate.FirstName} {contactToUpdate.LastName} Updated Successfully!";

                    // Get the member ID from the MemberContacts collection
                    var memberId = contactToUpdate.MemberContacts.FirstOrDefault()?.MemberId;

                    // Get the member name based on the contact's MemberContacts
                    var memberContact = contactToUpdate.MemberContacts.FirstOrDefault();
                    if (memberContact != null)
                    {
                        var member = await _context.Members.FindAsync(memberContact.MemberId);
                        if (member != null)
                        {
                            ViewBag.MemberName = member.MemberName; // Set the member's name
                        }
                        else
                        {
                            ViewBag.MemberName = "No member name provided"; // Handle null or missing member
                        }
                    }
                    else
                    {
                        ViewBag.MemberName = "No member associated"; // Handle case where no member is associated
                    }
                    if (memberId != null)
                    {
                        return RedirectToAction("Details", "Member", new { id = memberId });
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please try again later.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Contact)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "The record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Contact)databaseEntry.ToObject();

                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", $"Current value: {databaseValues.FirstName}");
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", $"Current value: {databaseValues.MiddleName}");
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", $"Current value: {databaseValues.LastName}");
                        if (databaseValues.Title != clientValues.Title)
                            ModelState.AddModelError("Title", $"Current value: {databaseValues.Title}");
                        if (databaseValues.Department != clientValues.Department)
                            ModelState.AddModelError("Department", $"Current value: {databaseValues.Department}");
                        if (databaseValues.Email != clientValues.Email)
                            ModelState.AddModelError("Email", $"Current value: {databaseValues.Email}");
                        if (databaseValues.Phone != clientValues.Phone)
                            ModelState.AddModelError("Phone", $"Current value: {databaseValues.Phone}");
                        if (databaseValues.LinkedInUrl != clientValues.LinkedInUrl)
                            ModelState.AddModelError("LinkedInUrl", $"Current value: {databaseValues.LinkedInUrl}");
                        if (databaseValues.IsVip != clientValues.IsVip)
                            ModelState.AddModelError("IsVip", $"Current value: {databaseValues.IsVip}");
                        if (databaseValues.IsArchieved != clientValues.IsArchieved)
                            ModelState.AddModelError("IsArchieved", $"Current value: {databaseValues.IsArchieved}");

                        ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");

                        // Update RowVersion for the next attempt
                        contactToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    ModelState.AddModelError("", $"Unable to save changes: {message}");
                }
            }

            return View(contactToUpdate);

        }


        // GET: Contact/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.MemberContacts).ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Contact: {contact.FirstName} {contact.LastName} Deleted Successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SaveContactNote(int id, string note)
        {
            var contactToUpdate = await _context.Contacts.FirstOrDefaultAsync(m => m.Id == id);

            if (contactToUpdate == null)
            {
                return Json(new { success = false, message = "Contact not found." });
            }

            // Update MemberNote
            contactToUpdate.ContactNote = note;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Note saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Notification(string selectedContactIds, string Subject, string emailContent)
        {

            // Manually add model state errors if Subject or Body are null/empty
            if (string.IsNullOrWhiteSpace(Subject))
            {
                ModelState.AddModelError("Subject", "Subject cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(emailContent))
            {
                ModelState.AddModelError("emailContent", "Email Body cannot be empty.");
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



            var contactIds = selectedContactIds.Split(',').Select(int.Parse).ToList();
            int folksCount = 0;

            try
            {
                List<EmailAddress> recipients = await _context.Contacts
                    .Where(p => contactIds.Contains(p.Id) && p.Email != null)
                    .Select(p => new EmailAddress
                    {
                        Name = p.Summary,  // Assuming 'Summary' contains the contact name
                        Address = p.Email
                    })
                    .ToListAsync();

                folksCount = recipients.Count;

                if (folksCount > 0)
                {
                    // Prepare the email content with personalized greetings for each contact
                    string emailContentWithGreetings = string.Empty;

                    foreach (var recipient in recipients)
                    {
                        emailContentWithGreetings += $"<p>Dear {recipient.Name},</p><p>{emailContent}</p><p>This is an automatically generated email from <strong>Niagara Industrial Association</strong> website to review.</p><br>";
                    }

                    var msg = new EmailMessage()
                    {
                        ToAddresses = recipients,
                        Subject = Subject,
                        Content = emailContentWithGreetings
                    };

                    await _emailSender.SendToManyAsync(msg);
                    TempData["Success"] = $"Email sent successfully!";

                    return Json(new { success = true, message = "Message sent to " + folksCount + " contact" + ((folksCount == 1) ? "." : "s.") });
                }
                else
                {
                    return Json(new { success = false, message = "Message NOT sent! No valid email addresses found for the selected contacts." });
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                return Json(new { success = false, message = "An error occurred while sending the message: " + ex.Message });
            }


        }

        [HttpPost]
        public async Task<IActionResult> SendWelcomeEmail(int contactId)
        {
            if (contactId <= 0)
            {
                return Json(new { success = false, message = "Invalid contact ID." });
            }

            try
            {
                // Fetch the contact with the given ID and ensure the email is not null
                var contact = await _context.Contacts
                    .Where(c => c.Id == contactId && c.Email != null)
                    .Select(c => new EmailAddress
                    {
                        Name = c.Summary,  // Assuming 'Summary' is the contact's name
                        Address = c.Email
                    })
                    .FirstOrDefaultAsync();

                if (contact == null)
                {
                    return Json(new { success = false, message = "No valid email address found for the selected contact." });
                }

                // Fetch the production email template for the Welcome email type
                var productionEmail = await _context.ProductionEmails
                    .Where(e => e.EmailType == EmailType.Welcome)
                    .FirstOrDefaultAsync();

                if (productionEmail == null)
                {
                    return Json(new { success = false, message = "No production email template found." });
                }

                // Prepare the email content with a personalized greeting
                string emailContentWithGreetings = $"<p>Dear {contact.Name},</p><p>{productionEmail.Body}</p><p>This is an automatically generated email from <strong>Niagara Industrial Association</strong> website to review.</p><br>";

                var msg = new EmailMessage()
                {
                    ToAddresses = new List<EmailAddress> { contact },
                    Subject = productionEmail.Subject,
                    Content = emailContentWithGreetings  // Include the greeting and body
                };

                await _emailSender.SendToManyAsync(msg);

                return Json(new { success = true, message = "Welcome email sent successfully to " + contact.Name + "." });
            }


            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: Could not send email. " + ex.GetBaseException().Message });
            }
        }


        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }

        private void PopulateDropdownLists()
        {
            var departments = _context.Contacts
                                      .Select(c => c.Department)
                                      .Distinct()
                                      .OrderBy(d => d)
                                      .ToList();
            var titles = _context.Contacts
                .Select(c => c.Title)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
            ViewData["Departments"] = new SelectList(departments);
            ViewData["Titles"] = new SelectList(titles);


        }
        public IActionResult GetContactPreview(int id)
        {
            // Fetch the contact by id, including related industries through ContactIndustries
            var contact = _context.Contacts
                .Include(c => c.MemberContacts).ThenInclude(c => c.Member)
                .Where(c => c.Id == id)  // Filter the contact by id
                .FirstOrDefault();  // Return the first result or null if not found

            // Check if contact was not found
            if (contact == null)
            {
                return NotFound();  // Return 404 if the contact does not exist
            }

            // Return the partial view with the contact data
            return PartialView("_ContactPreview", contact);  // Ensure the partial view name is correct
        }

        public IActionResult ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        List<Contact> contacts = new List<Contact>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var contact = new Contact
                            {
                                FirstName = worksheet.Cells[row, 1].Value?.ToString(),
                                Title = worksheet.Cells[row, 2].Value?.ToString(),
                                Department = worksheet.Cells[row, 3].Value?.ToString(),
                                Email = worksheet.Cells[row, 4].Value?.ToString(),
                                Phone = worksheet.Cells[row, 5].Value?.ToString(),
                                LinkedInUrl = worksheet.Cells[row, 6].Value?.ToString(),
                                IsVip = worksheet.Cells[row, 7].Value?.ToString() == "Yes"
                            };

                            contacts.Add(contact);
                        }

                        _context.Contacts.AddRange(contacts);
                        _context.SaveChanges();
                        TempData["Success"] = "Contacts imported successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing contacts: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult ExportSelectedContactsFields(List<string>? selectedFields, string? SearchString, string? Title, string? Department, string? MemberNameSearchString, string? VIP, bool applyFilters)
        {
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            // Apply filters if they are provided
            var contactsQuery = _context.Contacts
                .Include(c => c.MemberContacts)
                .ThenInclude(m => m.Member)
                .AsQueryable();// Assuming Contact has a navigation property to Member

            if (applyFilters)
            {
                // Filter by SearchString
                if (!string.IsNullOrEmpty(SearchString))
                {
                    contactsQuery = contactsQuery.Where(c =>
                        c.FirstName.ToUpper().Contains(SearchString.ToUpper()) ||
                        c.MiddleName.ToUpper().Contains(SearchString.ToUpper()) ||
                        c.LastName.ToUpper().Contains(SearchString.ToUpper())
                    );
                }

                // Filter by Title
                if (!string.IsNullOrEmpty(Title))
                {
                    contactsQuery = contactsQuery.Where(c => c.Title.ToUpper().Contains(Title.ToUpper()));
                }

                // Filter by Department
                if (!string.IsNullOrEmpty(Department))
                {
                    contactsQuery = contactsQuery.Where(c => c.Department.ToUpper().Contains(Department.ToUpper()));
                }

                // Filter by VIP status
                if (!string.IsNullOrEmpty(VIP))
                {
                    contactsQuery = contactsQuery.Where(c => c.IsVip);
                }

                if (!String.IsNullOrEmpty(MemberNameSearchString))
                {
                    contactsQuery = contactsQuery.Where(m => m.MemberContacts.Any(mc => mc.Member.MemberName.ToUpper().Contains(MemberNameSearchString.ToUpper())));
                }

            }


            var contacts = contactsQuery.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Contacts");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Contact Export";
                worksheet.Cells[1, 1, 1, selectedFields.Count].Merge = true; // Merge header cells
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                int headerRow = 2; // Header row for column names
                foreach (var field in selectedFields)
                {
                    var cell = worksheet.Cells[headerRow, col];
                    cell.Value = field;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Adding color to header
                    col++;
                }

                int row = 3; // Data starts from row 3
                foreach (var contact in contacts)
                {
                    col = 1;

                    // Check each selected field and export the corresponding data

                    if (selectedFields.Contains("ContactFirstName"))
                        worksheet.Cells[row, col++].Value = contact.FirstName ?? "N/A";

                    if (selectedFields.Contains("ContactMiddleName"))
                        worksheet.Cells[row, col++].Value = contact.MiddleName ?? "N/A";

                    if (selectedFields.Contains("ContactLastName"))
                        worksheet.Cells[row, col++].Value = contact.LastName ?? "N/A";

                    if (selectedFields.Contains("ContactTitle"))
                        worksheet.Cells[row, col++].Value = contact.Title ?? "N/A";

                    if (selectedFields.Contains("ContactDepartment"))
                        worksheet.Cells[row, col++].Value = contact.Department ?? "N/A";

                    if (selectedFields.Contains("Email"))
                        worksheet.Cells[row, col++].Value = contact.Email ?? "N/A";

                    if (selectedFields.Contains("Phone"))
                        worksheet.Cells[row, col++].Value = contact.PhoneFormatted ?? "N/A";

                    if (selectedFields.Contains("LinkedInUrl"))
                        worksheet.Cells[row, col++].Value = contact.LinkedInUrl ?? "N/A";

                    if (selectedFields.Contains("IsVip"))
                        worksheet.Cells[row, col++].Value = contact.IsVip ? "Yes" : "No";

                    if (selectedFields.Contains("ContactNote"))
                        worksheet.Cells[row, col++].Value = contact.ContactNote ?? "N/A";

                    if (selectedFields.Contains("MemberName"))
                    {
                        // If a contact is associated with multiple members, take the member names.
                        var memberNames = contact.MemberContacts
                            .Select(mc => mc.Member?.MemberName)
                            .Where(name => !string.IsNullOrEmpty(name)) // Ensure no null or empty member names
                            .ToList();

                        // If there are multiple members, join them with a comma, or display "N/A" if no member names are available.
                        worksheet.Cells[row, col++].Value = memberNames.Any() ? string.Join(", ", memberNames) : "N/A";
                    }


                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"ContactsExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }


    }

}