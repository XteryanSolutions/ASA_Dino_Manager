using System.Data;
using static ASA_Dino_Manager.Shared;

namespace ASA_Dino_Manager;

public partial class BabyPage : ContentPage
{
    ////////////////////    View Toggles    ////////////////////
    public static int ToggleExcluded = Shared.DefaultToggleB;

    ////////////////////    Selecting       ////////////////////
    public static string selectedID = "";
    public static bool isSelected = false;
    public static bool canDouble = false;
    public static bool isDouble = false;

    ////////////////////    Table Sorting   ////////////////////
    public static string sortM = Shared.DefaultSortM;
    public static string sortF = Shared.DefaultSortF;


    private string levelText = "";
    private string hpText = "";
    private string staminaText = "";
    private string oxygenText = "";
    private string foodText = "";
    private string weightText = "";
    private string damageText = "";
    private string notesText = "";

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
                    DataManager.GetDinoBabies(sortM, sortF);
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

        // Add side panel to left column
        AddToGrid(mainLayout, CreateSidePanel(), 0, 0);

        // Add main panel to right column
        AddToGrid(mainLayout, CreateMainPanel(), 0, 1);

        // don't unselect dino while editing stats
        if (!isDouble && isSelected)
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

        string toggleBtnText = "Toggle";
        if (ToggleExcluded == 0) { toggleBtnText = "All"; }
        else if (ToggleExcluded == 1) { toggleBtnText = "Included"; }
        else if (ToggleExcluded == 2) { toggleBtnText = "Excluded"; }
        else if (ToggleExcluded == 3) { toggleBtnText = "Archived"; }

        string excludeBtnText = "Exclude"; var excludeBtnColor = Shared.SecondaryColor;
        string archiveBtnText = "Archive"; var archiveBtnColor = Shared.TrinaryColor;

