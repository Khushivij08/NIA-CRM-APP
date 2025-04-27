using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using OfficeOpenXml;
using static Microsoft.IO.RecyclableMemoryStreamManager;


namespace NIA_CRM.Controllers
{
    [Authorize]
    public class MEventController : ElephantController
    {
        private readonly NIACRMContext _context;

        public MEventController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: MEvent

        public async Task<IActionResult> Index(int? page, int? pageSizeID, DateTime? date, string? SearchString, string? Participants, string? EventLocations, string? actionButton,
                                      string sortDirection = "desc", string sortField = "Event Name")
        {
            string[] sortOptions = new[] { "Event Name", "Event Date", "Event Location" };  // You can add more sort options if needed
            int numberFilters = 0;

            var MEvents = _context.MEvents.Include(m => m.MemberEvents).ThenInclude(m => m.Member).AsQueryable();

            ViewData["EventLocations"] = new SelectList(_context.MEvents.Select(e => e.EventLocation).Distinct().ToList());

            // Filter by Event Location
            if (!String.IsNullOrEmpty(EventLocations))
            {
                MEvents = MEvents.Where(e => e.EventLocation != null && e.EventLocation.ToUpper().Contains(EventLocations.ToUpper()));
                numberFilters++;
                ViewData["EventLocationFilter"] = EventLocations;
            }

            if (date.HasValue) // Check if date has a value instead of using TryParse
            {
                MEvents = MEvents.Where(c => c.EventDate.Date == date.Value.Date);
                numberFilters++;
                ViewData["DateFilter"] = date.Value.ToString("yyyy-MM-dd"); // Store in ViewData for UI persistence
            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                MEvents = MEvents.Where(p => p.EventName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;
            }

            if (!String.IsNullOrEmpty(Participants))
            {
                MEvents = MEvents.Where(p => p.MemberEvents
                    .Any(a => a.Member.MemberName != null && a.Member.MemberName.ToUpper().Contains(Participants.ToUpper())));
                numberFilters++;
                ViewData["ParticipantsFilter"] = Participants;
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
                    else
                    {
                        sortDirection = "desc"; // Default new sort fields to descending
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }

            MEvents = sortField switch
            {
                "Event Name" => sortDirection == "asc"
                    ? MEvents.OrderBy(e => e.EventName)
                    : MEvents.OrderByDescending(e => e.EventName),

                "Event Date" => sortDirection == "asc"
                    ? MEvents.OrderBy(e => e.EventDate) // Assuming Address has City
                    : MEvents.OrderByDescending(e => e.EventDate),

                "Event Location" => sortDirection == "asc"
                    ? MEvents.OrderBy(e => e.EventLocation) // Assuming the MembershipType has a Name
                    : MEvents.OrderByDescending(e => e.EventLocation),

                _ => MEvents
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
                return ExportToExcel(MEvents.ToList());
            }

            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {MEvents.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MEvent>.CreateAsync(MEvents.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        public IActionResult ExportToExcel(List<MEvent> MEvents)
        {
            var package = new ExcelPackage(); // No 'using' block to avoid disposal
            var worksheet = package.Workbook.Worksheets.Add("Events");

            // Adding headers
            worksheet.Cells[1, 1].Value = "Event Name";
            worksheet.Cells[1, 2].Value = "Event Description";
            worksheet.Cells[1, 3].Value = "Event Location";
            worksheet.Cells[1, 4].Value = "Event Date";

            // Populating data
            int row = 2;
            foreach (var eventItem in MEvents)
            {
                worksheet.Cells[row, 1].Value = eventItem.EventName;
                worksheet.Cells[row, 2].Value = eventItem.EventDescription;
                worksheet.Cells[row, 3].Value = eventItem.EventLocation;
                worksheet.Cells[row, 4].Value = eventItem.EventDate.ToString("yyyy-MM-dd"); // Format the Date
                row++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0; // Reset position before returning

            string excelName = "Events.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }



        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        List<MEvent> events = new List<MEvent>();

                        for (int row = 2; row <= rowCount; row++) // Skip header row
                        {
                            events.Add(new MEvent
                            {
                                EventName = worksheet.Cells[row, 1].Value?.ToString(),
                                EventDescription = worksheet.Cells[row, 2].Value?.ToString(),
                                EventLocation = worksheet.Cells[row, 3].Value?.ToString(),
                                EventDate = worksheet.Cells[row, 4].Value != null && DateTime.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out DateTime date) ? date
                                 : DateTime.MinValue// Default value if invalid or empty

                            });
                        }

                        _context.MEvents.AddRange(events);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Events imported successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error importing data: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }




        // GET: MEvent/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mEvent = await _context.MEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mEvent == null)
            {
                return NotFound();
            }

            return View(mEvent);
        }

        // GET: MEvent/Create
        public async Task<IActionResult> Create()
        {
            // Fetch all members for selection
            ViewBag.Members = await _context.Members
                .Select(m => new { Id = m.ID, m.MemberName })
                .ToListAsync();

            ViewBag.SelectedMembers = new List<int>(); // No pre-selected members

            return View();
        }


        // POST: MEvent/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDescription,EventLocation,EventDate")] MEvent mEvent, List<int> SelectedMembers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mEvent);
                await _context.SaveChangesAsync(); // ✅ Save event first to get ID

                // ✅ Add selected members to the junction table
                if (SelectedMembers != null && SelectedMembers.Any())
                {
                    foreach (var memberId in SelectedMembers)
                    {
                        _context.MemberEvents.Add(new MemberEvent { MEventID = mEvent.Id, MemberId = memberId });
                    }
                    await _context.SaveChangesAsync(); // ✅ Save after adding members
                }

                return RedirectToAction(nameof(Index));
            }

            // Repopulate ViewBag in case of validation errors
            ViewBag.Members = await _context.Members.Select(m => new { Id = m.ID, m.MemberName }).ToListAsync();
            ViewBag.SelectedMembers = SelectedMembers ?? new List<int>();

            return View(mEvent);
        }


        // GET: MEvent/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var eventModel = await _context.MEvents
                .Include(e => e.MemberEvents)
                .ThenInclude(me => me.Member)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null)
            {
                return NotFound();
            }

            // Fetch all members
            ViewBag.Members = await _context.Members
                .Select(m => new { Id = m.ID, m.MemberName })
                .ToListAsync();

            // Pre-select members assigned to this event
            ViewBag.SelectedMembers = eventModel.MemberEvents
                .Select(me => me.MemberId)
                .ToList();

            return View(eventModel);
        }

