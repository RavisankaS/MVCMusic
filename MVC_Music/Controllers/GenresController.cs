using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
    public class GenresController : Controller
    {
        private readonly MusicContext _context;

        public GenresController(MusicContext context)
        {
            _context = context;
        }

        // GET: Genres
        public async Task<IActionResult> Index()
        {
              return View(await _context.Genres
                  .AsNoTracking()
                  .ToListAsync());
        }

        // GET: Genres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Genres == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .FirstOrDefaultAsync(m => m.ID == id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // GET: Genres/Create
        [Authorize(Roles = "Supervisor,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Genres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Genre genre)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(genre);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to create the Genre. Try again, and if the problem persists see your system administrator.");
            }

            return View(genre);
        }

        // GET: Genres/Edit/5
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Genres == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        // POST: Genres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var genreToUpdate = await _context.Genres.FindAsync(id);

            //Check that you got it or exit with a not found error
            if (genreToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Genre>(genreToUpdate, "",
                d => d.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genreToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes to the Genre. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(genreToUpdate);
        }

        // GET: Genres/Delete/5
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Genres == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // POST: Genres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Genres == null)
            {
                return Problem("Entity set 'Genres' is null.");
            }
            var genre = await _context.Genres.FindAsync(id);
            try
            {
                if (genre != null)
                {
                    _context.Genres.Remove(genre);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Genre. Remember, you cannot delete an Genre specified for either a Album or Song.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(genre);
        }

        private bool GenreExists(int id)
        {
          return _context.Genres.Any(e => e.ID == id);
        }
    }
}
