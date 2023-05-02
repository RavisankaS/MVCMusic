using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace MVC_Music.Models
{
    public class Musician : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "Musician")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        [Display(Name = "Musician")]
        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        public int Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int a = today.Year - DOB.Year
                    - ((today.Month < DOB.Month || (today.Month == DOB.Month && today.Day < DOB.Day) ? 1 : 0));
                return a; /*Note: You could add .PadLeft(3) but spaces disappear in a web page. */
            }
        }

        [Display(Name = "SIN")]
        public string SINFormatted
        {
            get
            {
                return SIN.Substring(0, 3) + "-" + SIN.Substring(3, 3) + "-" + SIN.Substring(6, 3);
            }
        }

        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone[6..];
            }
        }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(30, ErrorMessage = "First name cannot be more than 30 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(30, ErrorMessage = "Middle name cannot be more than 30 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "You must enter the Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "You cannot leave the SIN blank.")]
        [RegularExpression("^\\d{9}$", ErrorMessage = "The SIN must be exactly 9 numeric digits.")]
        [StringLength(9)]//DS Note: we only include this to limit the size of the database field to 10
        public string SIN { get; set; }

        public MusicianPhoto MusicianPhoto { get; set; }

        public MusicianThumbnail MusicianThumbnail { get; set; }

        [Display(Name = "Primary Instrument")]
        [Required(ErrorMessage = "You must select the principal instrument the musician plays.")]
        public int InstrumentID { get; set; }

        [Display(Name = "Primary Instrument")]
        public Instrument Instrument { get; set; }

        [Display(Name = "All Instruments")]
        public ICollection<Play> Plays { get; set; } = new HashSet<Play>();

        [Display(Name = "Documents")]
        public ICollection<MusicianDocument> MusicianDocuments { get; set; } = new HashSet<MusicianDocument>();

        [Display(Name = "Performances")]
        public ICollection<Performance> Performances { get; set; } = new HashSet<Performance>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DOB > DateTime.Today)
            {
                yield return new ValidationResult("Date of Birth cannot be in the future.", new[] { "DOB" });
            }
            else if (Age < 7)
            {
                yield return new ValidationResult("Musician must be at least 7 years old.", new[] { "DOB" });
            }
        }
    }
}
