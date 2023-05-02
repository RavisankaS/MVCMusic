using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Xml.Linq;

namespace MVC_Music.Models
{
    public class Instrument
    {
        public int ID { get; set; }

        [Display(Name = "Instrument")]
        [Required(ErrorMessage = "You cannot leave the name of the Instrument blank.")]
        [StringLength(50, ErrorMessage = "Instrument name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        [Display(Name = "Primary Musicians")]
        public ICollection<Musician> Musicians { get; set; } = new HashSet<Musician>();

        [Display(Name = "Other Musicians")]
        public ICollection<Play> Plays { get; set; } = new HashSet<Play>();

    }
}
