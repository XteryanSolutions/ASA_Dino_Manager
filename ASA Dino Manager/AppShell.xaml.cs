using Microsoft.Maui.Controls;

namespace ASA_Dino_Manager
{
    public partial class AppShell : Shell
    {


        // IMPORTING
        public static bool Importing = false;
        public static bool ImportEnabled = true;
        public static int Delay = 5;
        public static int DefaultDelay = 30; // default import delay in seconds


        // benchmark stuff
        private static int ImportCount = 0;
        private static double ImportAvg = 0; // keep track of average import time


        public AppShell()
        {
            InitializeComponent();


            if (!FileManager.InitFileManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            if (!DataManager.InitDataManager())
            {
                // Exit app here
                Application.Current.Quit();
            }

            PopulateShellContents();
        }



        private void PopulateShellContents()
        {
            // Retrieve the tag list from DataManager and sort alphabetically
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");
            var sortedTagList = tagList.OrderBy(tag => tag).ToArray();

            // Loop through the sorted tags and create ShellContent dynamically
            foreach (var tag in sortedTagList)
            {
                var shellContent = new ShellContent
                {
                    Title = tag,
                    ContentTemplate = new DataTemplate(typeof(MainPage)), // Replace with the appropriate page
                    Route = tag
                };

                // Add the ShellContent to the Shell
                Items.Add(shellContent);
            }
        }


        public static void StartImport()
        {
            if (ImportEnabled)
            {
                if (FileManager.CheckPath(FileManager.GamePath))
                {
                    if (!Importing)
                    {
                        FileManager.Log("Starting Import thread");
                        Thread thread = new Thread(delegate () { // start new thread
                            try
                            {
                                Importing = true;
                                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                                DataManager.Import();
                                stopwatch.Stop();
                                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                                Importing = false;
                                FileManager.SaveFiles();
                                ImportCount++;
                                double outAVG = 0;
                                if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                                else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }

                                FileManager.Log("Imported files in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                                Delay = DefaultDelay; // only start up timer after scanning is done 
                            }
                            catch
                            {
                                FileManager.Log("Import thread failure");
                                Delay = DefaultDelay; // infinite retries????????
                            }
                        });
                        thread.Start();
                    }
                    else
                    {
                        FileManager.Log("DataBase locked");
                        // restart timer if database is locked
                        Delay = DefaultDelay;
                    }
                }
                else
                {
                    // scan for a new path and save it
                    FileManager.ScanPath();
                    Delay = DefaultDelay;
                }
            }
        }


    }
}
