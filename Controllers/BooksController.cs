using BaithuchanhORM.Data;
using BaithuchanhORM.Models;
using BaithuchanhORM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaithuchanhORM.Controllers;

public class BooksController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageFileService _imageService;

    public BooksController(AppDbContext db, ImageFileService imageService)
    {
        _db = db;
        _imageService = imageService;
    }

    // GET: /Books
    public async Task<IActionResult> Index()
    {
        var books = await _db.Books
            .Include(b => b.Images)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        return View(books);
    }

    // GET: /Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var book = await _db.Books
            .Include(b => b.Images)
            .FirstOrDefaultAsync(m => m.Id == id.Value);
        if (book is null) return NotFound();

        return View(book);
    }

    // GET: /Books/Create
    public IActionResult Create()
    {
        return View(new Book { PublishedDate = DateTime.Today });
    }

    // POST: /Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Author,Publisher,Price,Stock,PublishedDate")] Book book,
        List<IFormFile>? images)
    {
        if (!ModelState.IsValid) return View(book);

        _db.Add(book);
        await _db.SaveChangesAsync();

        var uploadErrors = await SaveUploadedImagesAsync(book.Id, images);
        if (uploadErrors.Count > 0)
        {
            foreach (var error in uploadErrors)
            {
                ModelState.AddModelError("images", error);
            }

            book.Images = await _db.BookImages.Where(i => i.BookId == book.Id).ToListAsync();
            return View(book);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var book = await _db.Books
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.Id == id.Value);
        if (book is null) return NotFound();

        return View(book);
    }

    // POST: /Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Author,Publisher,Price,Stock,PublishedDate")] Book book,
        List<IFormFile>? images)
    {
        if (id != book.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            book.Images = await _db.BookImages.Where(i => i.BookId == book.Id).ToListAsync();
            return View(book);
        }

        _db.Update(book);
        await _db.SaveChangesAsync();

        var uploadErrors = await SaveUploadedImagesAsync(book.Id, images);
        if (uploadErrors.Count > 0)
        {
            foreach (var error in uploadErrors)
            {
                ModelState.AddModelError("images", error);
            }

            book.Images = await _db.BookImages.Where(i => i.BookId == book.Id).ToListAsync();
            return View(book);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Books/DeleteImage/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int bookId)
    {
        var image = await _db.BookImages.FindAsync(id);
        if (image is null || image.BookId != bookId) return NotFound();

        _imageService.DeleteBookImages([image]);
        _db.BookImages.Remove(image);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = bookId });
    }

    // GET: /Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var book = await _db.Books
            .Include(b => b.Images)
            .FirstOrDefaultAsync(m => m.Id == id.Value);
        if (book is null) return NotFound();

        return View(book);
    }

    // POST: /Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _db.Books
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book is null) return RedirectToAction(nameof(Index));

        _imageService.DeleteBookImages(book.Images);
        _imageService.DeleteBookFolder(book.Id);
        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> SaveUploadedImagesAsync(int bookId, List<IFormFile>? images)
    {
        if (images is null || images.Count == 0) return [];

        var (saved, errors) = _imageService.SaveBookImages(bookId, images);
        if (saved.Count > 0)
        {
            foreach (var item in saved)
            {
                _db.BookImages.Add(new BookImage
                {
                    BookId = bookId,
                    FileName = item.FileName,
                    FilePath = item.FilePath
                });
            }

            await _db.SaveChangesAsync();
        }

        return errors;
    }
}
