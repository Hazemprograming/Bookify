using System.Xml.Linq;

namespace Bookify.Web.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    public class Book :BaseModel
    {
        public int Id { get; set; }
        [MaxLength(500)]
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        [MaxLength(100)]
        public string Publisher { get; set; }
        public DateTime PublishingDate { get; set; }
        public string? ImageUrl { get; set; }
        [MaxLength(50)]
        public string Hall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public string Discription { get; set; } = null!;

        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>(); 
    }
}
