using System.Data;

namespace ASA_Dino_Manager;

public partial class ArchivePage : ContentPage
{
    public ArchivePage()
    {
        InitializeComponent();

        // get the route
        var route = Shell.Current.CurrentState.Location.ToString();
        route = route.Replace("/", "");
        Vars.setRoute = route;


        // unselect any dinos
        if (Vars.selectedID != "")
        {
            FileManager.Log($"Unselected {Vars.selectedID}", 0);
            Vars.selectedID = "";
            Vars.showStats = false;
        }


        CreateContent();
    }


    public void CreateContent()
    {
        if (Monitor.TryEnter(Vars._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                FileManager.Log("Updating GUI -> " + Vars.setRoute, 0);



                if (Vars.setRoute == "Archive")
                {
                    if (!Vars.showStats) { this.Title = Vars.setRoute; }

                    // recompile the archive after archiving or unarchiving
                    DataManager.CompileDinoArchive();

                    if (DataManager.ArchiveTable.Rows.Count > 0)
                    {
                        ArchiveView();
                    }
                    else
                    {
                        DefaultView("No dinos in here :(");
                    }
                }


            }
            finally
            {
                Monitor.Exit(Vars._dbLock);
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

        var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Vars.okColor, FontSize = 22 };


        AddToGrid(mainLayout, image1, 0, 0);
        AddToGrid(mainLayout, label1, 1, 0);




        // Wrap the scrollable content in a ScrollView and add it to the second row
        var scrollView = new ScrollView { Content = scrollContent };

        AddToGrid(mainLayout, scrollView, 0, 0);


        // only attach the tapgesture if we have something selected
        if (Vars.selectedID != "")
        {
            AppShell.UnSelectDino(mainLayout);
        }

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


        // only attach the tapgesture if we have something selected
        if (Vars.selectedID != "")
        {
            AppShell.UnSelectDino(mainLayout);
        }

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

            topButton0.Clicked += AppShell.OnButton0Clicked;
            topButton1.Clicked += AppShell.OnButton1Clicked;

            if (Vars.showStats) // add theese only if we have a dino selected
            {
                var topButton2 = new Button { Text = btn2Text, BackgroundColor = bColor2 };
                topButton2.Clicked += AppShell.OnButton2Clicked;
                AddToGrid(grid, topButton2, 2, 0);


                var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
                topButton3.Clicked += AppShell.OnButton3Clicked;
                AddToGrid(grid, topButton3, 6, 0);
            }
        }
        else // show extra buttons in archive
        {
            if (Vars.showStats) // add theese only if we have a dino selected
            {
                var topButton3 = new Button { Text = btn3Text, BackgroundColor = bColor3 };
                topButton3.Clicked += AppShell.OnButton3Clicked;
                AddToGrid(grid, topButton3, 0, 0);


                var topButton4 = new Button { Text = "Purge", BackgroundColor = Vars.dangerColor };
                topButton4.Clicked += AppShell.OnButton4Clicked;
                AddToGrid(grid, topButton4, 5, 0);
            }

            var topButton5 = new Button { Text = "Purge All", BackgroundColor = Vars.dangerColor };
            topButton5.Clicked += AppShell.OnButton5Clicked;
            AddToGrid(grid, topButton5, 6, 0);
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
        int rowHeight = 20;
        int barH = (rowCount * rowHeight) + rowHeight + 11;
        if (rowCount > 5) { barH = 127; }

        if (Vars.ToggleExcluded == 3 && !Vars.showStats) { barH = 0; }
        if (Vars.ToggleExcluded == 2 && !Vars.showStats) { barH = 0; }


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
            AppShell.SelectDino(idL, id);
            AppShell.SelectDino(tagL, id);
            AppShell.SelectDino(nameL, id);
            AppShell.SelectDino(levelL, id);

            // add items to grid
            AddToGrid(grid, idL, rowIndex, 0);
            AddToGrid(grid, tagL, rowIndex, 1);
            AddToGrid(grid, nameL, rowIndex, 2);
            AddToGrid(grid, levelL, rowIndex, 3);

            rowIndex++;
        }

        return grid;
    }


}