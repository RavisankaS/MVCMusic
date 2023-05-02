using Microsoft.EntityFrameworkCore;
using MVC_Music.Models;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace MVC_Music.Data
{
    public static class MusicInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            MusicContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<MusicContext>();

            try
            {
                context.Database.EnsureDeleted();
                //Create the database if it does not exist
                context.Database.Migrate();

                //This approach to seeding data uses int and string arrays with loops to
                //create the data using random values
                Random random = new Random();

                //Prepare some string arrays for building objects
                string[] firstNames = new string[] { "Fred", "Barney", "Wilma", "Betty", "Dave", "Tim", "Elton", "Paul", "Shania", "Bruce" };
                string[] lastsNames = new string[] { "Stovell", "Jones", "Bloggs", "Flintstone", "Rubble", "Brown", "John", "McCartney", "Twain", "Cockburn" };
                string[] instruments = new string[] { "Lead Guitar", "Base Guitar", "Drums", "Keyboards", "Lead Vocals", "Harmonica", "Didgeridoo" };

                //Instrument
                if (!context.Instruments.Any())
                {
                    //loop through the array of Instrument names
                    foreach (string iname in instruments)
                    {
                        Instrument inst = new Instrument()
                        {
                            Name = iname
                        };
                        context.Instruments.Add(inst);
                    }
                    context.SaveChanges();
                }
                //Create a collection of the primary keys of the Instruments
                int[] instrumentIDs = context.Instruments.Select(a => a.ID).ToArray();
                int instrumentIDCount = instrumentIDs.Length;

                //Musician
                if (!context.Musicians.Any())
                {
                    // Start birthdate for randomly produced employees 
                    // We will subtract a random number of days from today
                    DateTime startDOB = DateTime.Today;

                    //Double loop through the arrays of names 
                    //and build the Musician as we go
                    foreach (string f in firstNames)
                    {
                        foreach (string l in lastsNames)
                        {
                            Musician m = new Musician()
                            {
                                FirstName = f,
                                MiddleName = f.Substring(1, 1).ToUpper(),//take second letter of first name
                                LastName = l,
                                SIN = random.Next(213214131, 989898989).ToString(),//Big enough int for required digits
                                //For the phone, needed one more digit than a random int can generate so
                                //concatenated 2 together as strings and then converted
                                Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                                InstrumentID = instrumentIDs[random.Next(instrumentIDCount)],
                                DOB = startDOB.AddDays(-random.Next(6500, 25000))
                            };
                            context.Musicians.Add(m);
                        }
                    }
                    context.SaveChanges();
                }
                //Create a collection of the primary keys of the Musicians
                int[] musicianIDs = context.Musicians.Select(a => a.ID).ToArray();
                int musicianIDCount = musicianIDs.Length;

                //Play
                //Add a few instruments to each musician
                if (!context.Plays.Any())
                {
                    //i loops through the primary keys of the musicians
                    //j is just a counter so we add a few instruments to a musician
                    //k lets us step through all instruments so we can make sure each gets used
                    int k = 0;//Start with the first instrument
                    foreach (int i in musicianIDs)
                    {
                        int howMany = random.Next(1, instrumentIDCount);//add a few instruments to a musician
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= instrumentIDCount) ? 0 : k;//Resets counter k to 0 if we have run out of instruments
                            Play p = new Play()
                            {
                                MusicianID = i,
                                InstrumentID = instrumentIDs[k]
                            };
                            context.Plays.Add(p);
                            k++;
                        }
                    }
                    context.SaveChanges();
                }

                //For 3B as described in Part 2C
                //Prepare some string arrays for building objects
                string[] genres = new string[] { "Classical", "Rock", "Pop", "Jazz", "Country", "Ambient", "Techno" };
                //Create 5 notes from Bacon ipsum
                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                //Genre
                if (!context.Genres.Any())
                {
                    //loop through the array of Genre names
                    foreach (string g in genres)
                    {
                        Genre genre = new()
                        {
                            Name = g
                        };
                        context.Genres.Add(genre);
                    }
                    context.SaveChanges();
                }

                //Note: we will use int arrays to hold valid Primary Key values we can 
                //randomly assingn as foreign keys
                //Create a collection of the primary keys of the Genres
                int[] genreIDs = context.Genres.Select(g => g.ID).ToArray();
                int genreIDCount = genreIDs.Length;

                //Album
                if (!context.Albums.Any())
                {
                    context.Albums.AddRange(
                     new Album
                     {
                         Name = "Rocket Food",
                         YearProduced = "2000",
                         Price = 19.99d,
                         GenreID = genreIDs[random.Next(genreIDs.Count())]
                     },
                     new Album
                     {
                         Name = "Songs of the Sea",
                         YearProduced = "1999",
                         Price = 9.99d,
                         GenreID = genreIDs[random.Next(genreIDs.Count())]
                     },
                     new Album
                     {
                         Name = "The Horse",
                         YearProduced = "1929",
                         Price = 99.99d,
                         GenreID = genreIDs[random.Next(genreIDs.Count())]
                     },
                     new Album
                     {
                         Name = "Freedom",
                         YearProduced = "2012",
                         Price = 29.99d,
                         GenreID = genreIDs[random.Next(genreIDs.Count())]
                     });
                    context.SaveChanges();
                }

                //Create a collection of the primary keys of the Albums
                int[] albumIDs = context.Albums.Select(a => a.ID).ToArray();
                int albumIDCount = albumIDs.Length;

                //Song
                if (!context.Songs.Any())
                {
                    // Start date for random recording dates
                    // We will subtract a random number of days from today
                    DateTime startDate = DateTime.Today;

                    //Double loop through the arrays of names 
                    //and build song title as you go
                    foreach (string f in firstNames)
                    {
                        foreach (string g in genres)
                        {
                            Song s = new()
                            {
                                Title = f + " " + g,//looks silly but gives unique names for the songs,
                                DateRecorded = startDate.AddDays(-random.Next(30, 1000)),
                                GenreID = genreIDs[random.Next(genreIDCount)],
                                AlbumID = albumIDs[random.Next(albumIDCount)]
                            };
                            context.Songs.Add(s);
                        }
                    }
                    context.SaveChanges();
                }
                //Create a collection of the primary keys of the Songss
                int[] songIDs = context.Songs.Select(a => a.ID).ToArray();
                int songIDCount = songIDs.Length;

                //Performance
                //Add a few musicians as performers on each song
                if (!context.Performances.Any())
                {
                    //i loops through the primary keys of the songs
                    //j is just a counter so we add a few musicians to a song
                    //k lets us step through all musicians so we can make sure each gets used
                    int k = 0;//Start with the first Musician
                    foreach (int i in songIDs)
                    {
                        int howMany = random.Next(1, 7);//How many musicians on a song
                        howMany = (howMany > musicianIDCount) ? musicianIDCount-1 : howMany; //Don't try to assign more musicians then are in the system
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= musicianIDCount) ? 0 : k;
                            Performance p = new()
                            {
                                Comments= baconNotes[random.Next(5)],
                                FeePaid = random.Next(500),
                                SongID = i,
                                MusicianID = musicianIDs[k],
                                InstrumentID = context.Musicians.Find(musicianIDs[k]).InstrumentID//Get the primary instrument of the musician
                            };
                            context.Performances.Add(p);
                            k++;
                        }
                    }
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
