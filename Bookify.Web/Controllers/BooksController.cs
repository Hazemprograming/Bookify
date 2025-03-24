using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _wepHostEnvironMent;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string> _allowedExtensions = new() { ".jpg",".jpeg",".png" };
        private int _maxSize = 2097152;
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
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create( BookFormViewModel model )
        {
            if (!ModelState.IsValid)
            {
                return View("Form", PopulateViewModel(model));
            }
                
            var book = _mapper.Map<Book>(model);
            if ( model.Image is not null )
            {
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxSize) 
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var ImageName = $"{Guid.NewGuid()}{extension}";

                var path = Path.Combine($"{_wepHostEnvironMent.WebRootPath}/Images/books",ImageName);

                using var streem = System.IO.File.Create(path);
                
                model.Image.CopyTo(streem);
                book.ImageUrl = ImageName;

            }
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category});
            }
           
            _context.Add(book);
            _context.SaveChanges();

          return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int Id)
        {
            var book = _context.Books.Find(Id);
            if (book is null)
            {
                return NotFound();
            }
            var viewModel = _mapper.Map<BookFormViewModel>(book);

            return View("Form", PopulateViewModel(viewModel));
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
