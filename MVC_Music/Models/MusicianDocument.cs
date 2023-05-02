using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MVC_Music.Models
{
    public class MusicianDocument : UploadedFile
    {
        [Display(Name = "Musician")]
        public int MusicianID { get; set; }

        public Musician Musician { get; set; }
    }
}
