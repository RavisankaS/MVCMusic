namespace MVC_Music.Models
{
    public class Play
    {
        public int InstrumentID { get; set; }
        public Instrument Instrument { get; set; }

        public int MusicianID { get; set; }
        public Musician Musician { get; set; }
    }
}
