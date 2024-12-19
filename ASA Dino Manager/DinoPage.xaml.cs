using System.Data;

namespace ASA_Dino_Manager;

public partial class DinoPage : ContentPage
{
    ////////////////////    View Toggles    ////////////////////
    public static int ToggleExcluded = 0;
    public static bool CurrentStats = false;

    ////////////////////    Selecting       ////////////////////
    private string selectedID = "";
    private bool isSelected = false;

    ////////////////////    Table Sorting   ////////////////////
    private string sortM = "";
    private string sortF = "";

    public DinoPage()
	{
        InitializeComponent();

        FileManager.Log($"Loaded: {Shared.setPage}", 0);

        // set page title
        this.Title = $"{Shared.setPage.Replace("_"," ")}";
        CreateContent();
    }

    public void CreateContent()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (!string.IsNullOrEmpty(Shared.selectedClass))
                {
                    if (isSelected)
                    {
                        DataManager.GetOneDinoData(selectedID);

                        if (ToggleExcluded != 3)
                        {
                            DataManager.SetMaxStats();
                        }
                    }
                    else
                    {
                        // sort data based on column clicked
                        DataManager.GetDinoData(Shared.selectedClass, sortM, sortF);


                        if (ToggleExcluded != 3)
                        {
                            DataManager.SetMaxStats();
                        }

                        if (!CurrentStats && ToggleExcluded != 2 && ToggleExcluded != 3)
                        {
                            DataManager.SetBinaryStats();
                            DataManager.GetBestPartner();
                        }
                    }
                }
                // Retrieve female data
                string[] females = DataManager.GetDistinctFilteredColumnData("Class", Shared.selectedClass, "Sex", "Female", "ID");
                // Retrieve male data
                string[] males = DataManager.GetDistinctFilteredColumnData("Class", Shared.selectedClass, "Sex", "Male", "ID");
                int totalC = females.Length + males.Length;

                FileManager.Log("Updating GUI -> " + Shared.setPage, 0);

                if (!isSelected) { this.Title = $"{Shared.setPage.Replace("_", " ")}"; }
                if (totalC == 0)
                {
                    DefaultView("No dinos in here :(");
                }
                else
                {
                    DinoView();
                   
                }
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

    private void DefaultView(string labelText)
    {
        var mainLayout = new Grid();

        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


        var scrollContent = new StackLayout
        {
            Spacing = 20,
            Padding = 3
        };

        var image1 = new Image { Source = "dino.png", HeightRequest = 155, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start };
        var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Shared.okColor, FontSize = 22 };


        AddToGrid(mainLayout, image1, 0, 0);
        AddToGrid(mainLayout, label1, 1, 0);


        // Wrap the scrollable content in a ScrollView and add it to the second row
        var scrollView = new ScrollView { Content = scrollContent };

        AddToGrid(mainLayout, scrollView, 0, 0);

        // only attach the tapgesture if we have something selected
        // for now its the only way to force refresh a page
        // so we attach it to everything so we can click
        UnSelectDino(mainLayout);

