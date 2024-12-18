using System.Data;
using System.Xml.Linq;

namespace ASA_Dino_Manager;

public partial class ArchivePage : ContentPage
{
    ////////////////////    Selecting       ////////////////////
    private string selectedID = "";
    private bool isSelected = false;

    public ArchivePage()
    {
        InitializeComponent();

        FileManager.Log($"Loaded: {Shared.setPage}", 0);

        // set page title
        this.Title = $"{Shared.setPage}";
        CreateContent();
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

    public void CreateContent()
    {
        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {

            try
            {
                // recompile the archive after archiving or unarchiving
                DataManager.CompileDinoArchive();

                FileManager.Log("Updating GUI -> " + Shared.setPage, 0);

                if (!isSelected) { this.Title = Shared.setPage; }

                if (DataManager.ArchiveTable.Rows.Count > 0)
                {
                    ArchiveView();
                }
                else
                {
                    DefaultView("No dinos in here :(");
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

    private void ArchiveView()
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
        AddToGrid(mainLayout, CreateSidePanel(), 0, 0);

        // Add main panel to right column
        AddToGrid(mainLayout, CreateMainPanel(), 0, 1);

        // attach unselect event after all content has been created
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


        string status = DataManager.GetStatus(selectedID);


        string btn3Text = "Archive"; var bColor3 = Shared.dangerColor;

        if (status == "Archived") { btn3Text = "Restore"; bColor3 = Shared.okColor; }


        // add theese only if we have a dino selected
        if (isSelected)
        {
            var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
            topButton3.Clicked += OnButton3Clicked;
            AddToGrid(grid, topButton3, 0, 0);


            var topButton4 = new Button { Text = "Purge", BackgroundColor = Shared.dangerColor };
            topButton4.Clicked += OnButton4Clicked;
            AddToGrid(grid, topButton4, 5, 0);
        }

        var topButton5 = new Button { Text = "Purge All", BackgroundColor = Shared.dangerColor };
        topButton5.Clicked += OnButton5Clicked;
        AddToGrid(grid, topButton5, 6, 0);


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
        int barH = (rowCount * Shared.rowHeight) + Shared.rowHeight + 12;
        if (rowCount > 5) { barH = (Shared.rowHeight * 5) + Shared.rowHeight + 5; }

        if (!isSelected) { barH = 0; }


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

        Shared.headerColor = Shared.breedColor;
        Shared.DefaultColor = Shared.breedColor;

        // Add header row
        AddToGrid(grid, new Label { Text = "ID", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor }, 0, 0);
        AddToGrid(grid, new Label { Text = "Tag", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor }, 0, 1);
        AddToGrid(grid, new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor }, 0, 2);
        AddToGrid(grid, new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor }, 0, 3);
        AddToGrid(grid, new Label { Text = "", FontAttributes = FontAttributes.Bold, TextColor = Shared.headerColor }, 0, 4);

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
                Shared.DefaultColor = Shared.femaleColor;
            }
            else
            {
                Shared.DefaultColor = Shared.maleColor;
            }

            var cellColor0 = Shared.DefaultColor;
            var cellColor1 = Shared.DefaultColor;
            var cellColor2 = Shared.DefaultColor;
            var cellColor3 = Shared.DefaultColor;
            var cellColor4 = Shared.DefaultColor;

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

    private void ClearSelection()
    {
        if (selectedID != "")
        {
            FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = Shared.setPage;
        }
    }


    // Button event handlers
    private void SelectDino(Label label, string id)
    {
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

                CreateContent();
            }
        };

        // Attach the TapGestureRecognizer to the label
        label.GestureRecognizers.Add(tapGesture);
    }

    private void UnSelectDino(Grid grid)
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
            DataManager.CompileDinoArchive();

            ClearSelection();
            CreateContent();
        }
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
                    DataManager.DeleteRowsByID(selectedID);
                    ClearSelection();
                    CreateContent();
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