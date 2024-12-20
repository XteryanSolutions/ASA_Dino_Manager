using System.Data;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ASA_Dino_Manager;

public partial class DinoPage : ContentPage
{
    ////////////////////    View Toggles    ////////////////////
    public static int ToggleExcluded = 0;
    public static bool CurrentStats = false;

    ////////////////////    Selecting       ////////////////////
    private string selectedID = "";
    private bool isSelected = false;
    private bool canDouble = false;
    private bool isDouble = false;


    ////////////////////    Table Sorting   ////////////////////
    private string sortM = "";
    private string sortF = "";

    public DinoPage()
    {
        InitializeComponent();

        FileManager.Log($"Loaded: {Shared.setPage}", 0);

        // set page title
        this.Title = $"{Shared.setPage.Replace("_", " ")}";
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

                        DataManager.SetMaxStats(ToggleExcluded);
                        DataManager.SetBinaryStats(ToggleExcluded);
                    }
                    else
                    {
                        // sort data based on column clicked
                        DataManager.GetDinoData(Shared.selectedClass, sortM, sortF);


                        DataManager.SetMaxStats(ToggleExcluded);
                        DataManager.SetBinaryStats(ToggleExcluded);

                        if (!CurrentStats)
                        {
                            DataManager.GetBestPartner();
                        }
                    }
                }
                FileManager.Log("Updating GUI -> " + Shared.setPage, 0);

                if (!isSelected) { this.Title = $"{Shared.setPage.Replace("_", " ")}"; }

                DinoView();
            }
            finally
            {
                Monitor.Exit(Shared._dbLock);
            }
        }
        else
        {
            FileManager.Log("Failed to acquire database lock within timeout.", 1);
            DefaultView("Dinos ran away :(");
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
        var mainLayout = new Grid
        {
            BackgroundColor = Shared.MainPanelColor
        };


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

    private void AddToGrid(Grid grid, View view, int row, int column, string title = "", bool selected = false)
    {
        // Ensure rows exist up to the specified index
        while (grid.RowDefinitions.Count <= row)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            // Determine the even row color based on the title
            Color evenRowColor = title switch
            {
                "Male" => Shared.MainPanelColor,
                "Bottom" => Shared.BottomPanelColor,
                _ => Shared.MainPanelColor // Default color if title doesn't match
            };

            // Determine the even row color based on the title
            Color oddRowColor = title switch
            {
                "Male" => Shared.OddMPanelColor,
                "Bottom" => Shared.OddBPanelColor,
                _ => Shared.OddMPanelColor // Default color if title doesn't match
            };

            // Choose the color based on the row index
            var rowColor = grid.RowDefinitions.Count % 2 == 0
                ? evenRowColor // Even rows
                : oddRowColor; // Odd rows

            // Override if row is selected
            if (selected) { rowColor = Shared.SelectedColor; }

            // Add a background color to the row
            var rowBackground = new BoxView { Color = rowColor };
            Grid.SetRow(rowBackground, grid.RowDefinitions.Count - 1);
            Grid.SetColumnSpan(rowBackground, grid.ColumnDefinitions.Count > 0
                ? grid.ColumnDefinitions.Count
                : 1); // Cover all columns
            grid.Children.Add(rowBackground);
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
            BackgroundColor = Shared.SidePanelColor
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
            if (isDouble)
            {
                var topButton4 = new Button { Text = "Back", BackgroundColor = Shared.okColor };
                topButton4.Clicked += OnButton4Clicked;
                AddToGrid(grid, topButton4, 0, 0);
            }
            else
            {
                AddToGrid(grid, topButton0, 0, 0);
                AddToGrid(grid, topButton1, 1, 0);

                topButton0.Clicked += OnButton0Clicked;
                topButton1.Clicked += OnButton1Clicked;
            }


            if (isSelected) // add theese only if we have a dino selected
            {
                // do not show exclude button while in archive view
                if (ToggleExcluded != 3)
                {
                    var topButton2 = new Button { Text = btn2Text, BackgroundColor = bColor2 };
                    topButton2.Clicked += OnButton2Clicked;
                    AddToGrid(grid, topButton2, 5, 0);
                }


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

        if (rowCount > 0) { barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + 13; } // 1 row
        if (rowCount > 1) { barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + 9; } // 2 rows
        if (rowCount > 2) { barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + 4; } // 3 rows
        if (rowCount > 3) { barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + (-1); } // 4 rows 
        if (rowCount > 4) { barH = ((rowCount * Shared.rowHeight) + Shared.headerSize) + (-5); } // 5 rows needs to be bigger to not activate scrolling
        if (rowCount > 5) { barH = ((5 * Shared.rowHeight) + Shared.headerSize) + (-7); }        // 6+ rows needs to be smaller not to show the 6th row

        // FileManager.Log($"barH: {barH} = {rowCount} * {Shared.rowHeight} + {Shared.headerSize} + {ofs}", 0);

        if (((ToggleExcluded == 3 || ToggleExcluded == 2 || CurrentStats) && !isSelected) || DataManager.BottomTable.Rows.Count < 1) { barH = 0; }

        // Define row definitions
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = barH }); // Scrollable content


        ////////////////////////////////////////////////////////////////////////////////////////////////////

        int count = DataManager.DinoCount(Shared.selectedClass, ToggleExcluded);


        if (count > 0 && !isDouble)
        {
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
                BackgroundColor = Shared.BottomPanelColor
            };


            bottomContent.Children.Add(CreateDinoGrid(DataManager.BottomTable, "Bottom"));

            // Wrap the scrollable content in a ScrollView and add it to the third row
            var bottomPanel = new ScrollView { Content = bottomContent };

            AddToGrid(grid, bottomPanel, 1, 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        else if (count > 0 && isDouble)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // make dino info box

            // Create scrollable content
            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            var grid1 = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };
            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0


            var imageContainer = new Grid
            {
                BackgroundColor = Shared.MainPanelColor, // Set the background color here
                Padding = 0
            };

            var image = new Image
            {
                Source = "dino.png",
                HeightRequest = 155,
                Aspect = Aspect.AspectFit
            };

            // Add the image to the container
            imageContainer.Children.Add(image);

            var label1 = new Label
            {
                Text = "Extended dino info (WIP)",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Style = (Style)Application.Current.Resources["Headline"],
                TextColor = Shared.okColor,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };


            AddToGrid(grid1, imageContainer, 0, 0);
            AddToGrid(grid1, label1, 1, 0);



            scrollContent.Children.Add(grid1);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(grid, scrollView, 0, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

        }
        else
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // create empty table content

            // Create scrollable content
            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            var grid1 = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };
            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0


            var imageContainer = new Grid
            {
                BackgroundColor = Shared.MainPanelColor, // Set the background color here
                Padding = 0
            };

            var image = new Image
            {
                Source = "dino.png",
                HeightRequest = 155,
                Aspect = Aspect.AspectFit
            };

            // Add the image to the container
            imageContainer.Children.Add(image);

            var label1 = new Label
            {
                Text = "No dinos in here :/",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Style = (Style)Application.Current.Resources["Headline"],
                TextColor = Shared.okColor,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };


            AddToGrid(grid1, imageContainer, 0, 0);
            AddToGrid(grid1, label1, 1, 0);



            scrollContent.Children.Add(grid1);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(grid, scrollView, 0, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

        }

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
        AddToGrid(grid, header0, 0, 0, title);
        AddToGrid(grid, header1, 0, 1, title);
        AddToGrid(grid, header2, 0, 2, title);
        AddToGrid(grid, header3, 0, 3, title);
        AddToGrid(grid, header4, 0, 4, title);
        AddToGrid(grid, header5, 0, 5, title);
        AddToGrid(grid, header6, 0, 6, title);
        AddToGrid(grid, header7, 0, 7, title);
        AddToGrid(grid, header8, 0, 8, title);
        AddToGrid(grid, header9, 0, 9, title);
        AddToGrid(grid, header10, 0, 10, title);
        AddToGrid(grid, header11, 0, 11, title);

        // only show last column headers if we select a dino
        if (title != "Bottom" || isSelected)
        {
            AddToGrid(grid, header12, 0, 12, title);
            AddToGrid(grid, header13, 0, 13, title);
            AddToGrid(grid, header14, 0, 14, title);
            AddToGrid(grid, header15, 0, 15, title);
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
            if (ToggleExcluded == 3)
            {
                if (status == "Archived") { status = ""; }
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


            // override offspring colors based on breed points
            if (title == "Bottom")
            {
                if (name.Contains("Breed #"))
                {
                    var nameSplit = name.Split(new[] { @"#" }, StringSplitOptions.RemoveEmptyEntries);
                    int maxRows = DataManager.ComboTable.Rows.Count;
                    string nr = nameSplit[1].Trim();
                    if (maxRows > 0)
                    {
                        int rowID = 1;
                        foreach (DataRow rowC in DataManager.ComboTable.Rows)
                        {
                            // locate the right row
                            if (rowID.ToString() == nr)
                            {
                                string IDC = rowC["res"].ToString(); // get the combined stats
                                string aC = IDC.Substring(0, 1); string bC = IDC.Substring(1, 1); string cC = IDC.Substring(2, 1);
                                string dC = IDC.Substring(3, 1); string eC = IDC.Substring(4, 1); string fC = IDC.Substring(5, 1);

                                if (aC == "2") { cellColor2 = Shared.greatColor; }
                                if (bC == "2") { cellColor3 = Shared.greatColor; }
                                if (cC == "2") { cellColor4 = Shared.greatColor; }
                                if (dC == "2") { cellColor5 = Shared.greatColor; }
                                if (eC == "2") { cellColor6 = Shared.greatColor; }
                                if (fC == "2") { cellColor7 = Shared.greatColor; }

                                if ((aC + bC + cC + dC + eC + fC) == "222222")
                                {
                                    // here is a golden offspring with all the best stats
                                    cellColor2 = Shared.superColor;
                                    cellColor3 = Shared.superColor;
                                    cellColor4 = Shared.superColor;
                                    cellColor5 = Shared.superColor;
                                    cellColor6 = Shared.superColor;
                                    cellColor7 = Shared.superColor;
                                }
                                break;
                            }
                            rowID++;
                        }
                    }
                }
            }

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



            bool selected = false;
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

                // figure out if we have this dino selected
                if (id == selectedID) { selected = true; }
            }

            // add items to grid
            AddToGrid(grid, nameL, rowIndex, 0, title, selected);
            AddToGrid(grid, levelL, rowIndex, 1, title, selected);
            AddToGrid(grid, hpL, rowIndex, 2, title, selected);
            AddToGrid(grid, staminaL, rowIndex, 3, title, selected);
            AddToGrid(grid, oxygenL, rowIndex, 4, title, selected);
            AddToGrid(grid, foodL, rowIndex, 5, title, selected);
            AddToGrid(grid, weightL, rowIndex, 6, title, selected);
            AddToGrid(grid, damageL, rowIndex, 7, title, selected);
            AddToGrid(grid, statusL, rowIndex, 8, title, selected);
            AddToGrid(grid, genL, rowIndex, 9, title, selected);
            AddToGrid(grid, papaL, rowIndex, 10, title, selected);
            AddToGrid(grid, mamaL, rowIndex, 11, title, selected);
            AddToGrid(grid, papaML, rowIndex, 12, title, selected);
            AddToGrid(grid, mamaML, rowIndex, 13, title, selected);
            AddToGrid(grid, imprintL, rowIndex, 14, title, selected);
            AddToGrid(grid, imprinterL, rowIndex, 15, title, selected);


            rowIndex++;
        }

        return grid;
    }

    private void ClearSelection()
    {
        if (selectedID != "" && !isDouble)
        {
            FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = $"{Shared.setPage.Replace("_", " ")}";
            canDouble = false;
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

                // activate double clicking
                canDouble = true;
                DisableDoubleClick(500);
            }
            else if (selectedID == id && canDouble)
            {
                // double click
                // open the dino extended info window
                isDouble = true;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Double click {name} ID: {id}", 0);

                CreateContent();
            }
            else
            {
                canDouble = false;
            }
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture);
    }

    private async Task DisableDoubleClick(int delayMilliseconds)
    {
        await Task.Delay(delayMilliseconds);
        canDouble = false;
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

    private void OnButton4Clicked(object? sender, EventArgs e)
    {
        isDouble = false;
        ClearSelection();
        CreateContent();
    }

}