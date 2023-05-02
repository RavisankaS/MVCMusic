using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC_Music.Data;
using MVC_Music.Models;
using MVC_Music.Utilities;
using MVC_Music.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;


namespace MVC_Music.Controllers
{
    [Authorize]
    public class SongPerformancesController : CustomControllers.ElephantController
    {
        private readonly MusicContext _context;

        public SongPerformancesController(MusicContext context)
        {
            _context = context;
        }

        // GET: SongPerformances
        public async Task<IActionResult> Index(int? SongID, int? page, int? pageSizeID,
            int? MusicianID, int? InstrumentID, string SearchString, string actionButton, 
            string sortDirection = "desc", string sortField = "Musician")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Get the URL with the last filter, sort and page parameters from THE SONG Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Songs");

            if (!SongID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Asume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = "btn-danger" if true;

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Musician", "Instrument", "Fee Paid" };

            var performances =from p in _context.Performances
                .Include(p => p.Instrument)
                .Include(p => p.Musician)
                .Include(p => p.Song)
                where p.SongID== SongID.GetValueOrDefault()
                select p;

            //Now get the MASTER record, the Song, so it can be displayed at the top of the screen
            var song = await _context.Songs
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == SongID.GetValueOrDefault());
            ViewBag.Song = song;

            if (MusicianID.HasValue)
            {
                performances = performances.Where(p => p.MusicianID == MusicianID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (InstrumentID.HasValue)
            {
                performances = performances.Where(p => p.InstrumentID == InstrumentID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                performances = performances.Where(p => p.Comments.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Instrument")
            {
                if (sortDirection == "asc")
                {
                    performances = performances
                        .OrderBy(p => p.Instrument.Name)
                        .ThenBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName); 
                }
                else
                {
                    performances = performances
                        .OrderByDescending(p => p.Instrument.Name)
                        .ThenBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName);
                }
            }
            else if (sortField == "Fee Paid")
            {
                if (sortDirection == "asc")
                {
                    performances = performances
                        .OrderBy(p => p.FeePaid)
                        .ThenBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName);
                }
                else
                {
                    performances = performances
                        .OrderByDescending(p => p.FeePaid)
                        .ThenBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName);
                }
            }
            else //Musician
            {
                if (sortDirection == "asc")
                {
                    performances = performances
                        .OrderBy(p => p.Musician.LastName)
                        .ThenBy(p => p.Musician.FirstName);
                }
                else
                {
                    performances = performances
                        .OrderByDescending(p => p.Musician.LastName)
                        .ThenByDescending(p => p.Musician.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Performance>.CreateAsync(performances.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Performances/Add
        public IActionResult Add(int? SongID, string SongTitle)
        {

            if (!SongID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }
            ViewData["SongTitle"] = SongTitle;

            Performance a = new()
            {
                SongID = SongID.GetValueOrDefault()
            };

            PopulateDropDownLists();
            return View(a);
        }

        // POST: Performances/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,MusicianID,InstrumentID,Comments," +
            "FeePaid,SongID")] Performance performance, string SongTitle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(performance);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to add performance. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            PopulateDropDownLists(performance);
            ViewData["SongTitle"] = SongTitle;
            return View(performance);
        }

        // GET: SongPerformance/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);
            if (performance == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(performance);
            return View(performance);
        }

        // POST: SongPerformance/Update/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var performanceToUpdate = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (performanceToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Performance>(performanceToUpdate, "",
                p => p.Comments, p => p.MusicianID, p => p.FeePaid, p => p.InstrumentID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!PerformanceExists(performanceToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }
            PopulateDropDownLists(performanceToUpdate);
            return View(performanceToUpdate);
        }

        // GET: SongPerformance/Remove/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var performance = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);
            if (performance == null)
            {
                return NotFound();
            }
            return View(performance);
        }

