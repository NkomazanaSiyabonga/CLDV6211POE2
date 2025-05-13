using CloudDevelopmentPOE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CloudDevelopmentPOE1.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Booking/Index
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool isDoubleBooked = await _context.Booking
                    .AnyAsync(b => b.VenueId == booking.VenueId && b.BookingDate.Date == booking.BookingDate.Date);

                if (isDoubleBooked)
                {
                    ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date.");
                    PopulateDropdowns(booking);
                    return View(booking);
                }

                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(booking);
            return View(booking);
        }

        // GET: Booking/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            PopulateDropdowns(booking);
            return View(booking);
        }

        // POST: Booking/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    else throw;
                }
            }

            PopulateDropdowns(booking);
            return View(booking);
        }

        // GET: Booking/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Booking/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/DeleteConfirmed/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }

        private void PopulateDropdowns(Booking? booking = null)
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "Name", booking?.VenueId);
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "Name", booking?.EventId);
        }
    }
}
