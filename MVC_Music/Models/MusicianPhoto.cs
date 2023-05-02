using System.ComponentModel.DataAnnotations;

namespace MVC_Music.Models
{
    public class MusicianPhoto
    {
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        [StringLength(255)]
        public string MimeType { get; set; }

        public int MusicianID { get; set; }
        public Musician Musician { get; set; }
    }
}
