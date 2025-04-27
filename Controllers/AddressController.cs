using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.Data;
using NIA_CRM.Models;

namespace NIA_CRM.Controllers
{
    public class AddressController : Controller
    {
        private readonly NIACRMContext _context;

        public AddressController(NIACRMContext context)
        {
            _context = context;
        }

        // GET: Address
        public async Task<IActionResult> Index()
        {
            var nIACRMContext = _context.Addresses.Include(a => a.Member);
            return View(await nIACRMContext.ToListAsync());
        }

        // GET: Address/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Address/Create
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
                    ViewBag.MemberName = member.MemberName;  // Store MemberName in ViewBag
                    ViewBag.MemberId = member.ID;  // Store MemberID for hidden input
                }
            }

            // If no member is preselected, show a dropdown for selecting a member
            ViewData["MemberId"] = new SelectList(_context.Members, "ID", "MemberName", memberId);

            return View();
        }


        // POST: Address/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberId,AddressLine1,AddressLine2,City,StateProvince,PostalCode")] Address address)
        {
            if (ModelState.IsValid)
            {
                _context.Add(address);
                await _context.SaveChangesAsync();
                // Pass isNewMember flag via ViewData to the next step (Contact Create)
                // Set 'IsNewMember' in TempData so that it can be used in the next request
                TempData["IsNewMember"] = true;  // Set it as true or false based on your flow
                TempData["SuccessMessage"] = $"Member Address Added Successfully!";

                return RedirectToAction(nameof(Create), "Contact", new { memberId = address.MemberId });
            }

            // Retrieve member info again to display banner if necessary
            var member = _context.Members
                .Where(m => m.ID == address.MemberId)
                .Select(m => new { m.ID, m.MemberName })
                .FirstOrDefault();

            if (member != null)
            {
                ViewBag.MemberName = member.MemberName;
                ViewBag.MemberId = member.ID;
            }

            // If no member is preselected, show a dropdown for selecting a member
            ViewData["MemberId"] = new SelectList(_context.Members, "ID", "MemberName", address.MemberId);

            return View(address);
        }


        // GET: Address/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            // Set the SelectList for MemberId dropdown
            ViewData["MemberId"] = new SelectList(_context.Members, "ID", "MemberName", address.MemberId);

            // Get the member name based on the address's MemberId
            var member = await _context.Members.FindAsync(address.MemberId);
            if (member != null)
            {
                ViewBag.MemberName = member.MemberName; // Set the member's name
            }
            else
            {
                ViewBag.MemberName = "No member name provided"; // Handle null or missing member
            }

            return View(address);
        }


        // POST: Address/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,AddressLine1,AddressLine2,City,StateProvince,PostalCode")] Address address)
        {
            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the address in the database
                    _context.Update(address);

                    // Retrieve the member's name to pass back to the view
                    var member = await _context.Members.FindAsync(address.MemberId);
                    if (member != null)
                    {
                        ViewBag.MemberName = member.MemberName; // Set the member's name for display
                    }
                    else
                    {
                        ViewBag.MemberName = "No member name provided"; // Handle null or missing member
                    }

                    // Save the changes to the database
                    await _context.SaveChangesAsync();

                    // Success message
                    TempData["SuccessMessage"] = "Member Address Updated Successfully!";

                    // Redirect to the Member's detail page
                    return RedirectToAction("Details", "Member", new { id = address.MemberId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // In case of invalid data, pass back the SelectList for MemberId
            ViewData["MemberId"] = new SelectList(_context.Members, "ID", "MemberName", address.MemberId);
            return View(address);
        }


        // GET: Address/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Address/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}