        string group = DataManager.GetGroup(selectedID);
        if (group == "Exclude") { excludeBtnText = "Include"; excludeBtnColor = Shared.PrimaryColor; }
        if (group == "Archived") { archiveBtnText = "Restore"; archiveBtnColor = Shared.PrimaryColor; }

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
        }

        if (isSelected) // add theese only if we have a dino selected
        {
            // do not show exclude button while in archive view
            if (ToggleExcluded != 3)
            {
                var excludeBtn = new Button { Text = excludeBtnText, BackgroundColor = excludeBtnColor };
                excludeBtn.Clicked += ExcludeBtnClicked;
                AddToGrid(grid, excludeBtn, 5, 0);
            }

            var archiveBtn = new Button { Text = archiveBtnText, BackgroundColor = archiveBtnColor };
            archiveBtn.Clicked += ArchiveBtnClicked;
            AddToGrid(grid, archiveBtn, 6, 0);
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

            // Get info about selected dino
            string currentID = selectedID;
            string sex = DataManager.GetLastColumnData("ID", currentID, "Sex");


            Color DefaultColor = Shared.maleColor;

            if (sex == "Male") { DefaultColor = Shared.maleColor; }
            else if (sex == "Female") { DefaultColor = Shared.femaleColor; }

            var cellColor0 = DefaultColor;
            var cellColor1 = DefaultColor;
            var cellColor2 = DefaultColor;
            var cellColor3 = DefaultColor;
            var cellColor4 = DefaultColor;
            var cellColor5 = DefaultColor;
            var cellColor6 = DefaultColor;
            var cellColor7 = DefaultColor;

            // get the current breed stats of selected dino
            string sep = DataManager.DecimalSeparator;

            string level = DataManager.GetFirstColumnData("ID", currentID, "Level").Replace(".", sep);
            string hp = DataManager.GetFirstColumnData("ID", currentID, "Hp").Replace(".", sep);
            string stamina = DataManager.GetFirstColumnData("ID", currentID, "Stamina").Replace(".", sep);
            string oxygen = DataManager.GetFirstColumnData("ID", currentID, "Oxygen").Replace(".", sep);
            string food = DataManager.GetFirstColumnData("ID", currentID, "Food").Replace(".", sep);
            string weight = DataManager.GetFirstColumnData("ID", currentID, "Weight").Replace(".", sep);
            string damage = DataManager.GetFirstColumnData("ID", currentID, "Damage").Replace(".", sep);


            //set the temp variables
            levelText = level;
            hpText = hp;
            staminaText = stamina;
            oxygenText = oxygen;
            foodText = food;
            weightText = weight;
            damageText = damage;


            //recolor stats (use -0.1 to account for rounding)
            if (DataManager.ToDouble(level) >= (DataManager.LevelMax - 0.1)) { cellColor1 = Shared.goodColor; }
            if (DataManager.ToDouble(hp) >= DataManager.HpMax - 0.1) { cellColor2 = Shared.goodColor; }
            if (DataManager.ToDouble(stamina) >= DataManager.StaminaMax - 0.1) { cellColor3 = Shared.goodColor; }
            if (DataManager.ToDouble(oxygen) >= DataManager.OxygenMax - 0.1) { cellColor4 = Shared.goodColor; }
            if (DataManager.ToDouble(food) >= DataManager.FoodMax - 0.1) { cellColor5 = Shared.goodColor; }
            if (DataManager.ToDouble(weight) >= DataManager.WeightMax - 0.1) { cellColor6 = Shared.goodColor; }
            if ((DataManager.ToDouble(damage) + 1) * 100 >= DataManager.DamageMax - 0.1) { cellColor7 = Shared.goodColor; }


            // mutation detection overrides normal coloring -> mutaColor
            string mutes = DataManager.GetMutes(currentID);
            if (mutes.Length == 6) // dont show mutations on current statview
            {
                string aC = mutes.Substring(0, 1); string bC = mutes.Substring(1, 1); string cC = mutes.Substring(2, 1);
                string dC = mutes.Substring(3, 1); string eC = mutes.Substring(4, 1); string fC = mutes.Substring(5, 1);
                //   string gC = mutes.Substring(6, 1); string hC = mutes.Substring(7, 1);

                if (aC == "1") { cellColor2 = Shared.mutaColor; }
                if (bC == "1") { cellColor3 = Shared.mutaColor; }
                if (cC == "1") { cellColor4 = Shared.mutaColor; }
                if (dC == "1") { cellColor5 = Shared.mutaColor; }
                if (eC == "1") { cellColor6 = Shared.mutaColor; }
                if (fC == "1") { cellColor7 = Shared.mutaColor; }


            }



            // add stat text
            var t0 = new Label { Text = "", Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.maleColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold };
            var t1 = new Label { Text = "Level", TextColor = cellColor1, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t2 = new Label { Text = "Hp", TextColor = cellColor2, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t3 = new Label { Text = "Stamina", TextColor = cellColor3, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t4 = new Label { Text = "Oxygen", TextColor = cellColor4, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t5 = new Label { Text = "Food", TextColor = cellColor5, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t6 = new Label { Text = "Weight", TextColor = cellColor6, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var t7 = new Label { Text = "Damage", TextColor = cellColor7, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };


            int rowid = 0;
            int colid = 0;
            AddToGrid(statGrid, t0, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t1, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t2, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t3, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t4, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t5, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t6, rowid++, colid, "", false, true);
            AddToGrid(statGrid, t7, rowid++, colid, "", false, true);




            var editLabel = new Label
            {
                Text = "Breeding Stats",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Style = (Style)Application.Current.Resources["Headline"],
                TextColor = cellColor0,
                FontSize = Shared.fontHSize,
                FontAttributes = FontAttributes.Bold
            };

            var textBox1 = new Entry { Text = level, Placeholder = "Level", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor1, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox2 = new Entry { Text = hp, Placeholder = "Hp", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor2, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox3 = new Entry { Text = stamina, Placeholder = "Stamina", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor3, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox4 = new Entry { Text = oxygen, Placeholder = "Oxygen", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor4, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox5 = new Entry { Text = food, Placeholder = "Food", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor5, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox6 = new Entry { Text = weight, Placeholder = "Weight", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor6, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };
            var textBox7 = new Entry { Text = damage, Placeholder = "Damage", WidthRequest = 200, HeightRequest = 10, TextColor = cellColor7, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };

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



            // AddToGrid(grid1, imageContainer, 0, 1);

            rowid = 0;
            colid = 1;

            AddToGrid(statGrid, editLabel, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox1, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox2, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox3, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox4, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox5, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox6, rowid++, colid, "", false, true);
            AddToGrid(statGrid, textBox7, rowid++, colid, "", false, true);


            // get parents id

            string papaID = DataManager.GetLastColumnData("ID", currentID, "Papa");
            string mamaID = DataManager.GetLastColumnData("ID", currentID, "Mama");

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


            DefaultColor = Shared.maleColor;


            cellColor1 = DefaultColor;
            cellColor2 = DefaultColor;
            cellColor3 = DefaultColor;
            cellColor4 = DefaultColor;
            cellColor5 = DefaultColor;
            cellColor6 = DefaultColor;
            cellColor7 = DefaultColor;


            //recolor stats (use -0.1 to account for rounding)
            if (DataManager.ToDouble(levelP) >= (DataManager.LevelMax - 0.1)) { cellColor1 = Shared.goodColor; }
            if (DataManager.ToDouble(hpP) >= DataManager.HpMax - 0.1) { cellColor2 = Shared.goodColor; }
            if (DataManager.ToDouble(staminaP) >= DataManager.StaminaMax - 0.1) { cellColor3 = Shared.goodColor; }
            if (DataManager.ToDouble(oxygenP) >= DataManager.OxygenMax - 0.1) { cellColor4 = Shared.goodColor; }
            if (DataManager.ToDouble(foodP) >= DataManager.FoodMax - 0.1) { cellColor5 = Shared.goodColor; }
            if (DataManager.ToDouble(weightP) >= DataManager.WeightMax - 0.1) { cellColor6 = Shared.goodColor; }
            if ((DataManager.ToDouble(damageP) + 1) * 100 >= DataManager.DamageMax - 0.1) { cellColor7 = Shared.goodColor; }


            // add papa stats
            var papaH = new Label { Text = papaName, Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.maleColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold };
            var labelP1 = new Label { Text = levelP, TextColor = cellColor1, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP2 = new Label { Text = hpP, TextColor = cellColor2, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP3 = new Label { Text = staminaP, TextColor = cellColor3, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP4 = new Label { Text = oxygenP, TextColor = cellColor4, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP5 = new Label { Text = foodP, TextColor = cellColor5, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP6 = new Label { Text = weightP, TextColor = cellColor6, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelP7 = new Label { Text = damageP, TextColor = cellColor7, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };


            rowid = 0;
            colid = 2;
            AddToGrid(statGrid, papaH, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP1, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP2, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP3, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP4, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP5, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP6, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelP7, rowid++, colid, "", false, true);


            string levelM = DataManager.GetFirstColumnData("ID", mamaID, "Level").Replace(".", sep);
            string hpM = DataManager.GetFirstColumnData("ID", mamaID, "Hp").Replace(".", sep);
            string staminaM = DataManager.GetFirstColumnData("ID", mamaID, "Stamina").Replace(".", sep);
            string oxygenM = DataManager.GetFirstColumnData("ID", mamaID, "Oxygen").Replace(".", sep);
            string foodM = DataManager.GetFirstColumnData("ID", mamaID, "Food").Replace(".", sep);
            string weightM = DataManager.GetFirstColumnData("ID", mamaID, "Weight").Replace(".", sep);
            string damageM = DataManager.GetFirstColumnData("ID", mamaID, "Damage").Replace(".", sep);



            DefaultColor = Shared.femaleColor;

            cellColor1 = DefaultColor;
            cellColor2 = DefaultColor;
            cellColor3 = DefaultColor;
            cellColor4 = DefaultColor;
            cellColor5 = DefaultColor;
            cellColor6 = DefaultColor;
            cellColor7 = DefaultColor;


            //recolor stats (use -0.1 to account for rounding)
            if (DataManager.ToDouble(levelM) >= (DataManager.LevelMax - 0.1)) { cellColor1 = Shared.goodColor; }
            if (DataManager.ToDouble(hpM) >= DataManager.HpMax - 0.1) { cellColor2 = Shared.goodColor; }
            if (DataManager.ToDouble(staminaM) >= DataManager.StaminaMax - 0.1) { cellColor3 = Shared.goodColor; }
            if (DataManager.ToDouble(oxygenM) >= DataManager.OxygenMax - 0.1) { cellColor4 = Shared.goodColor; }
            if (DataManager.ToDouble(foodM) >= DataManager.FoodMax - 0.1) { cellColor5 = Shared.goodColor; }
            if (DataManager.ToDouble(weightM) >= DataManager.WeightMax - 0.1) { cellColor6 = Shared.goodColor; }
            if ((DataManager.ToDouble(damageM) + 1) * 100 >= DataManager.DamageMax - 0.1) { cellColor7 = Shared.goodColor; }


            // add mama stats
            var mamaH = new Label { Text = mamaName, Style = (Style)Application.Current.Resources["Headline"], TextColor = Shared.femaleColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold };

            var labelM1 = new Label { Text = levelM, TextColor = cellColor1, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM2 = new Label { Text = hpM, TextColor = cellColor2, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM3 = new Label { Text = staminaM, TextColor = cellColor3, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM4 = new Label { Text = oxygenM, TextColor = cellColor4, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM5 = new Label { Text = foodM, TextColor = cellColor5, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM6 = new Label { Text = weightM, TextColor = cellColor6, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelM7 = new Label { Text = damageM, TextColor = cellColor7, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };


            rowid = 0;
            colid = 3;
            AddToGrid(statGrid, mamaH, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM1, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM2, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM3, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM4, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM5, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM6, rowid++, colid, "", false, true);
            AddToGrid(statGrid, labelM7, rowid++, colid, "", false, true);

            scrollContent.Children.Add(statGrid);


            rowid = 0;
            var notesGrid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };
            // Define columns
            notesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0

            // get some stats for validation
            string papaMute = DataManager.GetLastColumnData("ID", currentID, "PapaMute");
            string mamaMute = DataManager.GetLastColumnData("ID", currentID, "MamaMute");
            string imprint = DataManager.GetLastColumnData("ID", currentID, "Imprint");
            string imprinter = DataManager.GetLastColumnData("ID", currentID, "Imprinter");

            string notes = DataManager.GetNotes(currentID);
            // notes textbox defined here
            var textBoxN = new Editor { Text = notes, Placeholder = "Notes", WidthRequest = 600, HeightRequest = 200, TextColor = cellColor0, BackgroundColor = Shared.OddMPanelColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, Keyboard = Keyboard.Create(KeyboardFlags.None) };

            textBoxN.TextChanged += (sender, e) =>
            {
                editStats = true;
                notesText = e.NewTextValue;
            };

            AddToGrid(notesGrid, textBoxN, rowid++, 0, "", false, true);


            scrollContent.Children.Add(notesGrid);

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

        if (newTest == $"{Smap["Class"]}Class") { if (testingSort.Contains("ASC")) { sortChar = " " + upChar; } if (testingSort.Contains("DESC")) { sortChar = " " + downChar; } }
        var tagH = new Label { Text = $"{Smap["Class"]}Class{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };
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


        int rowIndex = 1; // Start adding rows below the header

        foreach (DataRow row in table.Rows)
        {
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
            double ageD = Math.Round(DataManager.ToDouble(row["Hp"].ToString()), 1);
            string ageT = ageD + "%";
            string timeT = row["Stamina"].ToString();
            string rateT = row["Oxygen"].ToString();
            string dateT = row["Status"].ToString();

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

            string notes = DataManager.GetNotes(id);
            if (notes != "")
            {
                status = Smap["Notes"] + status;
            }

            // Create a Labels
            var tagL = new Label { Text = dinoClass, TextColor = cellColor0 };
            var nameL = new Label { Text = name, TextColor = cellColor0 };
            var levelL = new Label { Text = level, TextColor = cellColor1 };
            //////////////
            var hpL = new Label { Text = ageT, TextColor = cellColor2 };
            var staminaL = new Label { Text = timeT, TextColor = cellColor3 };
            var oxygenL = new Label { Text = rateT, TextColor = cellColor4 };
            var foodL = new Label { Text = dateT, TextColor = cellColor5 };
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
            SelectDino(tagL, id);
            SelectDino(nameL, id);
            SelectDino(levelL, id);
            SelectDino(hpL, id);
            SelectDino(staminaL, id);
            SelectDino(oxygenL, id);
            SelectDino(foodL, id);
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

            // Reset startID for new row
            startID = 0;

            // Add base items to the grid
            AddToGrid(grid, tagL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, nameL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, levelL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, hpL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, staminaL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, oxygenL, rowIndex, startID++, title, selected, false, id);
            AddToGrid(grid, foodL, rowIndex, startID++, title, selected, false, id);
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

    private void ClearSelection()
    {
        if (selectedID != "" && !isDouble)
        {
            //  FileManager.Log($"Unselected {selectedID}", 0);
            selectedID = ""; isSelected = false; this.Title = $"{Shared.setPage.Replace("_", " ")}";
            canDouble = false; editStats = false;
        }
    }

    private bool IsValidDouble(string input)
    {
        editStats = true;
        return double.TryParse(input, out _); // Returns true if the input is a valid double
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

    void SelectDino(Label label, string id)
    {
        label.GestureRecognizers.Clear();
        // Create a TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (selectedID != id) // select a new dino
            {
                selectedID = id; isSelected = true;

                string name = DataManager.GetLastColumnData("ID", selectedID, "Name");
                this.Title = $"{name} - {id}"; // set title to dino name

                // activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
            else if (selectedID == id && canDouble) // select same dino within time
            {
                // double click  // open the dino extended info window
                //isDouble = true; canDouble = false;
            }
            else if (selectedID == id && !canDouble) // select same dino over time
            {
                // re activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
            CreateContent();
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
            }
            else if (selectedID == id && !canDouble) // select same dino over time
            {
                // re activate double clicking
                canDouble = true;
                DisableDoubleClick();
            }
            CreateContent();
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
            CreateContent();
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
        levelText = ""; hpText = ""; staminaText = ""; oxygenText = "";
        foodText = ""; weightText = ""; damageText = ""; notesText = "";
        isDouble = false; editStats = false;

        ClearSelection();
        CreateContent();
    }

    private void SaveBtnClicked(object? sender, EventArgs e)
    {
        if (editStats)
        {
           // DataManager.EditBreedStats(selectedID, levelText, hpText, staminaText, oxygenText, foodText, weightText, damageText, notesText);
            FileManager.needSave = true;
            dataValid = false;
            editStats = false;
        }

        // reset toggles etc.
        levelText = ""; hpText = ""; staminaText = ""; oxygenText = "";
        foodText = ""; weightText = ""; damageText = ""; notesText = "";
        isDouble = false;
        ClearSelection();
        CreateContent();
    }


}