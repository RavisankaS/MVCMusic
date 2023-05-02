using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MVC_Music.ViewModels
{
    public class PerformanceSummaryVM
    {
        public int ID { get; set;  }

        [Display(Name = "Musician")]
        public string FullName{get; set;}

        [Display(Name = "Average Fee Paid")]
        [DataType(DataType.Currency)]
        public double AverageFeePaid { get; set; }

        [Display(Name = "Highest Fee Paid")]
        [DataType(DataType.Currency)]
        public double HighestFeePaid { get; set; }

        [Display(Name = "Lowest Fee Paid")]
        [DataType(DataType.Currency)]
        public double LowestFeePaid { get; set; }

        [Display(Name = "Number of Performances")]
        public int NumberOfPerformances { get; set; }


    }
}
