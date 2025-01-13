using System.Data;
using static Ark_Dino_Manager.Shared;

namespace Ark_Dino_Manager;

public partial class ArchivePage : ContentPage
{
    ////////////////////    Selecting       ////////////////////
    public static string selectedID = "";
    public static bool isSelected = false;

    ////////////////////    Table Sorting   ////////////////////
    public static string sortA = Shared.DefaultSortA;

    // keep track of boxviews for recoloring
    private Dictionary<int, BoxView> boxViews = new Dictionary<int, BoxView>();
    private int boxID = 0;
    private int boxRowID = 0;

    Button PurgeBtn = new Button { };
    Button ArchiveBtn = new Button { };
    Button PurgeAllBtn = new Button { };

    public ArchivePage()
    {
        InitializeComponent();

        CreateContent();
    }

    private void AddToGrid(Grid grid, View view, int row, int column, bool selected = false, string id = "")
    {
        // Ensure rows exist up to the specified index
        while (grid.RowDefinitions.Count <= row)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            // Add a background color to every other row
            var rowColor = grid.RowDefinitions.Count % 2 == 0
                ? Shared.ArchivePanelColor // Even rows
                : Shared.OddAPanelColor; // Odd rows

            // Override if row is selected
            if (selected) { rowColor = Shared.SelectedColor; }

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

    public void CreateContent()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {

            try
            {
                FileManager.Log("Loading Archive Data", 0);
                DataManager.CompileDinoArchive(sortA);


                FileManager.Log("Updating GUI -> " + Shared.setPage, 0);
                if (!isSelected) { this.Title = $"{Shared.setPage.Replace("_", " ")}"; }
                else { this.Title = $"{DataManager.GetLastColumnData("ID", selectedID, "Name")} - {selectedID}"; }

                if (DataManager.ArchiveTable.Rows.Count > 0)
                {
                    ArchiveView();
                }
                else
                {
                    DefaultView("No dinos in here :(");
                }
            }
            catch
            {
                FileManager.Log("Failed updating Archive table", 2);
                DefaultView("Dinos imploded =O");
            }
            finally
            {
                Monitor.Exit(Shared._dbLock);
            }
        }
        else
        {
            DefaultView("Dinos ran away :(");
            FileManager.Log("ArchivePage Failed to acquire database lock", 1);
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

        var image1 = new Image { Source = "dino.png", HeightRequest = 400, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start };

        var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Shared.PrimaryColor, FontSize = 22 };


        AddToGrid(mainLayout, image1, 0, 0);
        AddToGrid(mainLayout, label1, 1, 0);




        // Wrap the scrollable content in a ScrollView and add it to the second row
        var scrollView = new ScrollView { Content = scrollContent };

        AddToGrid(mainLayout, scrollView, 0, 0);


        // attach the tapgesture
        UnSelectDino(mainLayout);


        this.Content = null;
        this.Content = mainLayout;
    }

