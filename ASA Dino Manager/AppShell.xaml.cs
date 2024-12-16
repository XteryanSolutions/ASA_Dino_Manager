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



        private bool _isTimerRunning = false; // Timer control flag


        public AppShell()
        {
            InitializeComponent(); this.Title = Vars.version;
            FileManager.Log("====== Started " + Vars.version + " ======");
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
            FileManager.Log($"AppShell -> {Vars.setRoute}");
            await Shell.Current.GoToAsync(Vars.setRoute);
        }

        public void UpdateShellContents()
        {
            Vars.eventDisabled = true;
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");
            string[] classList = DataManager.GetAllClasses();

            Vars.tagSize = tagList.Length;

            Items.Clear();

            // Retrieve the tag list from DataManager and sort alphabetically
            var sortedTagList = classList.OrderBy(tag => tag).ToArray();

            var shellContent1 = new ShellContent
            {
                Title = "Dino Manager",
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
            Vars.eventDisabled = false;
            FileManager.Log("Updated tagList");
        }


        public void ProcessAllData()
        {
            if (Vars.ImportEnabled)
            {
                // check if the gamepath works
                // maybe redundant checks???
                if (FileManager.CheckPath(FileManager.GamePath))
                {
                    if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            // import files first
                            DataManager.Import();

                            // check for changes
                            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");

                            if (DataManager.ModC > 0 || DataManager.AddC > 0 || tagList.Length > Vars.tagSize) // Check if we need to reload data
                            {
                                FileManager.Log("Updated DataBase");


                                FileManager.needSave = true;

                                UpdateShellContents();
                                MenuNavigation();
                            }


                            stopwatch.Stop();
                            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                            Vars.ImportCount++;
                            double outAVG = 0;
                            if (Vars.ImportCount < 2) { Vars.ImportAvg = elapsedMilliseconds; outAVG = Vars.ImportAvg; }
                            else { Vars.ImportAvg += elapsedMilliseconds; outAVG = Vars.ImportAvg / Vars.ImportCount; }
                            FileManager.Log("Imported data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                            FileManager.Log("=====================================================================");
                        }
                        catch
                        {
                            FileManager.Log("Import data failure");
                        }
                        finally
                        {
                            Monitor.Exit(Vars._dbLock);
                        }
                    }
                    else
                    {
                        FileManager.Log("Failed to acquire database lock within timeout.");
                    }
                }
                else
                {
                    // scan for a new path and save it
                    FileManager.ScanPath();
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
            Vars.Delay--;
            if (Vars.Delay < 0)
            {
                Vars.Delay = Vars.DefaultDelay;
                SaveData();
                ProcessAllData();
            }

            FileManager.WriteLog();
        }

        public void StopTimer()
        {
            _isTimerRunning = false; // Call this to stop the timer if needed
        }

        private void SaveData()
        {
            if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    FileManager.SaveFiles();
                }
                finally
                {
                    Monitor.Exit(Vars._dbLock);
                }
            }
            else
            {
                FileManager.Log("Failed to acquire database lock within timeout.");
            }
        }

    }
}
