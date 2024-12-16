using System.Data;
//using System.Drawing;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Controls.StyleSheets;
using System.Xml.Linq;
using Microsoft.Maui.Graphics.Text;
using System.Text;

namespace ASA_Dino_Manager
{

    public partial class MainPage : ContentPage
    {



        public MainPage()
        {
            InitializeComponent();


            if (!Vars.eventDisabled)
            {
                var route = Shell.Current.CurrentState.Location.ToString();
                route = route.Replace("/", "");
                Vars.setRoute = route;

                FileManager.Log($"MainPage setRoute -> {route}");


                //ToggleExcluded = 0; CurrentStats = false; // reset toggles when navigating
            }
            RefreshContent(false);
        }

        void SelectDino(Label label, string id)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event and pass additional data
                Vars.selectedID = id;

                string name = DataManager.GetLastColumnData("ID", Vars.selectedID, "Name");

                FileManager.Log($"Showing stats for ID: {id}");
                RefreshContent(true);
                this.Title = name;

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

        private void OnButton0Clicked(object? sender, EventArgs e)
        {
            Vars.ToggleExcluded++;
            if (Vars.ToggleExcluded == 4)
            {
                Vars.ToggleExcluded = 0;
            }
            // reload stuff
            RefreshContent(false);
        }

        private void OnButton1Clicked(object? sender, EventArgs e)
        {
            if (Vars.CurrentStats)
            {
                Vars.CurrentStats = false;
            }
            else
            {
                Vars.CurrentStats = true;
            }
            // reload stuff
            RefreshContent(false);
        }

        private void OnButton2Clicked(object? sender, EventArgs e)
        {
            // Handle the click event
            string status = DataManager.GetStatus(Vars.selectedID);
            if (status == "Exclude") { status = ""; }
            else if (status == "") { status = "Exclude"; }
            DataManager.SetStatus(Vars.selectedID, status);

            RefreshContent(false);
        }

        private void OnButton3Clicked(object? sender, EventArgs e)
        {
            // Handle the click event
            string status = DataManager.GetStatus(Vars.selectedID);
            if (status == "Archived") { status = ""; }
            else if (status == "") { status = "Archived"; }
            else if (status == "Exclude") { status = "Archived"; }
            DataManager.SetStatus(Vars.selectedID, status);

            RefreshContent(false);
        }

        private void OnButton4Clicked(object? sender, EventArgs e)
        {
            PurgeDinoAsync();
        }

