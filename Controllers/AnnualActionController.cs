using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using OfficeOpenXml;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class AnnualActionController : ElephantController
    {
        private readonly NIACRMContext _context;

        public AnnualActionController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: AnnualAction
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? createdDate, string? SearchString, string? Assignee, string? AnnualStatusFilter, string? actionButton,
                                        string sortDirection = "desc", string sortField = "Annual Actions Name")
        {
            string[] sortOptions = new[] { "Annual Actions Name", "Date", "Assignee", "Status" };
            int numberFilters = 0;

            var actions = _context.AnnualActions.AsQueryable();

            // Filter by Created Date
            if (!string.IsNullOrEmpty(createdDate))
            {
                DateTime filterDate;
                if (DateTime.TryParse(createdDate, out filterDate))
                {
                    actions = actions.Where(s => s.Date == filterDate.Date);
                    numberFilters++;
                    ViewData["DateFilter"] = createdDate;
                }
            }

            // Filter by Annual Action Name
            if (!String.IsNullOrEmpty(SearchString))
            {
                actions = actions.Where(p => p.Name.ToString().ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
                ViewData["SearchString"] = SearchString;
            }

            // Filter by Assignee
            if (!string.IsNullOrEmpty(Assignee))
            {
                actions = actions.Where(p => p.Asignee.ToUpper().Contains(Assignee.ToUpper()));
                numberFilters++;
                ViewData["AssigneeFilter"] = Assignee;
            }

            // Filter by Annual Status
            if (!string.IsNullOrEmpty(AnnualStatusFilter))
            {
                if (Enum.TryParse<AnnualStatus>(AnnualStatusFilter, out var annualStatus))
                {
                    actions = actions.Where(p => p.AnnualStatus == annualStatus);
                    numberFilters++;
                    ViewData["AnnualStatusFilter"] = AnnualStatusFilter;
                }
            }

            // Handle Sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    else
                    {
                        sortDirection = "desc";
                    }
                    sortField = actionButton;
                }
            }

            actions = sortField switch
            {
                "Annual Actions Name" => sortDirection == "asc" ? actions.OrderBy(e => e.Name) : actions.OrderByDescending(e => e.Name),
                "Date" => sortDirection == "asc" ? actions.OrderBy(e => e.Date) : actions.OrderByDescending(e => e.Date),
                "Assignee" => sortDirection == "asc" ? actions.OrderBy(e => e.Asignee) : actions.OrderByDescending(e => e.Asignee),
                "Status" => sortDirection == "asc" ? actions.OrderBy(e => e.AnnualStatus) : actions.OrderByDescending(e => e.AnnualStatus),
                _ => actions
            };

            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numberFilters"] = $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)";
                @ViewData["ShowFilter"] = " show";
            }

            if (!string.IsNullOrEmpty(actionButton) && actionButton == "ExportExcel")
            {
                return ExportToExcel(actions.ToList());
            }

            ViewData["SortDirection"] = sortDirection;
            ViewData["SortField"] = sortField;
            ViewData["numberFilters"] = numberFilters;
            ViewData["records"] = $"Records Found: {actions.Count()}";

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<AnnualAction>.CreateAsync(actions.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // New action to export to Excel
        private IActionResult ExportToExcel(List<AnnualAction> actions)
        {
            // Get the data you want to export
            var annualActions = _context.AnnualActions.ToList();

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Annual Actions");

                // Add the header row
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Note";
                worksheet.Cells[1, 3].Value = "Date";
                worksheet.Cells[1, 4].Value = "Assignee";
                worksheet.Cells[1, 5].Value = "Annual Status";

                // Add data rows
                for (int i = 0; i < annualActions.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = annualActions[i].Name;
                    worksheet.Cells[i + 2, 2].Value = annualActions[i].Note;
                    worksheet.Cells[i + 2, 3].Value = annualActions[i].Date?.ToString("yyyy-MM-dd"); // Format the Date
                    worksheet.Cells[i + 2, 4].Value = annualActions[i].Asignee;
                    worksheet.Cells[i + 2, 5].Value = annualActions[i].AnnualStatus;
                }

                // Set the response headers
                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AnnualActions.xlsx");
            }
        }

        // Action to handle Excel file import
        [HttpPost]
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
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Accessing the first worksheet
                        int rowCount = worksheet.Dimension.Rows; // Get the number of rows in the worksheet

                        List<AnnualAction> annualActions = new List<AnnualAction>();

                        for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip header
                        {
                            var name = worksheet.Cells[row, 1].Text; // Assuming data is in the first column
                            var note = worksheet.Cells[row, 2].Text; // Second column
                            var date = DateTime.TryParse(worksheet.Cells[row, 3].Text, out DateTime parsedDate) ? parsedDate : (DateTime?)null; // Third column
                            var assignee = worksheet.Cells[row, 4].Text; // Fourth column
                            var status = worksheet.Cells[row, 5].Text; // Fifth column

                            // Create a new AnnualAction object and populate it with the data from the worksheet
                            var annualAction = new AnnualAction
                            {
                                Name = name,
                                Note = note,
                                Date = date,
                                Asignee = assignee,
                                AnnualStatus = Enum.TryParse(worksheet.Cells[row, 5].Value?.ToString(), true, out AnnualStatus annualStatus) ? annualStatus : AnnualStatus.ToDo
                            };

                            annualActions.Add(annualAction); // Add the object to the list
                        }

                        _context.AnnualActions.AddRange(annualActions); // Add the list of annual actions to the database context
                        _context.SaveChanges(); // Save the changes to the database

                        TempData["Success"] = "Annual actions imported successfully!"; // Success message
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while importing the file: {ex.Message}"; // Error message
            }

            return RedirectToAction("Index"); // Redirect back to the Index page after import
        }


        // GET: AnnualAction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualAction = await _context.AnnualAction
                .FirstOrDefaultAsync(m => m.ID == id);
            if (annualAction == null)
            {
                return NotFound();
            }

            return View(annualAction);
        }

        // GET: AnnualAction/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AnnualAction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Note,Date,Asignee,AnnualStatus")] AnnualAction annualAction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(annualAction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)//This is a Transaction in the Database!
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. " +
                    "Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                
            }

            return View(annualAction);
        }

        // GET: AnnualAction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualAction = await _context.AnnualAction.FindAsync(id);
            if (annualAction == null)
            {
                return NotFound();
            }
            return View(annualAction);
        }

        // POST: AnnualAction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
           

            // Fetch the existing entity from the database
            var actionToUpdate = await _context.AnnualActions.FirstOrDefaultAsync(a => a.ID == id);

            

            if (actionToUpdate == null)
            {
                return NotFound();
            }

            // Attach the RowVersion for concurrency tracking
            _context.Entry(actionToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<AnnualAction>(actionToUpdate, "",
                a => a.Name, a => a.Note, a => a.Date, a => a.Asignee, a => a.AnnualStatus))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Details", new { id = actionToUpdate.ID });

                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again later.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (AnnualAction)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "The record was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (AnnualAction)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", $"Current value: {databaseValues.Name}");
                        if (databaseValues.Note != clientValues.Note)
                            ModelState.AddModelError("Note", $"Current value: {databaseValues.Note}");
                        if (databaseValues.Date != clientValues.Date)
                            ModelState.AddModelError("Date", $"Current value: {databaseValues.Date:d}");
                        if (databaseValues.Asignee != clientValues.Asignee)
                            ModelState.AddModelError("Asignee", $"Current value: {databaseValues.Asignee}");
                        if (databaseValues.AnnualStatus != clientValues.AnnualStatus)
                            ModelState.AddModelError("AnnualStatus", $"Current value: {databaseValues.AnnualStatus}");

                        ModelState.AddModelError("", "The record was modified by another user after you started editing. " +
                            "If you still want to save your changes, click the Save button again.");

                        // Update RowVersion for the next attempt
                        actionToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    ModelState.AddModelError("", $"Unable to save changes: {message}");
                }
            }

            return View(actionToUpdate);
        }


        // GET: AnnualAction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualAction = await _context.AnnualAction
                .FirstOrDefaultAsync(m => m.ID == id);
            if (annualAction == null)
            {
                return NotFound();
            }

            return View(annualAction);
        }

        // POST: MEvent/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var annualAction = await _context.AnnualAction.FindAsync(id);

                if (annualAction == null)
                {
                    return Json(new { success = false, message = "Annual Action not found!" });
                }

                _context.AnnualAction.Remove(annualAction);
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ Annual Action Deleted Successfully from Partial View");
                TempData["Success"] = "Annual Action Deleted Successfully from Partial View!"; // Success message
                return Json(new { success = true, message = "Annual Action deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the Annual Action." });
            }
        }


        private bool AnnualActionExists(int id)
        {
            return _context.AnnualAction.Any(e => e.ID == id);
        }

        public async Task<IActionResult> GetAnnualActionPreview(int id)
        {
            var member = await _context.AnnualAction
                .FirstOrDefaultAsync(m => m.ID == id); // No .Include() needed

            if (member == null)
            {
                return NotFound(); // Return 404 if the member doesn't exist
            }

            return PartialView("_AnnualActionPreview", member); // Ensure the partial view name matches
        }


        [HttpPost]
        public async Task<IActionResult> SaveAnnualActionNote(int id, string note)
        {
            var annualActionToUpdate = await _context.AnnualActions.FirstOrDefaultAsync(m => m.ID == id);

            if (annualActionToUpdate == null)
            {
                return Json(new { success = false, message = "Annual Action not found." });
            }

            // Update MemberNote
            annualActionToUpdate.Note = note;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Note saved successfully!"; // Success message

                return Json(new { success = true, message = "Note saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }

        }

        [HttpPost]
        public IActionResult ExportSelectedAnnualActionsFields(List<string>? selectedFields, string? SearchString, DateTime? DateFilter,
                                                       string? AssigneeFilter, string? AnnualStatusFilter, bool applyFilters)
        {
            if (selectedFields == null || selectedFields.Count == 0)
            {
                TempData["Error"] = "Please select at least one field to export.";
                return RedirectToAction("Index");
            }

            // Fetch the filtered data
            var annualActions = _context.AnnualActions.AsQueryable();

            // Apply filters only if applyFilters is true
            if (applyFilters)
            {
                if (!string.IsNullOrEmpty(SearchString))
                {
                    annualActions = annualActions.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper()));
                }

                if (DateFilter.HasValue)
                {
                    annualActions = annualActions.Where(a => a.Date == DateFilter.Value.Date);
                }

                // Filter by Assignee if provided
                if (!string.IsNullOrEmpty(AssigneeFilter))
                {
                    annualActions = annualActions.Where(a => a.Asignee.ToUpper().Contains(AssigneeFilter.ToUpper()));
                }

                // Filter by Annual Status if provided
                if (!string.IsNullOrEmpty(AnnualStatusFilter))
                {
                    if (Enum.TryParse<AnnualStatus>(AnnualStatusFilter, out var annualStatus))
                    {
                        annualActions = annualActions.Where(a => a.AnnualStatus == annualStatus);
                    }
                }
            }

            var filteredActions = annualActions.ToList(); // Execute query

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AnnualActions");
                int col = 1;

                // **Adding a proper header row**
                worksheet.Cells[1, 1].Value = "Annual Actions Export";
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
                foreach (var action in filteredActions)
                {
                    col = 1;

                    if (selectedFields.Contains("ActionName"))
                        worksheet.Cells[row, col++].Value = action.Name ?? "N/A";

                    if (selectedFields.Contains("Note"))
                        worksheet.Cells[row, col++].Value = action.Note ?? "N/A";

                    if (selectedFields.Contains("Date"))
                        worksheet.Cells[row, col++].Value = action.Date.HasValue ? action.Date.Value.ToString("yyyy-MM-dd") : "N/A";

                    if (selectedFields.Contains("Assignee"))
                        worksheet.Cells[row, col++].Value = action.Asignee ?? "N/A";

                    if (selectedFields.Contains("AnnualStatus"))
                        worksheet.Cells[row, col++].Value = action.AnnualStatus.ToString() ?? "N/A";

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"AnnualActionsExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

    }
}
