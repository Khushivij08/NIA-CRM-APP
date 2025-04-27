using System;
using System.Collections.Generic;
using System.Linq;
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


namespace NIA_CRM.Controllers
{
    [Authorize]
    public class OpportunityController : ElephantController
    {
        private readonly NIACRMContext _context;

        public OpportunityController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Opportunity
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? status, string? priority, string? SearchString, string? OpportunityAction, string? Interaction,  string? actionButton,
                                              string sortDirection = "desc", string sortField = "Opportunity Name")
        {
            string[] sortOptions = new[] { "Opportunity Name", "POC", "Interaction", "Status", "Priority", "Action" };  // You can add more sort options if needed

            int numberFilters = 0;



            var opportunities = _context.Opportunities.AsQueryable();

            // Filter by Opportunity Status
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OpportunityStatus>(status, out var selectedStatus))
                {
                    opportunities = opportunities.Where(c => c.OpportunityStatus == selectedStatus);
                    numberFilters++;
                    ViewData["StatusFilter"] = selectedStatus;
                }
            }

            // Filter by Opportunity Priority
            if (!string.IsNullOrEmpty(priority))
            {
                if (Enum.TryParse<OpportunityPriority>(priority, out var selectedPriority))
                {
                    opportunities = opportunities.Where(c => c.OpportunityPriority == selectedPriority);
                    numberFilters++;
                    ViewData["PriorityFilter"] = selectedPriority;
                }
            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                opportunities = opportunities.Where(p => p.OpportunityName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;

            }

            if (!String.IsNullOrEmpty(OpportunityAction))
            {
                opportunities = opportunities.Where(p => p.OpportunityAction.ToUpper().Contains(OpportunityAction.ToUpper()));
                numberFilters++;
                ViewData["OpportunityActionFilter"] = OpportunityAction;

            }

            if (!String.IsNullOrEmpty(Interaction))
            {
                opportunities = opportunities.Where(p => p.Interaction.ToUpper().Contains(Interaction.ToUpper()));
                numberFilters++;
                ViewData["InteractionFilter"] = Interaction;

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

            opportunities = sortField switch
            {
                "Opportunity Name" => sortDirection == "asc"
                    ? opportunities.OrderBy(e => e.OpportunityName)
                    : opportunities.OrderByDescending(e => e.OpportunityName),

                "POC" => sortDirection == "asc"
                    ? opportunities.OrderBy(e => e.POC) // Assuming Address has City
                    : opportunities.OrderByDescending(e => e.POC),

                "Interaction" => sortDirection == "asc"
                    ? opportunities.OrderBy(e => e.Interaction) // Assuming the MembershipType has a Name
                    : opportunities.OrderByDescending(e => e.Interaction),

                "Priority" => sortDirection == "asc"
                    ? opportunities.OrderBy(e => e.OpportunityPriority) // Assuming NAICSCode has Sector
                    : opportunities.OrderByDescending(e => e.OpportunityPriority),

                "Action" => sortDirection == "asc"
                    ? opportunities.OrderBy(e => e.OpportunityAction) // Assuming NAICSCode has Code
                    : opportunities.OrderByDescending(e => e.OpportunityAction),
                "Status" => sortDirection == "asc"
                ? opportunities.OrderBy(e => e.OpportunityStatus) // Assuming NAICSCode has Code
                : opportunities.OrderByDescending(e => e.OpportunityStatus),

                _ => opportunities
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

                return ExportOpportunitiesToExcel(opportunities.ToList());
            }

            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {opportunities.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Opportunity>.CreateAsync(opportunities.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // Export Opportunities to Excel
        public IActionResult ExportOpportunitiesToExcel(List<Opportunity> opportunities)
        {
            // Set the license context for EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //var opportunities = _context.Opportunities.ToList();

            // Initialize the Excel package
            var package = new ExcelPackage();

            // Add a worksheet to the package
            var worksheet = package.Workbook.Worksheets.Add("Opportunities");

            // Add headers to the worksheet
            worksheet.Cells[1, 1].Value = "Opportunity Name";
            worksheet.Cells[1, 2].Value = "Opportunity Action";
            worksheet.Cells[1, 3].Value = "POC";
            worksheet.Cells[1, 4].Value = "Account";
            worksheet.Cells[1, 5].Value = "Interaction";
            worksheet.Cells[1, 6].Value = "Last Contact";
            worksheet.Cells[1, 7].Value = "Opportunity Status";
            worksheet.Cells[1, 8].Value = "Opportunity Priority";

            // Populate data in the worksheet
            int row = 2;
            foreach (var opportunity in opportunities)
            {
                worksheet.Cells[row, 1].Value = opportunity.OpportunityName;
                worksheet.Cells[row, 2].Value = opportunity.OpportunityAction;
                worksheet.Cells[row, 3].Value = opportunity.POC;
                worksheet.Cells[row, 4].Value = opportunity.Account;
                worksheet.Cells[row, 5].Value = opportunity.Interaction;
                worksheet.Cells[row, 6].Value = opportunity.LastContact?.ToString("yyyy-MM-dd"); // Format the date
                worksheet.Cells[row, 7].Value = opportunity.OpportunityStatus;
                worksheet.Cells[row, 8].Value = opportunity.OpportunityPriority;
                row++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells.AutoFitColumns();

            // Save the package to a memory stream
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0; // Reset the stream position before returning it

            // Define the Excel file name
            string excelName = "Opportunities.xlsx";

            // Return the file as a download
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [HttpPost]
        public IActionResult ImportOpportunitiesFromExcel(IFormFile file)
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

                        List<Opportunity> opportunities = new List<Opportunity>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var opportunity = new Opportunity
                            {
                                OpportunityName = worksheet.Cells[row, 1].Value?.ToString(),
                                OpportunityAction = worksheet.Cells[row, 2].Value?.ToString(),
                                POC = worksheet.Cells[row, 3].Value?.ToString(),
                                Account = worksheet.Cells[row, 4].Value?.ToString(),
                                Interaction = worksheet.Cells[row, 5].Value?.ToString(),
                                LastContact = DateTime.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out DateTime lastContact) ? lastContact : (DateTime?)null,
                                OpportunityStatus = Enum.TryParse(worksheet.Cells[row, 7].Value?.ToString(), true, out OpportunityStatus status) ? status : OpportunityStatus.Qualification,
                                OpportunityPriority = Enum.TryParse(worksheet.Cells[row, 8].Value?.ToString(), true, out OpportunityPriority priority) ? priority : OpportunityPriority.Low
                            };

                            opportunities.Add(opportunity);
                        }

                        _context.Opportunities.AddRange(opportunities);
                        _context.SaveChanges();
                        TempData["Success"] = "Opportunities imported successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing opportunities: {ex.Message}";
            }

            return RedirectToAction("Index");
        }




        // GET: Opportunity/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities
                .FirstOrDefaultAsync(m => m.ID == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        // GET: Opportunity/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Opportunity/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OpportunityName,OpportunityAction,POC,Account,Interaction,LastContact,OpportunityStatus,OpportunityPriority")] Opportunity opportunity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(opportunity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(opportunity);
        }

        // GET: Opportunity/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity == null)
            {
                return NotFound();
            }
            return View(opportunity);
        }

        // POST: Opportunity/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, byte[] RowVersion)
        {
            var opportunityToUpdate = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.ID == id);

            if (opportunityToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(opportunityToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Opportunity>(
                    opportunityToUpdate, "",
                    o => o.OpportunityName, o => o.OpportunityAction, o => o.POC, o => o.Account,
                    o => o.Interaction, o => o.LastContact, o => o.OpportunityStatus, o => o.OpportunityPriority))
                {
                    try
                    {
                        // Update the opportunity record in the database
                        _context.Update(opportunityToUpdate);
                        await _context.SaveChangesAsync();
                        //return RedirectToAction(nameof(Index));
                        TempData["Success"] = "Opportunity updated successfully";
                        return RedirectToAction("Details", new { id = opportunityToUpdate.ID });

                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (Opportunity)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The opportunity was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (Opportunity)databaseEntry.ToObject();
                            // Compare each field and provide feedback on changes
                            if (databaseValues.OpportunityName != clientValues.OpportunityName)
                                ModelState.AddModelError("OpportunityName", $"Current value: {databaseValues.OpportunityName}");
                            if (databaseValues.OpportunityAction != clientValues.OpportunityAction)
                                ModelState.AddModelError("OpportunityAction", $"Current value: {databaseValues.OpportunityAction}");
                            if (databaseValues.POC != clientValues.POC)
                                ModelState.AddModelError("POC", $"Current value: {databaseValues.POC}");
                            if (databaseValues.Account != clientValues.Account)
                                ModelState.AddModelError("Account", $"Current value: {databaseValues.Account}");
                            if (databaseValues.Interaction != clientValues.Interaction)
                                ModelState.AddModelError("Interaction", $"Current value: {databaseValues.Interaction}");
                            if (databaseValues.LastContact != clientValues.LastContact)
                                ModelState.AddModelError("LastContact", $"Current value: {databaseValues.LastContact}");
                            if (databaseValues.OpportunityStatus != clientValues.OpportunityStatus)
                                ModelState.AddModelError("OpportunityStatus", $"Current value: {databaseValues.OpportunityStatus}");
                            if (databaseValues.OpportunityPriority != clientValues.OpportunityPriority)
                                ModelState.AddModelError("OpportunityPriority", $"Current value: {databaseValues.OpportunityPriority}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            opportunityToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
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

            return View(opportunityToUpdate);
        }


        // GET: Opportunity/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opportunity = await _context.Opportunities
                .FirstOrDefaultAsync(m => m.ID == id);
            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        // POST: Opportunity/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var opportunity = await _context.Opportunities.FindAsync(id);

                if (opportunity == null)
                {
                    return Json(new { success = false, message = "Opportunity not found!" });
                }

                _context.Opportunities.Remove(opportunity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Opportunity Deleted Successfully";
                return Json(new { success = true, message = "Opportunity deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the opportunity." });
            }
        }


        public async Task<IActionResult> GetOpportunityPreview(int id)
        {
            var opportunity = await _context.Opportunities.FirstOrDefaultAsync(m => m.ID == id); // Use async version for better performance

            if (opportunity == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_OpportunityPreview", opportunity); // Ensure the partial view name matches
        }
        
        private bool OpportunityExists(int id)
        {
            return _context.Opportunities.Any(e => e.ID == id);
        }




        [HttpPost]
        public IActionResult ExportSelectedOpportunitiesFields(List<string>? selectedFields, string? status, string? priority, string? SearchString, string? OpportunityAction, string? Interaction, bool applyFilters)
        {
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            var opportunities = _context.Opportunities.AsQueryable();

            // Apply filters only if applyFilters is true
            if (applyFilters)
            {
                // Filter by Opportunity Status
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<OpportunityStatus>(status, out var selectedStatus))
                    {
                        opportunities = opportunities.Where(o => o.OpportunityStatus == selectedStatus);
                    }
                }

                // Filter by Opportunity Priority
                if (!string.IsNullOrEmpty(priority))
                {
                    if (Enum.TryParse<OpportunityPriority>(priority, out var selectedPriority))
                    {
                        opportunities = opportunities.Where(o => o.OpportunityPriority == selectedPriority);
                    }
                }


                if (!string.IsNullOrEmpty(SearchString))
                {
                    opportunities = opportunities.Where(o => o.OpportunityName.ToUpper().Contains(SearchString.ToUpper()));
                }

                if (!string.IsNullOrEmpty(OpportunityAction))
                {
                    opportunities = opportunities.Where(o => o.OpportunityAction.ToUpper().Contains(OpportunityAction.ToUpper()));
                }

                if (!string.IsNullOrEmpty(Interaction))
                {
                    opportunities = opportunities.Where(o => o.Interaction.ToUpper().Contains(Interaction.ToUpper()));
                }
            }

            var Filteredopportunities = opportunities.ToList();  // Convert to list after filtering

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Opportunities");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Opportunities Export";
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
                foreach (var opportunity in Filteredopportunities)
                {
                    col = 1;

                    if (selectedFields.Contains("OpportunityName"))
                        worksheet.Cells[row, col++].Value = opportunity.OpportunityName ?? "N/A";

                    if (selectedFields.Contains("OpportunityAction")) 
                        worksheet.Cells[row, col++].Value = opportunity.OpportunityAction ?? "N/A";

                    if (selectedFields.Contains("POC"))
                        worksheet.Cells[row, col++].Value = opportunity.POC ?? "N/A";

                    if (selectedFields.Contains("Account"))
                        worksheet.Cells[row, col++].Value = opportunity.Account ?? "N/A";

                    if (selectedFields.Contains("Interaction"))
                        worksheet.Cells[row, col++].Value = opportunity.Interaction ?? "N/A";

                    if (selectedFields.Contains("LastContact"))
                        worksheet.Cells[row, col++].Value = opportunity.LastContact?.ToString("yyyy-MM-dd") ?? "N/A";

                    if (selectedFields.Contains("OpportunityStatus"))
                        worksheet.Cells[row, col++].Value = opportunity.OpportunityStatus.ToString() ?? "N/A";

                    if (selectedFields.Contains("OpportunityPriority"))
                        worksheet.Cells[row, col++].Value = opportunity.OpportunityPriority.ToString() ?? "N/A";

                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"OpportunitiesExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }


    }
}