        this.Content = null;
        this.Content = mainLayout;
    }

    public void DinoView()
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
        AddToGrid(mainLayout, CreateSidePanel(), 0, 0);

        // Add main panel to right column
        AddToGrid(mainLayout, CreateMainPanel(), 0, 1);


        // only attach the tapgesture if we have something selected
        // for now its the only way to force refresh a page
        // so we attach it to everything so we can click
        UnSelectDino(mainLayout);


        this.Content = mainLayout;
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

    private Grid CreateSidePanel()
    {
        var grid = new Grid
        {
            RowSpacing = 5,
            ColumnSpacing = 0,
            Padding = 5,
            BackgroundColor = Color.FromArgb("#312f38")
        };

        // Define columns
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content


        var bColor0 = Shared.noColor;
        var bColor1 = Shared.okColor;


        if (CurrentStats)
        {
            bColor1 = Shared.warnColor;
        }

        if (ToggleExcluded == 0)
        {
            bColor0 = Colors.LightBlue;
        }
        else if (ToggleExcluded == 1)
        {
            bColor0 = Shared.okColor;
        }
        else if (ToggleExcluded == 2)
        {
            bColor0 = Shared.warnColor;
        }
        else if (ToggleExcluded == 3)
        {
            bColor0 = Shared.dangerColor;
        }

        string btn0Text = "Toggle"; string btn1Text = "Breeding";
        if (ToggleExcluded == 0) { btn0Text = "All"; }
        else if (ToggleExcluded == 1) { btn0Text = "Included"; }
        else if (ToggleExcluded == 2) { btn0Text = "Excluded"; }
        else if (ToggleExcluded == 3) { btn0Text = "Archived"; }

        if (CurrentStats) { btn1Text = "Current"; }

        var topButton0 = new Button { Text = btn0Text, BackgroundColor = bColor0 };
        var topButton1 = new Button { Text = btn1Text, BackgroundColor = bColor1 };


        string status = DataManager.GetStatus(selectedID);


        string btn2Text = "Exclude"; var bColor2 = Shared.warnColor;
        string btn3Text = "Archive"; var bColor3 = Shared.dangerColor;


        if (status == "Exclude") { btn2Text = "Include"; bColor2 = Shared.okColor; }
        if (status == "Archived") { btn3Text = "Restore"; bColor3 = Shared.okColor; }



        if (Shared.setRoute != "Archive")
        {
            AddToGrid(grid, topButton0, 0, 0);
            AddToGrid(grid, topButton1, 1, 0);

            topButton0.Clicked += OnButton0Clicked;
            topButton1.Clicked += OnButton1Clicked;

            if (isSelected) // add theese only if we have a dino selected
            {
                var topButton2 = new Button { Text = btn2Text, BackgroundColor = bColor2 };
                topButton2.Clicked += OnButton2Clicked;
                AddToGrid(grid, topButton2, 2, 0);


                var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
                topButton3.Clicked += OnButton3Clicked;
                AddToGrid(grid, topButton3, 6, 0);
            }
        }

        return grid;
    }

    private Grid CreateMainPanel()
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
        int barH = 0;

        Shared.rowHeight = 24;
        int ofs = 0;
        if (rowCount > 0) { ofs = 13; barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + ofs; } // 1 row
        if (rowCount > 1) { ofs = 9; barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + ofs; } // 2 rows
        if (rowCount > 2) { ofs = 4; barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + ofs; } // 3 rows
        if (rowCount > 3) { ofs = -1; barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + ofs; } // 4 rows 
        if (rowCount > 4) { ofs = -5; barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + ofs; } // 5 rows needs to be bigger to not activate scrolling
        if (rowCount > 5) { ofs = -7; barH = ((5 * Shared.rowHeight) + Shared.headerSize) + (ofs); }        // 6+ rows needs to be smaller not to show the 6th row

        // FileManager.Log($"barH: {barH} = {rowCount} * {Shared.rowHeight} + {Shared.headerSize} + {ofs}", 0);

        if (((ToggleExcluded == 3 || ToggleExcluded == 2 || CurrentStats) && !isSelected) || DataManager.BottomTable.Rows.Count < 1) { barH = 0; }

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
        scrollContent.Children.Add(CreateDinoGrid(DataManager.MaleTable, "Male"));
        scrollContent.Children.Add(CreateDinoGrid(DataManager.FemaleTable, "Female"));

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

        bottomContent.Children.Add(CreateDinoGrid(DataManager.BottomTable, "Bottom"));

        // Wrap the scrollable content in a ScrollView and add it to the third row
        var bottomPanel = new ScrollView { Content = bottomContent };

        AddToGrid(grid, bottomPanel, 1, 1);

        ////////////////////////////////////////////////////////////////////////////////////////////////////



        return grid;
    }

    private Grid CreateDinoGrid(DataTable table, string title)
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


        Shared.DefaultColor = Shared.maleColor;

        if (title == "Male") { Shared.DefaultColor = Shared.maleColor; }
        else if (title == "Female") { Shared.DefaultColor = Shared.femaleColor; }
        else { Shared.DefaultColor = Shared.breedColor; }


        Shared.headerColor = Shared.DefaultColor;

        int fSize = Shared.headerSize;  // header fontsize

        var header0 = new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header1 = new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header2 = new Label { Text = "Hp", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header3 = new Label { Text = "Stamina", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header4 = new Label { Text = "Oxygen", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header5 = new Label { Text = "Food", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header6 = new Label { Text = "Weight", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header7 = new Label { Text = "Damage", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header8 = new Label { Text = "Status", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header9 = new Label { Text = "Gen", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header10 = new Label { Text = "Papa", FontAttributes = FontAttributes.Bold, TextColor = Shared.maleColor, FontSize = fSize };
        var header11 = new Label { Text = "Mama", FontAttributes = FontAttributes.Bold, TextColor = Shared.femaleColor, FontSize = fSize };

        var header12 = new Label { Text = "PapaMute", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header13 = new Label { Text = "MamaMute", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header14 = new Label { Text = "Imprint", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };
        var header15 = new Label { Text = "Imprinter", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor, FontSize = fSize };



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

        // only show last column headers if we select a dino
        if (title != "Bottom" || isSelected)
        {
            AddToGrid(grid, header12, 0, 12);
            AddToGrid(grid, header13, 0, 13);
            AddToGrid(grid, header14, 0, 14);
            AddToGrid(grid, header15, 0, 15);

        }

        int rowIndex = 1; // Start adding rows below the header

        foreach (DataRow row in table.Rows)
        {
            var cellColor0 = Shared.DefaultColor;
            var cellColor1 = Shared.DefaultColor;
            var cellColor2 = Shared.DefaultColor;
            var cellColor3 = Shared.DefaultColor;
            var cellColor4 = Shared.DefaultColor;
            var cellColor5 = Shared.DefaultColor;
            var cellColor6 = Shared.DefaultColor;
            var cellColor7 = Shared.DefaultColor;

            var cellColor8 = Shared.DefaultColor;

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


            if (ToggleExcluded == 2)
            {
                if (status == "Exclude") { status = ""; }
            }


            //recolor breeding stats
            if (DataManager.ToDouble(level) >= DataManager.LevelMax) { cellColor1 = Shared.goodColor; }
            if (DataManager.ToDouble(hp) >= DataManager.HpMax) { cellColor2 = Shared.goodColor; }
            if (DataManager.ToDouble(stamina) >= DataManager.StaminaMax) { cellColor3 = Shared.goodColor; }
            if (DataManager.ToDouble(oxygen) >= DataManager.OxygenMax) { cellColor4 = Shared.goodColor; }
            if (DataManager.ToDouble(food) >= DataManager.FoodMax) { cellColor5 = Shared.goodColor; }
            if (DataManager.ToDouble(weight) >= DataManager.WeightMax) { cellColor6 = Shared.goodColor; }
            if (DataManager.ToDouble(damage) >= DataManager.DamageMax) { cellColor7 = Shared.goodColor; }


            // mutation detection overrides normal coloring -> mutaColor
            string mutes = DataManager.GetMutes(id);
            if (mutes.Length == 6 && !CurrentStats) // dont show mutations on current statview
            {
                string aC = mutes.Substring(0, 1); string bC = mutes.Substring(1, 1); string cC = mutes.Substring(2, 1);
                string dC = mutes.Substring(3, 1); string eC = mutes.Substring(4, 1); string fC = mutes.Substring(5, 1);

                if (aC == "1") { cellColor2 = Shared.mutaColor; }
                if (bC == "1") { cellColor3 = Shared.mutaColor; }
                if (cC == "1") { cellColor4 = Shared.mutaColor; }
                if (dC == "1") { cellColor5 = Shared.mutaColor; }
                if (eC == "1") { cellColor6 = Shared.mutaColor; }
                if (fC == "1") { cellColor7 = Shared.mutaColor; }
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
            var papaL = new Label { Text = papa, TextColor = Shared.maleColor };
            var mamaL = new Label { Text = mama, TextColor = Shared.femaleColor };
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

    private void ClearSelection()
    {
        if (selectedID != "")
        {
            FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = $"{Shared.setPage.Replace("_", " ")}";
        }
    }


    // Button event handlers
    void SortColumn(Label label, string sex)
    {
        // Create a TapGestureRecognizer
        var tapGesture1 = new TapGestureRecognizer();
        tapGesture1.Tapped += (s, e) =>
        {
            // Handle the click event and pass additional data
            string column = label.Text;

            var splitM = sortM.Split(new[] { @" " }, StringSplitOptions.RemoveEmptyEntries);
            var splitF = sortF.Split(new[] { @" " }, StringSplitOptions.RemoveEmptyEntries);

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
                    if (sortM.Contains("ASC"))
                    {
                        sortM = column + " DESC";
                    }
                    else if (sortM.Contains("DESC"))
                    {
                        sortM = "";
                    }
                }
                else
                {
                    sortM = column + " ASC";
                }
            }
            else if (sex == "F")
            {
                // are we clicking the same column then toggle sorting
                if (outF == column)
                {
                    if (sortF.Contains("ASC")) // then switch to descending
                    {
                        sortF = column + " DESC";
                    }
                    else if (sortF.Contains("DESC")) // finally turn it off
                    {
                        sortF = "";
                    }
                }
                else // first sort ascending
                {
                    sortF = column + " ASC";
                }
            }

            FileManager.Log($"Sorted: {sortM} : {sortF}", 0);


            ClearSelection();
            CreateContent();
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture1);
    }

    void SelectDino(Label label, string id)
    {
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (selectedID != id) // dont select the same dino twice
            {
                selectedID = id; isSelected = true;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Selected {name} ID: {id}", 0); 

                CreateContent();
            }
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
            ClearSelection();
            CreateContent();
        };

        // Attach the TapGestureRecognizer to the label
        grid.GestureRecognizers.Add(tapGesture);
    }

    private void OnButton0Clicked(object? sender, EventArgs e)
    {
        ToggleExcluded++;
        if (ToggleExcluded == 4)
        {
            ToggleExcluded = 0;
        }
        FileManager.Log($"Toggle Exclude {ToggleExcluded}", 0);

        ClearSelection();
        CreateContent();
    }

    private void OnButton1Clicked(object? sender, EventArgs e)
    {
        if (CurrentStats)
        {
            CurrentStats = false;
        }
        else
        {
            CurrentStats = true;
        }
        FileManager.Log($"Toggle Stats {CurrentStats}", 0);

        ClearSelection();
        CreateContent();
    }

    private void OnButton2Clicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            string status = DataManager.GetStatus(selectedID);
            if (status == "Exclude") { status = ""; }
            else if (status == "") { status = "Exclude"; FileManager.Log($"Excluded ID: {selectedID}", 0); }
            DataManager.SetStatus(selectedID, status);

            ClearSelection();
            CreateContent();
        }
    }

    private void OnButton3Clicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            // Handle the click event
            string status = DataManager.GetStatus(selectedID);
            if (status == "Archived") { status = ""; FileManager.Log($"Restored ID: {selectedID}", 0); }
            else if (status == "") { status = "Archived"; FileManager.Log($"Archived ID: {selectedID}", 0); }
            else if (status == "Exclude") { status = "Archived"; FileManager.Log($"Archived ID: {selectedID}", 0); }
            DataManager.SetStatus(selectedID, status);

            // recompile the archive after archiving or unarchiving
            // maybe redundant
            //DataManager.CompileDinoArchive();

            ClearSelection();
            CreateContent();
        }

    }


}