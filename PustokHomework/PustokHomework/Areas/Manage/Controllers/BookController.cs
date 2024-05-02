using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Pustok.Helpers;
using PustokHomework.Areas.Manage.ViewModels;
using PustokHomework.Data;
using PustokHomework.Models;

namespace PustokHomework.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public BookController(AppDbContext appDbContext,IWebHostEnvironment webHostEnvironment)
        {
            _context = appDbContext;
            _env = webHostEnvironment;
        }
        public IActionResult Index(int page = 1)
        {
            var query = _context.Books.Include(x => x.Genre).Include(x=>x.Author);
            return View(PaginatedList<Book>.Create(query, page, 2));
        }

        public IActionResult Create()
        {
            ViewBag.Authors = _context.Author.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            return View();  
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Authors = _context.Author.ToList();
                ViewBag.Genres = _context.Genres.ToList();
                ViewBag.Tags = _context.Tags.ToList();
                return View(book);
            }

            if (!_context.Author.Any(x => x.Id == book.AuthorId))
                return RedirectToAction("notfound", "error");

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
                return RedirectToAction("notfound", "error");


            foreach (var item in book.TagIds)
            {
                if(!_context.Tags.Any(x=>x.Id == item)) return RedirectToAction("notfound", "error");

                BookTag bookTag = new BookTag
                {
                    TagId = item,
                };

                book.BookTags.Add(bookTag);
            }


            BookImage bookImage = new BookImage
            {
                Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book"),
                PosterStatus = true
            };
            book.Images.Add(bookImage);
            BookImage bookImage2 = new BookImage
            {
                Name = FileManager.Save(book.HoverFile, _env.WebRootPath, "uploads/book"),
                PosterStatus = false
            };
            book.Images.Add(bookImage2);
           


            foreach (var imgFile in book.ImageFiles)
            {
                BookImage bookImg = new BookImage
                {
                    Name = FileManager.Save(imgFile, _env.WebRootPath, "uploads/book"),
                    PosterStatus = null,
                };
                book.Images.Add(bookImg);
            }

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction("index");
        }



        public IActionResult Edit(int id)
        {
            Book? book = _context.Books.Include(x => x.Images).Include(x => x.BookTags).FirstOrDefault(x => x.Id == id);

            if (book == null) RedirectToAction("notfound", "error");


            ViewBag.Authors = _context.Author.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            book.TagIds = book.BookTags.Select(x => x.TagId).ToList();

            return View(book);
        }

        [HttpPost]
        public IActionResult Edit(Book book)
        {
            Book? existBook = _context.Books.Include(x=>x.Images).FirstOrDefault(x=>x.Id == book.Id);
            if (existBook == null) RedirectToAction("notfound", "error");

            if (!_context.Author.Any(x => x.Id == book.AuthorId))
                return RedirectToAction("notfound", "error");

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
                return RedirectToAction("notfound", "error");


            string posterFile = null;
            if (book.PosterFile != null)
            {
                posterFile = existBook.Images.First().Name;
                existBook.Images.First().Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book");
            }

            string hoverFile = null;
            if (book.HoverFile != null)
            {
                hoverFile = existBook.Images.First().Name;
                existBook.Images.First().Name = FileManager.Save(book.PosterFile, _env.WebRootPath, "uploads/book");
            }


            existBook.Name = book.Name;
            existBook.Description = book.Description;
            existBook.SalePrice = book.SalePrice;
            existBook.CostPrice = book.CostPrice;
            existBook.DiscountPercent = book.DiscountPercent;
            existBook.IsNew = book.IsNew;
            existBook.IsFeatured = book.IsFeatured;
            existBook.StockStatus = book.StockStatus;
            existBook.ModifiedAt = book.ModifiedAt;

            if (posterFile != null)
            {
                FileManager.Delete(_env.WebRootPath, "uploads/book", posterFile);
            }

            if (hoverFile != null)
            {
                FileManager.Delete(_env.WebRootPath, "uploads/book", hoverFile);
            }

            _context.SaveChanges();

            return RedirectToAction("index");

        }


        public IActionResult DeletePhoto(int id)
        {
            var book = _context.BookImages.FirstOrDefault(m => m.Id == id);

             if (book == null) RedirectToAction("notfound", "error");

            _context.BookImages.Remove(book);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