    private void ArchiveView()
    {
        // ==============================================================    Create Archive Layout   =====================================================

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

        // attach the tapgesture
        UnSelectDino(mainLayout);


        this.Content = mainLayout;
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

        // Create rows for buttons
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Autosize this row to keep following buttons at bottom
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 5
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 6


        ArchiveBtn = new Button { Text = "Restore", BackgroundColor = Shared.PrimaryColor };
        ArchiveBtn.Clicked += ArchiveBtnClicked;
        AddToGrid(grid, ArchiveBtn, 0, 0);

        PurgeBtn = new Button { Text = "Purge", BackgroundColor = Shared.TrinaryColor };
        PurgeBtn.Clicked += PurgeBtnClicked;
        AddToGrid(grid, PurgeBtn, 5, 0);

        PurgeAllBtn = new Button { Text = "Purge All", BackgroundColor = Shared.TrinaryColor };
        PurgeAllBtn.Clicked += PurgeAllBtnClicked;
        AddToGrid(grid, PurgeAllBtn, 6, 0);


        if (!isSelected) // Hide buttons when nothing is selected
        {
            PurgeBtn.IsVisible = false;
            ArchiveBtn.IsVisible = false;
        }
        else
        {
            ArchiveBtn.IsVisible = true;
            PurgeBtn.IsVisible = true;
        }
        if (DataManager.ArchiveTable.Rows.Count > 0)
        {
            PurgeAllBtn.IsVisible = true;
        }
        else
        {
            PurgeAllBtn.IsVisible = false;
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

        // Define row definitions
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // Create scrollable content
        var scrollContent = new StackLayout
        {
            Spacing = 20,
            Padding = 3
        };

        // reset boxViews
        boxID = 0; boxRowID = 0;
        boxViews = new Dictionary<int, BoxView>();

        // Add male and female tables
        scrollContent.Children.Add(CreateArchiveGrid(DataManager.ArchiveTable, "Archive"));

        // Wrap the scrollable content in a ScrollView and add it to the second row
        var scrollView = new ScrollView { Content = scrollContent };

        AddToGrid(grid, scrollView, 0, 1);

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        return grid;
    }

    private Grid CreateArchiveGrid(DataTable table, string title)
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


        // add sorting symbol to the sorted column
        string sortChar = "";

        string tableSort = sortA;

        string newTest = "";
        if (tableSort.Contains("ASC")) { newTest = tableSort.Substring(0, tableSort.Length - 4); }
        if (tableSort.Contains("DESC")) { newTest = tableSort.Substring(0, tableSort.Length - 5); }

        string upChar = Smap["SortUp"];
        string downChar = Smap["SortDown"];


        sortChar = ""; if (newTest == $"{Smap["ID"]}ID") { if (tableSort.Contains("ASC")) { sortChar = " " + upChar; } if (tableSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var idH = new Label { Text = $"{Smap["ID"]}ID{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.TrinaryColor };

        sortChar = ""; if (newTest == $"{Smap["Tag"]}Tag") { if (tableSort.Contains("ASC")) { sortChar = " " + upChar; } if (tableSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var tagH = new Label { Text = $"{Smap["Tag"]}Tag{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.TrinaryColor };

        sortChar = ""; if (newTest == $"{Smap["Name"]}Name") { if (tableSort.Contains("ASC")) { sortChar = " " + upChar; } if (tableSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var nameH = new Label { Text = $"{Smap["Name"]}Name{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.TrinaryColor };

        sortChar = ""; if (newTest == $"{Smap["Level"]}Level") { if (tableSort.Contains("ASC")) { sortChar = " " + upChar; } if (tableSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var levelH = new Label { Text = $"{Smap["Level"]}Level{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.TrinaryColor };

        sortChar = ""; if (newTest == $"{Smap["Class"]}Class") { if (tableSort.Contains("ASC")) { sortChar = " " + upChar; } if (tableSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var classH = new Label { Text = $"{Smap["Class"]}Class{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = Shared.TrinaryColor };


        // Add header row
        AddToGrid(grid, idH, 0, 0);
        AddToGrid(grid, tagH, 0, 1);
        AddToGrid(grid, nameH, 0, 2);
        AddToGrid(grid, levelH, 0, 3);
        AddToGrid(grid, classH, 0, 4);

        // make columns sortable
        SortColumn(idH);
        SortColumn(tagH);
        SortColumn(nameH);
        SortColumn(levelH);
        SortColumn(classH);

        int rowIndex = 1; // Start adding rows below the header

        foreach (DataRow row in table.Rows)
        {
            boxRowID++;

            string id = row["ID"].ToString();
            string tag = row["Tag"].ToString();
            string name = row["Name"].ToString();
            string level = row["Level"].ToString();
            string dinoclass = row["Class"].ToString();

            // Set color based on sex
            Color cellColor = Shared.bottomColor;
            string sex = DataManager.GetLastColumnData("ID", id, "Sex");
            if (sex == "Female") { cellColor = Shared.femaleColor; }
            else { cellColor = Shared.maleColor; }

            // Create Labels
            var idL = new Label { Text = id, TextColor = cellColor };
            var tagL = new Label { Text = tag, TextColor = cellColor };
            var nameL = new Label { Text = name, TextColor = cellColor };
            var levelL = new Label { Text = level, TextColor = cellColor };
            var classL = new Label { Text = dinoclass, TextColor = cellColor };

            // Make labels selectable
            SelectDino(idL, id, boxRowID);
            SelectDino(tagL, id, boxRowID);
            SelectDino(nameL, id, boxRowID);
            SelectDino(levelL, id, boxRowID);
            SelectDino(classL, id, boxRowID);

            bool selected = false;
            if (id == selectedID) { selected = true; }

            // add items to grid
            AddToGrid(grid, idL, rowIndex, 0, selected, id);
            AddToGrid(grid, tagL, rowIndex, 1, selected, id);
            AddToGrid(grid, nameL, rowIndex, 2, selected, id);
            AddToGrid(grid, levelL, rowIndex, 3, selected, id);
            AddToGrid(grid, classL, rowIndex, 4, selected, id);

            rowIndex++;
        }

        return grid;
    }

    void SortColumn(Label label)
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

            var splitA = sortA.Split(new[] { @" " }, StringSplitOptions.RemoveEmptyEntries);

            string outA = "";

            if (splitA.Length > 0)
            {
                outA = splitA[0];
            }

            // are we clicking the same column then toggle sorting
            if (outA == column)
            {
                if (sortA.Contains("ASC")) // then switch to descending
                {
                    sortA = column + " DESC";
                }
                else if (sortA.Contains("DESC")) // finally turn it off
                {
                    sortA = "";
                }
            }
            else // first sort ascending
            {
                sortA = column + " ASC";
            }

            FileManager.Log($"Sorted: {sortA}", 0);

            ClearSelection();
            CreateContent();
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture1);
    }

    private void ClearSelection()
    {
        if (selectedID != "")
        {
            FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = Shared.setPage;

            PurgeBtn.IsVisible = false;
            ArchiveBtn.IsVisible = false;

            // recolor all rows to default
            DefaultRowColors();
        }
    }

    private async Task PurgeDinoAsync(string dinoID)
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

                    DataManager.DeleteRowsByID(dinoID);
                    ClearSelection();
                    CreateContent();
                }
                catch { FileManager.Log("Failed Purging dino", 2); }
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

    private async Task PurgeAllAsync()
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
                    ClearSelection();
                    CreateContent();
                }
                catch
                {
                    FileManager.Log("Failed Purging all dinos", 2);
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

    private void DefaultRowColors()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (boxViews.Count > 0)
                {
                    int rowsT = boxViews.Count;

                    for (int i = 0; i < rowsT; i++) // color all male rows
                    {
                        // start coloring the rows with Solid color
                        boxViews[i].Color = i % 2 == 0 ? OddAPanelColor : ArchivePanelColor;
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



    // Button event handlers
    private void SelectDino(Label label, string id, int boxid = 0)
    {
        label.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (selectedID != id) // dont select the same dino twice
            {
                selectedID = id;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Selected {name} ID: {id}", 0); isSelected = true;


                // recolor all rows to default
                DefaultRowColors();

                boxViews[boxid].Color = SelectedColor;

                // make buttons visible
                PurgeBtn.IsVisible = true;
                ArchiveBtn.IsVisible = true;

                // CreateContent();
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
            if (selectedID != id) // dont select the same dino twice
            {
                selectedID = id;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                FileManager.Log($"Selected {name} ID: {id}", 0); isSelected = true;

                // recolor all rows to default
                DefaultRowColors();

                inp.Color = SelectedColor;

                // make buttons visible
                PurgeBtn.IsVisible = true;
                ArchiveBtn.IsVisible = true;

                //CreateContent();
            }
        };

        // Attach the TapGestureRecognizer to the label
        inp.GestureRecognizers.Add(tapGesture);
    }

    private void UnSelectDino(Grid grid)
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

    private void ArchiveBtnClicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            // Handle the click event

            DataManager.SetGroup(selectedID, "");
            FileManager.Log($"Restored ID: {selectedID}", 0);

            // recompile the archive after archiving or unarchiving
            DataManager.CompileDinoArchive(sortA);

            ClearSelection();
            CreateContent();
        }
    }

    private void PurgeBtnClicked(object? sender, EventArgs e)
    {
        PurgeDinoAsync(selectedID); // passing id here already to not loose it to updates
    }

    private void PurgeAllBtnClicked(object? sender, EventArgs e)
    {
        PurgeAllAsync();
    }

}