        // POST: MEvent/Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion, List<int> SelectedMembers)
        {
            var mEventToUpdate = await _context.MEvents
                                               .Include(e => e.MemberEvents)
                                               .FirstOrDefaultAsync(e => e.Id == id);

            if (mEventToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(mEventToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<MEvent>(
                    mEventToUpdate, "",
                    m => m.EventName, m => m.EventDescription, m => m.EventLocation, m => m.EventDate))
                {
                    try
                    {
                        // Remove existing Member-Event relations before adding new ones
                        _context.MemberEvents.RemoveRange(mEventToUpdate.MemberEvents);
                        await _context.SaveChangesAsync();  // ✅ Save first to ensure clean state

                        // Add new selections only if `SelectedMembers` is not empty
                        if (SelectedMembers != null && SelectedMembers.Any())
                        {
                            foreach (var memberId in SelectedMembers)
                            {
                                _context.MemberEvents.Add(new MemberEvent { MEventID = mEventToUpdate.Id, MemberId = memberId });
                            }
                        }

                        await _context.SaveChangesAsync(); // Save after adding new members
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (MEvent)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The event was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (MEvent)databaseEntry.ToObject();

                            if (databaseValues.EventName != clientValues.EventName)
                                ModelState.AddModelError("EventName", $"Current value: {databaseValues.EventName}");
                            if (databaseValues.EventDescription != clientValues.EventDescription)
                                ModelState.AddModelError("EventDescription", $"Current value: {databaseValues.EventDescription}");
                            if (databaseValues.EventLocation != clientValues.EventLocation)
                                ModelState.AddModelError("EventLocation", $"Current value: {databaseValues.EventLocation}");
                            if (databaseValues.EventDate != clientValues.EventDate)
                                ModelState.AddModelError("EventDate", $"Current value: {databaseValues.EventDate}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            mEventToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
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

            return View(mEventToUpdate);
        }



        // GET: MEvent/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mEvent = await _context.MEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mEvent == null)
            {
                return NotFound();
            }

            return View(mEvent);
        }

        // POST: MEvent/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var mEvent = await _context.MEvents.FindAsync(id);

                if (mEvent == null)
                {
                    return Json(new { success = false, message = "Event not found!" });
                }

                _context.MEvents.Remove(mEvent);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event Deleted Successfully!";
                return Json(new { success = true, message = "Event deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the event." });
            }
        }


        private bool MEventExists(int id)
        {
            return _context.MEvents.Any(e => e.Id == id);
        }



        public async Task<IActionResult> GetEventPreview(int id)
        {
            var opportunity = await _context.MEvents.Include(m => m.MemberEvents).ThenInclude(m => m.Member).FirstOrDefaultAsync(m => m.Id == id); // Use async version for better performance

            if (opportunity == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_EventPreview", opportunity); // Ensure the partial view name matches
        }


        [HttpPost]
        public IActionResult ExportSelectedMemberEventsFields(List<string>? selectedFields, string? SearchString, DateTime? DateFilter, string? Participants, string? EventLocations, bool applyFilters)
        {
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            var memberEvents = _context.MEvents
                .Include(me => me.MemberEvents)
                .ThenInclude(m => m.Member)
                .AsQueryable();

            // Apply filters if applyFilter is true
            if (applyFilters)
            {
                if (!string.IsNullOrEmpty(SearchString))
                {
                    memberEvents = memberEvents.Where(e => e.EventName.Contains(SearchString));
                }

                if (DateFilter.HasValue)
                {
                    memberEvents = memberEvents.Where(e => e.EventDate.Date == DateFilter.Value.Date);
                }

                if (!string.IsNullOrEmpty(Participants))
                {
                    memberEvents = memberEvents.Where(e => e.MemberEvents
                        .Any(m => m.Member.MemberName != null && m.Member.MemberName.ToUpper().Contains(Participants.ToUpper())));
                }

                if (!string.IsNullOrEmpty(EventLocations))
                {
                    memberEvents = memberEvents.Where(e => e.EventLocation.ToUpper().Contains(EventLocations.ToUpper()));
                }
            }

            var memberEventsList = memberEvents.ToList(); // Execute query

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("MemberEvents");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Member Events Export";
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
                foreach (var eventItem in memberEventsList)
                {
                    col = 1;

                    if (selectedFields.Contains("MemberName"))
                    {
                        var memberNames = eventItem.MemberEvents?
                            .Select(me => me.Member?.MemberName)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList();

                        worksheet.Cells[row, col++].Value = memberNames.Any() ? string.Join(", ", memberNames) : "N/A";
                    }

                    if (selectedFields.Contains("EventName"))
                        worksheet.Cells[row, col++].Value = eventItem.EventName ?? "N/A";

                    if (selectedFields.Contains("EventDescription"))
                        worksheet.Cells[row, col++].Value = eventItem.EventDescription ?? "N/A";

                    if (selectedFields.Contains("EventLocation"))
                        worksheet.Cells[row, col++].Value = eventItem.EventLocation ?? "N/A";

                    if (selectedFields.Contains("EventDate"))
                        worksheet.Cells[row, col++].Value = eventItem.EventDate.ToString("yyyy-MM-dd") ?? "N/A";


                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"MemberEventsExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }



    }
}
