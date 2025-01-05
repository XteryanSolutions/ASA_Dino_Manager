using Microsoft.Maui.Controls;
using System;

namespace ASA_Dino_Manager
{
    public partial class AppShell : Shell
    {
        private bool _isTimerRunning = false; // Timer control flag
        private bool disableNavSet = false;

        // Benchmark stuff
        private int ImportCount = 0;
        private double ImportAvg = 0; // keep track of average import time


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
            string[] classList = DataManager.GetAllClassesShort();

            // if manager initialized and we are not scanning and there is dinos in taglist
            if (!FileManager.Scanning && classList.Length > 0)
            {
               // DataManager.CleanDataBaseByID();
                DataManager.DeepCleanDatabase();
                CreateMenuContents();
            }
            else
            {
                Items.Clear();
                var shellContent = new ShellContent
                {
                    Title = "Looking for dinos",
                    ContentTemplate = new DataTemplate(typeof(MainPage)),
                    Route = "Looking_for_dinos"
                };
                Items.Add(shellContent);
                FileManager.Log("No dinos =(", 0);
                Shared.setPage = $"Looking_for_dinos";
            }
            StartTimer();
        }

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // FileManager.Log($"Navigated to: {e.Current.Location}", 0);
        }

        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (!disableNavSet) // make sure we are allowed to set new setPage
            {
                // now we should have the new target
                string target = e.Target.Location.ToString();

                // replace all / and trim it
                if (target.Replace("/", "").Trim() != Shared.setPage) // we navigate to a new page reset values
                {
                    Shared.setPage = target.Replace("/", "").Trim();
                    // got a new setpage
                    UpdateMenuContents(Shared.setPage);

                    // reset toggles and unselect dino when navigating
                    DinoPage.CurrentStats = Shared.DefaultStat; DinoPage.ToggleExcluded = Shared.DefaultToggle;
                    DinoPage.isSelected = false; DinoPage.isDouble = false; DinoPage.canDouble = false; DinoPage.selectedID = "";
                    DinoPage.sortM = Shared.DefaultSortM; DinoPage.sortF = Shared.DefaultSortF;

                    BabyPage.ToggleExcluded = Shared.DefaultToggleB; 
                    BabyPage.isSelected = false; BabyPage.isDouble = false; BabyPage.canDouble = false; BabyPage.selectedID = "";
                    BabyPage.sortM = Shared.DefaultSortM; BabyPage.sortF = Shared.DefaultSortF; BabyPage.speciesToggle = "All";

                    ArchivePage.isSelected = false; ArchivePage.selectedID = ""; ArchivePage.sortA = Shared.DefaultSortA;
                }
                else
                {
                    Shared.setPage = target.Replace("/", "").Trim();
                }
                

                FileManager.Log($"Navigating -> {Shared.setPage}", 0);

                // set the selected class to the entire class string
                // so we need to translate readable back to unreadable
                Shared.selectedClass = DataManager.ShortClassToLong(Shared.setPage);
            }
            else
            {
                // FileManager.Log("setPage is disabled", 1);
            }
        }

        public void CreateMenuContents()
        {
            disableNavSet = true; // FileManager.Log("Disabled setPage", 0);
            string[] classList = DataManager.GetAllClassesShort();

            DataManager.classSize = classList.Length;


            Items.Clear();

            // Retrieve the tag list from DataManager and sort alphabetically
            var sortedClassList = classList.OrderBy(tag => tag).ToArray();


            var shellContent1 = new ShellContent
            {
                Title = "Dino Manager",
                ContentTemplate = new DataTemplate(typeof(MainPage)),
                Route = $"ASA"
            };

            var shellContent2 = new ShellContent
            {
                Title = "Dino Archive",
                ContentTemplate = new DataTemplate(() => new ArchivePage()),
                Route = $"Archive"
            };

            var shellContent3 = new ShellContent
            {
                Title = "Baby Dinos",
                ContentTemplate = new DataTemplate(() => new BabyPage()),
                Route = $"Baby_Dinos"
            };

            // Add the ShellContent to the Shell
            Items.Add(shellContent1);
            Items.Add(shellContent2);
            Items.Add(shellContent3);

            // Loop through the sorted tags and create ShellContent dynamically
            foreach (var dinoClass in sortedClassList)
            {
                // string dinoTag = DataManager.ClassForTag(dinoClass);

                // int totalC = DataManager.DinoCount(dinoTag);

                var shellContent = new ShellContent
                {
                    Title = dinoClass.Replace("_", " "),
                    ContentTemplate = new DataTemplate(typeof(DinoPage)),
                    Route = $"{dinoClass}"
                };

                // Add the ShellContent to the Shell
                Items.Add(shellContent);
            }
            disableNavSet = false; // FileManager.Log("Enabled setPage", 0);
            FileManager.Log("Created classList", 0);
        }

        public void UpdateMenuContents(string page)
        {
            if (page != "")
            {
                disableNavSet = true; // FileManager.Log("Disabled setPage", 0);
                string[] classList = DataManager.GetAllClassesShort();
                DataManager.classSize = classList.Length;


                Items.Clear();

                // Retrieve the tag list from DataManager and sort alphabetically
                var sortedClassList = classList.OrderBy(tag => tag).ToArray();


                string location = page;
                string route = location.Replace(" ", "_");


                if (route.Contains("ASA")) 
                {
                    var shellContent0 = new ShellContent
                    {
                        Title = $"Dino Manager",
                        ContentTemplate = new DataTemplate(typeof(MainPage)),
                        Route = $"{route}"
                    };
                    Items.Add(shellContent0);
                }
                else if (route.Contains("Archive"))
                {
                    var shellContent0 = new ShellContent
                    {
                        Title = $"Dino Archive",
                        ContentTemplate = new DataTemplate(typeof(ArchivePage)),
                        Route = $"{route}"
                    };
                    Items.Add(shellContent0);
                }
                else if (route.Contains("Baby_Dinos"))
                {
                    var shellContent0 = new ShellContent
                    {
                        Title = $"Baby Dinos",
                        ContentTemplate = new DataTemplate(typeof(BabyPage)),
                        Route = $"{route}"
                    };
                    Items.Add(shellContent0);
                }
                else
                {
                    var shellContent0 = new ShellContent
                    {
                        Title = $"{location}",
                        ContentTemplate = new DataTemplate(typeof(DinoPage)),
                        Route = $"{route}"
                    };
                    Items.Add(shellContent0);
                }
                
                

                var shellContent1 = new ShellContent
                {
                    Title = "Dino Manager",
                    ContentTemplate = new DataTemplate(typeof(MainPage)),
                    Route = $"ASA"
                };

                var shellContent2 = new ShellContent
                {
                    Title = "Dino Archive",
                    ContentTemplate = new DataTemplate(() => new ArchivePage()),
                    Route = $"Archive"
                };

                var shellContent3 = new ShellContent
                {
                    Title = "Baby Dinos",
                    ContentTemplate = new DataTemplate(() => new BabyPage()),
                    Route = $"Baby_Dinos"
                };

                // Add the ShellContent to the Shell
                if (!route.Contains("ASA")) { Items.Add(shellContent1); }
                if (!route.Contains("Archive")) { Items.Add(shellContent2); }
                if (!route.Contains("Baby")) { Items.Add(shellContent3); }

                // Loop through the sorted tags and create ShellContent dynamically
                foreach (var dinoClass in sortedClassList)
                {
                    // string dinoTag = DataManager.ClassForTag(dinoClass);

                    // int totalC = DataManager.DinoCount(dinoTag);


                    var shellContent = new ShellContent
                    {
                        Title = dinoClass.Replace("_", " "),
                        ContentTemplate = new DataTemplate(typeof(DinoPage)),
                        Route = $"{dinoClass}"
                    };

                    if (route != dinoClass)
                    {
                        // Add the ShellContent to the Shell
                        Items.Add(shellContent);
                    }
                }
                disableNavSet = false; // FileManager.Log("Enabled setPage", 0);
            }
            else
            {
                FileManager.Log("No setPage", 1);
            }
        }

        public void ProcessAllFiles()
        {
            if (Shared.ImportEnabled)
            {
                // check if the gamepath works
                // maybe redundant checks???
                if (FileManager.CheckPath())
                {
                    if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            // handle import files first
                            DataManager.Import();

                            if (DataManager.ModC > 0 || DataManager.AddC > 0 || (Shared.setPage == "Baby_Dinos" && !BabyPage.isDouble))
                            {
                                UpdateMenuContents(Shared.setPage);
                            }     

                            if (DataManager.ModC > 0)
                            {
                                if (DataManager.ModC > 1)
                                {
                                    this.Title = $"{Shared.version} - Updated {DataManager.ModC} dinos";
                                }
                                else
                                {
                                    this.Title = $"{Shared.version} - Updated {DataManager.ModC} dino";
                                }
                            }
                            else if (DataManager.AddC > 0)
                            {
                                if (DataManager.AddC > 1)
                                {
                                    this.Title = $"{Shared.version} - Added {DataManager.AddC} dinos";
                                }
                                else
                                {
                                    this.Title = $"{Shared.version} - Added {DataManager.AddC} dino";
                                }
                            }
                            else
                            {
                                this.Title = Shared.version;
                            }

                            // Check if we need to reload data
                            if (DataManager.ModC > 0 || DataManager.AddC > 0)
                            {
                                // reset counters
                                DataManager.AddC = 0; DataManager.ModC = 0;

                                FileManager.Log("Updated DataBase", 0);

                                // request a save
                                FileManager.needSave = true;
                            }

                            stopwatch.Stop();
                            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;


                            ImportCount++;
                            double outAVG = 0;
                            if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                            else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }
                            FileManager.Log("Imported data in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG, 0);

                            // dynamicly adjust import time so we dont import when nothing is happening
                            if (!FileManager.needSave)
                            {
                                // add 1 second to delay
                                Shared.CurrentDelay = Shared.CurrentDelay + 1;
                                if (Shared.CurrentDelay > Shared.MaxDelay) { Shared.CurrentDelay = Shared.MaxDelay; }
                                // else { FileManager.Log($"No change. Increasing delay -> {Shared.CurrentDelay}", 0); }
                            }
                            else // and back down to rapid import if we detect a change
                            {
                                // FileManager.Log($"Change detected. Back to default time {Shared.DefaultDelay}", 0);
                                Shared.CurrentDelay = Shared.DefaultDelay;
                                // also set the delay thats already running
                                Shared.Delay = Shared.DefaultDelay;
                            }
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
            // check for a save request every second
            // maybe adjust this dynamicly too to not save every change
            SaveData();

            Shared.Delay--;
            if (Shared.Delay < 0)
            {
                Shared.Delay = Shared.CurrentDelay;
                ProcessAllFiles();
            }

            FileManager.WriteLog();
        }

        private void SaveData()
        {
            if (FileManager.needSave)
            {
                
                if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        FileManager.SaveFiles();
                        this.Title = this.Title + " " + Shared.Smap["Save"]; DisableSaveIcon();
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

        private async Task DisableSaveIcon()
        {
            await Task.Delay(1500);
            this.Title = this.Title.Replace(Shared.Smap["Save"], "");
        }
    }
}
