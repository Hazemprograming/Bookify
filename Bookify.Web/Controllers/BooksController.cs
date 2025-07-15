using Bookify.Web.Core.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _wepHostEnvironMent;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private List<string> _allowedExtensions = new() { ".jpg",".jpeg",".png" };
        private int _allowedMaxSize = 2097152;
        public BooksController(ApplicationDbContext context, IMapper mapper
            , IWebHostEnvironment wepHostEnvironMent)
        {
            _context = context;
            _mapper = mapper;
            _wepHostEnvironMent = wepHostEnvironMent;

        }

        public IActionResult Index()  
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetBooks()
        {

            var skip = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];

            var sortColumnIndex = Request.Form["order[0][column]"];
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                 .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(searchValue))
            {
                books = books.Where(b => b.Title.Contains(searchValue) || b.Author.Name.Contains(searchValue));
            }

            books = books.OrderBy($"{sortColumn} {sortColumnDirection}");
            
            var data = books.Skip(skip).Take(pageSize).ToList();

            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);
                var recordsTotal = books.Count();


            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData };

            return Ok(jsonData);
        }
        public IActionResult Details(int id) 
        { 
            var book = _context.Books  
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();
            var viewModel = _mapper.Map<BookViewModel>(book);
            return View(viewModel);
        }
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
             if(!ModelState.IsValid)
            {
                return View("Form", PopulateViewModel(model));
            }

            var book = _mapper.Map<Book>(model);
            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }

                if (_allowedMaxSize < model.Image.Length)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var ImageName = $"{Guid.NewGuid()}{extension}";

                var path = Path.Combine($"{_wepHostEnvironMent.WebRootPath}/Images/books", ImageName);
                var thumbPath = Path.Combine($"{_wepHostEnvironMent.WebRootPath}/Images/books/Thumb", ImageName);

                using var streem = System.IO.File.Create(path);
                await model.Image.CopyToAsync(streem);

                streem.Dispose();

                book.ImageUrl = $"/Images/books/{ImageName}";
                book.ImageThumbnailUrl = $"/Images/books/Thumb/{ImageName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height =  image.Height / ratio;
                image.Mutate(i => i.Resize(width:200 ,height: (int)height));
                image.Save(thumbPath);


                
            }
            foreach (var category in model.SelectedCategories)
            {   
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            _context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details),new {id = book.Id});
        }
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == id);
            if (book is null)
            {
                return NotFound();
            }
            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);
            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", PopulateViewModel(model));
            }

            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
            {
                return NotFound();
            }

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var oldImagePath = $"{_wepHostEnvironMent.WebRootPath}{book.ImageUrl}";
                    var oldThumbPath = $"{_wepHostEnvironMent.WebRootPath}{book.ImageThumbnailUrl}";
                   
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);

                    if (System.IO.File.Exists(oldThumbPath))
                        System.IO.File.Delete(oldThumbPath);
                }
                 
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }

                if (_allowedMaxSize < model.Image.Length)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var ImageName = $"{Guid.NewGuid()}{extension}";

                var path = Path.Combine($"{_wepHostEnvironMent.WebRootPath}/Images/books", ImageName);
                var thumbPath = Path.Combine($"{_wepHostEnvironMent.WebRootPath}/Images/books/Thumb", ImageName);

                using var streem = System.IO.File.Create(path);
                await model.Image.CopyToAsync(streem);

                streem.Dispose();

                model.ImageUrl = $"/Images/books/{ImageName}";
                model.ImageThumbnailUrl = $"/Images/books/Thumb/{ImageName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;

                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                image.Save(thumbPath);


            }

            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }

            book = _mapper.Map(model, book); 
            book.LastUpdate = DateTime.Now;

            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Details),new {id = book.Id});
        }

        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? Model = null)
        {
            BookFormViewModel viewModel = Model is null ? new BookFormViewModel() :  Model;

            var authors = _context.Authors.Where(a => !a.IsDeleted).ToList();
            var categories = _context.Categories.Where(a => !a.IsDeleted).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
        
            return viewModel;
        }
    }
}
