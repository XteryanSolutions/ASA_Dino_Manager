//using Android.Nfc;
using System;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace ASA_Dino_Manager
{
    public partial class AppShell : Shell
    {
        public string version = "ASA Dino Manager 0.04.35";

        // IMPORTING
        public static bool ImportEnabled = false;
        public static int Delay = 5;
        public static int DefaultDelay = 30; // default import delay in seconds

        public static int tagSize = 0;

        // benchmark stuff
        private static int ImportCount = 0;
        private static double ImportAvg = 0; // keep track of average import time

        private bool _isTimerRunning = false; // Timer control flag
        public static bool needUpdate = false;

        public static readonly object _dbLock = new object();

        public AppShell()
        {
            InitializeComponent(); this.Title = version;
            FileManager.Log("====== Started " + version + " ======"); 
            if (!FileManager.InitFileManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            FileManager.Log("FileManager initialized");
            if (!DataManager.InitDataManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            FileManager.Log("dataManager initialized");
            DataManager.CleanDataBaseByID();

            UpdateShellContents();

            StartTimer();
        }

        public void UpdateShellContents()
        {
            string[] classList = DataManager.GetAllClasses();
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");


            string[] dinoList = DataManager.GetAllDistinctColumnData("Tag");


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
                FileManager.Log("No dinos");
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


                var shellContent1 = new ShellContent
                {
                    Title = "Dino Manager ",
                    ContentTemplate = new DataTemplate(typeof(MainPage)), // Replace with the appropriate page
                    Route = "ASA"
                };
                // Add the ShellContent to the Shell
                Items.Add(shellContent1);

                var shellContent2 = new ShellContent
                {
                    Title = "Dino Archive",
                    ContentTemplate = new DataTemplate(typeof(MainPage)), // Replace with the appropriate page
                    Route = "Archive"
                };
                // Add the ShellContent to the Shell
                Items.Add(shellContent2);


                // Loop through the sorted tags and create ShellContent dynamically
                foreach (var tag in sortedTagList)
                {

                    string dinoTag = DataManager.TagForClass(tag);
                    // Retrieve female data
                    string[] females = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Female", "ID");
                    // Retrieve male data
                    string[] males = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Male", "ID");

                    int totalC = females.Length + males.Length;

                    var shellContent = new ShellContent
                    {
                        Title = tag + " ("+ totalC + ")",
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

        public void StartProcess()
        {
            FileManager.Log("Starting Data Process...");
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();


                DataManager.Import();
                FileManager.Log("Scanned files");

                if (DataManager.selectedClass != "")
                {
                    if (DataManager.ModC > 0 || DataManager.AddC > 0 || DataManager.forceLoad) // Check if we need to reload data
                    {
                        FileManager.Log("Updated DataBase");
                        needUpdate = true;
                        FileManager.needSave = true;
                    }
                }
                UpdateShellContents();

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                ImportCount++;
                double outAVG = 0;
                if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }

                FileManager.Log("Processed data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                Delay = DefaultDelay; // only start up timer after scanning is done 
                FileManager.Log("=====================================================================");
            }
            catch
            {
                FileManager.Log("Processed data failure");
                Delay = DefaultDelay; // infinite retries????????
            }
        }

        public void ProcessAllData()
        {
            if (ImportEnabled)
            {
                if (FileManager.CheckPath(FileManager.GamePath))
                {
                    if (Monitor.TryEnter(_dbLock, TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            //Console.WriteLine("Database lock acquired. Updating content...");
                            StartProcess();
                        }
                        finally
                        {
                            Monitor.Exit(_dbLock);
                        }
                    }
                    else
                    {
                        FileManager.Log("Failed to acquire database lock within timeout.");
                        // restart timer if database is locked
                        Delay = DefaultDelay;
                        // Console.WriteLine("Failed to acquire database lock within timeout.");
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
            if (FileManager.GamePath != "")
            {
                ImportEnabled = true;
            }
            else
            {
                ImportEnabled = false;
            }


            Delay--;
            if (Delay == 2)
            {
                SaveData();
            }
            else if (Delay == 0)
            {
                ProcessAllData();
            }
            else if (Delay < -60) // if its taken longer than a minute to finish try again
            {
                ProcessAllData();
                Delay = DefaultDelay;
            }
           
        }

        public void StopTimer()
        {
            _isTimerRunning = false; // Call this to stop the timer if needed
        }

        private void SaveData()
        {
            if (Monitor.TryEnter(_dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    FileManager.SaveFiles();
                }
                finally
                {
                    Monitor.Exit(_dbLock);
                }
            }
            else
            {
                FileManager.Log("Failed to acquire database lock within timeout.");
            }
        }

    }
}
