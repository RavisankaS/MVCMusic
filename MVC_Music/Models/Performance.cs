using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Numerics;
using System.Security.AccessControl;

namespace MVC_Music.Models
{
    public class Performance
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the comments blank.")]
        [MinLength(10, ErrorMessage ="Comment must be at least 10 characters.")]
        [MaxLength(4000,ErrorMessage = "Comment cannot be more than 4,000 characters.")]
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [Display(Name ="Fee Paid")]
        [Required(ErrorMessage = "You must enter a fee paid for the performance.")]
        [DataType(DataType.Currency)]
        public double FeePaid { get; set; } = 0d;

        [Display(Name = "Song")]
        public int SongID { get; set; }
        public Song Song { get; set; }

        [Display(Name = "Musician")]
        public int MusicianID { get; set; }
        public Musician Musician { get; set; }

        [Display(Name = "Instrument")]
        public int InstrumentID { get; set; }
        public Instrument Instrument { get; set; }

    }
}
