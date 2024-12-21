using System.Data;
using System.Data.Common;
using System.Xml.Linq;


namespace ASA_Dino_Manager;

public partial class DinoPage : ContentPage
{
    ////////////////////    View Toggles    ////////////////////
    public static int ToggleExcluded = Shared.DefaultToggle;
    public static bool CurrentStats = Shared.DefaultStat;

    ////////////////////    Selecting       ////////////////////
    public static string selectedID = "";
    public static bool isSelected = false;
    public static bool canDouble = false;
    public static bool isDouble = false;


    ////////////////////    Table Sorting   ////////////////////
    public static string sortM = Shared.DefaultSortM;
    public static string sortF = Shared.DefaultSortF;

    private string upChar = "▲";
    private string downChar = "▼";

    private string levelText = "";
    private string hpText = "";
    private string staminaText = "";
    private string oxygenText = "";
    private string foodText = "";
    private string weightText = "";
    private string damageText = "";
    private string notesText = "";

    private bool editStats = false;

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
            catch
            {
                FileManager.Log("Failed updating dinos", 2);
                DefaultView("Dinos exploded :O");
            }
            finally
            {
                Monitor.Exit(Shared._dbLock);
            }
        }
        else
        {
            FileManager.Log("DinoPage Failed to acquire database lock", 1);
            DefaultView("Dinos walked away :(");
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
        var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Shared.PrimaryColor, FontSize = 22 };


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
        if (!isDouble)
        {
            UnSelectDino(mainLayout);
        }



        this.Content = mainLayout;
    }

    private void AddToGrid(Grid grid, View view, int row, int column, string title = "", bool selected = false, bool isDoubl = false)
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

            // Override if in dino extended view
            if (isDoubl) { rowColor = Shared.MainPanelColor; }

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


        var bColor0 = Shared.DefaultBColor;
        var bColor1 = Shared.PrimaryColor;


        if (CurrentStats)
        {
            bColor1 = Shared.SecondaryColor;
        }

        if (ToggleExcluded == 0)
        {
            bColor0 = Colors.LightBlue;
        }
        else if (ToggleExcluded == 1)
        {
            bColor0 = Shared.PrimaryColor;
        }
        else if (ToggleExcluded == 2)
        {
            bColor0 = Shared.SecondaryColor;
        }
        else if (ToggleExcluded == 3)
        {
            bColor0 = Shared.TrinaryColor;
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


        string btn2Text = "Exclude"; var bColor2 = Shared.SecondaryColor;
        string btn3Text = "Archive"; var bColor3 = Shared.TrinaryColor;


        if (status == "Exclude") { btn2Text = "Include"; bColor2 = Shared.PrimaryColor; }
        if (status == "Archived") { btn3Text = "Restore"; bColor3 = Shared.PrimaryColor; }


        if (isDouble)
        {
            var topButton5 = new Button { Text = "Save", BackgroundColor = Shared.PrimaryColor };
            topButton5.Clicked += OnButton5Clicked;
            AddToGrid(grid, topButton5, 0, 0);


            var topButton4 = new Button { Text = "Back", BackgroundColor = Shared.SecondaryColor };
            topButton4.Clicked += OnButton4Clicked;
            AddToGrid(grid, topButton4, 1, 0);
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


        return grid;
    }

    private Grid CreateMainPanel()
    {
        editStats = false;
        var maingrid = new Grid
        {
            RowSpacing = 0,
            ColumnSpacing = 5,
            Padding = 0,
        };

        // Define columns
        maingrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0

        // Dynamically adjust the bottom bar height
        int rowCount = DataManager.BottomTable.Rows.Count;
        int maxVisibleRows = 5; int barH;
        int buffer = Shared.sizeOffset; // Extra buffer to prevent scrolling

        if (rowCount > 0)
        {
            // Adjust based on row count
            int offset = 13 - Math.Min(rowCount, maxVisibleRows) * 4;
            barH = (Math.Min(rowCount, maxVisibleRows) * Shared.rowHeight) + Shared.headerSize + offset + buffer;
            if (rowCount > 5) { barH = 127; } // prevent showing the top of the 6th row
        }
        else
        {
            barH = 0; // No rows, no bar height
        }

        // FileManager.Log($"barH: {barH} = {rowCount} * {Shared.rowHeight} + {Shared.headerSize} + {ofs}", 0);

        if (((ToggleExcluded == 3 || ToggleExcluded == 2 || CurrentStats) && !isSelected) || DataManager.BottomTable.Rows.Count < 1) { barH = 0; }

        // Define row definitions
        maingrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content




        ////////////////////////////////////////////////////////////////////////////////////////////////////

        int count = DataManager.DinoCount(Shared.selectedClass, ToggleExcluded);

        if (count > 0 && !isDouble) // more than 0 dinos and not double clicked
        {
            // create the row for bottompanel if not in dinoEview
            maingrid.RowDefinitions.Add(new RowDefinition { Height = barH }); // Scrollable content

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
            // changed to include horizontal scrolling
            var scrollView = new ScrollView { Content = scrollContent, Orientation = ScrollOrientation.Horizontal };

            AddToGrid(maingrid, scrollView, 0, 1);

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

            AddToGrid(maingrid, bottomPanel, 1, 1);

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
                Padding = 3,
            };

            var grid1 = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };
            // Define columns
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0


            var editLabel = new Label
            {
                Text = "Edit breed stats",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Style = (Style)Application.Current.Resources["Headline"],
                TextColor = Shared.goodColor,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            };

            // get the current breed stats of selected dino
            string id = selectedID;
            string sep = DataManager.DecimalSeparator;

            string level = DataManager.GetFirstColumnData("ID", id, "Level").Replace(".", sep);
            string hp = DataManager.GetFirstColumnData("ID", id, "Hp").Replace(".", sep);
            string stamina = DataManager.GetFirstColumnData("ID", id, "Stamina").Replace(".", sep);
            string oxygen = DataManager.GetFirstColumnData("ID", id, "Oxygen").Replace(".", sep);
            string food = DataManager.GetFirstColumnData("ID", id, "Food").Replace(".", sep);
            string weight = DataManager.GetFirstColumnData("ID", id, "Weight").Replace(".", sep);
            string damage = DataManager.GetFirstColumnData("ID", id, "Damage").Replace(".", sep);


            //set the temp variables
            levelText = level;
            hpText = hp;
            staminaText = stamina;
            oxygenText = oxygen;
            foodText = food;
            weightText = weight;
            damageText = damage;


            var textBox1 = new Entry { Text = level, Placeholder = "Level", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox2 = new Entry { Text = hp, Placeholder = "Hp", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox3 = new Entry { Text = stamina, Placeholder = "Stamina", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox4 = new Entry { Text = oxygen, Placeholder = "Oxygen", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox5 = new Entry { Text = food, Placeholder = "Food", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox6 = new Entry { Text = weight, Placeholder = "Weight", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };
            var textBox7 = new Entry { Text = damage, Placeholder = "Damage", WidthRequest = 200, HeightRequest = 10, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };

            var textBoxN = new Entry { Placeholder = "Notes", WidthRequest = 200, HeightRequest = 200, TextColor = Colors.Black, BackgroundColor = Colors.LightGray, FontSize = 16, HorizontalOptions = LayoutOptions.Start };


            textBox1.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { levelText = e.NewTextValue; }
            };
            textBox2.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { hpText = e.NewTextValue; }
            };
            textBox3.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { staminaText = e.NewTextValue; }
                staminaText = e.NewTextValue;
            };
            textBox4.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { oxygenText = e.NewTextValue; }
            };
            textBox5.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { foodText = e.NewTextValue; }
            };
            textBox6.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { weightText = e.NewTextValue; }
            };
            textBox7.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { damageText = e.NewTextValue; }
            };
            textBoxN.TextChanged += (sender, e) =>
            {
                string newText = e.NewTextValue;
                // Set notes text here
            };





            // AddToGrid(grid1, imageContainer, 0, 1);

            int rowid = 0;

            AddToGrid(grid1, editLabel, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox1, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox2, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox3, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox4, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox5, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox6, rowid++, 0, "", false, true);
            AddToGrid(grid1, textBox7, rowid++, 0, "", false, true);


            // get parents id

            string papaID = DataManager.GetLastColumnData("ID", id, "Papa");
            string mamaID = DataManager.GetLastColumnData("ID", id, "Mama");

            string papaName = DataManager.GetLastColumnData("ID", papaID, "Name");
            string mamaName = DataManager.GetLastColumnData("ID", mamaID, "Name");

            if (papaName == "") { papaName = "Papa Stats"; }
            if (mamaName == "") { mamaName = "Mama Stats"; }


            string levelP = DataManager.GetFirstColumnData("ID", papaID, "Level").Replace(".", sep);
            string hpP = DataManager.GetFirstColumnData("ID", papaID, "Hp").Replace(".", sep);
            string staminaP = DataManager.GetFirstColumnData("ID", papaID, "Stamina").Replace(".", sep);
            string oxygenP = DataManager.GetFirstColumnData("ID", papaID, "Oxygen").Replace(".", sep);
            string foodP = DataManager.GetFirstColumnData("ID", papaID, "Food").Replace(".", sep);
            string weightP = DataManager.GetFirstColumnData("ID", papaID, "Weight").Replace(".", sep);
            string damageP = DataManager.GetFirstColumnData("ID", papaID, "Damage").Replace(".", sep);


            // add papa stats
            var papaH = new Label { Text = papaName, Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.maleColor, FontSize = 20, FontAttributes = FontAttributes.Bold };
            var labelP1 = new Label { Text = levelP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP2 = new Label { Text = hpP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP3 = new Label { Text = staminaP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP4 = new Label { Text = oxygenP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP5 = new Label { Text = foodP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP6 = new Label { Text = weightP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP7 = new Label { Text = damageP, TextColor = Shared.maleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };


            rowid = 0;
            AddToGrid(grid1, papaH, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP1, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP2, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP3, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP4, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP5, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP6, rowid++, 1, "", false, true);
            AddToGrid(grid1, labelP7, rowid++, 1, "", false, true);


            string levelM = DataManager.GetFirstColumnData("ID", mamaID, "Level").Replace(".", sep);
            string hpM = DataManager.GetFirstColumnData("ID", mamaID, "Hp").Replace(".", sep);
            string staminaM = DataManager.GetFirstColumnData("ID", mamaID, "Stamina").Replace(".", sep);
            string oxygenM = DataManager.GetFirstColumnData("ID", mamaID, "Oxygen").Replace(".", sep);
            string foodM = DataManager.GetFirstColumnData("ID", mamaID, "Food").Replace(".", sep);
            string weightM = DataManager.GetFirstColumnData("ID", mamaID, "Weight").Replace(".", sep);
            string damageM = DataManager.GetFirstColumnData("ID", mamaID, "Damage").Replace(".", sep);


            // add mama stats
            var mamaH = new Label { Text = mamaName, Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.femaleColor, FontSize = 20, FontAttributes = FontAttributes.Bold };

            var labelM1 = new Label { Text = levelM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM2 = new Label { Text = hpM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM3 = new Label { Text = staminaM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM4 = new Label { Text = oxygenM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM5 = new Label { Text = foodM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM6 = new Label { Text = weightM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM7 = new Label { Text = damageM, TextColor = Shared.femaleColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };


            rowid = 0;
            AddToGrid(grid1, mamaH, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM1, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM2, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM3, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM4, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM5, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM6, rowid++, 2, "", false, true);
            AddToGrid(grid1, labelM7, rowid++, 2, "", false, true);

            scrollContent.Children.Add(grid1);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(maingrid, scrollView, 0, 0);
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
            maingrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0


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
                TextColor = Shared.goodColor,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold
            };


            AddToGrid(grid1, imageContainer, 0, 0);
            AddToGrid(grid1, label1, 1, 0);



            scrollContent.Children.Add(grid1);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(maingrid, scrollView, 0, 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////

        }

        return maingrid;
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


        Color DefaultColor = Shared.maleColor;
        Color headerColor = DefaultColor;

        if (title == "Male") { DefaultColor = Shared.maleColor; headerColor = Shared.maleHeaderColor; }
        else if (title == "Female") { DefaultColor = Shared.femaleColor; headerColor = Shared.femaleHeaderColor; }
        else { DefaultColor = Shared.bottomColor; headerColor = Shared.bottomHeaderColor; }

        bool hasO2 = true;

        if (DataManager.OxygenMax == 150) { hasO2 = false; }


        int fSize = Shared.headerSize;  // header fontsize

        // add sorting symbol to the sorted column
        string sortChar = "";

        // find out wich table we are sorting
        string testingSort = "";
        if (title == "Male") { testingSort = sortM; }
        else if (title == "Female") { testingSort = sortF; }


        string newTest = "";
        if (testingSort.Contains("ASC"))
        {
            newTest = testingSort.Substring(0, testingSort.Length - 4);
        }
        if (testingSort.Contains("DESC"))
        {
            newTest = testingSort.Substring(0, testingSort.Length - 5);
        }

        sortChar = ""; if (newTest == "Name") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header0 = new Label { Text = $"Name{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Level") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header1 = new Label { Text = $"Level{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Hp") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header2 = new Label { Text = $"Hp{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Stamina") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header3 = new Label { Text = $"Stamina{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Oxygen") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header4 = new Label { Text = $"Oxygen{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Food") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header5 = new Label { Text = $"Food{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Weight") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header6 = new Label { Text = $"Weight{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Damage") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header7 = new Label { Text = $"Damage{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Status") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header8 = new Label { Text = $"Status{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Gen") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header9 = new Label { Text = $"Gen{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Papa") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header10 = new Label { Text = $"Papa{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.maleColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Mama") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header11 = new Label { Text = $"Mama{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.femaleColor, FontSize = fSize };
        sortChar = ""; if (newTest == "PapaMute") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header12 = new Label { Text = $"PapaMute{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "MamaMute") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header13 = new Label { Text = $"MamaMute{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Imprint") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header14 = new Label { Text = $"Imprint{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == "Imprinter") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var header15 = new Label { Text = $"Imprinter{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };



        string sexS = "T";
        if (title == "Male") { sexS = "M"; }
        else { sexS = "F"; }

        if (title != "Bottom") // not sortable bottom row
        {
            SortColumn(header0, sexS);
            SortColumn(header1, sexS);
            SortColumn(header2, sexS);
            SortColumn(header3, sexS);

            if (hasO2) { SortColumn(header4, sexS); }

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

        // Add base header row
        AddToGrid(grid, header0, 0, 0, title);
        AddToGrid(grid, header1, 0, 1, title);
        AddToGrid(grid, header2, 0, 2, title);
        AddToGrid(grid, header3, 0, 3, title);


        int startID = 4;

        // If hasO2, include the O2 column (header4)
        if (hasO2)
        {
            AddToGrid(grid, header4, 0, startID++, title); // Increment startID after adding header4
        }

        // Add remaining headers
        AddToGrid(grid, header5, 0, startID++, title);
        AddToGrid(grid, header6, 0, startID++, title);
        AddToGrid(grid, header7, 0, startID++, title);
        AddToGrid(grid, header8, 0, startID++, title);
        AddToGrid(grid, header9, 0, startID++, title);
        AddToGrid(grid, header10, 0, startID++, title);
        AddToGrid(grid, header11, 0, startID++, title);

        // Add last column headers if conditions are met
        if (title != "Bottom" || isSelected)
        {
            AddToGrid(grid, header12, 0, startID++, title);
            AddToGrid(grid, header13, 0, startID++, title);
            AddToGrid(grid, header14, 0, startID++, title);
            AddToGrid(grid, header15, 0, startID++, title);
        }

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



            if (ToggleExcluded == 0)
            {
                status = DataManager.GetStatus(id);
            }
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

                                if (aC == "2") { cellColor2 = Shared.bestColor; }
                                if (bC == "2") { cellColor3 = Shared.bestColor; }
                                if (cC == "2") { cellColor4 = Shared.bestColor; }
                                if (dC == "2") { cellColor5 = Shared.bestColor; }
                                if (eC == "2") { cellColor6 = Shared.bestColor; }
                                if (fC == "2") { cellColor7 = Shared.bestColor; }

                                if ((aC + bC + cC + dC + eC + fC) == "222222")
                                {
                                    // here is a golden offspring with all the best stats
                                    cellColor2 = Shared.goldColor;
                                    cellColor3 = Shared.goldColor;
                                    cellColor4 = Shared.goldColor;
                                    cellColor5 = Shared.goldColor;
                                    cellColor6 = Shared.goldColor;
                                    cellColor7 = Shared.goldColor;
                                }
                                break;
                            }
                            rowID++;
                        }
                    }
                }
            }

            // oxygen breed point override
            if (!hasO2)
            {
                if (title == "Bottom" && status.Length == 9 && name.Contains("Breed #"))
                {
                    string newBP = status.Trim();

                    string gP = newBP.Substring(0, 1); // take first char
                    string aP = newBP.Substring(4, 1); // take 5th char

                    double GP = DataManager.ToDouble(gP) - 1;
                    double AP = DataManager.ToDouble(aP);
                    double SP = GP + AP;

                    status = $"{GP} + {AP} = {SP}";
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
                if (hasO2) { SelectDino(oxygenL, id); }
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
                // for row coloring purposes
                if (id == selectedID) { selected = true; }
            }


            // Reset startID for new row
            startID = 4;

            // Add base items to the grid
            AddToGrid(grid, nameL, rowIndex, 0, title, selected);
            AddToGrid(grid, levelL, rowIndex, 1, title, selected);
            AddToGrid(grid, hpL, rowIndex, 2, title, selected);
            AddToGrid(grid, staminaL, rowIndex, 3, title, selected);

            // Add dynamic items starting from the existing startID
            if (hasO2)
            {
                AddToGrid(grid, oxygenL, rowIndex, startID++, title, selected); // Add oxygen if applicable
            }

            AddToGrid(grid, foodL, rowIndex, startID++, title, selected);
            AddToGrid(grid, weightL, rowIndex, startID++, title, selected);
            AddToGrid(grid, damageL, rowIndex, startID++, title, selected);
            AddToGrid(grid, statusL, rowIndex, startID++, title, selected);
            AddToGrid(grid, genL, rowIndex, startID++, title, selected);
            AddToGrid(grid, papaL, rowIndex, startID++, title, selected);
            AddToGrid(grid, mamaL, rowIndex, startID++, title, selected);
            AddToGrid(grid, papaML, rowIndex, startID++, title, selected);
            AddToGrid(grid, mamaML, rowIndex, startID++, title, selected);
            AddToGrid(grid, imprintL, rowIndex, startID++, title, selected);
            AddToGrid(grid, imprinterL, rowIndex, startID++, title, selected);

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

    private bool IsValidDouble(string input)
    {
        editStats = true;
        return double.TryParse(input, out _); // Returns true if the input is a valid double
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

            if (column.Contains(upChar) || column.Contains(downChar))
            {
                column = column.Substring(0, column.Length - 2);
            }


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
            if (selectedID != id) // select a new dino
            {
                selectedID = id; isSelected = true;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Selected {name} ID: {id}", 0);

                // activate double clicking

                canDouble = true;
                DisableDoubleClick(500);
            }
            else if (selectedID == id && canDouble) // select same dino within time
            {
                // double click  // open the dino extended info window
                isDouble = true; canDouble = false;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                //this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Double click {name} ID: {id}", 0);

            }
            else if (selectedID == id && !canDouble) // select same dino over time
            {
                // re activate double clicking
                canDouble = true;
                DisableDoubleClick(500);
            }
            CreateContent();
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
        // reset toggles etc.
        levelText = ""; hpText = ""; staminaText = ""; oxygenText = "";
        foodText = ""; weightText = "";damageText = "";
        isDouble = false;
        ClearSelection();
        CreateContent();
    }

    private void OnButton5Clicked(object? sender, EventArgs e)
    {
        // save data here

        // find the breed stats of
        // selectedID
        // and edit them

        DataManager.EditBreedStats(selectedID, levelText, hpText, staminaText, oxygenText, foodText, weightText, damageText);
        FileManager.needSave = true;

        // reset toggles etc.
        levelText = ""; hpText = ""; staminaText = ""; oxygenText = "";
        foodText = ""; weightText = ""; damageText = "";
        isDouble = false;
        ClearSelection();
        CreateContent();
    }



}