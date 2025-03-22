using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string> _allowedExtentions = new() { "jpg",".jpeg",".png" };
        private int _maxLength = 2097152;
        public BooksController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                if (_allowedExtentions.Contains(Path.GetExtension(model.Image.FileName)))
                {

                    return View("Form", PopulateViewModel(model));
                }
            }
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category});
            }
           
            _context.Add(book);
            _context.SaveChanges();

          return RedirectToAction(nameof(Index));
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
