//using Android.Nfc;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace ASA_Dino_Manager
{
    public partial class AppShell : Shell
    {


        // IMPORTING
        public static bool Importing = false;
        public static bool ImportEnabled = false;
        public static int Delay = 5;
        public static int DefaultDelay = 30; // default import delay in seconds

        public static int tagSize = 0;

        // benchmark stuff
        private static int ImportCount = 0;
        private static double ImportAvg = 0; // keep track of average import time

        private bool _isTimerRunning = false; // Timer control flag

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

            DataManager.CleanDataBaseByID();

            PopulateShellContents();

            StartTimer();
        }


        private void PopulateShellContents()
        {
            string[] classList = DataManager.GetAllClasses();
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");

            

            if (tagList.Length < 1)
            {
                Items.Clear();
                var shellContent = new ShellContent
                {
                    Title = "Looking for dinos",
                    ContentTemplate = new DataTemplate(typeof(MainPage)), // Replace with the appropriate page
                    Route = "Looking for dinos"
                };

                // Add the ShellContent to the Shell
                Items.Add(shellContent);

                return; // exit early if the tagList is empty
            }
            if (tagList.Length > tagSize)
            {
                FileManager.Log("updated tagList");
                Items.Clear();
                tagSize = tagList.Length;
                //ClearShell();
                

                // Retrieve the tag list from DataManager and sort alphabetically
                var sortedTagList = classList.OrderBy(tag => tag).ToArray();

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
            else
            {
               // StartImport();
            }

        }

        public void ProcessAllData()
        {
            if (ImportEnabled)
            {
                if (FileManager.CheckPath(FileManager.GamePath))
                {
                    if (!Importing)
                    {
                        FileManager.Log("Starting Data Process...");
                        try
                        {
                            Importing = true;
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();


                            DataManager.Import();
                            FileManager.Log("imported");

                            if (DataManager.selectedClass != "")
                            {
                                if (DataManager.ModC > 0 || DataManager.AddC > 0 || DataManager.forceLoad) // Check if we need to reload data
                                {
                                    FileManager.Log("updated database");
                                    //DataManager.GetDinoData(DataManager.selectedClass);
                                    //DataManager.SetMaxStats();
                                    //DataManager.SetBinaryStats();
                                    //DataManager.GetBestPartner();
                                    MainPage.Update();
                                }
                            }
                            PopulateShellContents();


                            stopwatch.Stop();
                            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                            Importing = false;
                            FileManager.SaveFiles();
                            ImportCount++;
                            double outAVG = 0;
                            if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                            else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }

                            FileManager.Log("Processed data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                            Delay = DefaultDelay; // only start up timer after scanning is done 
                        }
                        catch
                        {
                            FileManager.Log("Processed data failure");
                            Delay = DefaultDelay; // infinite retries????????
                        }
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

        private void StartTimer()
        {
            _isTimerRunning = true; // Flag to control the timer

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!_isTimerRunning)
                    return false; // Stop the timer

                TriggerFunction();
                return true; // Continue running the timer
            });
        }

        private void TriggerFunction()
        {
            //StartImport();
            // Logic to be executed every 5 seconds
            Console.WriteLine($"Function triggered at {DateTime.Now}");


            if (FileManager.GamePath != "")
            {
                ImportEnabled = true;
            }
            else
            {
                ImportEnabled = false;
            }

            Delay--;
            if (Delay == 0)
            {
                ProcessAllData();
            }
            else if (Delay < -60) // if its taken longer than a minute to finish try again
            {
                ProcessAllData();
                Delay = DefaultDelay;
            }

            FileManager.WriteLog();
        }

        public void StopTimer()
        {
            _isTimerRunning = false; // Call this to stop the timer if needed
        }


    }
}
