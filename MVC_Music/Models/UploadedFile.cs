using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MVC_Music.Models
{
    public class UploadedFile
    {
        public int ID { get; set; }

        [StringLength(255, ErrorMessage = "The name of the file cannot be more than 255 characters.")]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot be more than 4,000 characters.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Type of File")]
        public string MimeType { get; set; }

        public FileContent FileContent { get; set; } = new FileContent();
    }
}
