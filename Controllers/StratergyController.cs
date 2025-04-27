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
    public class StratergyController : ElephantController
    {
        private readonly NIACRMContext _context;

        public StratergyController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Stratergy
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? createdDate, string? SearchString, string? StrategyAssigneeFilter,
                                                string? StrategyTermFilter, string? StrategyStatusFilter, string? actionButton,
                                                string sortDirection = "desc", string sortField = "Strategy Name")
        {
            string[] sortOptions = new[] { "Strategy Name", "Strategy Assignee", "Created Date", "SearchString", "Strategy Term", "Strategy Status" }; // Add other fields if needed
            int numberFilters = 0;
            var strategies = _context.Strategies.AsQueryable();

            // Filter by Created Date
            if (!string.IsNullOrEmpty(createdDate))
            {
                DateTime filterDate;
                if (DateTime.TryParse(createdDate, out filterDate))
                {
                    strategies = strategies.Where(s => s.CreatedDate.Date == filterDate.Date);
                    numberFilters++;
                    ViewData["DateFilter"] = createdDate; // Retain the selected filter in the view
                }
            }

            // Filter by Strategy Assignee
            if (!string.IsNullOrEmpty(StrategyAssigneeFilter))
            {
                strategies = strategies.Where(p => p.StrategyAssignee.ToUpper().Contains(StrategyAssigneeFilter.ToUpper()));
                numberFilters++;
                ViewData["StrategyAssigneeFilter"] = StrategyAssigneeFilter;
            }

            // Filter by Strategy Term (string filtering)
            if (!string.IsNullOrEmpty(StrategyTermFilter))
            {
                if (Enum.TryParse(StrategyTermFilter, out StrategyTerm term))
                {
                    strategies = strategies.Where(s => s.StrategyTerm == term);
                    numberFilters++;
                    ViewData["StrategyTermFilter"] = StrategyTermFilter; // Store the selected value as a string
                }
            }

            // Filter by Strategy Status (string filtering)
            if (!string.IsNullOrEmpty(StrategyStatusFilter))
            {
                if (Enum.TryParse(StrategyStatusFilter, out StrategyStatus status))
                {
                    strategies = strategies.Where(s => s.StrategyStatus == status);
                    numberFilters++;
                    ViewData["StrategyStatusFilter"] = StrategyStatusFilter; // Store the selected value as a string
                }
            }



            // Filter by Search String (e.g., Strategy Name or other field)
            if (!String.IsNullOrEmpty(SearchString))
            {
                strategies = strategies.Where(p => p.StrategyName.ToUpper().Contains(SearchString.ToUpper()) ||
                                                    p.StrategyAssignee.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;
            }

            // Handle sorting
            if (!String.IsNullOrEmpty(actionButton)) // Form Submitted!
            {
                page = 1; // Reset page to start

                if (sortOptions.Contains(actionButton)) // Change of sort is requested
                {
                    if (actionButton == sortField) // Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    else
                    {
                        sortDirection = "desc"; // Default new sort fields to descending
                    }
                    sortField = actionButton; // Sort by the button clicked
                }
            }

            strategies = sortField switch
            {
                "Strategy Name" => sortDirection == "asc"
                    ? strategies.OrderBy(e => e.StrategyName)
                    : strategies.OrderByDescending(e => e.StrategyName),

                "Strategy Assignee" => sortDirection == "asc"
                    ? strategies.OrderBy(e => e.StrategyAssignee)
                    : strategies.OrderByDescending(e => e.StrategyAssignee),

                "Created Date" => sortDirection == "asc"
                    ? strategies.OrderBy(e => e.CreatedDate)
                    : strategies.OrderByDescending(e => e.CreatedDate),

                "Strategy Term" => sortDirection == "asc"
                    ? strategies.OrderBy(e => e.StrategyTerm)
                    : strategies.OrderByDescending(e => e.StrategyTerm),

                "Strategy Status" => sortDirection == "asc"
                    ? strategies.OrderBy(e => e.StrategyStatus)
                    : strategies.OrderByDescending(e => e.StrategyStatus),

                _ => strategies
            };

            // Apply filters and sorting feedback to the view
            if (numberFilters != 0)
            {
                // Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                // Show how many filters have been applied
                ViewData["numberFilters"] = $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)";
                // Keep the Bootstrap collapse open
                ViewData["ShowFilter"] = " show";
            }

            if (!string.IsNullOrEmpty(actionButton) && actionButton == "ExportExcel")
            {
                return ExportToExcel(strategies.ToList());
            }

            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {strategies.Count()}";

            // Handle paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Strategy>.CreateAsync(strategies.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // Export to Excel Action
        public IActionResult ExportToExcel(List<Strategy> strategies)
        {
            //// Get the data you want to export
            //var strategies = _context.Strategies.ToList();

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Strategies");

                // Add the header row
                worksheet.Cells[1, 1].Value = "Strategy Name";
                worksheet.Cells[1, 2].Value = "Assignee";
                worksheet.Cells[1, 3].Value = "Note";
                worksheet.Cells[1, 4].Value = "Created Date";
                worksheet.Cells[1, 5].Value = "Term";
                worksheet.Cells[1, 6].Value = "Status";

                // Add data rows
                for (int i = 0; i < strategies.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = strategies[i].StrategyName;
                    worksheet.Cells[i + 2, 2].Value = strategies[i].StrategyAssignee;
                    worksheet.Cells[i + 2, 3].Value = strategies[i].StrategyNote;
                    worksheet.Cells[i + 2, 4].Value = strategies[i].CreatedDate.ToString("yyyy-MM-dd"); // Format the Date
                    worksheet.Cells[i + 2, 5].Value = strategies[i].StrategyTerm;
                    worksheet.Cells[i + 2, 6].Value = strategies[i].StrategyStatus;
                }

                // Set the response headers for the Excel file
                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Strategies.xlsx");
            }
        }


        [HttpPost]
        public IActionResult ImportData(IFormFile file)
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
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Access the first worksheet
                        int rowCount = worksheet.Dimension.Rows; // Get the number of rows in the worksheet

                        List<Strategy> strategies = new List<Strategy>();

                        for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip header row
                        {
                            var strategy = new Strategy
                            {
                                StrategyName = worksheet.Cells[row, 1].Text, // First column: StrategyName
                                StrategyAssignee = worksheet.Cells[row, 2].Text, // Second column: StrategyAssignee
                                StrategyNote = worksheet.Cells[row, 3].Text, // Third column: StrategyNote
                                CreatedDate = DateTime.Parse(worksheet.Cells[row, 4].Text), // Fourth column: CreatedDate
                                StrategyTerm = Enum.TryParse(worksheet.Cells[row, 5].Text, true, out StrategyTerm term) ? term : StrategyTerm.ShortTerm, // Fifth column: StrategyTerm
                                StrategyStatus = Enum.TryParse(worksheet.Cells[row, 6].Text, true, out StrategyStatus status) ? status : StrategyStatus.ToDo // Sixth column: StrategyStatus
                            };

                            strategies.Add(strategy); // Add the strategy object to the list
                        }

                        _context.Strategies.AddRange(strategies); // Add all strategies to the database context
                        _context.SaveChanges(); // Save the changes to the database

                        TempData["Success"] = "Data imported successfully!"; // Success message
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while importing the file: {ex.Message}"; // Error message
            }

            return RedirectToAction("Index"); // Redirect back to the Index page after import
        }

        // GET: Stratergy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strategy = await _context.Strategys
                .FirstOrDefaultAsync(m => m.ID == id);
            if (strategy == null)
            {
                return NotFound();
            }

            return View(strategy);
        }

        // GET: Stratergy/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stratergy/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StrategyName,StrategyAssignee,StrategyNote,CreatedDate,StrategyTerm,StrategyStatus")] Strategy strategy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(strategy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Strategy Created Successfully";

                return RedirectToAction(nameof(Index));
            }
            return View(strategy);
        }

        // GET: Stratergy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strategy = await _context.Strategys.FindAsync(id);
            if (strategy == null)
            {
                return NotFound();
            }
            return View(strategy);
        }

        // POST: Stratergy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var strategyToUpdate = await _context.Strategies
                .FirstOrDefaultAsync(s => s.ID == id);

            if (strategyToUpdate == null)
            {
                return NotFound();
            }

            // Attach RowVersion for concurrency tracking
            _context.Entry(strategyToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            // Try updating the model with user input
            if (ModelState.IsValid)
            {
                if (await TryUpdateModelAsync<Strategy>(
                    strategyToUpdate, "",
                    s => s.StrategyName, s => s.StrategyAssignee, s => s.StrategyNote,
                    s => s.CreatedDate, s => s.StrategyTerm, s => s.StrategyStatus))
                {
                    try
                    {
                        // Update the strategy record in the database
                        _context.Update(strategyToUpdate);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Strategy Updated Successfully";

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var clientValues = (Strategy)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();

                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError("", "The strategy was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (Strategy)databaseEntry.ToObject();
                            // Compare each field and provide feedback on changes
                            if (databaseValues.StrategyName != clientValues.StrategyName)
                                ModelState.AddModelError("StrategyName", $"Current value: {databaseValues.StrategyName}");
                            if (databaseValues.StrategyAssignee != clientValues.StrategyAssignee)
                                ModelState.AddModelError("StrategyAssignee", $"Current value: {databaseValues.StrategyAssignee}");
                            if (databaseValues.StrategyNote != clientValues.StrategyNote)
                                ModelState.AddModelError("StrategyNote", $"Current value: {databaseValues.StrategyNote}");
                            if (databaseValues.CreatedDate != clientValues.CreatedDate)
                                ModelState.AddModelError("CreatedDate", $"Current value: {databaseValues.CreatedDate}");
                            if (databaseValues.StrategyTerm != clientValues.StrategyTerm)
                                ModelState.AddModelError("StrategyTerm", $"Current value: {databaseValues.StrategyTerm}");
                            if (databaseValues.StrategyStatus != clientValues.StrategyStatus)
                                ModelState.AddModelError("StrategyStatus", $"Current value: {databaseValues.StrategyStatus}");

                            ModelState.AddModelError("", "The record was modified by another user after you started editing. If you still want to save your changes, click the Save button again.");
                            strategyToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
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

            return View(strategyToUpdate);
        }


        // GET: Stratergy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strategy = await _context.Strategys
                .FirstOrDefaultAsync(m => m.ID == id);
            if (strategy == null)
            {
                return NotFound();
            }

            return View(strategy);
        }

        // POST: Stratergy/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var strategy = await _context.Strategys.FindAsync(id);
            if (strategy == null)
            {
                return Json(new { success = false, message = "Strategy not found." });
            }

            _context.Strategys.Remove(strategy);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Strategy Deleted Successfully";


            return Json(new { success = true });
        }


        private bool StrategyExists(int id)
        {
            return _context.Strategys.Any(e => e.ID == id);
        }
        public async Task<IActionResult> GetStratergyPreview(int id)
        {
            var member = await _context.Strategies.FirstOrDefaultAsync(m => m.ID == id); // Use async version for better performance

            if (member == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_StratergyPreview", member); // Ensure the partial view name matches
        }


        [HttpPost]
        public async Task<IActionResult> SaveStratergyNote(int id, string note)
        {
            var memberToUpdate = await _context.Strategies.FirstOrDefaultAsync(m => m.ID == id);

            if (memberToUpdate == null)
            {
                return Json(new { success = false, message = "Stratergy not found." });
            }

            // Update MemberNote
            memberToUpdate.StrategyNote = note;

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
        public IActionResult ExportSelectedStrategiesFields(List<string>? selectedFields, string? createdDate, string? SearchString,
                                                     string? StrategyAssigneeFilter, string? StrategyTermFilter,
                                                     string? StrategyStatusFilter, bool applyFilters)
        {
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            var strategies = _context.Strategies.AsQueryable();

            // Apply filters if 'applyFilters' is true
            if (applyFilters)
            {
                // Filter by Created Date
                if (!string.IsNullOrEmpty(createdDate))
                {
                    DateTime filterDate;
                    if (DateTime.TryParse(createdDate, out filterDate))
                    {
                        strategies = strategies.Where(s => s.CreatedDate.Date == filterDate.Date);
                    }
                }

                // Filter by Strategy Assignee
                if (!string.IsNullOrEmpty(StrategyAssigneeFilter))
                {
                    strategies = strategies.Where(p => p.StrategyAssignee.ToUpper().Contains(StrategyAssigneeFilter.ToUpper()));
                }

                // Filter by Strategy Term (Enum filtering)
                if (!string.IsNullOrEmpty(StrategyTermFilter))
                {
                    if (Enum.TryParse(StrategyTermFilter, out StrategyTerm term))
                    {
                        strategies = strategies.Where(s => s.StrategyTerm == term);
                    }
                }

                // Filter by Strategy Status (Enum filtering)
                if (!string.IsNullOrEmpty(StrategyStatusFilter))
                {
                    if (Enum.TryParse(StrategyStatusFilter, out StrategyStatus status))
                    {
                        strategies = strategies.Where(s => s.StrategyStatus == status);
                    }
                }

                // Filter by Search String
                if (!string.IsNullOrEmpty(SearchString))
                {
                    strategies = strategies.Where(p => p.StrategyName.ToUpper().Contains(SearchString.ToUpper()) ||
                                                        p.StrategyAssignee.ToUpper().Contains(SearchString.ToUpper()));
                }
            }

            // Convert to list after applying filters
            var filteredStrategies = strategies.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Strategies");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Strategies Export";
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
                foreach (var strategy in filteredStrategies)
                {
                    col = 1;

                    if (selectedFields.Contains("StrategyName"))
                        worksheet.Cells[row, col++].Value = strategy.StrategyName ?? "N/A";

                    if (selectedFields.Contains("StrategyAssignee"))
                        worksheet.Cells[row, col++].Value = strategy.StrategyAssignee ?? "N/A";

                    if (selectedFields.Contains("StrategyNote"))
                        worksheet.Cells[row, col++].Value = strategy.StrategyNote ?? "N/A";

                    if (selectedFields.Contains("CreatedDate"))
                        worksheet.Cells[row, col++].Value = strategy.CreatedDate.ToString("yyyy-MM-dd") ?? "N/A";

                    if (selectedFields.Contains("StrategyTerm"))
                        worksheet.Cells[row, col++].Value = strategy.StrategyTerm.ToString() ?? "N/A";

                    if (selectedFields.Contains("StrategyStatus"))
                        worksheet.Cells[row, col++].Value = strategy.StrategyStatus.ToString() ?? "N/A";

                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"StrategiesExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

       

    }
}
