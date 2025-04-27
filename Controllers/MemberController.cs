using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using NIA_CRM.ViewModels;
using Org.BouncyCastle.Utilities.Encoders;
using Microsoft.AspNetCore.Authorization;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class MemberController : ElephantController
    {
        private readonly NIACRMContext _context;

        public MemberController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Member
        public async Task<IActionResult> Index(string? SearchString, string? JoinDate, int? page, int? pageSizeID, string? actionButton, int? MembershipTypes, int? Sectors, int? NAICSCodes, string? Cities, string sortDirection = "asc", string sortField = "Member Name")

        {

            PopulateDropdowns();
            string[] sortOptions = { "Member Name", "City", "Membership Type", "Sector", "NAICS Code", "Contact Name", "Email" };
            int numberFilters = 0;

            var members = _context.Members
                                    .Include(m => m.MemberThumbnail)
                                    .Include(m => m.Address)
                                    .Include(m => m.MemberMembershipTypes).ThenInclude(m => m.MembershipType)
                                    .Include(m => m.MemberContacts).ThenInclude(m => m.Contact)
                                    .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                                    .Include(m => m.MemberSectors).ThenInclude(m => m.Sector)
                                    .Where(m => !m.Cancellations.Any())  // Exclude members with cancellations
                                    .AsNoTracking();





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

            members = sortField switch
            {
                "Member Name" => sortDirection == "asc"
                    ? members.OrderBy(e => e.MemberName)
                    : members.OrderByDescending(e => e.MemberName),

                "City" => sortDirection == "asc"
                    ? members.OrderBy(e => e.Address.City)
                    : members.OrderByDescending(e => e.Address.City),

                "Membership Type" => sortDirection == "asc"
                    ? members.OrderBy(e => e.MemberMembershipTypes.FirstOrDefault().MembershipType.TypeName)
                    : members.OrderByDescending(e => e.MemberMembershipTypes.FirstOrDefault().MembershipType.TypeName),

                "Sector" => sortDirection == "asc"
                    ? members.OrderBy(e => e.MemberSectors.FirstOrDefault().Sector.SectorName)
                    : members.OrderByDescending(e => e.MemberSectors.FirstOrDefault().Sector.SectorName),

                "NAICS Code" => sortDirection == "asc"
                    ? members.OrderBy(e => e.IndustryNAICSCodes.FirstOrDefault().NAICSCode.Code)
                    : members.OrderByDescending(e => e.IndustryNAICSCodes.FirstOrDefault().NAICSCode.Code),

                "Contact Name" => sortDirection == "asc"
                    ? members.OrderBy(e => e.MemberContacts.FirstOrDefault().Contact.FirstName)
                             .ThenBy(e => e.MemberContacts.FirstOrDefault().Contact.LastName)
                    : members.OrderByDescending(e => e.MemberContacts.FirstOrDefault().Contact.FirstName)
                             .ThenByDescending(e => e.MemberContacts.FirstOrDefault().Contact.LastName),

                "Email" => sortDirection == "asc"
                    ? members.OrderBy(e => e.MemberContacts.FirstOrDefault().Contact.Email)
                    : members.OrderByDescending(e => e.MemberContacts.FirstOrDefault().Contact.Email),

                _ => members
            };



            if (!string.IsNullOrEmpty(SearchString))
            {
                members = members.Where(m =>
                    m.MemberName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;
            }

            if (!string.IsNullOrEmpty(JoinDate))
            {
                if (DateTime.TryParse(JoinDate, out var parsedDate))
                {
                    members = members.Where(m => m.JoinDate == parsedDate);
                    ViewData["JoinDate"] = JoinDate;
                }
                else
                {
                    ModelState.AddModelError("JoinDate", "Invalid date format. Please use YYYY-MM-DD.");
                }
            }

            if (MembershipTypes.HasValue)
            {
                var membershipType = _context.MembershipTypes
                                             .FirstOrDefault(m => m.Id == MembershipTypes.Value);

                if (membershipType != null)
                {
                    members = members
                        .Where(p => p.MemberMembershipTypes
                            .Any(mmt => mmt.MembershipTypeId == MembershipTypes.Value));

                    numberFilters++;
                    ViewData["MembershipTypesFilter"] = membershipType.TypeName;
                }
            }

            // Filter by Sectors
            if (Sectors.HasValue)
            {
                var sector = _context.Sectors.FirstOrDefault(s => s.Id == Sectors.Value);
                if (sector != null)
                {
                    members = members.Where(m => m.MemberSectors.Any(ms => ms.SectorId == Sectors.Value));
                    numberFilters++;
                    ViewData["SectorsFilter"] = sector.SectorName;
                }
            }

            // Filter by NAICS Codes
            if (NAICSCodes.HasValue)
            {
                var naicsCode = _context.NAICSCodes.FirstOrDefault(n => n.Id == NAICSCodes.Value);
                if (naicsCode != null)
                {
                    members = members.Where(m => m.IndustryNAICSCodes.Any(nc => nc.NAICSCodeId == NAICSCodes.Value));
                    numberFilters++;
                    ViewData["NAICSCodesFilter"] = naicsCode.Code;
                }
            }

            // Filter by City
            if (!string.IsNullOrEmpty(Cities))
            {
                members = members.Where(m => m.Address.City.ToUpper() == Cities.ToUpper());
                numberFilters++;
                ViewData["CitiesFilter"] = Cities;
            }

            // Store the total number of filters applied
            ViewData["numberFilters"] = numberFilters > 0 ? $"{numberFilters} Filters Applied" : "0 Filters Applied";


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

                return ExportMembersToExcel(members.ToList());
            }


            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {members.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Member>.CreateAsync(members, page ?? 1, pageSize);

            return View(pagedData);
        }


        private IActionResult ExportMembersToExcel(List<Member> members)
        {
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Members");

            // Adding headers
            worksheet.Cells[1, 1].Value = "Member ID";
            worksheet.Cells[1, 2].Value = "Member Name";
            worksheet.Cells[1, 3].Value = "City";
            worksheet.Cells[1, 4].Value = "Join Date";
            worksheet.Cells[1, 5].Value = "Membership Type";
            worksheet.Cells[1, 6].Value = "Address Line 1";
            worksheet.Cells[1, 7].Value = "Address Line 2";
            worksheet.Cells[1, 8].Value = "City";
            worksheet.Cells[1, 9].Value = "State/Province";
            worksheet.Cells[1, 10].Value = "Postal Code";
            worksheet.Cells[1, 11].Value = "Phone Number";
            worksheet.Cells[1, 12].Value = "Email Address";

            // Populating data
            int row = 2;
            foreach (var member in members)
            {
                worksheet.Cells[row, 1].Value = member.ID;
                worksheet.Cells[row, 2].Value = member.MemberName;
                worksheet.Cells[row, 3].Value = member.Address?.City ?? "N/A"; // Access City directly in one-to-one relation
                worksheet.Cells[row, 4].Value = member.JoinDate.ToString("yyyy-MM-dd") ?? "N/A"; // Format date
                worksheet.Cells[row, 5].Value = member.MemberMembershipTypes.FirstOrDefault()?.MembershipType?.TypeName ?? "N/A";


                // Separating address components
                var address = member.Address;
                if (address != null)
                {
                    worksheet.Cells[row, 6].Value = address.AddressLine1;
                    worksheet.Cells[row, 7].Value = address.AddressLine2;
                    worksheet.Cells[row, 8].Value = address.City;
                    worksheet.Cells[row, 9].Value = address.StateProvince;
                    worksheet.Cells[row, 10].Value = address.PostalCode;
                }
                else
                {
                    worksheet.Cells[row, 6].Value = "No Address Available";
                    worksheet.Cells[row, 7].Value = "N/A";
                    worksheet.Cells[row, 8].Value = "N/A";
                    worksheet.Cells[row, 9].Value = "N/A";
                    worksheet.Cells[row, 10].Value = "N/A";
                }

                // Separating contact information
                var contact = member.MemberContacts.FirstOrDefault();
                if (contact != null)
                {
                    worksheet.Cells[row, 11].Value = contact.Contact.Phone;
                    worksheet.Cells[row, 12].Value = contact.Contact.Email;
                }
                else
                {
                    worksheet.Cells[row, 11].Value = "No Contact Available";
                    worksheet.Cells[row, 12].Value = "N/A";
                }

                row++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0; // Reset position before returning

            string excelName = $"Members_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ImportMembersFromExcel(IFormFile file)
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
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

                        if (worksheet == null || worksheet.Dimension == null)
                        {
                            TempData["Error"] = "The Excel file is empty or not formatted correctly.";
                            return RedirectToAction("Index");
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        // Ensure the required columns exist (minimum 7 columns based on the import logic)
                        if (colCount < 7)
                        {
                            TempData["Error"] = "The Excel file is missing required columns.";
                            return RedirectToAction("Index");
                        }

                        List<Member> members = new List<Member>();

                        for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip headers
                        {
                            if (worksheet.Cells[row, 1].Value == null) // Skip empty rows
                                continue;

                            // Validate and assign required fields
                            var member = new Member
                            {
                                // Member ID (Required)
                                ID = int.TryParse(worksheet.Cells[row, 1].Value?.ToString(), out var id) && id > 0 ? id : 0,

                                // Member Name (Required)
                                MemberName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "Unknown",

                                // Join Date (Required)
                                JoinDate = DateTime.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var joinDate)
                                            ? joinDate
                                            : DateTime.MinValue, // Use DateTime.MinValue if invalid

                                // Membership Type (Required)
                                MemberMembershipTypes = new List<MemberMembershipType>
                        {
                            new MemberMembershipType
                            {
                                MembershipType = new MembershipType
                                {
                                    TypeName = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "Unknown"
                                }
                            }
                        },

                                // Address (Required: City and AddressLine1)
                                Address = new Address
                                {
                                    City = worksheet.Cells[row, 3]?.Value?.ToString()?.Trim() ?? "N/A",
                                    AddressLine1 = worksheet.Cells[row, 6]?.Value?.ToString()?.Trim() ?? "N/A",
                                    PostalCode = string.IsNullOrWhiteSpace(worksheet.Cells[row, 8]?.Value?.ToString()) ? "N/A" : worksheet.Cells[row, 8]?.Value?.ToString()

                                },

                                // Contact Information (Required: Phone, Email)
                                MemberContacts = new List<MemberContact>()
                            };

                            // Handle contact information safely
                            string contactInfo = worksheet.Cells[row, 7]?.Value?.ToString() ?? "";
                            string[] contactParts = contactInfo.Split('|');

                            member.MemberContacts.Add(new MemberContact
                            {
                                Contact = new Contact
                                {
                                    Phone = contactParts.Length > 0 ? contactParts[0].Trim() : "No Phone",
                                    Email = contactParts.Length > 1 ? contactParts[1].Trim() : "No Email"
                                }
                            });

                            members.Add(member);
                        }

                        // Save to database
                        if (members.Any())
                        {
                            _context.Members.AddRange(members);
                            _context.SaveChanges();
                            TempData["Success"] = "Members imported successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "No valid data found in the Excel file.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the full exception message including stack trace and inner exception details
                string fullErrorMessage = $"Error importing members: {ex.Message}\nStack Trace: {ex.StackTrace}";

                // If the exception has an inner exception, include it
                if (ex.InnerException != null)
                {
                    fullErrorMessage += $"\nInner Exception: {ex.InnerException.Message}\nStack Trace: {ex.InnerException.StackTrace}";
                }

                // Store the detailed error message in TempData
                TempData["Error"] = "Please Arrange the excel data as per the provided sample fie and put all those fields that are required!";

                // Optionally, log the error to a file or logging system for further review
                // You can use a logging framework like Serilog, NLog, or log4net for better logging management
            }

            return RedirectToAction("Index");
        }





        // GET: Member/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Address)
                .Include(m => m.MemberMembershipTypes).ThenInclude(m => m.MembershipType)
                .Include(m => m.MemberTags).ThenInclude(m => m.MTag)
                .Include(m => m.MemberSectors).ThenInclude(m => m.Sector)
                .Include(m => m.MemberContacts).ThenInclude(m => m.Contact)
                .Include(m => m.MemberLogo)
                .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Member/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            Member member = new Member
            {
                Address = new Address(),  // Initializing with one empty address (instead of a list)
                MemberContacts = new List<MemberContact> { new MemberContact() }  // Initializing with one empty contact
            };

            PopulateAssignedMTagData(member);
            PopulateAssignedSectorData(member);
            PopulateAssignedMembershipTypeData(member);
            PopulateAssignedNaicsCodeData(member);
            DeleteItems();
            return View();
        }

        // POST: Member/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,MemberName,MemberSize,WebsiteUrl,JoinDate,IsPaid,MemberNote")]
                                                Member member, IFormFile? thePicture, string[] selectedOptionsTag,
                                                string[] selectedOptionsSector, string[] selectedOptionsMembership, string[] selectedOptionsNaicsCode)
        {
            try
            {
                // Update Member Tags (MTag)
                UpdateMemberMTag(selectedOptionsTag, member);

                // Update Member Sectors
                UpdateMemberSector(selectedOptionsSector, member);

                // Update Member Membership Types
                UpdateMemberMembershipType(selectedOptionsMembership, member);
                UpdateMemberNaicsCode(selectedOptionsNaicsCode, member);
                DeleteItems();
                if (ModelState.IsValid)
                {
                    // Handle file upload for picture
                    await AddPicture(member, thePicture);

                    // Add the member to the context and save changes
                    _context.Add(member);
                    await _context.SaveChangesAsync();

                    // Success message
                    TempData["Success"] = $"Member: {member.MemberName} added successfully!";
                    // Redirect to the index view
                    // Assuming you have a list of addresses and you want to pass the MemberId of the first address

                    return RedirectToAction(nameof(Create), "Address", new { MemberId = member.ID });
                    //return View(member);



                    // If no address found, handle it appropriately (e.g., show an error or return to a list page)
                    //return RedirectToAction("Index", "Address");
                }
            }
            catch (RetryLimitExceededException)
            {

                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Members.MemberName"))
                {
                    ModelState.AddModelError("Member Name", "Unable to save changes. Remember, " +
                        "you cannot have duplicate Member Names.");
                }
                else
                {
                    ModelState.AddModelError("", $"Unable to save changes: {message}");
                }
            }

            // Populate assigned data if the model state is invalid
            PopulateAssignedMTagData(member);
            PopulateAssignedSectorData(member);
            PopulateAssignedMembershipTypeData(member);  // New method to populate membership type data
            PopulateAssignedNaicsCodeData(member);

            return View(member);
        }


        // GET: Member/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var member = await _context.Members
                .Include(m => m.MemberThumbnail)
                .Include(m => m.MemberSectors).ThenInclude(m => m.Sector)
                .Include(m => m.MemberTags).ThenInclude(m => m.MTag)
                .Include(m => m.MemberMembershipTypes).ThenInclude(m => m.MembershipType)
                .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                .Include(m => m.MemberLogo)
                .FirstOrDefaultAsync(m => m.ID == id);

            PopulateAssignedMTagData(member);
            PopulateAssignedSectorData(member);
            PopulateAssignedMembershipTypeData(member);
            PopulateAssignedNaicsCodeData(member);

            if (member == null)
            {
                return NotFound();
            }

            // Get the member name based on the address's MemberId
            if (member != null)
            {
                ViewBag.MemberName = member.MemberName; // Set the member's name
            }
            else
            {
                ViewBag.MemberName = "No member name provided"; // Handle null or missing member
            }

            return View(member);
        }

        // POST: Member/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Member/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, string? chkRemoveImage, IFormFile? thePicture, string[] selectedOptionsTag,
                                                string[] selectedOptionsSector, string[] selectedOptionsMembership, string[] selectedOptionsNaicsCode)
        {
            var memberToUpdate = await _context.Members
                .Include(m => m.MemberThumbnail)
                .Include(m => m.MemberSectors).ThenInclude(m => m.Sector)
                .Include(m => m.MemberTags).ThenInclude(m => m.MTag)
                .Include(m => m.MemberMembershipTypes).ThenInclude(m => m.MembershipType)
                .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                .Include(m => m.MemberLogo)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (memberToUpdate == null)
            {
                return NotFound();
            }



            // Try updating the model with user input
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.MemberName, m => m.MemberSize, m => m.WebsiteUrl, m => m.JoinDate, m => m.IsPaid, m => m.MemberLogo))
            {
                try
                {

                    // Update Member Tags (MTag)
                    UpdateMemberMTag(selectedOptionsTag, memberToUpdate);

                    // Update Member Sectors
                    UpdateMemberSector(selectedOptionsSector, memberToUpdate);

                    // Update Member Membership Types
                    UpdateMemberMembershipType(selectedOptionsMembership, memberToUpdate);
                    UpdateMemberNaicsCode(selectedOptionsNaicsCode, memberToUpdate);

                    // Handle image removal if the checkbox is checked
                    if (chkRemoveImage != null)
                    {
                        // If we are deleting the image and thumbnail, make sure to notify the Change Tracker
                        memberToUpdate.MemberThumbnail = _context.MemebrThumbnails.FirstOrDefault(p => p.MemberID == memberToUpdate.ID);

                        // Set the image fields to null to delete them
                        memberToUpdate.MemberLogo = null;
                        memberToUpdate.MemberThumbnail = null;

                        // Mark the properties as modified to ensure the changes are tracked
                        _context.Entry(memberToUpdate).Property(m => m.MemberLogo).IsModified = true;
                        _context.Entry(memberToUpdate).Property(m => m.MemberThumbnail).IsModified = true;
                    }
                    else
                    {
                        // Add or update the picture if one is provided
                        await AddPicture(memberToUpdate, thePicture);
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Member: {memberToUpdate.MemberName} updated successfully!";
                    // Get the member name based on the address's MemberId
                    if (memberToUpdate != null)
                    {
                        ViewBag.MemberName = memberToUpdate.MemberName; // Set the member's name
                    }
                    else
                    {
                        ViewBag.MemberName = "No member name provided"; // Handle null or missing member
                    }

                    // Redirect to the Details view for the updated member
                    return RedirectToAction("Details", new { id = memberToUpdate.ID });
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Please try again later.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Member)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "The record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Member)databaseEntry.ToObject();

                        // Compare each field to provide specific feedback
                        if (databaseValues.MemberName != clientValues.MemberName)
                            ModelState.AddModelError("MemberName", $"Current value: {databaseValues.MemberName}");
                        if (databaseValues.MemberSize != clientValues.MemberSize)
                            ModelState.AddModelError("MemberSize", $"Current value: {databaseValues.MemberSize}");
                        if (databaseValues.WebsiteUrl != clientValues.WebsiteUrl)
                            ModelState.AddModelError("WebsiteUrl", $"Current value: {databaseValues.WebsiteUrl}");
                        if (databaseValues.JoinDate != clientValues.JoinDate)
                            ModelState.AddModelError("JoinDate", $"Current value: {databaseValues.JoinDate}");
                        if (databaseValues.IsPaid != clientValues.IsPaid)
                            ModelState.AddModelError("IsPaid", $"Current value: {databaseValues.IsPaid}");

                        ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");

                        // Update RowVersion for the next attempt
                        memberToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    if (message.Contains("UNIQUE") && message.Contains("Members.MemberName"))
                    {
                        ModelState.AddModelError("Member Name", "Unable to save changes. Remember, " +
                            "you cannot have duplicate Member Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Unable to save changes: {message}");
                    }

                }
            }


            PopulateAssignedMTagData(memberToUpdate);
            PopulateAssignedSectorData(memberToUpdate);
            PopulateAssignedMembershipTypeData(memberToUpdate);
            PopulateAssignedNaicsCodeData(memberToUpdate);

            // If we reach here, something went wrong, so return the view with the model
            return View(memberToUpdate);
        }



        // GET: Member/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.MemberThumbnail)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Member: {member.MemberName} archived successfully!";

            return RedirectToAction(nameof(Index));
        }

        private async Task AddPicture(Member member, IFormFile thePicture)
        {
            //Get the picture and save it with the Patient (2 sizes)
            if (thePicture != null)
            {
                string mimeType = thePicture.ContentType;
                long fileLength = thePicture.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("image"))
                    {
                        using var memoryStream = new MemoryStream();
                        await thePicture.CopyToAsync(memoryStream);
                        var pictureArray = memoryStream.ToArray();//Gives us the Byte[]

                        //Check if we are replacing or creating new
                        if (member.MemberLogo != null)
                        {
                            //We already have pictures so just replace the Byte[]
                            member.MemberLogo.Content = ResizeImage.ShrinkImageWebp(pictureArray, 500, 600);

                            //Get the Thumbnail so we can update it.  Remember we didn't include it
                            member.MemberThumbnail = _context.MemebrThumbnails.Where(p => p.MemberID == member.ID).FirstOrDefault();
                            if (member.MemberThumbnail != null)
                            {
                                member.MemberThumbnail.Content = ResizeImage.ShrinkImageWebp(pictureArray, 115, 125);
                            }
                        }
                        else //No pictures saved so start new
                        {
                            member.MemberLogo = new MemberLogo
                            {
                                Content = ResizeImage.ShrinkImageWebp(pictureArray, 500, 600),
                                MimeType = "image/webp"
                            };
                            member.MemberThumbnail = new MemberThumbnail
                            {
                                Content = ResizeImage.ShrinkImageWebp(pictureArray, 115, 125),
                                MimeType = "image/webp"
                            };
                        }
                    }
                }
            }
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
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

        private void PopulateDropdowns()
        {
            var membershipTypes = _context.MembershipTypes.ToList();
            var sectors = _context.Sectors.ToList();
            var naicsCodes = _context.NAICSCodes.ToList();
            var cities = _context.Addresses
                .Where(a => !string.IsNullOrEmpty(a.City)) // Exclude null/empty values
                .Select(a => a.City.Trim()) // Trim extra spaces
                .Distinct()
                .OrderBy(c => c) // Sort alphabetically
                .ToList();

            ViewData["MembershipTypes"] = new SelectList(membershipTypes, "Id", "TypeName");
            ViewData["Sectors"] = new SelectList(sectors, "Id", "SectorName");
            ViewData["NAICSCodes"] = new SelectList(naicsCodes, "Id", "Code");
            ViewData["Cities"] = new SelectList(cities);
        }

        private void PopulateAssignedMTagData(Member member)
        {
            // Get all available MTags
            var allTags = _context.MTags.ToList();
            // Get the set of currently selected MTag IDs for the member
            var currentTagsHS = new HashSet<int>(member.MemberTags.Select(mt => mt.MTagID));

            // Lists for selected and available tags
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();

            // Populate selected and available lists based on whether the tag is selected
            foreach (var tag in allTags)
            {
                var option = new ListOptionVM
                {
                    ID = tag.Id,
                    DisplayText = tag.MTagName
                };

                if (currentTagsHS.Contains(tag.Id))
                {
                    selected.Add(option);
                }
                else
                {
                    available.Add(option);
                }
            }

            // Sort and assign to ViewData for use in the view
            ViewData["selOptsTag"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOptsTag"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        private void PopulateAssignedSectorData(Member member)
        {
            // Get all available MTags
            var allTags = _context.Sectors.ToList();
            // Get the set of currently selected MTag IDs for the member
            var currentTagsHS = new HashSet<int>(member.MemberSectors.Select(mt => mt.SectorId));

            // Lists for selected and available tags
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();

            // Populate selected and available lists based on whether the tag is selected
            foreach (var tag in allTags)
            {
                var option = new ListOptionVM
                {
                    ID = tag.Id,
                    DisplayText = tag.SectorName
                };

                if (currentTagsHS.Contains(tag.Id))
                {
                    selected.Add(option);
                }
                else
                {
                    available.Add(option);
                }
            }

            // Sort and assign to ViewData for use in the view
            ViewData["selOptsSector"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOptsSector"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        private void PopulateAssignedMembershipTypeData(Member member)
        {
            // Get all available MembershipTypes
            var allMembershipTypes = _context.MembershipTypes.ToList();
            // Get the set of currently selected MembershipType IDs for the member
            var currentMembershipTypesHS = new HashSet<int>(member.MemberMembershipTypes.Select(mt => mt.MembershipTypeId));

            // Lists for selected and available membership types
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();

            // Populate selected and available lists based on whether the membership type is selected
            foreach (var membershipType in allMembershipTypes)
            {
                var option = new ListOptionVM
                {
                    ID = membershipType.Id,
                    DisplayText = membershipType.TypeName
                };

                if (currentMembershipTypesHS.Contains(membershipType.Id))
                {
                    selected.Add(option);
                }
                else
                {
                    available.Add(option);
                }
            }

            // Sort and assign to ViewData for use in the view
            ViewData["selOptsMembership"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOptsMembership"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        private void PopulateAssignedNaicsCodeData(Member member)
        {
            // Get all available NAICS Codes
            var allNaicsCode = _context.NAICSCodes.ToList();

            // Get the set of currently selected NAICS Code IDs for the member
            var currentNaicsCodesHS = new HashSet<int>(member.IndustryNAICSCodes.Select(nc => nc.NAICSCodeId));

            // Lists for selected and available NAICS codes
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();

            // Populate selected and available lists based on whether the NAICS code is selected
            foreach (var naicsCode in allNaicsCode)
            {
                var option = new ListOptionVM
                {
                    ID = naicsCode.Id,  // Ensure "Id" exists in the NAICSCodes entity
                    DisplayText = naicsCode.Code // Adjust this based on your model property
                };

                if (currentNaicsCodesHS.Contains(naicsCode.Id))
                {
                    selected.Add(option);
                }
                else
                {
                    available.Add(option);
                }
            }

            // Sort and assign to ViewData for use in the view
            ViewData["selOptsNaiceCode"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOptsNaicsCode"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }



        private void UpdateMemberMTag(string[] selectedOptions, Member memberToUpdate)
        {
            // If no specialties (MTags) are selected, clear the existing collection
            if (selectedOptions == null)
            {
                memberToUpdate.MemberTags = new List<MemberTag>();
                return;
            }

            // Create a HashSet for the selected options (IDs as strings)
            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            // Get the current MTag IDs of the member as a HashSet
            var currentTagsHS = new HashSet<int>(memberToUpdate.MemberTags.Select(mt => mt.MTagID));

            // Iterate over all available MTags
            foreach (var tag in _context.MTags)
            {
                // Check if the MTag is selected
                if (selectedOptionsHS.Contains(tag.Id.ToString())) // It's selected
                {
                    if (!currentTagsHS.Contains(tag.Id)) // It's not already in the member's tags list
                    {
                        // Add the tag to the member's tags
                        memberToUpdate.MemberTags.Add(new MemberTag
                        {
                            MTagID = tag.Id,
                            MemberId = memberToUpdate.ID
                        });
                    }
                }
                else // It's not selected
                {
                    if (currentTagsHS.Contains(tag.Id)) // But it is in the member's current list
                    {
                        // Remove the tag from the member's tags
                        var tagToRemove = memberToUpdate.MemberTags
                            .FirstOrDefault(mt => mt.MTagID == tag.Id);
                        if (tagToRemove != null)
                        {
                            memberToUpdate.MemberTags.Remove(tagToRemove);
                        }
                    }
                }
            }
        }

        private void UpdateMemberSector(string[] selectedOptions, Member memberToUpdate)
        {
            // If no specialties (MTags) are selected, clear the existing collection
            if (selectedOptions == null)
            {
                memberToUpdate.MemberSectors = new List<MemberSector>();
                return;
            }

            // Create a HashSet for the selected options (IDs as strings)
            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            // Get the current MTag IDs of the member as a HashSet
            var currentTagsHS = new HashSet<int>(memberToUpdate.MemberSectors.Select(mt => mt.SectorId));

            // Iterate over all available MTags
            foreach (var tag in _context.Sectors)
            {
                // Check if the MTag is selected
                if (selectedOptionsHS.Contains(tag.Id.ToString())) // It's selected
                {
                    if (!currentTagsHS.Contains(tag.Id)) // It's not already in the member's tags list
                    {
                        // Add the tag to the member's tags
                        memberToUpdate.MemberSectors.Add(new MemberSector
                        {
                            SectorId = tag.Id,
                            MemberId = memberToUpdate.ID
                        });
                    }
                }
                else // It's not selected
                {
                    if (currentTagsHS.Contains(tag.Id)) // But it is in the member's current list
                    {
                        // Remove the tag from the member's tags
                        var tagToRemove = memberToUpdate.MemberSectors
                            .FirstOrDefault(mt => mt.SectorId == tag.Id);
                        if (tagToRemove != null)
                        {
                            memberToUpdate.MemberSectors.Remove(tagToRemove);
                        }
                    }
                }
            }
        }

        private void UpdateMemberMembershipType(string[] selectedOptions, Member memberToUpdate)
        {
            // If no membership types are selected, clear the existing collection
            if (selectedOptions == null)
            {
                memberToUpdate.MemberMembershipTypes = new List<MemberMembershipType>();
                return;
            }

            // Create a HashSet for the selected options (IDs as strings)
            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            // Get the current MembershipType IDs of the member as a HashSet
            var currentMembershipTypesHS = new HashSet<int>(memberToUpdate.MemberMembershipTypes.Select(mt => mt.MembershipTypeId));

            // Iterate over all available MembershipTypes
            foreach (var membershipType in _context.MembershipTypes)
            {
                // Check if the MembershipType is selected
                if (selectedOptionsHS.Contains(membershipType.Id.ToString())) // It's selected
                {
                    if (!currentMembershipTypesHS.Contains(membershipType.Id)) // It's not already in the member's membership types list
                    {
                        // Add the membership type to the member's list
                        memberToUpdate.MemberMembershipTypes.Add(new MemberMembershipType
                        {
                            MembershipTypeId = membershipType.Id,
                            MemberId = memberToUpdate.ID
                        });
                    }
                }
                else // It's not selected
                {
                    if (currentMembershipTypesHS.Contains(membershipType.Id)) // But it is in the member's current list
                    {
                        // Remove the membership type from the member's list
                        var membershipTypeToRemove = memberToUpdate.MemberMembershipTypes
                            .FirstOrDefault(mt => mt.MembershipTypeId == membershipType.Id);
                        if (membershipTypeToRemove != null)
                        {
                            memberToUpdate.MemberMembershipTypes.Remove(membershipTypeToRemove);
                        }
                    }
                }
            }
        }


        private void UpdateMemberNaicsCode(string[] selectedOptions, Member memberToUpdate)
        {
            if (selectedOptions == null)
            {
                // If no NAICS codes are selected, clear the existing collection
                memberToUpdate.IndustryNAICSCodes = new List<IndustryNAICSCode>();
                return;
            }

            var selectedOptionsHS = new HashSet<int>(selectedOptions.Select(int.Parse));
            var currentNaicsCodeHS = new HashSet<int>(memberToUpdate.IndustryNAICSCodes.Select(naics => naics.NAICSCodeId));

            // Iterate over all available NAICS Codes
            foreach (var naicsCode in _context.NAICSCodes)
            {
                if (selectedOptionsHS.Contains(naicsCode.Id))  // Selected in UI
                {
                    if (!currentNaicsCodeHS.Contains(naicsCode.Id))  // Not already added
                    {
                        memberToUpdate.IndustryNAICSCodes.Add(new IndustryNAICSCode
                        {
                            NAICSCodeId = naicsCode.Id,
                            MemberId = memberToUpdate.ID
                        });
                    }
                }
                else  // Not selected in UI
                {
                    if (currentNaicsCodeHS.Contains(naicsCode.Id))  // But exists in DB
                    {
                        var naicsToRemove = memberToUpdate.IndustryNAICSCodes
                            .FirstOrDefault(nc => nc.NAICSCodeId == naicsCode.Id);
                        if (naicsToRemove != null)
                        {
                            memberToUpdate.IndustryNAICSCodes.Remove(naicsToRemove);
                        }
                    }
                }
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveMemberNote(int id, string note)
        {
            var memberToUpdate = await _context.Members.FirstOrDefaultAsync(m => m.ID == id);

            if (memberToUpdate == null)
            {
                return Json(new { success = false, message = "Member not found." });
            }

            // Update MemberNote
            memberToUpdate.MemberNote = note;

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
        [Authorize(Roles = "Admin")]
        public IActionResult ExportSelectedFields(List<string>? selectedFields, string? SearchString, DateTime? JoinDate, string? MembershipType, string? Sectors, string? NAICSCodes, string? Cities, bool applyFilters)
        {
            // Check if no fields are selected
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            // Fetch member data along with related entities
            var members = _context.Members
                                    .Include(m => m.MemberThumbnail)
                                    .Include(m => m.Address)
                                    .Include(m => m.MemberMembershipTypes).ThenInclude(m => m.MembershipType)
                                    .Include(m => m.MemberContacts).ThenInclude(m => m.Contact)
                                    .Include(m => m.IndustryNAICSCodes).ThenInclude(m => m.NAICSCode)
                                    .Include(m => m.MemberSectors).ThenInclude(m => m.Sector)
                                    .AsQueryable();

            // Apply filters only if applyFilters is true
            if (applyFilters)
            {
                // Apply SearchString filter
                if (!string.IsNullOrEmpty(SearchString))
                {
                    members = members.Where(m => m.MemberName.ToUpper().Contains(SearchString.ToUpper()));
                }

                // Apply JoinDate filter
                if (JoinDate.HasValue)
                {
                    members = members.Where(m => m.JoinDate.Date == JoinDate.Value.Date);
                }

                // Apply MembershipTypes filter
                if (!string.IsNullOrEmpty(MembershipType))
                {
                    members = members.Where(m => m.MemberMembershipTypes
                        .Any(mt => mt.MembershipType.TypeName.ToUpper().Contains(MembershipType.ToUpper())));
                }

                // Apply Sectors filter
                if (!string.IsNullOrEmpty(Sectors))
                {
                    members = members.Where(m => m.MemberSectors
                        .Any(ms => ms.Sector.SectorName.ToUpper() == Sectors.ToUpper()));
                }

                // Apply NAICSCodes filter
                if (!string.IsNullOrEmpty(NAICSCodes))
                {
                    members = members.Where(m => m.IndustryNAICSCodes
                        .Any(nc => nc.NAICSCode.Code.ToUpper() == NAICSCodes.ToUpper()));
                }

                // Apply Cities filter
                if (!string.IsNullOrEmpty(Cities))
                {
                    members = members.Where(m => m.Address.City.ToUpper() == Cities.ToUpper());
                }
            }


            // Execute the query to get the filtered results
            var filteredMembers = members.ToList();



            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Members");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Member Export";
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


                int row = 3;

                // Populate the worksheet with member data based on selected fields
                foreach (var member in filteredMembers)
                {
                    col = 1;

                    // Add values for each selected field based on the checkboxes
                    if (selectedFields.Contains("MemberName"))
                        worksheet.Cells[row, col++].Value = member.MemberName ?? "N/A";

                    if (selectedFields.Contains("MemberSize"))
                        worksheet.Cells[row, col++].Value = member?.MemberSize.ToString() ?? "N/A";

                    if (selectedFields.Contains("WebsiteUrl"))
                        worksheet.Cells[row, col++].Value = member?.WebsiteUrl ?? "N/A";

                    if (selectedFields.Contains("JoinDate"))
                        worksheet.Cells[row, col++].Value = member.JoinDate.ToString("yyyy-MM-dd") ?? "N/A";

                    if (selectedFields.Contains("MembershipType"))
                    {
                        var membershipTypes = member?.MemberMembershipTypes
                            .Select(mmt => mmt.MembershipType?.TypeName) // Assuming MembershipType.TypeName is the field you want to export
                            .ToList();
                        worksheet.Cells[row, col++].Value = membershipTypes != null && membershipTypes.Any() ? string.Join(", ", membershipTypes) : "N/A";
                    }

                    if (selectedFields.Contains("MemberNote"))
                        worksheet.Cells[row, col++].Value = member?.MemberNote ?? "N/A";

                    // Apply export for Sectors
                    if (selectedFields.Contains("Sectors"))
                    {
                        var sectors = member?.MemberSectors
                            .Select(ms => ms.Sector?.SectorName) // Assuming Sector.Name is the field you want to export
                            .ToList();
                        worksheet.Cells[row, col++].Value = sectors != null && sectors.Any() ? string.Join(", ", sectors) : "N/A";
                    }

                    // Apply export for NAICSCodes
                    if (selectedFields.Contains("NAICSCodes"))
                    {
                        var naicsCodes = member?.IndustryNAICSCodes
                            .Select(nc => nc.NAICSCode?.Code) // Assuming NAICSCode.Code is the field you want to export
                            .ToList();
                        worksheet.Cells[row, col++].Value = naicsCodes != null && naicsCodes.Any() ? string.Join(", ", naicsCodes) : "N/A";
                    }

                    // Contact Fields
                    if (selectedFields.Contains("ContactFullName"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.Summary ?? "N/A";

                    if (selectedFields.Contains("ContactTitle"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.Title ?? "N/A";

                    if (selectedFields.Contains("ContactDepartment"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.Department ?? "N/A";

                    if (selectedFields.Contains("ContactEmail"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.Email ?? "N/A";

                    if (selectedFields.Contains("ContactPhone"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.PhoneFormatted ?? "N/A";

                    if (selectedFields.Contains("ContactLinkedIn"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.LinkedInUrl ?? "N/A";

                    if (selectedFields.Contains("ContactNote"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.ContactNote ?? "N/A";

                    if (selectedFields.Contains("IsVip"))
                        worksheet.Cells[row, col++].Value = member?.MemberContacts?.FirstOrDefault()?.Contact?.IsVip == true ? "VIP" : "N/A";

                    // Address Fields
                    if (selectedFields.Contains("AddressLine1"))
                        worksheet.Cells[row, col++].Value = member?.Address?.AddressLine1 ?? "N/A";

                    if (selectedFields.Contains("AddressLine2"))
                        worksheet.Cells[row, col++].Value = member?.Address?.AddressLine2 ?? "N/A";

                    if (selectedFields.Contains("City"))
                        worksheet.Cells[row, col++].Value = member?.Address?.City ?? "N/A";

                    if (selectedFields.Contains("StateProvince"))
                        worksheet.Cells[row, col++].Value = member?.Address?.StateProvince.ToString() ?? "N/A";

                    if (selectedFields.Contains("PostalCode"))
                        worksheet.Cells[row, col++].Value = member?.Address?.PostalCode ?? "N/A";

                    if (selectedFields.Contains("FormattedAddress"))
                        worksheet.Cells[row, col++].Value = member?.Address?.FormattedAddress ?? "N/A";



                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Prepare the Excel file for download
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                //TempData["Success"] = "Member data export completed successfully! Check your Downloads folder.";

                string excelName = $"MembersExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }

        }
        public IActionResult DeleteItems()
        {
            // Fetch the lists from the database
            var membershipTypes = _context.MembershipTypes.ToList();
            var tags = _context.MTags.ToList();
            var sectors = _context.Sectors.ToList();
            var naicsCodes = _context.NAICSCodes.ToList();

            // Create SelectLists for each type to pass to the view
            ViewData["MembershipTypeSelectList"] = new SelectList(membershipTypes, "Id", "TypeName"); // 'Id' is the value, 'TypeName' is the text
            ViewData["TagSelectList"] = new SelectList(tags, "Id", "Name"); // 'Id' is the value, 'Name' is the text
            ViewData["SectorSelectList"] = new SelectList(sectors, "Id", "SectorName"); // 'Id' is the value, 'SectorName' is the text
            ViewData["NaicsCodeSelectList"] = new SelectList(naicsCodes, "Id", "Code"); // 'Id' is the value, 'Code' is the text

            return View();
        }



    }
}
