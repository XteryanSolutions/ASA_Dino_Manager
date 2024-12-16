//using Android.Nfc;
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Maui.Controls;
using Windows.Devices.Bluetooth.Advertisement;

namespace ASA_Dino_Manager
{
    public partial class AppShell : Shell
    {
        public string version = "ASA Dino Manager 0.04.37";

        // IMPORTING
        public static bool ImportEnabled = false;
        public static int Delay = 5;
        public static int DefaultDelay = 10; // default import delay in seconds

        public static int tagSize = 0;

        // benchmark stuff
        private static int ImportCount = 0;
        private static double ImportAvg = 0; // keep track of average import time

        private bool _isTimerRunning = false; // Timer control flag
        public static bool needUpdate = false;

        public static readonly object _dbLock = new object();

        public static string setRoute = "";

        private bool isNavigationSuspended = false;


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
            FileManager.Log("DataManager initialized");


            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");

            // if manager initialized and we are not scanning and there is dinos in taglist
            if (!FileManager.Scanning && tagList.Length > 0) 
            {
                DataManager.CleanDataBaseByID();
                UpdateShellContents();
            }
            else
            {
                Items.Clear();
                var shellContent = new ShellContent
                {
                    Title = "Looking for dinos",
                    ContentTemplate = new DataTemplate(typeof(MainPage)),
                    Route = "Looking for dinos"
                };
                Items.Add(shellContent);
                FileManager.Log("No dinos =(");
            }

            StartTimer();


        }


        public async Task MenuNavigation()
        {
            FileManager.Log($"AppShell -> {setRoute}");
            await Shell.Current.GoToAsync(setRoute);
        }

        public void UpdateShellContents()
        {
            isNavigationSuspended = true;
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");
            string[] classList = DataManager.GetAllClasses();

            tagSize = tagList.Length;

            Items.Clear();

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
                    Title = tag + " (" + totalC + ")",
                    ContentTemplate = new DataTemplate(typeof(MainPage)), // Replace with the appropriate page
                    Route = tag
                };

                // Add the ShellContent to the Shell
                Items.Add(shellContent);
            }
            FileManager.Log("Updated tagList");
            isNavigationSuspended = false;
        }

        public void StartProcess()
        {
            //FileManager.Log("Starting Import Process...");
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                DataManager.Import();
                //FileManager.Log("Scanned files");


                if (DataManager.ModC > 0 || DataManager.AddC > 0 || DataManager.forceLoad) // Check if we need to reload data
                {
                    FileManager.Log("Updated DataBase");
                    FileManager.needSave = true;
                    needUpdate = true;
                }

                string[] tagList = DataManager.GetAllDistinctColumnData("Tag");
                if (tagList.Length > tagSize)
                {
                   UpdateShellContents();
                }

               // UpdateShellContents();

                MenuNavigation();


                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                ImportCount++;
                double outAVG = 0;
                if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }
                FileManager.Log("Imported data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                FileManager.Log("=====================================================================");
            }
            catch
            {
                FileManager.Log("Import data failure");

            }
            Delay = DefaultDelay; // infinite retries????????
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
