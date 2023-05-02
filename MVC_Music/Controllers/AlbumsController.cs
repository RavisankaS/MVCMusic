using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Music.Data;
using MVC_Music.Models;

namespace MVC_Music.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly MusicContext _context;

        public AlbumsController(MusicContext context)
        {
            _context = context;
        }

        // GET: Albums
        public async Task<IActionResult> Index()
        {
            var albums = _context.Albums
                .Include(a => a.Genre)
                .Include(a=>a.Songs)
                .AsNoTracking();

            return View(await albums.ToListAsync());
        }

        // GET: Albums/Details/5
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Genre)
                .Include(a => a.Songs)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,YearProduced,Price,GenreID")] Album album)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(album);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to create Album. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(album);
            return View(album);
        }

        // GET: Albums/Edit/5
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(album);
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var albumToUpdate = await _context.Albums
                .FirstOrDefaultAsync(m => m.ID == id);
            if (albumToUpdate == null)
            {
                return NotFound();
            }

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(albumToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Album>(albumToUpdate, "",
                d => d.Name, d => d.YearProduced, d => d.Price, d => d.GenreID))
            {
                try
                {
                    _context.Update(albumToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Album)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Album was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Album)databaseEntry.ToObject();
                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", "Current value: "
                                + databaseValues.Name);
                        if (databaseValues.YearProduced != clientValues.YearProduced)
                            ModelState.AddModelError("YearProduced", "Current value: "
                                + databaseValues.YearProduced);
                        if (databaseValues.Price != clientValues.Price)
                            ModelState.AddModelError("Price", "Current value: "
                                + String.Format("{0:c}", databaseValues.Price));
                        //For the foreign key, we need to go to the database to get the information to show
                        if (databaseValues.GenreID != clientValues.GenreID)
                        {
                            Genre databaseGenre = await _context.Genres.FirstOrDefaultAsync(i => i.ID == databaseValues.GenreID);
                            ModelState.AddModelError("GenreID", $"Current value: {databaseGenre?.Name}");
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to Album List' hyperlink.");
                        albumToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes to the Album. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateDropDownLists(albumToUpdate);
            return View(albumToUpdate);
        }

        // GET: Albums/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Albums == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Genre)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Albums == null)
            {
                return Problem("Entity set 'Albums' is null.");
            }
            var album = await _context.Albums
                .Include(a => a.Genre)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (album != null)
                {
                    _context.Albums.Remove(album);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Album. Remember, you cannot delete a Album with Songs on it.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(album);
        }


        private SelectList GenreList(int? selectedId)
        {
            return new SelectList(_context
                .Genres
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }

        private void PopulateDropDownLists(Album album = null)
        {
            ViewData["GenreID"] = GenreList(album?.GenreID);
        }


        private bool AlbumExists(int id)
        {
          return _context.Albums.Any(e => e.ID == id);
        }
    }
}
