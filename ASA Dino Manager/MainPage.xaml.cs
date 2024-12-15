using System.Data;
//using System.Drawing;
using Microsoft.Maui.Handlers;
//using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Controls.StyleSheets;

namespace ASA_Dino_Manager
{

    public partial class MainPage : ContentPage
    {
        // Toggles for viewing stats
        public static int ToggleExcluded = 0;
        public static bool CurrentStats = false;

        // Benchmark stuff
        private static int RefreshCount = 0;
        private static double RefreshAvg = 0; // keep track of average import time

        private bool _isTimerRunning = false; // Timer control flag

        private bool isLoaded = false; // tag to prevent extra navigation triggers

        public string selectedID = "";


        public MainPage()
        {
            InitializeComponent();

            Shell.Current.Navigated += OnShellNavigated;

            StartTimer();
        }

        private void StartTimer()
        {
            _isTimerRunning = true; // Flag to control the timer

            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                if (!_isTimerRunning)
                    return false; // Stop the timer

                TriggerFunction();
                return true; // Continue running the timer
            });
        }

        private void TriggerFunction()
        {
            if (AppShell.needUpdate)
            {
                FileManager.Log("Import Requesting GUI refresh");
                AppShell.needUpdate = false;
                RefreshContent(false);
            }
           // RefreshContent();

            FileManager.WriteLog();
        }

        public void StopTimer()
        {
            _isTimerRunning = false; // Call this to stop the timer if needed
        }


        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Check if the navigation is to the current page
            if (e.Source == ShellNavigationSource.ShellItemChanged)
            {
               if (!isLoaded)
                {
                    FileManager.Log("Navigated Species");
                    RefreshContent(false);
                    isLoaded = true;
                }
            }
        }

        private Grid CreateButtonGrid()
        {
            var grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 5,
                Padding = 10
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

            var bColor1 = Colors.LightBlue;
            var bColor2 = Colors.LightBlue;


            if (MainPage.CurrentStats)
            {
                bColor2 = Colors.LightGreen;
            }

            if (MainPage.ToggleExcluded == 0)
            {
                bColor1 = Colors.LightBlue;
            }
            else if (MainPage.ToggleExcluded == 1)
            {
                bColor1 = Colors.LightGreen;
            }
            else if (MainPage.ToggleExcluded == 2)
            {
                bColor1 = Colors.LightYellow;
            }
            else if (MainPage.ToggleExcluded == 3)
            {
                bColor1 = Colors.IndianRed;
            }

            string staText = "Breeding Stats";

            string btnText = "Toggle";
            if (ToggleExcluded == 0) { btnText = "All"; }
            else if (ToggleExcluded == 1) { btnText = "Included"; }
            else if (ToggleExcluded == 2) { btnText = "Excluded"; }
            else if (ToggleExcluded == 3) { btnText = "Archived"; }

            if (CurrentStats) { staText = "Current Stats"; }

            var topButton1 = new Button { Text = btnText, BackgroundColor = bColor1 };
            var topButton2 = new Button { Text = staText, BackgroundColor = bColor2 };



            AddToGrid(grid, topButton1 , 0, 0);
            AddToGrid(grid, topButton2, 0, 1);



            // Create the fixed button


            topButton1.Clicked += OnLeftButtonClicked;
            topButton2.Clicked += OnRightButtonClicked;


            return grid;
        }

        void SelectDino(Label label, string id)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event and pass additional data
                selectedID = id;

                FileManager.Log($"Showing stats for ID: {id}");
                RefreshContent(true);
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        void UnSelectDino(Grid grid)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event
                RefreshContent(false);
            };

            // Attach the TapGestureRecognizer to the label
            grid.GestureRecognizers.Add(tapGesture);
        }
         
        void ExcludeDino(Label label)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event
                string status = DataManager.GetStatus(selectedID);
                if (status == "Exclude") { status = ""; }
                else if (status == "") { status = "Exclude"; }
                DataManager.SetStatus(selectedID,status);

                RefreshContent(false);
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        void ArchiveDino(Label label)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event
                string status = DataManager.GetStatus(selectedID);
                if (status == "Archived") { status = ""; }
                else if (status == "") { status = "Archived"; }
                else if (status == "Exclude") { status = "Archived"; }
                DataManager.SetStatus(selectedID, status);

                RefreshContent(false); 
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        private async Task PurgeDinoAsync()
        {
            FileManager.Log("Purge Dino???");
            bool answer = await Application.Current.MainPage.DisplayAlert(
    "Purge dino from DataBase",         // Title
    "Do you want to proceed?", // Message
    "Yes",                    // Yes button text
    "No"                      // No button text
);

            if (answer)
            {
                // User selected "Yes"
                FileManager.Log("Yep DO IT");

                if (Monitor.TryEnter(AppShell._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        FileManager.needSave = true;
                        DataManager.DeleteRowsByID(selectedID);
                    }
                    finally
                    {
                        Monitor.Exit(AppShell._dbLock);
                    }
                }
                else
                {
                    FileManager.Log("Failed to acquire database lock within timeout.");
                }
                RefreshContent(false);
            }
            else
            {
                // User selected "No"
            }
        }

        private async Task PurgeAllAsync()
        {
            FileManager.Log("Purge Dino???");
            bool answer = await Application.Current.MainPage.DisplayAlert(
    "Purge All dinos from DataBase",         // Title
    "Do you want to proceed?", // Message
    "Yes",                    // Yes button text
    "No"                      // No button text
);

            if (answer)
            {
                // User selected "Yes"
                FileManager.Log("Yep DO IT");

                DataManager.PurgeAll();

                RefreshContent(false);
            }
            else
            {
                // User selected "No"
            }
        }

        void PurgeDino(Label label)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                PurgeDinoAsync();
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        void PurgeAll(Label label)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                PurgeAllAsync();
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture);
        }

        private void OnLeftButtonClicked(object? sender, EventArgs e)
        {
            ToggleExcluded++;
            if (ToggleExcluded == 4)
            {
                ToggleExcluded = 0;
            }
            // reload stuff
            RefreshContent(false);
        }

        private void OnRightButtonClicked(object? sender, EventArgs e)
        {
            if (CurrentStats)
            {
                CurrentStats = false;
            }
            else
            {
                CurrentStats = true;
            }
            // reload stuff
            RefreshContent(false);
        }

        private void AddToGrid(Grid grid, View view, int row, int column)
        {
            // Ensure rows exist up to the specified index
            while (grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }

            // Set the row and column for the view
            Grid.SetRow(view, row);
            Grid.SetColumn(view, column);

            // Add the view to the grid
            grid.Children.Add(view);
        }

        private Grid CreateDinoGrid(DataTable table, string title, bool showStats)
        {
            var grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 2
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 3
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 5
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 6
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 7
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 8
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 9
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 10
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 11


            var maleColor = Colors.LightBlue;
            var femaleColor = Colors.Pink;
            var breedColor = Colors.LightYellow;
            var goodColor = Colors.LightGreen;



            var headerColor = maleColor;
            var DefaultColor = maleColor;


            if (title == "Male") { DefaultColor = maleColor; }
            else if (title == "Female") { DefaultColor = femaleColor; }
            else { DefaultColor = breedColor; }


            headerColor = DefaultColor;




            // Add header row
            AddToGrid(grid, new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 0);
            AddToGrid(grid, new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 1);
            AddToGrid(grid, new Label { Text = "Hp", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 2);
            AddToGrid(grid, new Label { Text = "Stamina", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 3);
            AddToGrid(grid, new Label { Text = "Oxygen", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 4);
            AddToGrid(grid, new Label { Text = "Food", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 5);
            AddToGrid(grid, new Label { Text = "Weight", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 6);
            AddToGrid(grid, new Label { Text = "Damage", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 7);
            AddToGrid(grid, new Label { Text = "Status", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 8);
            AddToGrid(grid, new Label { Text = "Papa", FontAttributes = FontAttributes.Bold, TextColor = maleColor }, 0, 9);
            AddToGrid(grid, new Label { Text = "Mama", FontAttributes = FontAttributes.Bold, TextColor = femaleColor }, 0, 10);

            //AddToGrid(grid, new Label { Text = "", FontAttributes = FontAttributes.Bold, TextColor = femaleColor }, 0, 11);



            int rowIndex = 1; // Start adding rows below the header

            foreach (DataRow row in table.Rows)
            {
                var cellColor0 = DefaultColor;

                var cellColor1 = DefaultColor;
                var cellColor2 = DefaultColor;
                var cellColor3 = DefaultColor;
                var cellColor4 = DefaultColor;
                var cellColor5 = DefaultColor;
                var cellColor6 = DefaultColor;
                var cellColor7 = DefaultColor;
                var cellColor8 = DefaultColor;




                string name = row["Name"].ToString();
                string level = row["Level"].ToString();
                string hp = row["Hp"].ToString();
                string stamina = row["Stamina"].ToString();
                string oxygen = row["Oxygen"].ToString();
                string food = row["Food"].ToString();
                string weight = row["Weight"].ToString();
                string damage = row["Damage"].ToString();
                string status = row["Status"].ToString();

                string papa = row["Papa"].ToString();
                string mama = row["Mama"].ToString();

                string id = row["ID"].ToString();

                if (DataManager.ToDouble(level) >= DataManager.LevelMax) { cellColor2 = goodColor; }
                if (DataManager.ToDouble(hp) >= DataManager.HpMax) { cellColor2 = goodColor; }
                if (DataManager.ToDouble(stamina) >= DataManager.StaminaMax) { cellColor3 = goodColor; }
                if (DataManager.ToDouble(oxygen) >= DataManager.OxygenMax) { cellColor4 = goodColor; }
                if (DataManager.ToDouble(food) >= DataManager.FoodMax) { cellColor5 = goodColor; }
                if (DataManager.ToDouble(weight) >= DataManager.WeightMax) { cellColor6 = goodColor; }
                if (DataManager.ToDouble(damage) >= DataManager.DamageMax) { cellColor7 = goodColor; }


                string age = row["Age"].ToString();
                double ageD = DataManager.ToDouble(age);

                if (ageD < 100 && !name.Contains("Breed #") && status == "") { status = ageD + "% Grown"; }

                // Create a Label
                var nameL = new Label { Text = name, TextColor = cellColor0 };
                var levelL = new Label { Text = level, TextColor = cellColor1 };
                var hpL = new Label { Text = hp, TextColor = cellColor2 };
                var staminaL = new Label { Text = stamina, TextColor = cellColor3 };
                var oxygenL = new Label { Text = oxygen, TextColor = cellColor4 };
                var foodL = new Label { Text = food, TextColor = cellColor5 };
                var weightL = new Label { Text = weight, TextColor = cellColor6 };
                var damageL = new Label { Text = damage, TextColor = cellColor7 };
                var statusL = new Label { Text = status, TextColor = cellColor8 };
                var papaL = new Label { Text = papa, TextColor = maleColor };
                var mamaL = new Label { Text = mama, TextColor = femaleColor };


                if (title != "Bottom")
                {
                    // Call the method to create and attach TapGesture
                    SelectDino(nameL, id);
                    SelectDino(levelL, id);
                    SelectDino(hpL, id);
                    SelectDino(staminaL, id);
                    SelectDino(oxygenL, id);
                    SelectDino(foodL, id);
                    SelectDino(weightL, id);
                    SelectDino(damageL, id);
                    SelectDino(statusL, id);
                    SelectDino(papaL, id);
                    SelectDino(mamaL, id);
                }


                // add items to grid
                AddToGrid(grid, nameL, rowIndex, 0);
                AddToGrid(grid, levelL, rowIndex, 1);
                AddToGrid(grid, hpL, rowIndex, 2);
                AddToGrid(grid, staminaL, rowIndex, 3);
                AddToGrid(grid, oxygenL, rowIndex, 4);
                AddToGrid(grid, foodL, rowIndex, 5);
                AddToGrid(grid, weightL, rowIndex, 6);
                AddToGrid(grid, damageL, rowIndex, 7);
                AddToGrid(grid, statusL, rowIndex, 8);
                AddToGrid(grid, papaL, rowIndex, 9);
                AddToGrid(grid, mamaL, rowIndex, 10);

                rowIndex++;
            }


            if (title == "Bottom" && showStats)
            {
                string tx = "Exclude"; var cellColor = Colors.Yellow;
                if (DataManager.GetStatus(selectedID) == "Exclude") { tx = "Include"; cellColor = Colors.LightGreen; }
                var excludeL = new Label { Text = tx, TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                ExcludeDino(excludeL);
                AddToGrid(grid, excludeL, 0, 11);
            }

            if (title == "Bottom" && showStats)
            {
                string tx = "Archive"; var cellColor = Colors.Red;
                if (DataManager.GetStatus(selectedID) == "Archived") { tx = "Restore"; cellColor = Colors.LightGreen; }  
                var archiveL = new Label { Text = tx, TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                ArchiveDino(archiveL);
                AddToGrid(grid, archiveL, 1, 11);
            }

            return grid;
        }

        private Grid CreateArchiveGrid(DataTable table, string title, bool showStats)
        {
            var grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 2
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 3
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4

            var maleColor = Colors.LightBlue;
            var femaleColor = Colors.Pink;
            var breedColor = Colors.LightYellow;
            var goodColor = Colors.LightGreen;

            var headerColor = breedColor;
            var DefaultColor = breedColor;
           
            if (title != "Bottom")
            {
                // Add header row
                AddToGrid(grid, new Label { Text = "ID", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 0);
                AddToGrid(grid, new Label { Text = "Tag", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 1);
                AddToGrid(grid, new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 2);
                AddToGrid(grid, new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 3);
                AddToGrid(grid, new Label { Text = "", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 4);

                int rowIndex = 1; // Start adding rows below the header

                foreach (DataRow row in table.Rows)
                {
                    string id = row["ID"].ToString();
                    string tag = row["Tag"].ToString();
                    string name = row["Name"].ToString();
                    string level = row["Level"].ToString();

                    string sex = DataManager.GetLastColumnData("ID", id, "Sex");
                    if (sex == "Female") 
                    {
                        DefaultColor = femaleColor;
                    }
                    else
                    {
                        DefaultColor = maleColor;
                    }

                    var cellColor0 = DefaultColor;
                    var cellColor1 = DefaultColor;
                    var cellColor2 = DefaultColor;
                    var cellColor3 = DefaultColor;
                    var cellColor4 = DefaultColor;

                    // Create a Label
                    var idL = new Label { Text = id, TextColor = cellColor0 };
                    var tagL = new Label { Text = tag, TextColor = cellColor1 };
                    var nameL = new Label { Text = name, TextColor = cellColor2 };
                    var levelL = new Label { Text = level, TextColor = cellColor3 };

                    // Call the method to create and attach TapGesture
                    SelectDino(idL, id);
                    SelectDino(tagL, id);
                    SelectDino(nameL, id);
                    SelectDino(levelL, id);

                    // add items to grid
                    AddToGrid(grid, idL, rowIndex, 0);
                    AddToGrid(grid, tagL, rowIndex, 1);
                    AddToGrid(grid, nameL, rowIndex, 2);
                    AddToGrid(grid, levelL, rowIndex, 3);

                    rowIndex++;
                }
            }
            else
            {
                if (showStats)
                {   
                    string tx = "Archive"; var cellColor = Colors.Yellow;
                    if (DataManager.GetStatus(selectedID) == "Archived")  { tx = "Restore"; cellColor = Colors.LightGreen; }
                    var archiveL = new Label { Text = tx, TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                    ArchiveDino(archiveL);
                    AddToGrid(grid, archiveL, 0, 0);
                }
                if (showStats)
                {  
                    string name= DataManager.GetLastColumnData("ID", selectedID, "Name");
                    string tx = "[ " + name; var cellColor = DefaultColor;
                    string sex = DataManager.GetLastColumnData("ID", selectedID, "Sex");
                    if (sex == "Female")
                    {
                        cellColor = femaleColor;
                    }
                    else
                    {
                        cellColor = maleColor;
                    }
                    var archiveL = new Label { Text = tx, TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                    ArchiveDino(archiveL);
                    AddToGrid(grid, archiveL, 0, 1);
                }

                if (showStats)
                {
                    string tx = selectedID + " ]"; var cellColor = DefaultColor;
                    string sex = DataManager.GetLastColumnData("ID", selectedID, "Sex");
                    if (sex == "Female")
                    {
                        cellColor = femaleColor;
                    }
                    else
                    {
                        cellColor = maleColor;
                    }
                    var archiveL = new Label { Text = tx, TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                    ArchiveDino(archiveL);
                    AddToGrid(grid, archiveL, 0, 2);
                }


                if (showStats)
                {
                    var cellColor = Colors.Red;
                    var archiveL = new Label { Text = "Purge", TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                    PurgeDino(archiveL);
                    AddToGrid(grid, archiveL, 0, 3);
                }

                if (true)
                {
                    var cellColor = Colors.Red;
                    var archiveL = new Label { Text = "Purge All", TextColor = cellColor, HorizontalOptions = LayoutOptions.End };
                    PurgeAll(archiveL);
                    AddToGrid(grid, archiveL, 0, 4);
                }

            }


            return grid;
        }

        public void RefreshContent(bool stat)
        {
            if (Monitor.TryEnter(AppShell._dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    MyDelayedOperationAsync(stat);
                }
                finally
                {
                    Monitor.Exit(AppShell._dbLock);
                }
            }
            else
            {
                FileManager.Log("Failed to acquire database lock within timeout.");
                //Console.WriteLine("Failed to acquire database lock within timeout.");
            }
        }

        public async Task MyDelayedOperationAsync(bool showStats)
        {
            await Task.Delay(10); // Wait for 2 seconds
            isLoaded = false;

            RouteContent(showStats);
            
        }

        public void RouteContent(bool showStats)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();    // start timer here
            var route = Shell.Current.CurrentState.Location.ToString();
            route = route.Replace("/", "");


            // get the selected species
            string dinoTag = DataManager.TagForClass(route);
            DataManager.selectedClass = dinoTag;

            // update data with new status
            //DataManager.GetDinoData(DataManager.selectedClass);

            // Load necessary data based on toggles
            if (!string.IsNullOrEmpty(DataManager.selectedClass))
            {
                if (showStats)
                {
                    DataManager.GetOneDinoData(selectedID);

                    if (MainPage.ToggleExcluded != 3)
                    {
                        DataManager.SetMaxStats();
                    }
                }
                else
                {
                    DataManager.GetDinoData(DataManager.selectedClass);

                    if (MainPage.ToggleExcluded != 3)
                    {
                        DataManager.SetMaxStats();
                    }

                    if (!CurrentStats && MainPage.ToggleExcluded != 2 && MainPage.ToggleExcluded != 3)
                    {
                        DataManager.SetBinaryStats();
                        DataManager.GetBestPartner();
                    }
                }
            }


            // Retrieve female data
            string[] females = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Female", "ID");
            // Retrieve male data
            string[] males = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Male", "ID");
            int totalC = females.Length + males.Length;


            FileManager.Log("Routing -> " + route);
            this.Content = null;
            if (route == "Looking for dinos")
            {

            }
            else if (route == "ASA")
            {
                UpdateStartContentPage("Feed Dino");
            }
            else if (route == "Archive")
            {
                DataManager.GetDinoArchive();

                if (DataManager.ArchiveTable.Rows.Count < 1)
                {
                    UpdateStartContentPage("No dinos in here :(");
                }
                else
                {
                    UpdateArchiveContentPage(showStats);
                }
               
            }
            else
            {
                if (totalC == 0) 
                {
                    UpdateStartContentPage("No dinos in here :(");
                }
                else
                {
                    UpdateMainContentPage(showStats);
                }
            }
            stopwatch.Stop(); // stop timer here

            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            RefreshCount++;
            double outAVG = 0;
            if (RefreshCount < 2) { RefreshAvg = elapsedMilliseconds; outAVG = RefreshAvg; }
            else { RefreshAvg += elapsedMilliseconds; outAVG = RefreshAvg / RefreshCount; }
            FileManager.Log("Refreshed GUI - " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
            FileManager.Log("=====================================================================");
        }


        private void UpdateStartContentPage(string labelText)
        {
            var mainLayout = new Grid();
            UnSelectDino(mainLayout);

            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            var image1 = new Image { Source = "dino.png",HeightRequest = 155,Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start};

            var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center ,VerticalOptions = LayoutOptions.Start };


            AddToGrid(mainLayout, image1, 0, 0);
            AddToGrid(mainLayout, label1, 1, 0);




            // Wrap the scrollable content in a ScrollView and add it to the second row
            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(mainLayout, scrollView, 0, 0);

            this.Content = null;
            this.Content = mainLayout;
        }

        private void UpdateArchiveContentPage(bool showStats)
        {
            // ==============================================================    Create Dino Layout   =====================================================

            // Create the main layout
            var mainLayout = new Grid();

            UnSelectDino(mainLayout);



            // dynamically adjust the bottom bar height
            int t = DataManager.BottomTable.Rows.Count;
            int rowH = 20;
            int barH = (t * rowH) + rowH + 10;
            if (t > 5) { barH = 127; }

            else if (MainPage.ToggleExcluded == 3 && !showStats) { barH = 0; }
            else if (MainPage.ToggleExcluded == 2 && !showStats) { barH = 0; }

            barH = 40;


            // Define row definitions
            // mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = barH }); // Scrollable content


            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scrollable content
            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            scrollContent.Children.Add(CreateArchiveGrid(DataManager.ArchiveTable, "Archive", showStats));

            // Wrap the scrollable content in a ScrollView and add it to the second row
            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(mainLayout, scrollView, 0, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scrollable content
            var bottomContent = new StackLayout
            {
                Spacing = 0,
                Padding = 3,
                BackgroundColor = Color.FromArgb("#312f38")
            };

            bottomContent.Children.Add(CreateArchiveGrid(DataManager.ArchiveTable, "Bottom", showStats));

            // Wrap the scrollable content in a ScrollView and add it to the third row
            var bottomPanel = new ScrollView { Content = bottomContent };

            AddToGrid(mainLayout, bottomPanel, 1, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            this.Content = mainLayout;
        }

        public void UpdateMainContentPage(bool showStats)
        {
            // ==============================================================    Create Dino Layout   =====================================================

            // Create the main layout
            var mainLayout = new Grid();

            UnSelectDino(mainLayout);

            // dynamically adjust the bottom bar height
            int rowCount = DataManager.BottomTable.Rows.Count;
            int rowHeight = 20;
            int barH = (rowCount * rowHeight) + rowHeight + 11;
            if (rowCount > 5) { barH = 127; }

            if (MainPage.ToggleExcluded == 3 && !showStats) { barH = 0; }
            if (MainPage.ToggleExcluded == 2 && !showStats) { barH = 0; }


            // Define row definitions
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = barH }); // Scrollable content





            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Add the button grid
            AddToGrid(mainLayout, CreateButtonGrid(), 0, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scrollable content
            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            // Add male and female tables
            scrollContent.Children.Add(CreateDinoGrid(DataManager.MaleTable, "Male", showStats));
            scrollContent.Children.Add(CreateDinoGrid(DataManager.FemaleTable, "Female", showStats));

            // Wrap the scrollable content in a ScrollView and add it to the second row
            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(mainLayout, scrollView, 1, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scrollable content
            var bottomContent = new StackLayout
            {
                Spacing = 0,
                Padding = 3,
                BackgroundColor = Color.FromArgb("#312f38")
            };

            bottomContent.Children.Add(CreateDinoGrid(DataManager.BottomTable, "Bottom", showStats));

            // Wrap the scrollable content in a ScrollView and add it to the third row
            var bottomPanel = new ScrollView { Content = bottomContent };

            AddToGrid(mainLayout, bottomPanel, 2, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            this.Content = mainLayout;
        }




    }
}
