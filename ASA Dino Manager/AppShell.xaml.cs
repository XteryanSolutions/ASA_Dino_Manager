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

        public DataTemplate ArchivePage = new DataTemplate(typeof(ArchivePage));


        public AppShell()
        {
            InitializeComponent(); this.Title = Shared.version;
            FileManager.Log("====== Started " + Shared.version + " ======", 0);
            if (!FileManager.InitFileManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            FileManager.Log("FileManager initialized", 0);
            if (!DataManager.InitDataManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            FileManager.Log("DataManager initialized", 0);




            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");

            // if manager initialized and we are not scanning and there is dinos in taglist
            if (!Shared.Scanning && tagList.Length > 0)
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
                FileManager.Log("No dinos =(", 0);
            }
            StartTimer();

        }

        public void UpdateShellContents()
        {
            Shared.eventDisabled = true; FileManager.Log("Disabled Navigation", 0);
            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");
            string[] classList = DataManager.GetAllClasses();

            Shared.tagSize = tagList.Length;

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
                ContentTemplate = new DataTemplate(() => new ArchivePage()), // Always a fresh page
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
            Shared.eventDisabled = false; FileManager.Log("Enabled Navigation", 0);
            FileManager.Log("Updated tagList", 0);
        }

        public void ProcessAllData()
        {
            if (Shared.ImportEnabled)
            {
                // check if the gamepath works
                // maybe redundant checks???
                if (FileManager.CheckPath(Shared.GamePath))
                {
                    if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            // import files first
                            DataManager.Import();

                            // check for changes
                            string[] tagList = DataManager.GetAllDistinctColumnData("Tag");



                            // Check if we need to reload data
                            if (DataManager.ModC > 0 || DataManager.AddC > 0 || tagList.Length > Shared.tagSize)
                            {
                                FileManager.Log("Updated DataBase", 0);

                                Shared.needSave = true;
                                UpdateShellContents();
                                //ForceNavigation();
                            }


                            stopwatch.Stop();
                            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                            Shared.ImportCount++;
                            double outAVG = 0;
                            if (Shared.ImportCount < 2) { Shared.ImportAvg = elapsedMilliseconds; outAVG = Shared.ImportAvg; }
                            else { Shared.ImportAvg += elapsedMilliseconds; outAVG = Shared.ImportAvg / Shared.ImportCount; }
                            FileManager.Log("Imported data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG, 0);
                            FileManager.Log("=====================================================================", 0);
                        }
                        catch
                        {
                            FileManager.Log("Import data failure", 1);
                        }
                        finally
                        {
                            Monitor.Exit(Shared._dbLock);
                        }
                    }
                    else
                    {
                        FileManager.Log("Failed to acquire database lock within timeout.", 1);
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
            Shared.Delay--;
            if (Shared.Delay < 0)
            {
                Shared.Delay = Shared.DefaultDelay;
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
            if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    FileManager.SaveFiles();
                }
                finally
                {
                    Monitor.Exit(Shared._dbLock);
                }
            }
            else
            {
                FileManager.Log("Failed to acquire database lock within timeout.", 1);
            }
        }

    }
}
