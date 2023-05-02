using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MVC_Music.Data;
using MVC_Music.Models;
using MVC_Music.Utilities;
using OfficeOpenXml;
using Instrument = MVC_Music.Models.Instrument;

namespace MVC_Music.Controllers
{
    [Authorize]
    public class InstrumentsController : CustomControllers.ElephantController
    {
        private readonly MusicContext _context;

        public InstrumentsController(MusicContext context)
        {
            _context = context;
        }

        // GET: Instruments
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            var instruments = _context.Instruments
                .Include(i => i.Musicians)
                .Include(i => i.Plays).ThenInclude(p => p.Musician)
                .OrderBy(i => i.Name)
                .AsNoTracking();

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "musicians");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Instrument>.CreateAsync(instruments.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Instruments/Details/5
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Instruments == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instruments
                .Include(i => i.Musicians)
                .Include(i => i.Plays).ThenInclude(p => p.Musician)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        // GET: Instruments/Create
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public IActionResult Create()
        {
            var instrument = new Instrument();
            PopulatePlaysInstrumentData(instrument);
            return View();
        }

        
        // POST: Instruments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Instrument instrument, string[] selectedOptions)
        {
            try
            {
                UpdatePlays(selectedOptions, instrument);
                if (ModelState.IsValid)
                {
                    _context.Add(instrument);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { instrument.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulatePlaysInstrumentData(instrument);
            return View(instrument);
        }

        // GET: Instruments/Edit/5
        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Instruments == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instruments
                .Include(i => i.Plays).ThenInclude(p => p.Musician)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instrument == null)
            {
                return NotFound();
            }
            PopulatePlaysInstrumentData(instrument);
            return View(instrument);
        }

        // POST: Instruments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
            //Go get the Instrument to update
            var instrumentToUpdate = await _context.Instruments
                .Include(i => i.Plays).ThenInclude(p => p.Musician)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (instrumentToUpdate == null)
            {
                return NotFound();
            }

            UpdatePlays(selectedOptions, instrumentToUpdate);

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Instrument>(instrumentToUpdate, "",
                d => d.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { instrumentToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstrumentExists(instrumentToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }

            PopulatePlaysInstrumentData(instrumentToUpdate);
            return View(instrumentToUpdate);
        }

        // GET: Instruments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Instruments == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instruments
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        // POST: Instruments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Instruments == null)
            {
                return Problem("Entity set 'MusicContext.Instruments'  is null.");
            }
            var instrument = await _context.Instruments
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (instrument != null)
                {
                    _context.Instruments.Remove(instrument);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Instrument. Remember, you cannot delete an Instrument that any Musician plays.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(instrument);
        }


        private void PopulatePlaysInstrumentData(Instrument instrument)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Musicians;
            var currentOptionsHS = new HashSet<int>(instrument.Plays.Select(b => b.MusicianID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var m in allOptions)
            {
                if (currentOptionsHS.Contains(m.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FormalName
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = m.ID,
                        DisplayText = m.FormalName
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdatePlays(string[] selectedOptions, Instrument instrumentToUpdate)
        {
            if (selectedOptions == null)
            {
                instrumentToUpdate.Plays = new List<Play>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(instrumentToUpdate.Plays.Select(b => b.MusicianID));
            foreach (var m in _context.Musicians)
            {
                if (selectedOptionsHS.Contains(m.ID.ToString()))//Musician is selected
                {
                    if (!currentOptionsHS.Contains(m.ID))//but not currently in the Instrument's collection - Add it!
                    {
                        instrumentToUpdate.Plays.Add(new Play
                        {
                            MusicianID = m.ID,
                            InstrumentID = instrumentToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(m.ID))//but is currently in the Instrument's collection - Remove it!
                    {
                        Play musicianToRemove = instrumentToUpdate.Plays.FirstOrDefault(d => d.MusicianID == m.ID);
                        _context.Remove(musicianToRemove);
                    }
                }
            }
        }

        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            //Note: This is a very basic example and has 
            //no ERROR HANDLING.  It also assumes that
            //duplicate values are allowed, both in the 
            //uploaded data and the DbSet.
            ExcelPackage excel;
            using (var memoryStream = new MemoryStream())
            {
                await theExcel.CopyToAsync(memoryStream);
                excel = new ExcelPackage(memoryStream);
            }
            var workSheet = excel.Workbook.Worksheets[0];
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            //Start a new list to hold imported objects
            List<Instrument> instruments = new List<Instrument>();

            for (int row = start.Row; row <= end.Row; row++)
            {
                // Row by row...
                Instrument a = new Instrument
                {
                    Name = workSheet.Cells[row, 1].Text
                };
                instruments.Add(a);
            }
            _context.Instruments.AddRange(instruments);
            _context.SaveChanges();
            return RedirectToAction("Index", new { Tab = "Instruments-Tab" });
        }

        private bool InstrumentExists(int id)
        {
          return _context.Instruments.Any(e => e.ID == id);
        }
    }
}
