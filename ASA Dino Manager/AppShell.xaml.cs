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
                ContentTemplate = ArchivePage, // Replace with the appropriate page
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
                                ForceNavigation();
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

        public static void SelectDino(Label label, string id)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                if (Shared.selectedID != id) // dont select the same dino twice
                {
                    Shared.selectedID = id;

                    string name = DataManager.GetLastColumnData("ID", Shared.selectedID, "Name");
                    //this.Title = $"{name} - {id}"; // set title to dino name

                    FileManager.Log($"Selected {name} ID: {id}", 0); Shared.showStats = true;

                    ForceNavigation();
                }
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        public static void UnSelectDino(Grid grid)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                FileManager.Log($"Unselected {Shared.selectedID}", 0);
                Shared.selectedID = ""; Shared.showStats = false;
                // Handle the click event
                ForceNavigation();
            };

            // Attach the TapGestureRecognizer to the label
            grid.GestureRecognizers.Add(tapGesture);
        }

        public static void ForceUnselect()
        {
            if (Shared.selectedID != "")
            {
                FileManager.Log($"Force Unselected {Shared.selectedID}", 0);
                Shared.selectedID = ""; Shared.showStats = false;
            }
        }

        public static void OnButton0Clicked(object? sender, EventArgs e)
        {
            Shared.ToggleExcluded++;
            if (Shared.ToggleExcluded == 4)
            {
                Shared.ToggleExcluded = 0;
            }
            ForceUnselect();
            FileManager.Log($"Toggle Exclude {Shared.ToggleExcluded}", 0);
            // reload stuff
            ForceNavigation();
        }

        public static void OnButton1Clicked(object? sender, EventArgs e)
        {
            if (Shared.CurrentStats)
            {
                Shared.CurrentStats = false;
            }
            else
            {
                Shared.CurrentStats = true;
            }
            ForceUnselect();
            FileManager.Log($"Toggle Stats {Shared.CurrentStats}", 0);
            // reload stuff

            ForceNavigation();
        }

        public static void OnButton2Clicked(object? sender, EventArgs e)
        {
            if (Shared.selectedID != "")
            {
                string status = DataManager.GetStatus(Shared.selectedID);
                if (status == "Exclude") { status = ""; }
                else if (status == "") { status = "Exclude"; FileManager.Log($"Excluded ID: {Shared.selectedID}", 0); }
                DataManager.SetStatus(Shared.selectedID, status);

                FileManager.Log($"Unselected {Shared.selectedID}", 0);
                Shared.selectedID = ""; Shared.showStats = false;

                ForceNavigation();
            }
        }

        public static void OnButton3Clicked(object? sender, EventArgs e)
        {
            if (Shared.selectedID != "")
            {
                // Handle the click event
                string status = DataManager.GetStatus(Shared.selectedID);
                if (status == "Archived") { status = ""; FileManager.Log($"Restored ID: {Shared.selectedID}", 0); }
                else if (status == "") { status = "Archived"; FileManager.Log($"Archived ID: {Shared.selectedID}", 0); }
                else if (status == "Exclude") { status = "Archived"; FileManager.Log($"Archived ID: {Shared.selectedID}", 0); }
                DataManager.SetStatus(Shared.selectedID, status);


                // recompile the archive after archiving or unarchiving
                DataManager.CompileDinoArchive();

                FileManager.Log($"Unselected {Shared.selectedID}", 0);
                Shared.selectedID = ""; Shared.showStats = false;

                ForceNavigation();
            }

        }

        public static void OnButton4Clicked(object? sender, EventArgs e)
        {
            PurgeDinoAsync();
        }

        public static void OnButton5Clicked(object? sender, EventArgs e)
        {
            PurgeAllAsync();
        }

        public static async Task PurgeDinoAsync()
        {
            FileManager.Log("Purge Dino???", 1);
            bool answer = await Application.Current.MainPage.DisplayAlert(
    "Purge dino from DataBase",         // Title
    "Do you want to proceed?", // Message
    "Yes",                    // Yes button text
    "No"                      // No button text
);

            if (answer)
            {
                if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        Shared.needSave = true;
                        DataManager.DeleteRowsByID(Shared.selectedID);
                        // recompile archive after deleting a row
                        DataManager.CompileDinoArchive();
                        ForceNavigation();
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

        public static async Task PurgeAllAsync()
        {
            FileManager.Log("Purge Dino???", 1);
            bool answer = await Application.Current.MainPage.DisplayAlert(
    "Purge All dinos from DataBase",         // Title
    "Do you want to proceed?", // Message
    "Yes",                    // Yes button text
    "No"                      // No button text
);

            if (answer)
            {
                // User selected "Yes"  
                if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        DataManager.PurgeAll();
                        FileManager.Log("Purged All Dinos", 1);
                        // recompile archive after deleting all rows
                        DataManager.CompileDinoArchive();
                        ForceNavigation();
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

        public static async Task ForceNavigation()
        {
            FileManager.Log($"Going ->  {Shared.setPage} -> {Shared.setRoute}", 0);

            FileManager.Log($"Selected -> {Shared.selectedID}", 0);


            await AppShell.Current.GoToAsync($"{Shared.setPage}", true);



        }


    }
}
