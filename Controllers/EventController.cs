using CloudDevelopmentPOE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudDevelopmentPOE1.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Event/Index
        public async Task<IActionResult> Index()
        {
            var events = await _context.Event.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        // GET: Event/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            ViewData["Venues"] = _context.Venue.ToList();
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Venues"] = _context.Venue.ToList();
            return View(@event);
        }

        // GET: Event/Edit/{id}

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["Venues"] = _context.Venue.ToList();
            return View(@event);
        }

        // POST: Event/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Event.Any(e => e.EventId == @event.EventId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Venues"] = _context.Venue.ToList();
            return View(@event);
        }

        // GET: Event/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Event/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            if (@event == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check for existing bookings
            bool hasEventBookings = await _context.Booking.AnyAsync(b => b.EventId == id);
            if (hasEventBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event as it has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                // Log the error (optional)
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Error deleting the event.";
            }

            return RedirectToAction(nameof(Index));
        }


           
        }


    }


