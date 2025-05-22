using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }


        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();
        
        
        // Start methode die wordt aangeroepen wanneer de foto pagina opent.
        public void Start()
        {
            // Get today's day number (0 = Sunday, 6 = Saturday)
            var now = DateTime.Now;
            int today = (int)now.DayOfWeek;

            // Clear existing list
            PicturesToDisplay.Clear();

            // Initializeer de lijst met fotos
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Extract the day number from folder name (e.g., "0_Zondag" -> 0)
                string folderName = Path.GetFileName(dir);
                if (!int.TryParse(folderName.Split('_')[0], out int folderDay))
                    continue;

                // Only process photos from today's folder
                if (folderDay == today)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        // Extract ID from filename (e.g., "10_05_30_id8824.jpg" -> 8824)
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string idPart = fileName.Split('_').Last();
                        int photoId = int.Parse(idPart.Substring(2)); // Remove "id" prefix

                        PicturesToDisplay.Add(new KioskPhoto() { Id = photoId, Source = file });
                    }
                }
            }

            // Update de fotos
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {
            // Reload pictures when refresh is clicked
            Start();
        }
    }
}