        private void OnButton5Clicked(object? sender, EventArgs e)
        {
            PurgeAllAsync();
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
                if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        FileManager.needSave = true;
                        DataManager.DeleteRowsByID(Vars.selectedID);
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
                RefreshContent(false);
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
                if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        DataManager.PurgeAll();
                        FileManager.Log("Purged All Dinos");
                        RefreshContent(false);
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 11

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 12
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 13
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 14
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 15



            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 16


            Vars.DefaultColor = Vars.maleColor;

            if (title == "Male") { Vars.DefaultColor = Vars.maleColor; }
            else if (title == "Female") { Vars.DefaultColor = Vars.femaleColor; }
            else { Vars.DefaultColor = Vars.breedColor; }


            Vars.headerColor = Vars.DefaultColor;

            int fSize = 16;  // header fontsize

            var header0 = new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header1 = new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header2 = new Label { Text = "Hp", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header3 = new Label { Text = "Stamina", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header4 = new Label { Text = "Oxygen", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header5 = new Label { Text = "Food", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header6 = new Label { Text = "Weight", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header7 = new Label { Text = "Damage", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header8 = new Label { Text = "Status", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header9 = new Label { Text = "Gen", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header10 = new Label { Text = "Papa", FontAttributes = FontAttributes.Bold, TextColor = Vars.maleColor, FontSize = fSize };
            var header11 = new Label { Text = "Mama", FontAttributes = FontAttributes.Bold, TextColor = Vars.femaleColor, FontSize = fSize };

            var header12 = new Label { Text = "PapaMute", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header13 = new Label { Text = "MamaMute", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header14 = new Label { Text = "Imprint", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };
            var header15 = new Label { Text = "Imprinter", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor, FontSize = fSize };



            string sexS = "T";
            if (title == "Male") { sexS = "M"; }
            else { sexS = "F"; }

            if (title != "Bottom") // not sortable bottom row
            {
                SortColumn(header0, sexS);
                SortColumn(header1, sexS);
                SortColumn(header2, sexS);
                SortColumn(header3, sexS);
                SortColumn(header4, sexS);
                SortColumn(header5, sexS);
                SortColumn(header6, sexS);
                SortColumn(header7, sexS);
                SortColumn(header8, sexS);
                SortColumn(header9, sexS);
                SortColumn(header10, sexS);
                SortColumn(header11, sexS);
                SortColumn(header12, sexS);
                SortColumn(header13, sexS);
                SortColumn(header14, sexS);
                SortColumn(header15, sexS);
            }

            // Add header row
            AddToGrid(grid, header0, 0, 0);
            AddToGrid(grid, header1, 0, 1);
            AddToGrid(grid, header2, 0, 2);
            AddToGrid(grid, header3, 0, 3);
            AddToGrid(grid, header4, 0, 4);
            AddToGrid(grid, header5, 0, 5);
            AddToGrid(grid, header6, 0, 6);
            AddToGrid(grid, header7, 0, 7);
            AddToGrid(grid, header8, 0, 8);
            AddToGrid(grid, header9, 0, 9);
            AddToGrid(grid, header10, 0, 10);
            AddToGrid(grid, header11, 0, 11);

            if (title != "Bottom" || showStats)
            {
                AddToGrid(grid, header12, 0, 12);
                AddToGrid(grid, header13, 0, 13);
                AddToGrid(grid, header14, 0, 14);
                AddToGrid(grid, header15, 0, 15);

            }

            int rowIndex = 1; // Start adding rows below the header

            foreach (DataRow row in table.Rows)
            {
                var cellColor0 = Vars.DefaultColor;
                var cellColor1 = Vars.DefaultColor;
                var cellColor2 = Vars.DefaultColor;
                var cellColor3 = Vars.DefaultColor;
                var cellColor4 = Vars.DefaultColor;
                var cellColor5 = Vars.DefaultColor;
                var cellColor6 = Vars.DefaultColor;
                var cellColor7 = Vars.DefaultColor;

                var cellColor8 = Vars.DefaultColor;

                string id = row["ID"].ToString();


                string name = row["Name"].ToString();
                string level = row["Level"].ToString();
                //////////////
                string hp = row["Hp"].ToString();
                string stamina = row["Stamina"].ToString();
                string oxygen = row["Oxygen"].ToString();
                string food = row["Food"].ToString();
                string weight = row["Weight"].ToString();
                string damage = row["Damage"].ToString();
                //////////////
                string status = row["Status"].ToString();
                string gen = row["Gen"].ToString();
                string papa = row["Papa"].ToString();
                string mama = row["Mama"].ToString();
                string papaM = row["PapaMute"].ToString();
                string mamaM = row["MamaMute"].ToString();
                string imprint = row["Imprint"].ToString();
                string imprinter = row["Imprinter"].ToString();


                if (Vars.ToggleExcluded == 2)
                {
                    if (status == "Exclude") { status = ""; }
                }


                //recolor breeding stats
                if (DataManager.ToDouble(level) >= DataManager.LevelMax) { cellColor1 = Vars.goodColor; }
                if (DataManager.ToDouble(hp) >= DataManager.HpMax) { cellColor2 = Vars.goodColor; }
                if (DataManager.ToDouble(stamina) >= DataManager.StaminaMax) { cellColor3 = Vars.goodColor; }
                if (DataManager.ToDouble(oxygen) >= DataManager.OxygenMax) { cellColor4 = Vars.goodColor; }
                if (DataManager.ToDouble(food) >= DataManager.FoodMax) { cellColor5 = Vars.goodColor; }
                if (DataManager.ToDouble(weight) >= DataManager.WeightMax) { cellColor6 = Vars.goodColor; }
                if (DataManager.ToDouble(damage) >= DataManager.DamageMax) { cellColor7 = Vars.goodColor; }


                // mutation detection overrides normal coloring -> mutaColor
                string mutes = DataManager.GetMutes(id);
                if (mutes.Length == 6 && !Vars.CurrentStats) // dont show mutations on current statview
                {
                    string aC = mutes.Substring(0, 1); string bC = mutes.Substring(1, 1); string cC = mutes.Substring(2, 1);
                    string dC = mutes.Substring(3, 1); string eC = mutes.Substring(4, 1); string fC = mutes.Substring(5, 1);

                    if (aC == "1") { cellColor2 = Vars.mutaColor; }
                    if (bC == "1") { cellColor3 = Vars.mutaColor; }
                    if (cC == "1") { cellColor4 = Vars.mutaColor; }
                    if (dC == "1") { cellColor5 = Vars.mutaColor; }
                    if (eC == "1") { cellColor6 = Vars.mutaColor; }
                    if (fC == "1") { cellColor7 = Vars.mutaColor; }
                }

                // Baby detection
                string age = row["Age"].ToString();
                double ageD = DataManager.ToDouble(age);
                if (ageD < 100 && !name.Contains("Breed #") && status == "") { status = ageD + "% Grown"; }



                // Create a Labels
                var nameL = new Label { Text = name, TextColor = cellColor0 };
                var levelL = new Label { Text = level, TextColor = cellColor1 };
                //////////////
                var hpL = new Label { Text = hp, TextColor = cellColor2 };
                var staminaL = new Label { Text = stamina, TextColor = cellColor3 };
                var oxygenL = new Label { Text = oxygen, TextColor = cellColor4 };
                var foodL = new Label { Text = food, TextColor = cellColor5 };
                var weightL = new Label { Text = weight, TextColor = cellColor6 };
                var damageL = new Label { Text = damage, TextColor = cellColor7 };
                //////////////
                var statusL = new Label { Text = status, TextColor = cellColor8 };
                var genL = new Label { Text = gen, TextColor = cellColor8 };
                var papaL = new Label { Text = papa, TextColor = Vars.maleColor };
                var mamaL = new Label { Text = mama, TextColor = Vars.femaleColor };
                var papaML = new Label { Text = papaM, TextColor = cellColor8 };
                var mamaML = new Label { Text = mamaM, TextColor = cellColor8 };
                var imprintL = new Label { Text = imprint, TextColor = cellColor8 };
                var imprinterL = new Label { Text = imprinter, TextColor = cellColor8 };




                if (title != "Bottom") // dont make bottom panel selectable
                {
                    // Attach TapGesture to all labels
                    SelectDino(nameL, id);
                    SelectDino(levelL, id);
                    SelectDino(hpL, id);
                    SelectDino(staminaL, id);
                    SelectDino(oxygenL, id);
                    SelectDino(foodL, id);
                    SelectDino(weightL, id);
                    SelectDino(damageL, id);
                    SelectDino(statusL, id);
                    SelectDino(genL, id);
                    SelectDino(papaL, id);
                    SelectDino(mamaL, id);
                    SelectDino(papaML, id);
                    SelectDino(mamaML, id);
                    SelectDino(imprintL, id);
                    SelectDino(imprinterL, id);
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
                AddToGrid(grid, genL, rowIndex, 9);
                AddToGrid(grid, papaL, rowIndex, 10);
                AddToGrid(grid, mamaL, rowIndex, 11);
                AddToGrid(grid, papaML, rowIndex, 12);
                AddToGrid(grid, mamaML, rowIndex, 13);
                AddToGrid(grid, imprintL, rowIndex, 14);
                AddToGrid(grid, imprinterL, rowIndex, 15);



                rowIndex++;
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

            Vars.headerColor = Vars.breedColor;
            Vars.DefaultColor = Vars.breedColor;

            // Add header row
            AddToGrid(grid, new Label { Text = "ID", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor }, 0, 0);
            AddToGrid(grid, new Label { Text = "Tag", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor }, 0, 1);
            AddToGrid(grid, new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor }, 0, 2);
            AddToGrid(grid, new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor }, 0, 3);
            AddToGrid(grid, new Label { Text = "", FontAttributes = FontAttributes.Bold, TextColor = Vars.headerColor }, 0, 4);

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
                    Vars.DefaultColor = Vars.femaleColor;
                }
                else
                {
                    Vars.DefaultColor = Vars.maleColor;
                }

                var cellColor0 = Vars.DefaultColor;
                var cellColor1 = Vars.DefaultColor;
                var cellColor2 = Vars.DefaultColor;
                var cellColor3 = Vars.DefaultColor;
                var cellColor4 = Vars.DefaultColor;

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

            return grid;
        }

        public void RefreshContent(bool stat)
        {
            if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    MyDelayedOperationAsync(stat);
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

        public async Task MyDelayedOperationAsync(bool showStats)
        {
            await Task.Delay(10); // Wait for 2 seconds

            RouteContent(showStats);
        }

        public void RouteContent(bool showStats)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();    // start timer here

            // get the selected species
            string dinoTag = DataManager.TagForClass(Vars.setRoute);
            Vars.selectedClass = dinoTag;

            // update data with new status
            //DataManager.GetDinoData(DataManager.selectedClass);

            // Load necessary data based on toggles
            if (!string.IsNullOrEmpty(Vars.selectedClass) && Vars.setRoute != "Looking for dinos" && Vars.setRoute != "ASA" && Vars.setRoute != "Archive")
            {
                if (showStats)
                {
                    DataManager.GetOneDinoData(Vars.selectedID);

                    if (Vars.ToggleExcluded != 3)
                    {
                        DataManager.SetMaxStats();
                    }
                }
                else
                {
                    // sort data based on column clicked
                    DataManager.GetDinoData(Vars.selectedClass, Vars.sortM, Vars.sortF);


                    if (Vars.ToggleExcluded != 3)
                    {
                        DataManager.SetMaxStats();
                    }

                    if (!Vars.CurrentStats && Vars.ToggleExcluded != 2 && Vars.ToggleExcluded != 3)
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


            FileManager.Log("Displaying -> " + Vars.setRoute);
            this.Content = null;
            if (Vars.setRoute == "Looking for dinos")
            {
                if (!showStats) { this.Title = "No dinos around here!"; }
                UpdateStartContentPage("Looking for dinos =/");
            }
            else if (Vars.setRoute == "ASA")
            {
                this.Title = "Dino Manager";
                UpdateStartContentPage("Remember to feed your dinos!!!");
            }
            else if (Vars.setRoute == "Archive")
            {
                if (!showStats) { this.Title = Vars.setRoute; }
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
                if (!showStats) { this.Title = Vars.setRoute; }
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
            Vars.RefreshCount++;
            double outAVG = 0;
            if (Vars.RefreshCount < 2) { Vars.RefreshAvg = elapsedMilliseconds; outAVG = Vars.RefreshAvg; }
            else { Vars.RefreshAvg += elapsedMilliseconds; outAVG = Vars.RefreshAvg / Vars.RefreshCount; }
            FileManager.Log("Refreshed GUI - " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
            FileManager.Log("=====================================================================");
        }

        private Grid CreateSidePanel(bool showStats)
        {
            var grid = new Grid
            {
                RowSpacing = 5,
                ColumnSpacing = 0,
                Padding = 5,
                BackgroundColor = Color.FromArgb("#312f38")
            };


            // Define columns
            //grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto}); // 0


            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content




            var bColor0 = Vars.noColor;
            var bColor1 = Vars.okColor;


            if (Vars.CurrentStats)
            {
                bColor1 = Vars.warnColor;
            }

            if (Vars.ToggleExcluded == 0)
            {
                bColor0 = Colors.LightBlue;
            }
            else if (Vars.ToggleExcluded == 1)
            {
                bColor0 = Vars.okColor;
            }
            else if (Vars.ToggleExcluded == 2)
            {
                bColor0 = Vars.warnColor;
            }
            else if (Vars.ToggleExcluded == 3)
            {
                bColor0 = Vars.dangerColor;
            }

            string btn0Text = "Toggle"; string btn1Text = "Breeding";
            if (Vars.ToggleExcluded == 0) { btn0Text = "All"; }
            else if (Vars.ToggleExcluded == 1) { btn0Text = "Included"; }
            else if (Vars.ToggleExcluded == 2) { btn0Text = "Excluded"; }
            else if (Vars.ToggleExcluded == 3) { btn0Text = "Archived"; }

            if (Vars.CurrentStats) { btn1Text = "Current"; }

            var topButton0 = new Button { Text = btn0Text, BackgroundColor = bColor0 };
            var topButton1 = new Button { Text = btn1Text, BackgroundColor = bColor1 };


            string status = DataManager.GetStatus(Vars.selectedID);


            string btn2Text = "Exclude"; var bColor2 = Vars.warnColor;
            string btn3Text = "Archive"; var bColor3 = Vars.dangerColor;


            if (status == "Exclude") { btn2Text = "Include"; bColor2 = Vars.okColor; }
            if (status == "Archived") { btn3Text = "Restore"; bColor3 = Vars.okColor; }



            if (Vars.setRoute != "Archive")
            {
                AddToGrid(grid, topButton0, 0, 0);
                AddToGrid(grid, topButton1, 1, 0);

                topButton0.Clicked += OnButton0Clicked;
                topButton1.Clicked += OnButton1Clicked;

                if (showStats) // add theese only if we have a dino selected
                {
                    var topButton2 = new Button { Text = btn2Text, BackgroundColor = bColor2 };
                    topButton2.Clicked += OnButton2Clicked;
                    AddToGrid(grid, topButton2, 2, 0);


                    var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
                    topButton3.Clicked += OnButton3Clicked;
                    AddToGrid(grid, topButton3, 6, 0);
                }
            }
            else // show extra buttons in archive
            {
                if (showStats) // add theese only if we have a dino selected
                {
                    var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
                    topButton3.Clicked += OnButton3Clicked;
                    AddToGrid(grid, topButton3, 0, 0);


                    var topButton4 = new Button { Text = "Purge", BackgroundColor = Vars.dangerColor };
                    topButton4.Clicked += OnButton4Clicked;
                    AddToGrid(grid, topButton4, 5, 0);
                }

                var topButton5 = new Button { Text = "Purge All", BackgroundColor = Vars.dangerColor };
                topButton5.Clicked += OnButton5Clicked;
                AddToGrid(grid, topButton5, 6, 0);
            }

            return grid;
        }

        private Grid CreateMainPanel(bool showStats)
        {
            var grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 5,
                Padding = 0, 
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0


            // dynamically adjust the bottom bar height
            int rowCount = DataManager.BottomTable.Rows.Count;
            int rowHeight = 21;
            int barH = (rowCount * rowHeight) + rowHeight + 12;
            if (rowCount > 5) { barH = 127; }

            if (((Vars.ToggleExcluded == 3 || Vars.ToggleExcluded == 2) && !showStats) || DataManager.BottomTable.Rows.Count < 1) { barH = 0; }


            // Define row definitions
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content
            grid.RowDefinitions.Add(new RowDefinition { Height = barH }); // Scrollable content


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

            AddToGrid(grid, scrollView, 0, 1);

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

            AddToGrid(grid, bottomPanel, 1, 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////



            return grid;
        }

        private Grid CreateArchivePanel(bool showStats)
        {
            var grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 5,
                Padding = 0,
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0


            // dynamically adjust the bottom bar height
            int rowCount = DataManager.BottomTable.Rows.Count;
            int rowHeight = 20;
            int barH = (rowCount * rowHeight) + rowHeight + 11;
            if (rowCount > 5) { barH = 127; }

            if (Vars.ToggleExcluded == 3 && !showStats) { barH = 0; }
            if (Vars.ToggleExcluded == 2 && !showStats) { barH = 0; }


            // Define row definitions
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


            var bColor1 = Colors.LightBlue;
            var bColor2 = Colors.LightBlue;



            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scrollable content
            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3

            };

            // Add male and female tables
            scrollContent.Children.Add(CreateArchiveGrid(DataManager.ArchiveTable, "Archive", showStats));

            // Wrap the scrollable content in a ScrollView and add it to the second row
            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(grid, scrollView, 0, 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////



            return grid;
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

            var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center ,VerticalOptions = LayoutOptions.Start ,FontAttributes = FontAttributes.Bold, TextColor = Vars.okColor, FontSize = 22 };


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
            // ==============================================================    Create Archive Layout   =====================================================

            // Create the main layout
            var mainLayout = new Grid();


            // create main layout with 2 columns

            // Define row definitions
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // 0


            mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 }); // 0
            mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Add side panel to left column
            AddToGrid(mainLayout, CreateSidePanel(showStats), 0, 0);

            // Add main panel to right column
            AddToGrid(mainLayout, CreateArchivePanel(showStats), 0, 1);

            // attach unselect event after all content has been created
            UnSelectDino(mainLayout);

            this.Content = mainLayout;
        }

        public void UpdateMainContentPage(bool showStats)
        {
            // ==============================================================    Create Dino Layout   =====================================================

            // Create the main layout
            var mainLayout = new Grid();

           
            // create main layout with 2 columns

            // Define row definitions
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // 0


            mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 }); // 0
            mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            // Add side panel to left column
            AddToGrid(mainLayout, CreateSidePanel(showStats), 0, 0);

            // Add main panel to right column
            AddToGrid(mainLayout, CreateMainPanel(showStats), 0, 1);

            // attach unselect event after all content has been created
            UnSelectDino(mainLayout); 

            this.Content = mainLayout;
        }

        void SortColumn(Label label,string sex)
        {
            // Create a TapGestureRecognizer
            var tapGesture1 = new TapGestureRecognizer();
            tapGesture1.Tapped += (s, e) =>
            {
                // Handle the click event and pass additional data
                string column = label.Text;

                var splitM = Vars.sortM.Split(new[] { @" " }, StringSplitOptions.RemoveEmptyEntries);
                var splitF = Vars.sortF.Split(new[] { @" " }, StringSplitOptions.RemoveEmptyEntries);

                string outM = "";
                string outF = "";

                if (splitM.Length > 0)
                {
                    outM = splitM[0];
                }
                if (splitF.Length > 0)
                {
                    outF = splitF[0];
                }

                if (sex == "M")
                {
                    // are we clicking the same column then toggle sorting
                    if (outM == column) 
                    {
                        if (Vars.sortM.Contains("ASC"))
                        {
                            Vars.sortM = column + " DESC";
                        }
                        else if (Vars.sortM.Contains("DESC"))
                        {
                            Vars.sortM = "";
                        }
                    }
                    else
                    {
                        Vars.sortM = column + " ASC";
                    }
                }
                else if (sex == "F")
                {
                    // are we clicking the same column then toggle sorting
                    if (outF == column)
                    {
                        if (Vars.sortF.Contains("ASC")) // then switch to descending
                        {
                            Vars.sortF = column + " DESC";
                        }
                        else if (Vars.sortF.Contains("DESC")) // finally turn it off
                        {
                            Vars.sortF = "";
                        }
                    }
                    else // first sort ascending
                    {
                        Vars.sortF = column + " ASC";
                    }
                }

                FileManager.Log($"Sorted: {Vars.sortM} : {Vars.sortF}");

                RefreshContent(false);
            };

            // Attach the TapGestureRecognizer to the label
            label.GestureRecognizers.Add(tapGesture1);
        }

    }
}
