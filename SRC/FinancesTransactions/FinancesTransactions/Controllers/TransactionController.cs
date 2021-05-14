using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinancesTransactions.Models;
using Microsoft.AspNetCore.Http;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;

namespace FinancesTransactions.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionDbContext _context;

        private IHostingEnvironment _env;

        public TransactionController(TransactionDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            return View(await _context.Transactions.OrderBy(x=> x.Date).ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionModel = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionModel == null)
            {
                return NotFound();
            }

            return View(transactionModel);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            TransactionModel transactionModel = new TransactionModel();
            foreach (var formFile in files)
            {
                var lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(_env.WebRootPath, @"..\OFX\" + formFile.FileName));
                foreach (var line in lines)
                {
                    if (line.Equals("<STMTTRN>"))
                    {
                        transactionModel = new TransactionModel();
                    }
                    else if (line.Contains("<TRNTYPE>"))
                    {
                        transactionModel.Type = line.Replace("<TRNTYPE>", "");
                    }
                    else if (line.Contains("<DTPOSTED>"))
                    {
                        string date = line.Replace("<DTPOSTED>", "");
                        date = date.Substring(0, 8);
                        date = DateTime.ParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        transactionModel.Date = DateTime.Parse(date);
                    }
                    else if (line.Contains("<TRNAMT>"))
                    {
                        transactionModel.Amount = double.Parse((double.Parse(line.Replace("<TRNAMT>", "")) / 100).ToString("f2"));
                    }
                    else if (line.Contains("<MEMO>"))
                    {
                        transactionModel.Memo = line.Replace("<MEMO>", "");
                    }
                    else if (line.Equals("</STMTTRN>"))
                    {
                        var alreadyExists = await _context.Transactions.AnyAsync(x => x.Type == transactionModel.Type &&
                        x.Date == transactionModel.Date && x.Amount == transactionModel.Amount && 
                        x.Memo == transactionModel.Memo);
                        if (alreadyExists == false)
                        {
                            _context.Add(transactionModel);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Date,Amount,Memo")] TransactionModel transactionModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transactionModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transactionModel);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionModel = await _context.Transactions.FindAsync(id);
            if (transactionModel == null)
            {
                return NotFound();
            }
            return View(transactionModel);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Date,Amount,Memo")] TransactionModel transactionModel)
        {
            if (id != transactionModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionModelExists(transactionModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transactionModel);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionModel = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionModel == null)
            {
                return NotFound();
            }

            return View(transactionModel);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transactionModel = await _context.Transactions.FindAsync(id);
            _context.Transactions.Remove(transactionModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionModelExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
