﻿//using Android.Nfc;
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
        private bool disableAuto = false;


        public AppShell()
        {
            InitializeComponent(); 

            // set the title of the app to current version
            this.Title = Shared.version; 

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


            this.Navigated += OnShellNavigated;
            this.Navigating += OnShellNavigating;


            // create default shell
            CreateContent();
        }

        private void CreateContent()
        {
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

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            FileManager.Log($"Navigated to: {e.Current.Location}", 0);
            FileManager.Log($"before = {Shared.setPage}", 0);

            // reset toggles when navigating
            Shared.ToggleExcluded = 0; Shared.CurrentStats = false;
            Shared.showStats = false;


            // now we should have the new destination
            // from here route us to the same destination + 1


            // get the route
            var route = e.Current.Location.ToString();

            var routeSplit = route.Split(new[] { @"." }, StringSplitOptions.RemoveEmptyEntries);

            Shared.setPage = routeSplit[0];

            Shared.setPage = Shared.setPage.Replace("/", "").Trim();

            FileManager.Log($"after = {Shared.setPage}", 0);


            string dinoTag = DataManager.TagForClass(Shared.setPage);
            Shared.selectedClass = dinoTag;


            if (!disableAuto) // run once per navigation event to reroute to a new fresh page
            {
                disableAuto = true;
                string newRoute = $"{Shared.setPage}.{Shared.ReLoad()}";






                if (newRoute.Contains("Archive"))
                {
                    Routing.RegisterRoute(newRoute, typeof(ArchivePage));
                }
                else if (newRoute.Contains("ASA"))
                {
                    Routing.RegisterRoute(newRoute, typeof(MainPage));
                }
                else
                {
                    Routing.RegisterRoute(newRoute, typeof(DinoPage));
                }



                FileManager.Log($"Force navigating -> //{newRoute}", 0);

                Shell.Current.GoToAsync($"//{newRoute}");

                _ = UnlockNavigationAfterDelay(1000);

            }

        }

        private async Task UnlockNavigationAfterDelay(int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds);
            disableAuto = false; // Re-enable navigation
        }

        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            //FileManager.Log($"Navigating to: {e.Target.Location}", 0);

            // set default route
            if (Shared.setPage == "") 
            {
                Shared.setPage = $"ASA";
            }

            FileManager.Log($"setPage = {Shared.setPage}", 0);
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
                Route = $"ASA.{Shared.ReLoad()}"
            };
            // Add the ShellContent to the Shell
            Items.Add(shellContent1);



            var shellContent2 = new ShellContent
            {
                Title = "Dino Archive",
                ContentTemplate = new DataTemplate(() => new ArchivePage()), // Always a fresh page
                Route = $"Archive.{Shared.ReLoad()}"
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
                    Route = $"{tag}.{Shared.ReLoad()}"
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
