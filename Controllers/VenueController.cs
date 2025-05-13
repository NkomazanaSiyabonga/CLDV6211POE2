using CloudDevelopmentPOE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Azure.Storage.Blobs;

namespace CloudDevelopmentPOE1.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Venue/Index
        public async Task<IActionResult> Index()
        {
            var venue = await _context.Venue.ToListAsync();
            return View(venue);
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {


                // Handle image upload to Azure Blob Storage if an image file was provided
                // This is Step 4C: Modify Controller to receive ImageFile from View (user upload)
                // This is Step 5: Upload selected image to Azure Blob Storage
                if (venue.ImageFile != null)
                {

                    // Upload image to Blob Storage (Azure)
                    var blobUrl = await UploadImageToBlobAsync(venue.ImageFile); //Main part of Step 5 B (upload image to Azure Blob Storage)

                    // Step 6: Save the Blob URL into ImageUrl property (the database)
                    venue.ImageUrl = blobUrl;
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Venue updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Update failed: {ex.Message}";
                }
            }

            return View(venue);
        }


        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            // Check for existing bookings
            bool hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue as it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=clouddevstorageacc;AccountKey=NVXYNnsre+irXFfYeJTLFV0LuXVi+FuT0TuIwT8GkY6u8dEkgu+cxRSXR5KdMCy48vLUebyLoxeh+AStDyTzhg==;EndpointSuffix=core.windows.net";
            var containerName = "cldvcontainer";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString(); // Return the URL
        }

    }

}
