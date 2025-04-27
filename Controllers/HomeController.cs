using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.CustomControllers;
using NIA_CRM.Data;
using NIA_CRM.Models;
using NIA_CRM.Utilities;
using NIA_CRM.ViewModels;
using SQLitePCL;

namespace NIA_CRM.Controllers
{
    [Authorize]
    public class HomeController : ElephantController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NIACRMContext _context;
        public HomeController(ILogger<HomeController> logger, NIACRMContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult ClearSuccessMessage()
        {
            TempData["Success"] = null;
            return Ok();
        }

        [HttpPost]
        public IActionResult ClearTempData()
        {
            TempData["Error"] = null; // Clear the error message
            return Ok(); // Respond with a successful status
        }

        public async Task<IActionResult> Index()
        {
            var addresses = await _context.Members
                                            .Include(m => m.Address)
                                            .Where(m => m.Address != null) // Ensure no null addresses
                                            .ToListAsync();

            var cityCounts = await _context.Members
                                            .Include(m => m.Address)
                                            .Where(m => m.Address != null)
                                            .GroupBy(m => m.Address.City)
                                            .Select(g => new { City = g.Key, Count = g.Count() })
                                            .ToListAsync();

            // Debugging line (optional)
            Console.WriteLine("City counts: " + string.Join(", ", cityCounts.Select(c => $"{c.City}: {c.Count}")));



            var membershipCount = await _context.MemberMembershipTypes
                                                .GroupBy(mmt => mmt.MembershipType)
                                                .Select(g => new
                                                {
                                                    MembershipType = g.Key,
                                                    Count = g.Count()
                                                })
                                                .ToArrayAsync();

            var sectorCount = await _context.MemberSectors
                                            .Include(ms => ms.Sector)
                                            .GroupBy(ms => ms.Sector.SectorName)
                                            .Select(g => new
                                            {
                                                SectorName = g.Key,
                                                Count = g.Count()
                                            })
                                            .ToArrayAsync();


            var tagCount = await _context.MemberTags
                                            .Include(mt => mt.MTag)
                                            .GroupBy(mt => mt.MTag.MTagName)
                                            .Select(g => new
                                            {
                                                TagName = g.Key,
                                                Count = g.Count()
                                            })
                                            .ToArrayAsync();


            var archivedMemberCount = await _context.Cancellations
                                                     .Where(c => c.IsCancelled == true)
                                                     .CountAsync();


            var totalMemberCount = await _context.Members.CountAsync();

            var activeMemberCount = totalMemberCount - archivedMemberCount;




            var memberJoinDates = await _context.Members
                                                 .GroupBy(m => new { m.JoinDate.Year, m.JoinDate.Month })  // No need to check for null
                                                 .Select(g => new
                                                 {
                                                     Year = g.Key.Year,
                                                     Month = g.Key.Month,
                                                     MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                                                     Count = g.Count()
                                                 }).ToListAsync();
            // Execute the query asynchronously
            var memberAddress = await _context.Members
                                                .Include(m => m.Address)  // Include the Address navigation property
                                                .GroupBy(m => m.Address.City)  // Group by the City in Address
                                                .Select(g => new
                                                {
                                                    City = g.Key,
                                                    Count = g.Count()  // Count how many members have an address in each city
                                                })
                                                .ToListAsync();  // Execute the query asynchronously


            double retentionRate = 0;
            if (totalMemberCount > 0)
            {
                retentionRate = Math.Round((double)activeMemberCount / totalMemberCount * 100, 2);
            }

            // Pass cityCounts as a model or through ViewData
            ViewData["CityCounts"] = cityCounts;
            ViewData["MemberCount"] = await _context.Members.CountAsync();
            ViewData["MembershipCount"] = membershipCount;
            ViewData["MembersJoins"] = memberJoinDates;
            ViewData["MembersAddress"] = memberAddress;
            ViewData["RetentionRate"] = retentionRate;
            ViewData["ActiveMemberCount"] = activeMemberCount;  // Pass the count to the view
            ViewData["ArchivedMemberCount"] = archivedMemberCount;  // Pass the count to the view
            ViewData["TagCount"] = tagCount;  // Pass data to the view
            ViewData["SectorCount"] = sectorCount;  // Pass data to the view



            return View(); // Pass nothing if using ViewData, or you can directly pass data via View()
        }


        [HttpGet]
        [Route("Home/GetDashboardLayout")]
        public async Task<IActionResult> GetDashboardLayout()

        {
            string userId = User.Identity.Name;
            var layout = await _context.DashboardLayouts
                .FirstOrDefaultAsync(dl => dl.UserId == userId);

            if (layout != null)
            {
                return Ok(new { layoutData = layout.LayoutData });
            }

            return NotFound(new { message = "No layout found" });
        }

        [HttpPost]
        [Route("Home/SaveDashboardLayout")]
        public async Task<IActionResult> SaveDashboardLayout([FromBody] DashboardLayout layout)

        {
            string userId = User.Identity.Name;
            var existingLayout = await _context.DashboardLayouts
                .FirstOrDefaultAsync(dl => dl.UserId == userId);

            if (existingLayout != null)
            {
                existingLayout.LayoutData = layout.LayoutData;
                _context.DashboardLayouts.Update(existingLayout);
            }
            else
            {
                layout.UserId = userId;
                _context.DashboardLayouts.Add(layout);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }


        [HttpGet]
        public IActionResult PingSession()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ForceLogout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Clear all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Redirect to login page without ReturnUrl
            return Redirect("/Identity/Account/Login");
        }
    }
}
