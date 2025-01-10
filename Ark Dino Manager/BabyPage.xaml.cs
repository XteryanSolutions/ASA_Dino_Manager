using System.Data;
using System.Xml.Linq;
using static Ark_Dino_Manager.Shared;

namespace Ark_Dino_Manager;

public partial class BabyPage : ContentPage
{
    ////////////////////    View Toggles    ////////////////////
    public static int ToggleExcluded = Shared.DefaultToggleB;
    public static string speciesToggle = "All";

    ////////////////////    Selecting       ////////////////////
    public static string selectedID = "";
    public static bool isSelected = false;
    public static bool canDouble = false;
    public static bool isDouble = false;

    // keep track of boxviews for recoloring
    private Dictionary<int, BoxView> boxViews = new Dictionary<int, BoxView>();
    private int boxID = 0;
    private int boxRowID = 0;

    Button ExcludeBtn = new Button { };
    Button ArchiveBtn = new Button { };

    ////////////////////    Table Sorting   ////////////////////
    public static string sortM = Shared.DefaultSortM;
    public static string sortF = Shared.DefaultSortF;


    private string rateText = "";
    private string classText = "";

    private bool editStats = false;

    private bool dataValid = false;

    public BabyPage()
    {
        InitializeComponent();

        dataValid = false; // refresh data
        CreateContent();
    }

    public void CreateContent()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (!dataValid)
                {
                    FileManager.Log("Loading Baby Data", 0);
                    // sort data based on column clicked

                    DataManager.GetDinoData("", sortM, sortF, ToggleExcluded, false, true);
                   // DataManager.GetDinoBabies(sortM, sortF);

                    dataValid = true;
                }


                FileManager.Log("Updating GUI -> " + Shared.setPage, 0);
                if (!isSelected) { this.Title = $"{Shared.setPage.Replace("_", " ")}"; }
                else { this.Title = $"{DataManager.GetLastColumnData("ID", selectedID, "Name")} - {selectedID}"; }

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
        if (isSelected)
        {
            UnSelectDino(mainLayout);
        }

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

        // Define row definitions
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // 0

        mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 }); // 0
        mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // reset boxViews
        boxID = 0; boxRowID = 0;
        boxViews = new Dictionary<int, BoxView>();

        // Add side panel to left column
        AddToGrid(mainLayout, CreateSidePanel(), 0, 0);

        // Add main panel to right column
        AddToGrid(mainLayout, CreateMainPanel(), 0, 1);

        // don't unselect dino while editing stats
        if (!isDouble)
        {
            UnSelectDino(mainLayout);
        }

        this.Content = mainLayout;
    }

    private void AddToGrid(Grid grid, View view, int row, int column, string title = "", bool selected = false, bool isDoubl = false, string id = "")
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

            boxViews[boxID++] = rowBackground;

            // make background on row selectable to increase surface area
            if (id != "") // only when an id is passed
            {
                SelectBG(rowBackground, id);
            }

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
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // extra columns to put anchor bottom buttons

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Define button colors
        var toggleBtnColor = Shared.DefaultBColor;
        var speciesBtnColor = Shared.DefaultBColor;

        if (ToggleExcluded == 0)
        {
            toggleBtnColor = Shared.DefaultBColor;
        }
        else if (ToggleExcluded == 1)
        {
            toggleBtnColor = Shared.PrimaryColor;
        }
        else if (ToggleExcluded == 2)
        {
            toggleBtnColor = Shared.SecondaryColor;
        }
        else if (ToggleExcluded == 3)
        {
            toggleBtnColor = Shared.TrinaryColor;
        }


        string speciesBtnText = speciesToggle;


        string toggleBtnText = "Toggle";
        if (ToggleExcluded == 0) { toggleBtnText = "All"; }
        else if (ToggleExcluded == 1) { toggleBtnText = "Included"; }
        else if (ToggleExcluded == 2) { toggleBtnText = "Excluded"; }
        else if (ToggleExcluded == 3) { toggleBtnText = "Archived"; }


        if (isDouble)
        {
            var saveBtn = new Button { Text = "Save", BackgroundColor = Shared.TrinaryColor };
            saveBtn.Clicked += SaveBtnClicked;
            AddToGrid(grid, saveBtn, 0, 0);


            var backBtn = new Button { Text = "Back", BackgroundColor = Shared.PrimaryColor };
            backBtn.Clicked += BackBtnClicked;
            AddToGrid(grid, backBtn, 1, 0);
        }
        else
        {
            var toggleBtn = new Button { Text = toggleBtnText, BackgroundColor = toggleBtnColor };
            AddToGrid(grid, toggleBtn, 0, 0);
            toggleBtn.Clicked += ToggleBtnClicked;


            var speciesBtn = new Button { Text = speciesBtnText, BackgroundColor = speciesBtnColor };
            //AddToGrid(grid, speciesBtn, 1, 0);
            speciesBtn.Clicked += SpeciesBtnClicked;
        }


        // add dynamic buttons (shown only when dino is selected)
        ExcludeBtn = new Button { Text = "" };
        ExcludeBtn.Clicked += ExcludeBtnClicked;
        AddToGrid(grid, ExcludeBtn, 5, 0);

        ArchiveBtn = new Button { Text = "" };
        ArchiveBtn.Clicked += ArchiveBtnClicked;
        AddToGrid(grid, ArchiveBtn, 6, 0);


        string group = DataManager.GetGroup(selectedID);
        if (group == "Exclude") { ExcludeBtn.Text = "Include"; ExcludeBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ExcludeBtn.Text = "Exclude"; ExcludeBtn.BackgroundColor = Shared.SecondaryColor; }

        if (group == "Archived") { ArchiveBtn.Text = "Include"; ArchiveBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ArchiveBtn.Text = "Archive"; ArchiveBtn.BackgroundColor = Shared.TrinaryColor; }

        if (!isSelected)
        {
            ExcludeBtn.IsVisible = false;
            ArchiveBtn.IsVisible = false;
        }
        else
        {
            ExcludeBtn.IsVisible = true;
            ArchiveBtn.IsVisible = true;
        }

        return grid;
    }

    private Grid CreateMainPanel()
    {
        var maingrid = new Grid { RowSpacing = 0, ColumnSpacing = 5, Padding = 0, };

        // Define columns
        maingrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0

        // Define row definitions
        maingrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content



        int count = DataManager.FemaleTable.Rows.Count + DataManager.MaleTable.Rows.Count;

        if (count > 0 && !isDouble) // more than 0 dinos and not double clicked
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Create scrollable content
            var scrollContent = new StackLayout { Spacing = 20, Padding = 3 };

            // Add male and female tables
            scrollContent.Children.Add(CreateDinoGrid(DataManager.MaleTable, "Male"));
            scrollContent.Children.Add(CreateDinoGrid(DataManager.FemaleTable, "Female"));

            // Wrap the scrollable content in a ScrollView and add it
            var scrollView = new ScrollView { Content = scrollContent, Orientation = ScrollOrientation.Horizontal };

            AddToGrid(maingrid, scrollView, 0, 1);
            ////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        else if (count > 0 && isDouble)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // make dino info box
            var scrollContent = new StackLayout { Spacing = 20, Padding = 3, };

            // Create Grid for stats
            var statGrid = new Grid { RowSpacing = 0, ColumnSpacing = 20, Padding = 3 };

            // Define columns
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0

            // Get info about selected dino and class
            string currentID = selectedID;
            string sex = DataManager.GetLastColumnData("ID", currentID, "Sex");
            string dinoClass = DataManager.GetLastColumnData("ID", currentID, "Class");

            string shortClass = DataManager.LongClassToShort(dinoClass);
            classText = shortClass;

            string rate = DataManager.GetRate(shortClass);

            Color DefaultColor = Shared.maleColor;

            if (sex == "Male") { DefaultColor = Shared.maleColor; }
            else if (sex == "Female") { DefaultColor = Shared.femaleColor; }

            var cellColor0 = DefaultColor;
            var cellColor1 = DefaultColor;


            // add stat text
            var t0 = new Label { Text = "", Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.maleColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold };
            var t1 = new Label { Text = "Age %/hr", TextColor = cellColor1, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };

            int rowid = 0;
            int colid = 0;
            AddToGrid(statGrid, t0, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t1, rowid++, colid, "", false, true);


            var editLabel = new Label
            {
                Text = $"{shortClass} Aging Rate",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Style = (Style)Application.Current.Resources["Headline"],
                TextColor = cellColor0,
                FontSize = Shared.fontHSize,
                FontAttributes = FontAttributes.Bold
            };

            var textBox1 = new Entry { Text = rate, Placeholder = "0 for auto", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor1, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };

            textBox1.TextChanged += (sender, e) =>
            {
                if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
                else { rateText = e.NewTextValue; }
            };

            rowid = 0;
            colid = 1;

            AddToGrid(statGrid, editLabel, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox1, rowid++, colid, "", false, true);



            scrollContent.Children.Add(statGrid);


            rowid = 0;


            //AddToGrid(notesGrid, textBoxN, rowid++, 0, "", false, true);


            //scrollContent.Children.Add(notesGrid);

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
        var grid = new Grid { RowSpacing = 0, ColumnSpacing = 20, Padding = 3 };

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
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 14


        Color DefaultColor = Shared.maleColor;
        Color headerColor = DefaultColor;

        // find out wich table we are sorting
        string testingSort = "";
        if (title == "Male") { DefaultColor = Shared.maleColor; headerColor = Shared.maleHeaderColor; testingSort = sortM; }
        else if (title == "Female") { DefaultColor = Shared.femaleColor; headerColor = Shared.femaleHeaderColor; testingSort = sortF; }
        else { DefaultColor = Shared.bottomColor; headerColor = Shared.bottomHeaderColor; }

        // sorting direction
        string newTest = "";
        if (testingSort.Contains("ASC"))
        {
            newTest = testingSort.Substring(0, testingSort.Length - 4);
        }
        if (testingSort.Contains("DESC"))
        {
            newTest = testingSort.Substring(0, testingSort.Length - 5);
        }

        // header fontsize
        int fSize = Shared.headerSize;

        // sorting symbols
        string upChar = Smap["SortUp"];
        string downChar = Smap["SortDown"];

        string sortChar = "";

        // maybe some width adjustment for headers to line up the tables
        int colID = 0;
        int[] cellW = { 110, 100 }; // maybe figure out the max width of any cells


        if (newTest == $"{Smap["Class"]}Class") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var tagH = new Label { Text = $"{Smap["Class"]}Class{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize, WidthRequest = cellW[colID++] };
        sortChar = ""; if (newTest == $"{Smap["Name"]}Name") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var nameH = new Label { Text = $"{Smap["Name"]}Name{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Level"]}Level") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var levelH = new Label { Text = $"{Smap["Level"]}Level{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Age"]}Age") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var ageH = new Label { Text = $"{Smap["Age"]}Age{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Time"]}Time") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var timeH = new Label { Text = $"{Smap["Time"]}Time{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Rate"]}Rate") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var rateH = new Label { Text = $"{Smap["Rate"]}Rate{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        // no need to sort date
        var dateH = new Label { Text = $"{Smap["Date"]}Date", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };

        sortChar = ""; if (newTest == $"{Smap["Status"]}Status") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var statusH = new Label { Text = $"{Smap["Status"]}Status{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Gen"]}Gen") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var genH = new Label { Text = $"{Smap["Gen"]}Gen{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Papa"]}Papa") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var papaH = new Label { Text = $"{Smap["Papa"]}Papa{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.maleHeaderColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Mama"]}Mama") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var mamaH = new Label { Text = $"{Smap["Mama"]}Mama{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.femaleHeaderColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Mutation"]}PapaMute") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var papamH = new Label { Text = $"{Smap["Mutation"]}PapaMute{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.maleHeaderColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Mutation"]}MamaMute") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var mamamH = new Label { Text = $"{Smap["Mutation"]}MamaMute{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.femaleHeaderColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Imprint"]}Imprint") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var imprintH = new Label { Text = $"{Smap["Imprint"]}Imprint{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
        sortChar = ""; if (newTest == $"{Smap["Imprinter"]}Imprinter") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var imprinterH = new Label { Text = $"{Smap["Imprinter"]}Imprinter{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };



        // make columns sortable
        SortColumn(tagH, title);
        SortColumn(nameH, title);
        SortColumn(levelH, title);
        SortColumn(ageH, title);
        SortColumn(timeH, title);
        SortColumn(rateH, title);
        //SortColumn(dateH, title);
        //SortColumn(statusH, title);
        SortColumn(genH, title);
        SortColumn(papaH, title);
        SortColumn(mamaH, title);
        SortColumn(papamH, title);
        SortColumn(mamamH, title);
        SortColumn(imprintH, title);
        SortColumn(imprinterH, title);


        int startID = 0;
        // Add base header row
        AddToGrid(grid, tagH, 0, startID++, title);// tag
        AddToGrid(grid, nameH, 0, startID++, title);// name
        AddToGrid(grid, levelH, 0, startID++, title);// level
        AddToGrid(grid, ageH, 0, startID++, title);// age
        AddToGrid(grid, timeH, 0, startID++, title);// time
        AddToGrid(grid, rateH, 0, startID++, title);// rate
        AddToGrid(grid, dateH, 0, startID++, title);// date
        AddToGrid(grid, imprintH, 0, startID++, title);// imprint
        AddToGrid(grid, genH, 0, startID++, title);// gen
        AddToGrid(grid, statusH, 0, startID++, title);// status
        AddToGrid(grid, papaH, 0, startID++, title);// papa
        AddToGrid(grid, mamaH, 0, startID++, title);// mama
        AddToGrid(grid, papamH, 0, startID++, title);// papam
        AddToGrid(grid, mamamH, 0, startID++, title);// mamam

        AddToGrid(grid, imprinterH, 0, startID++, title);// imprinter


        // add one xtra id for female header row
        if (title == "Female") { boxRowID++; }

        int rowIndex = 1; // Start adding rows below the header
        foreach (DataRow row in table.Rows)
        {
            boxRowID++;

            var cellColor0 = DefaultColor;
            var cellColor1 = DefaultColor;
            var cellColor2 = DefaultColor;
            var cellColor3 = DefaultColor;
            var cellColor4 = DefaultColor;
            var cellColor5 = DefaultColor;

            var cellColor8 = DefaultColor;


            string id = row["ID"].ToString();


            string dinoClass = row["Tag"].ToString();
            string name = row["Name"].ToString();
            if (name == "") { name = "I need a name"; }
            string level = row["Level"].ToString();
            //////////////
            double ageF = DataManager.ToDouble(DataManager.GetLastColumnData("ID", id, "BabyAge")) * 100;
            double ageD = Math.Round(DataManager.ToDouble(row["Age"].ToString()), 1);
            string ageT = ageD + "%";
            string timeT = row["Time"].ToString();
            string rateT = row["Rate"].ToString();
            string dateT = row["DateT"].ToString();

            string gen = row["Gen"].ToString();




            string papa = row["Papa"].ToString();
            string mama = row["Mama"].ToString();
            string papaM = row["PapaMute"].ToString();
            string mamaM = row["MamaMute"].ToString();
            string imprint = row["Imprint"].ToString();
            string imprinter = row["Imprinter"].ToString();

            string status = "";

            int totalMinutes = (int)(DataManager.ToDouble(timeT));
            int days = totalMinutes / (24 * 60);
            int hours = (totalMinutes % (24 * 60)) / 60;
            int minutes = totalMinutes % 60;


            double ageRateHr = Math.Round(DataManager.ToDouble(rateT) * 60, 2);

            // check if manual age rate is set
            string age2 = DataManager.GetRate(dinoClass);
            double age2D = DataManager.ToDouble(age2);
            if (age2D > 0)
            {
                cellColor4 = goodColor;
            }

            rateT = $"{ageRateHr} %/hr";
            timeT = $"{days}d {hours}h {minutes}m";


            if (ageT.Contains("NaN"))
            {
                ageT = Math.Round(ageF, 1) + " %";
            }
            if (rateT.Contains("NaN") || ageRateHr == 0)
            {
                rateT = Smap["Missing"];
                timeT = Smap["Missing"];
                dateT = Smap["Missing"];
            }


            status = "Baby";
            if (ageD > 10 || ageF > 10)
            {
                status = "Juvenile";
            }
            if (ageD > 50 || ageF > 50)
            {
                status = "Adolescent";
            }
            if (ageD >= 100 || ageF >= 100)
            {
                status = "Adult";
            }

            string notes = DataManager.GetNotes(id);
            if (notes != "")
            {
                status = Smap["Notes"] + status;
            }

            // mark all generation dependant data as invalid
            if (mama == Shared.Smap["Warning"] && papa == Shared.Smap["Warning"])
            {
                papaM = Shared.Smap["Warning"];
                mamaM = Shared.Smap["Warning"];
                gen = Shared.Smap["Warning"];
            }


            // Create a Labels
            var tagL = new Label { Text = dinoClass, TextColor = cellColor0 };
            var nameL = new Label { Text = name, TextColor = cellColor0 };
            var levelL = new Label { Text = level, TextColor = cellColor1 };
            //////////////
            var ageL = new Label { Text = ageT, TextColor = cellColor2 };
            var timeL = new Label { Text = timeT, TextColor = cellColor3 };
            var rateL = new Label { Text = rateT, TextColor = cellColor4 };
            var dateL = new Label { Text = dateT, TextColor = cellColor5 };
            //////////////
            var statusL = new Label { Text = status, TextColor = cellColor8 };
            var genL = new Label { Text = gen, TextColor = cellColor8 };
            var papaL = new Label { Text = papa, TextColor = Shared.maleColor };
            var mamaL = new Label { Text = mama, TextColor = Shared.femaleColor };
            var papaML = new Label { Text = papaM, TextColor = Shared.maleColor };
            var mamaML = new Label { Text = mamaM, TextColor = Shared.femaleColor };
            var imprintL = new Label { Text = imprint, TextColor = cellColor8 };
            var imprinterL = new Label { Text = imprinter, TextColor = cellColor8 };


            bool selected = false;

            // Attach TapGesture to all labels
            SelectDino(tagL, id, boxRowID);
            SelectDino(nameL, id, boxRowID);
            SelectDino(levelL, id, boxRowID);
            SelectDino(ageL, id, boxRowID);
            SelectDino(timeL, id, boxRowID);
            SelectDino(rateL, id, boxRowID);
            SelectDino(dateL, id, boxRowID);
            SelectDino(statusL, id, boxRowID);
            SelectDino(genL, id, boxRowID);
            SelectDino(papaL, id, boxRowID);
            SelectDino(mamaL, id, boxRowID);
            SelectDino(papaML, id, boxRowID);
            SelectDino(mamaML, id, boxRowID);
            SelectDino(imprintL, id, boxRowID);
            SelectDino(imprinterL, id, boxRowID);

            // figure out if we have this dino selected
            // for row coloring purposes
            if (id == selectedID) { selected = true; }

            // Reset startID for new row
            startID = 0;

            // Add base items to the grid
            AddToGrid(grid, tagL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, nameL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, levelL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, ageL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, timeL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, rateL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, dateL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, imprintL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, genL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, statusL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, papaL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, mamaL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, papaML, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, mamaML, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, imprinterL, rowIndex, startID++, title, selected, false, id);

            rowIndex++;
        }

        return grid;
    }

    private void DefaultRowColors()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (boxViews.Count > 0)
                {
                    int rowsM = DataManager.MaleTable.Rows.Count;
                    int rowsT = boxViews.Count;
                    int z = 0;

                    for (int i = 0; i < rowsT; i++) // color all male rows
                    {
                        // start coloring the rows with Solid color
                        if (i <= rowsM)
                        {
                            boxViews[i].Color = i % 2 == 0 ? OddMPanelColor : MainPanelColor;
                        }
                        else // use z instead of i to reset odd & even at new table
                        {
                            boxViews[i].Color = z % 2 == 0 ? OddMPanelColor : MainPanelColor;
                            z++;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                Monitor.Exit(Shared._dbLock);
            }
        }
        else
        {
            FileManager.Log("Recoloring failure", 1);
        }
    }

    private void ClearSelection()
    {
        if (selectedID != "" && !isDouble)
        {
            //  FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = $"{Shared.setPage.Replace("_", " ")}";
            canDouble = false; editStats = false;

            ExcludeBtn.IsVisible = false;
            ArchiveBtn.IsVisible = false;

            // recolor all rows to default
            DefaultRowColors();
        }
    }

    private bool IsValidDouble(string input)
    {
        editStats = true;
        return double.TryParse(input, out _); // Returns true if the input is a valid double
    }

    private void ButtonGroup()
    {
        string group = DataManager.GetGroup(selectedID);
        if (group == "Exclude") { ExcludeBtn.Text = "Include"; ExcludeBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ExcludeBtn.Text = "Exclude"; ExcludeBtn.BackgroundColor = Shared.SecondaryColor; }

        if (group == "Archived") { ArchiveBtn.Text = "Include"; ArchiveBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ArchiveBtn.Text = "Archive"; ArchiveBtn.BackgroundColor = Shared.TrinaryColor; }

        ExcludeBtn.IsVisible = true;
        ArchiveBtn.IsVisible = true;
    }

    // Some event handlers
    void SortColumn(Label label, string sex)
    {
        label.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture1 = new TapGestureRecognizer();
        tapGesture1.Tapped += (s, e) =>
        {
            // Handle the click event and pass additional data
            string column = label.Text;

            if (column.Contains(Smap["SortUp"]) || column.Contains(Smap["SortDown"]))
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

            if (sex == "Male")
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
            else if (sex == "Female")
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

            dataValid = false;
            ClearSelection();
            CreateContent();
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture1);
    }

    void SelectDino(Label label, string id, int boxid = 0)
    {
        label.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (selectedID != id) // select a new dino
            {
                selectedID = id; isSelected = true;

                // recolor all rows to default
                DefaultRowColors();

                boxViews[boxid].Color = SelectedColor;

                // make buttons visible
                ButtonGroup();

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                // activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
            else if (selectedID == id && canDouble) // select same dino within time
            {
                // double click  // open the dino extended info window
                isDouble = true; canDouble = false;
                CreateContent();
            }
            else if (selectedID == id && !canDouble) // select same dino over time
            {
                // re activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture);
    }

    void SelectBG(BoxView inp, string id)
    {
        inp.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (selectedID != id) // select a new dino
            {
                selectedID = id; isSelected = true;

                // recolor all rows to default
                DefaultRowColors();

                inp.Color = SelectedColor;

                // make buttons visible
                ButtonGroup();

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                // activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
            else if (selectedID == id && canDouble) // select same dino within time
            {
                // double click  // open the dino extended info window
                isDouble = true; canDouble = false;
                CreateContent();
            }
            else if (selectedID == id && !canDouble) // select same dino over time
            {
                // re activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
        };

        // Attach the TapGestureRecognizer to the label
        inp.GestureRecognizers.Add(tapGesture);
    }

    private async Task DisableDoubleClick()
    {
        await Task.Delay(Shared.doubleClick);
        canDouble = false;
    }

    void UnSelectDino(Grid grid)
    {
        grid.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            ClearSelection();
            //CreateContent();
        };

        // Attach the TapGestureRecognizer to the label
        grid.GestureRecognizers.Add(tapGesture);
    }


    // Button event handlers
    private void ToggleBtnClicked(object? sender, EventArgs e)
    {
        ToggleExcluded++;
        if (ToggleExcluded == 4)
        {
            ToggleExcluded = 0;
        }
        FileManager.Log($"Toggle Exclude {ToggleExcluded}", 0);

        dataValid = false;
        ClearSelection();
        CreateContent();
    }

    private void SpeciesBtnClicked(object? sender, EventArgs e)
    {
        if (speciesToggle == "All")
        {

        }

        FileManager.Log($"Toggle Species", 0);

        dataValid = false;
        ClearSelection();
        CreateContent();
    }

    private void ExcludeBtnClicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            string status = DataManager.GetGroup(selectedID);
            if (status == "Exclude") { status = ""; }
            else if (status == "") { status = "Exclude"; FileManager.Log($"Excluded ID: {selectedID}", 0); }
            DataManager.SetGroup(selectedID, status);

            dataValid = false;
            ClearSelection();
            CreateContent();
        }
    }

    private void ArchiveBtnClicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            // Handle the click event
            string status = DataManager.GetGroup(selectedID);
            if (status == "Archived") { status = ""; FileManager.Log($"Restored ID: {selectedID}", 0); }
            else if (status == "") { status = "Archived"; FileManager.Log($"Archived ID: {selectedID}", 0); }
            else if (status == "Exclude") { status = "Archived"; FileManager.Log($"Archived ID: {selectedID}", 0); }
            DataManager.SetGroup(selectedID, status);

            dataValid = false;
            ClearSelection();
            CreateContent();
        }

    }

    private void BackBtnClicked(object? sender, EventArgs e)
    {
        // reset toggles etc.
        rateText = ""; classText = "";
        isDouble = false; editStats = false;

        ClearSelection();
        CreateContent();
    }

    private void SaveBtnClicked(object? sender, EventArgs e)
    {
        if (editStats)
        {
            // DataManager.EditBreedStats(selectedID, levelText, hpText, staminaText, O2Text, foodText, weightText, damageText, notesText);
            DataManager.SetRate(classText, rateText);


            FileManager.needSave = true;
            dataValid = false;
            editStats = false;
        }

        // reset toggles etc.
        rateText = ""; classText = "";
        isDouble = false;
        ClearSelection();
        CreateContent();
    }


}