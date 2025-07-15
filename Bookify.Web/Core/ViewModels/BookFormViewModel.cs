using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null!, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DublicatedBooks)]
        public string Title { get; set; } = null!;
        
        [Display (Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.DublicatedBooks)]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }
        
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; }

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Errors.NotAllowedFatureDate)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string Hall { get; set; } = null!;

        [Display(Name = "Is Available For Rental")]
        public bool IsAvailableForRental { get; set; }
        public string Discription { get; set; } = null!;
        
        [Display(Name = "Selected Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