        // POST: SongPerformance/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var performance = await _context.Performances
               .Include(a => a.Musician)
               .Include(a => a.Instrument)
               .Include(a => a.Song)
               .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                _context.Performances.Remove(performance);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            return View(performance);
        }

        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public IActionResult DownloadPerformances()
        {
            //Get the Performances
            var sumQ = _context.Performances.Include(a => a.Musician).Include(a => a.Song)
                .GroupBy(a => new { a.MusicianID, a.Musician.LastName, a.Musician.FirstName, a.Musician.MiddleName })
                .Select(grp => new PerformanceSummaryVM
                {
                    ID = grp.Key.MusicianID,
                    FullName = grp.Key.FirstName + " " + grp.Key.LastName,
                    AverageFeePaid = grp.Average(a => a.FeePaid),
                    HighestFeePaid = grp.Max(a => a.FeePaid),
                    LowestFeePaid = grp.Min(a => a.FeePaid),
                    NumberOfPerformances = grp.Count(),
                }).OrderBy(s => s.FullName);
            //How many rows?
            int numRows = sumQ.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    var workSheet = excel.Workbook.Worksheets.Add("Performances");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(sumQ, true);


                    //Style fee column for currency
                    workSheet.Column(4).Style.Numberformat.Format = "###,##0.00";

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    workSheet.Cells[4, 1, numRows + 3, 2].Style.Font.Bold = true;

                    //Note: these are fine if you are only 'doing' one thing to the range of cells.
                    //Otherwise you should USE a range object for efficiency
                    using (ExcelRange NumberOfPerformances = workSheet.Cells[numRows + 6, 6])//
                    {
                        NumberOfPerformances.Formula = "Sum(" + workSheet.Cells[6, 6].Address + ":" + workSheet.Cells[numRows + 5, 6].Address + ")";
                        NumberOfPerformances.Style.Font.Bold = true;
                        NumberOfPerformances.Style.Numberformat.Format = "Total =" + " " + "##";
                    }

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 6])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }
                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Performance Report";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 6])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 4])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    try
                    {
                        Byte[] theData = excel.GetAsByteArray();
                        string filename = "Performance.xlsx";
                        string mimeType = "performance/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(theData, mimeType, filename);
                    }
                    catch (Exception)
                    {
                        return BadRequest("Could not build and download the file.");
                    }
                }
            }
            return NotFound("No data.");
        }

        [Authorize(Roles = "Staff,Supervisor,Admin")]
        public async Task<IActionResult> PerformanceSummary(int? page, int? pageSizeID)
        {
            var sumQ = _context.Performances.Include(a => a.Musician).Include(a => a.Song)
                .GroupBy(a => new { a.MusicianID, a.Musician.LastName, a.Musician.FirstName, a.Musician.MiddleName })
                .Select(grp => new PerformanceSummaryVM
                {
                    FullName = grp.Key.FirstName + " " +grp.Key.LastName,
                    AverageFeePaid = grp.Average(a => a.FeePaid),
                    HighestFeePaid = grp.Max(a => a.FeePaid),
                    LowestFeePaid = grp.Min(a => a.FeePaid),
                    NumberOfPerformances = grp.Count(),
                }).OrderBy(s => s.FullName);

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "PerformanceSummary");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<PerformanceSummaryVM>.CreateAsync(sumQ.AsNoTracking(), page ?? 1, pageSize);
            return View(pagedData);
        }

        private SelectList MusicianList(int? selectedId)
        {
            return new SelectList(_context
                .Musicians
                .OrderBy(m => m.LastName)
                .ThenBy(m=>m.FirstName), "ID", "FormalName", selectedId);
        }
        private SelectList InstrumentList(int? selectedId)
        {
            return new SelectList(_context
                .Instruments
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Performance performance = null)
        {
            ViewData["MusicianID"] = MusicianList(performance?.MusicianID);
            ViewData["InstrumentID"] = InstrumentList(performance?.InstrumentID);
        }


        private bool PerformanceExists(int id)
        {
          return _context.Performances.Any(e => e.ID == id);
        }
    }
}
