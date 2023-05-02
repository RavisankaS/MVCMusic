using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Music.Data;
using MVC_Music.Models;
using MVC_Music.Utilities;

namespace MVC_Music.Controllers
{
    [Authorize]
    public class MusicianDocumentsController : CustomControllers.ElephantController
    {
        private readonly MusicContext _context;

        public MusicianDocumentsController(MusicContext context)
        {
            _context = context;
        }

        // GET: MusicianDocuments
        public async Task<IActionResult> Index(int? page, int? pageSizeID,
            int? MusicianID, string SearchString)
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Asume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = "btn-danger" if true;

            var musicianDocuments =from d in _context.MusicianDocuments
                .Include(m => m.Musician)
                select d;

            if (MusicianID.HasValue)
            {
                musicianDocuments = musicianDocuments.Where(p => p.MusicianID == MusicianID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                musicianDocuments = musicianDocuments.Where(p => p.FileName.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            // Always sort by File Name
            musicianDocuments = musicianDocuments.OrderBy(m => m.FileName);

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MusicianDocument>.CreateAsync(musicianDocuments.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MusicianDocuments/Edit/5
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MusicianDocuments == null)
            {
                return NotFound();
            }

            var musicianDocument = await _context.MusicianDocuments
                .Include(m => m.Musician)
                .AsNoTracking()
                .FirstOrDefaultAsync(m=>m.ID==id);
            if (musicianDocument == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(musicianDocument);
            return View(musicianDocument);
        }

        // POST: MusicianDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var musicianDocumentToUpdate = await _context.MusicianDocuments
                .Include(m => m.Musician)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (musicianDocumentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<MusicianDocument>(musicianDocumentToUpdate, "",
                d => d.FileName, d => d.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MusicianDocumentExists(musicianDocumentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save the update. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateDropDownLists(musicianDocumentToUpdate);
            return View(musicianDocumentToUpdate);
        }

        // GET: MusicianDocuments/Delete/5
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MusicianDocuments == null)
            {
                return NotFound();
            }

            var musicianDocument = await _context.MusicianDocuments
                .Include(m => m.Musician)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (musicianDocument == null)
            {
                return NotFound();
            }

            return View(musicianDocument);
        }

        // POST: MusicianDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MusicianDocuments == null)
            {
                return Problem("Entity set 'MusicContext.MusicianDocuments'  is null.");
            }
            var musicianDocument = await _context.MusicianDocuments.FindAsync(id);
            try
            {
                if (musicianDocument != null)
                {
                    _context.MusicianDocuments.Remove(musicianDocument);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save the update. Try again, and if the problem persists see your system administrator.");
            }
            return View(musicianDocument);

        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private SelectList MusicianSelectList(int? id)
        {
            var dQuery = from d in _context.Musicians
                         orderby d.LastName, d.FirstName
                         select d;
            return new SelectList(dQuery, "ID", "FormalName", id);
        }
        private void PopulateDropDownLists(MusicianDocument musicianDocument = null)
        {
            ViewData["MusicianID"] = MusicianSelectList(musicianDocument?.MusicianID);
        }

        private bool MusicianDocumentExists(int id)
        {
          return _context.MusicianDocuments.Any(e => e.ID == id);
        }
    }
}
