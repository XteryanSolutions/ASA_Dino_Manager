using Microsoft.Maui;
using System.Data;
using System.Diagnostics;
using static Ark_Dino_Manager.DataManager;
using static Ark_Dino_Manager.Shared;

namespace Ark_Dino_Manager;

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
    public static bool showTree = false;


    // keep track of boxviews for recoloring
    private Dictionary<int, BoxView> boxViews = new Dictionary<int, BoxView>();
    private int boxID = 0;
    private int boxRowID = 0;

    Button ExcludeBtn = new Button { };
    Button ArchiveBtn = new Button { };

    ////////////////////    Table Sorting   ////////////////////
    public static string sortM = Shared.DefaultSortM;
    public static string sortF = Shared.DefaultSortF;

    private string levelText = "";
    private string hpText = "";
    private string staminaText = "";
    private string O2Text = "";
    private string foodText = "";
    private string weightText = "";
    private string damageText = "";
    private string notesText = "";
    private string speedText = "";
    private string craftText = "";
    private string regenText = "";
    private string capacityText = "";
    private string emissionText = "";

    ////////////////////    Data   ////////////////////
    private bool editStats = false;
    private bool dataValid = false;
    private int dinoCount = 0;

    private bool hasO2 = false;
    private bool hasSpeed = false;
    private bool hasCraft = false;
    private bool hasCharge = false;
    private bool hasStamina = false;
    private bool hasDamage = false;
    private bool hasEmission = false;

    private Stopwatch timer1 = Stopwatch.StartNew();


    public DinoPage()
    {
        InitializeComponent();

        dataValid = false;
        CreateContent();
    }

    private void FromHere()
    {
        timer1 = Stopwatch.StartNew();
    }

    private void ToHere(string text)
    {
        timer1.Stop();
        var elapsedMilliseconds = timer1.Elapsed.TotalMilliseconds;
        Shared.loadCount++; double outAVG = 0;
        if (Shared.loadCount < 2) { Shared.loadAvg = elapsedMilliseconds; outAVG = Shared.loadAvg; }
        else { Shared.loadAvg += elapsedMilliseconds; outAVG = Shared.loadAvg / Shared.loadCount; }
        FileManager.Log($"{text}: {elapsedMilliseconds}ms Avg: {outAVG}", 0);
    }


    public void CreateContent()
    {
        FromHere(); // Start benchmark timer here
        FileManager.Log("Updating GUI -> " + Shared.setPage, 0);
        if (!isSelected) { this.Title = $"{Shared.setPage.Replace("_", " ")}"; }

        if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
        {
            try
            {
                if (!string.IsNullOrEmpty(Shared.selectedClass))
                {
                    if (!dataValid)
                    {
                        FileManager.Log("Loading All Data", 0);
                        // sort data based on column clicked
                        DataManager.GetDinoData(Shared.selectedClass, sortM, sortF, ToggleExcluded, CurrentStats);

                        // check for sats we dont need
                        if (DataManager.StaminaMax == 0) { hasStamina = false; } else { hasStamina = true; }
                        if (DataManager.O2Max == 0 || DataManager.O2Max == 150) { hasO2 = false; } else { hasO2 = true; }
                        if (DataManager.CraftMax == 0 || DataManager.CraftMax == 100) { hasCraft = false; } else { hasCraft = true; }
                        if (DataManager.RegenMax == 0 || DataManager.RegenMax == 100) { hasCharge = false; } else { hasCharge = true; }
                        if (DataManager.DamageMax == 0 || DataManager.DamageMax == 100) { hasDamage = false; } else { hasDamage = true; }
                        if (DataManager.EmissionMax == 0 || DataManager.EmissionMax == 100) { hasEmission = false; } else { hasEmission = true; }


                        // load this data only when showing all and included
                        if (ToggleExcluded == 0 || ToggleExcluded == 1 || ToggleExcluded == 2)
                        {
                            DataManager.EvaluateDinos();
                            if (!CurrentStats && !showTree)
                            {
                                DataManager.GetBestPartner();
                            }
                        }
                        dinoCount = DataManager.DinoCount(Shared.selectedClass, ToggleExcluded);
                        dataValid = true;
                    }
                }

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
        ToHere("Time1"); // Stop timer and show results
    }

    private void DefaultView(string labelText, string imageSource = "bigegg.png")
    {
        var mainLayout = new Grid();

        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
        mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


        var scrollContent = new StackLayout
        {
            Spacing = 20,
            Padding = 3
        };

        var image1 = new Image { Source = imageSource, HeightRequest = 400, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start };
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


        mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = sidePanelSize }); // 0 // width of the sidepanel
        mainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        // reset boxViews
        boxID = 0; boxRowID = 0;
        boxViews = new Dictionary<int, BoxView>();

        // Add side panel to left column
        AddToGrid(mainLayout, CreateSidePanel(), 0, 0);

        // Add main panel to right column
        AddToGrid(mainLayout, CreateMainPanel(), 0, 1);

        // only attach the tapgesture if we have something selected
        if (!isDouble)
        {
            UnSelectDino(mainLayout);
        }


        this.Content = mainLayout;
    }

    private void AddToGrid(Grid grid, View view, int row, int column, string title = "", bool selected = false, bool isDoubl = false, string id = "")
    {
        Color c1 = Shared.MainPanelColor;
        Color c1_o = Shared.MainPanelColor;

        Color c2 = Shared.BottomPanelColor;

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

            if (title != "Bottom")
            {
                boxViews[boxID++] = rowBackground;
            }

            // make background on row selectable to increase surface area
            if (title != "Bottom") // not the bottom panel
            {
                if (id != "") // only when an id is passed
                {
                    SelectBG(rowBackground, id);
                }
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
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Scrollable content

        var ToggleBtnColor = Shared.DefaultBColor;
        var StatsBtnColor = Shared.PrimaryColor;

        if (ToggleExcluded == 0)
        {
            ToggleBtnColor = Shared.DefaultBColor;
        }
        else if (ToggleExcluded == 1)
        {
            ToggleBtnColor = Shared.PrimaryColor;
        }
        else if (ToggleExcluded == 2)
        {
            ToggleBtnColor = Shared.SecondaryColor;
        }
        else if (ToggleExcluded == 3)
        {
            ToggleBtnColor = Shared.TrinaryColor;
        }

        string ToggleBtnText = "Toggle";
        if (ToggleExcluded == 0) { ToggleBtnText = Smap["All"]; }
        else if (ToggleExcluded == 1) { ToggleBtnText = Smap["Include"]; }
        else if (ToggleExcluded == 2) { ToggleBtnText = Smap["Exclude"]; }


        string StatsBtnText = Smap["Breeding"];
        if (CurrentStats) { StatsBtnText = Smap["Current"]; StatsBtnColor = Shared.SecondaryColor; }


        var BackBtn = new Button { Text = Smap["Back"], BackgroundColor = Shared.PrimaryColor, FontSize = buttonFontSize };

        if (isDouble)
        {
            var SaveBtn = new Button { Text = Smap["Save"], BackgroundColor = Shared.TrinaryColor, FontSize = buttonFontSize };
            SaveBtn.Clicked += SaveBtnClicked;
            AddToGrid(grid, SaveBtn, 0, 0);

            BackBtn.Clicked += BackBtnClicked;
            AddToGrid(grid, BackBtn, 1, 0);
        }
        else if (showTree)
        {
            BackBtn.Clicked += BackBtnClicked;
            AddToGrid(grid, BackBtn, 0, 0);
        }
        else
        {
            var ToggleBtn = new Button { Text = ToggleBtnText, BackgroundColor = ToggleBtnColor, FontSize = buttonFontSize };
            ToggleBtn.Clicked += ToggleBtnClicked;
            AddToGrid(grid, ToggleBtn, 0, 0);


            var StatsBtn = new Button { Text = StatsBtnText, BackgroundColor = StatsBtnColor, FontSize = buttonFontSize };
            StatsBtn.Clicked += StatsBtnClicked;
            AddToGrid(grid, StatsBtn, 1, 0);
        }

        if (!showTree && !isDouble)
        {
            var TreeBtn = new Button { Text = Smap["Heritage"], BackgroundColor = DefaultBColor, FontSize = buttonFontSize };
            TreeBtn.Clicked += TreeBtnClicked;
            AddToGrid(grid, TreeBtn, 3, 0);
        }

        // ExcludeBtn.Text = "Include";

        ExcludeBtn = new Button { Text = "", FontSize = buttonFontSize };
        ExcludeBtn.Clicked += ExcludeBtnClicked;
        AddToGrid(grid, ExcludeBtn, 5, 0);

        ArchiveBtn = new Button { Text = "", FontSize = buttonFontSize };
        ArchiveBtn.Clicked += ArchiveBtnClicked;
        AddToGrid(grid, ArchiveBtn, 6, 0);

        string group = DataManager.GetGroup(selectedID);
        if (group == "Exclude") { ExcludeBtn.Text = Smap["Include"]; ExcludeBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ExcludeBtn.Text = Smap["Exclude"]; ExcludeBtn.BackgroundColor = Shared.SecondaryColor; }

        if (group == "Archived") { ArchiveBtn.Text = Smap["Include"]; ArchiveBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ArchiveBtn.Text = Smap["Archive"]; ArchiveBtn.BackgroundColor = Shared.TrinaryColor; }

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

    private Label EditHeader(string headerText, Color fontColor)
    {
        return new Label { Text = headerText, Style = (Style)Application.Current.Resources["Headline"], TextColor = fontColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold };
    }

    private Label EditRowLabel(string rowText, Color fontColor, string what)
    {
        string sep = DataManager.DecimalSeparator;
        string tester = rowText.Replace("O2", "Oxygen");
        tester = tester.Replace("Regen", "ChargeRegen");
        tester = tester.Replace("Capacity", "ChargeCapacity");

        string papaID = DataManager.GetLastColumnData("ID", selectedID, "Papa");
        string mamaID = DataManager.GetLastColumnData("ID", selectedID, "Mama");

        string mamaName = DataManager.GetLastColumnData("ID", mamaID, "Name");
        string papaName = DataManager.GetLastColumnData("ID", papaID, "Name");

        string value = "";
        if (what == "D")
        {
            value = DataManager.GetFirstColumnData("ID", selectedID, tester).Replace(".", sep);
        }
        if (what == "P")
        {
            value = DataManager.GetFirstColumnData("ID", papaID, tester).Replace(".", sep);
            fontColor = Shared.maleColor;
        }
        if (what == "M")
        {
            value = DataManager.GetFirstColumnData("ID", mamaID, tester).Replace(".", sep);
            fontColor = Shared.femaleColor;
        }

        double outV = DataManager.ToDouble(value);
        double tesValue = 0; bool tes = true;
        if (rowText == "Level") { tesValue = DataManager.LevelMax; tes = false; } // disable recoloring
        if (rowText == "Hp") { tesValue = DataManager.HpMax; }
        if (rowText == "Stamina") { tesValue = DataManager.StaminaMax; if (!hasStamina) { tes = false; } }
        if (rowText == "O2") { tesValue = DataManager.O2Max; if (!hasO2) { tes = false; } }
        if (rowText == "Food") { tesValue = DataManager.FoodMax; }
        if (rowText == "Weight") { tesValue = DataManager.WeightMax; }
        if (rowText == "Damage") { tesValue = DataManager.DamageMax; if (outV != 0) { outV = (outV + 1) * 100; } }
        if (rowText == "Speed") { tesValue = DataManager.SpeedMax;  outV = (outV + 1) * 100; tes = false; } // disable recoloring
        if (rowText == "CraftSkill") { tesValue = DataManager.CraftMax; if (!hasCraft) { tes = false; } if (outV != 0) { outV = (outV + 1) * 100; } }
        if (rowText == "Regen") { tesValue = DataManager.RegenMax; if (!hasCharge) { tes = false; } }
        if (rowText == "Capacity") { tesValue = DataManager.CapacityMax; if (!hasCharge) { tes = false; } }
        if (rowText == "Emission") { tesValue = DataManager.EmissionMax; if (!hasEmission) { tes = false; } if (outV != 0) { outV = (outV + 1) * 100; } }

        if (what != "D")
        {
            if (mamaID == "" || mamaID == "N/A" || mamaName == "N/A" || mamaName == "") { outV = 0; tes = false; } // set label to 0 and no recoloring if there is no mama
            if (papaID == "" || papaID == "N/A" || papaName == "N/A" || papaName == "") { outV = 0; tes = false; } // set label to 0 and no recoloring if there is no mama
        }

        if (tes)
        {
            if ((outV + statViewOffset) >= tesValue) { fontColor = Shared.goodColor; }

            // mutation detection overrides normal coloring -> mutaColor
            string mutes = DataManager.GetMutes(selectedID);
            if (mutes.Length >= 10 && what == "D")
            {
                if (rowText == "Hp") { if (mutes.Substring(0, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(0, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Stamina") { if (mutes.Substring(1, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(1, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "O2") { if (mutes.Substring(2, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(2, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Food") { if (mutes.Substring(3, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(3, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Weight") { if (mutes.Substring(4, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(4, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Damage") { if (mutes.Substring(5, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(5, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "CraftSkill") { if (mutes.Substring(6, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(6, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Regen") { if (mutes.Substring(7, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(7, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Capacity") { if (mutes.Substring(8, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(8, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Emission") { if (mutes.Substring(9, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(9, 1) == "2") { fontColor = Shared.mutaBadColor; } }

            }
        }

        if (what != "D")
        {
            rowText = outV.ToString();
        }

        Label outLabel = new Label { Text = rowText, TextColor = fontColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };

        return outLabel;
    }

    private Entry EditRowBox(string rowText, Color fontColor)
    {
        string sep = DataManager.DecimalSeparator;
        string tester = rowText.Replace("O2", "Oxygen");
        tester = tester.Replace("Regen", "ChargeRegen");
        tester = tester.Replace("Capacity", "ChargeCapacity");

        string value = DataManager.GetFirstColumnData("ID", selectedID, tester).Replace(".", sep);
        if (value == "") { value = "0"; }

        double outV = DataManager.ToDouble(value);
        double tesValue = 0; bool tes = true;
        if (rowText == "Level") { levelText = value; tesValue = DataManager.LevelMax; tes = false; } // disable recoloring
        if (rowText == "Hp") { hpText = value; tesValue = DataManager.HpMax; }
        if (rowText == "Stamina") { staminaText = value; tesValue = DataManager.StaminaMax; if (!hasStamina) { tes = false; } }
        if (rowText == "O2") { O2Text = value; tesValue = DataManager.O2Max; if (!hasO2) { tes = false; } }
        if (rowText == "Food") { foodText = value; tesValue = DataManager.FoodMax; }
        if (rowText == "Weight") { weightText = value; tesValue = DataManager.WeightMax; }
        if (rowText == "Damage") { damageText = value; tesValue = DataManager.DamageMax; if (!hasDamage) { tes = false; } outV = (outV + 1) * 100; }
        if (rowText == "Speed") { speedText = value; tesValue = DataManager.SpeedMax; outV = (outV + 1) * 100; tes = false; }// disable recoloring
        if (rowText == "CraftSkill") { craftText = value; tesValue = DataManager.CraftMax; if (!hasCraft) { tes = false; } outV = (outV + 1) * 100; }
        if (rowText == "Regen") { regenText = value; tesValue = DataManager.RegenMax; if (!hasCharge) { tes = false; } }
        if (rowText == "Capacity") { capacityText = value; tesValue = DataManager.CapacityMax; if (!hasCharge) { tes = false; } }
        if (rowText == "Emission") { emissionText = value; tesValue = DataManager.EmissionMax; if (!hasEmission) { tes = false; } outV = (outV + 1) * 100; }


        if (tes)
        {
            if (outV >= (tesValue - statViewOffset)) { fontColor = Shared.goodColor; }

            // mutation detection overrides normal coloring -> mutaColor
            string mutes = DataManager.GetMutes(selectedID);
            if (mutes.Length >= 7)
            {
                if (rowText == "Hp") { if (mutes.Substring(0, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(0, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Stamina") { if (mutes.Substring(1, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(1, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "O2") { if (mutes.Substring(2, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(2, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Food") { if (mutes.Substring(3, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(3, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Weight") { if (mutes.Substring(4, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(4, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Damage") { if (mutes.Substring(5, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(5, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "CraftSkill") { if (mutes.Substring(6, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(6, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Regen") { if (mutes.Substring(7, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(7, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Capacity") { if (mutes.Substring(8, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(8, 1) == "2") { fontColor = Shared.mutaBadColor; } }
                if (rowText == "Emission") { if (mutes.Substring(9, 1) == "1") { fontColor = Shared.mutaColor; } else if (mutes.Substring(9, 1) == "2") { fontColor = Shared.mutaBadColor; } }

            }
        }

        //if (value == "0") { outV = 0; }

        Entry outEntry = new Entry { Text = outV.ToString(), Placeholder = rowText, WidthRequest = 200, HeightRequest = 10, TextColor = fontColor, BackgroundColor = Shared.OddMPanelColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start };

        outEntry.TextChanged += (sender, e) =>
        {
            if (!IsValidDouble(e.NewTextValue)) { ((Entry)sender).Text = e.OldTextValue; }
            else
            {
                value = e.NewTextValue;
                double tValue = (ToDouble(value) / 100) - 1;
                if (rowText == "Level") { levelText = value; }
                if (rowText == "Hp") { hpText = value; }
                if (rowText == "Stamina") { staminaText = value; }
                if (rowText == "O2") { O2Text = value; }
                if (rowText == "Food") { foodText = value; }
                if (rowText == "Weight") { weightText = value; }
                if (rowText == "Damage") { damageText = tValue.ToString(); }
                if (rowText == "Speed") { speedText = tValue.ToString(); }
                if (rowText == "CraftSkill") { craftText = tValue.ToString(); }
                if (rowText == "Regen") { regenText = value; }
                if (rowText == "Capacity") { capacityText = value; }
                if (rowText == "Emission") { emissionText = tValue.ToString(); }
            }
        };

        return outEntry;
    }

    private Grid CreateMainPanel()
    {
        editStats = false;
        var maingrid = new Grid { RowSpacing = 0, ColumnSpacing = 5, Padding = 0, };

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

        if (showTree || CurrentStats || DataManager.BottomTable.Rows.Count < 1) { barH = 0; }

        // Define row definitions
        maingrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content

        // PointsFromStat(string dinoClass, string statString, double inStat , double defaultOut = 0)

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        if (dinoCount > 0 && !isDouble && !showTree) // more than 0 dinos and not double clicked and not in showTree
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
            scrollContent.Children.Add(CreateDinoGrid("MaleFemale"));

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


            bottomContent.Children.Add(CreateDinoGrid("Bottom"));

            // paint the rows
            DefaultRowColors();

            // Wrap the scrollable content in a ScrollView and add it to the third row
            var bottomPanel = new ScrollView { Content = bottomContent };

            AddToGrid(maingrid, bottomPanel, 1, 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        else if (dinoCount > 0 && isDouble && !showTree)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // make dino info box

            // Create scrollable content
            var scrollContent = new StackLayout { Spacing = 20, Padding = 3, };

            // Create Grid for stats
            var statGrid = new Grid { RowSpacing = 0, ColumnSpacing = 20, Padding = 3 };

            // Define columns
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 2
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 3
            statGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4


            // Get info about selected dino
            string sex = DataManager.GetLastColumnData("ID", selectedID, "Sex");

            // get parents id
            string papaID = DataManager.GetLastColumnData("ID", selectedID, "Papa");
            string mamaID = DataManager.GetLastColumnData("ID", selectedID, "Mama");

            string papaName = DataManager.GetLastColumnData("ID", papaID, "Name");
            string mamaName = DataManager.GetLastColumnData("ID", mamaID, "Name");

            if (papaName == "") { papaName = "Papa Stats"; }
            if (mamaName == "") { mamaName = "Mama Stats"; }


            Color DefaultColor = Shared.maleColor;

            DefaultColor = Shared.maleColor;
            if (sex == "Male") { DefaultColor = Shared.maleColor; }
            else if (sex == "Female") { DefaultColor = Shared.femaleColor; }

            // fill rows with data
            int rowID = 0; int colID = 0; string labelT = "";

            AddToGrid(statGrid, EditHeader("", DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditHeader("Edit Stats", DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditHeader(papaName, Shared.maleColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditHeader(mamaName, Shared.femaleColor), rowID, colID++, "", false, true);

            labelT = "Level"; rowID++; colID = 0;
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

            labelT = "Hp"; rowID++; colID = 0;
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

            if (hasStamina)
            {
                labelT = "Stamina"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            if (hasO2)
            {
                labelT = "O2"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            if (hasCharge)
            {
                labelT = "Capacity"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

                labelT = "Regen"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            labelT = "Food"; rowID++; colID = 0;
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

            labelT = "Weight"; rowID++; colID = 0;
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

            if (hasDamage)
            {
                labelT = "Damage"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            if (hasEmission)
            {
                labelT = "Emission"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            if (hasCraft)
            {
                labelT = "CraftSkill"; rowID++; colID = 0;
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
                AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);
            }

            labelT = "Speed"; rowID++; colID = 0;
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "D"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowBox(labelT, DefaultColor), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "P"), rowID, colID++, "", false, true);
            AddToGrid(statGrid, EditRowLabel(labelT, DefaultColor, "M"), rowID, colID++, "", false, true);

            scrollContent.Children.Add(statGrid);

            int rowid = 0;
            var notesGrid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 20,
                Padding = 3
            };
            // Define columns
            notesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0


            string notes = DataManager.GetNotes(selectedID);


            string ageText = ""; bool validAgeRate = false;
            Color growColor = Shared.PrimaryColor;


            var grown = new Label { Text = ageText, Style = (Style)Application.Current.Resources["Headline"], TextColor = growColor, FontSize = Shared.fontHSize, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Start };

            // notes textbox defined here
            var textBoxN = new Editor { Text = notes, Placeholder = "Notes", WidthRequest = 600, HeightRequest = 200, TextColor = Shared.mutaColor, BackgroundColor = Shared.OddMPanelColor, FontSize = 16, HorizontalOptions = LayoutOptions.Start, Keyboard = Keyboard.Create(KeyboardFlags.None) };

            textBoxN.TextChanged += (sender, e) =>
            {
                editStats = true;
                notesText = e.NewTextValue;
            };

            if (validAgeRate) // only add label if dino is still aging
            {
                AddToGrid(notesGrid, grown, rowid++, 0, "", false, true);
            }

            AddToGrid(notesGrid, textBoxN, rowid++, 0, "", false, true);


            scrollContent.Children.Add(notesGrid);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(maingrid, scrollView, 0, 0);
            ////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        else if (showTree)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // make tree of life grid

            // Create scrollable content
            var scrollContent = new StackLayout { Spacing = 20, Padding = 3, };

            // Create Grid for generations
            var genGrid = new Grid { RowSpacing = 0, ColumnSpacing = 20, Padding = 3 };

            // get amount of generations
            double gen = DataManager.MaxGenerations(selectedClass);


            int i = 0;
            while (i <= gen)
            {
                // Define a column for each generation
                genGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });



                // add stuff to each column for each generation
                int rowid = 0; // starting at row 0
                string bText = $"Generation: {i}";
                if (i == 0) { bText = $"Generation: {Smap["Missing"]}"; }
                var t = new Label { Text = bText, TextColor = Shared.goldColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
                AddToGrid(genGrid, t, rowid++, i, "", false, true);


                string[] pairs = DataManager.GetGenParents(selectedClass, i);
                foreach (string pair in pairs)
                {
                    var parts = pair.Split(',');

                    // Check for missing or empty values
                    string papaID = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "UnknownPapa";
                    string mamaID = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "UnknownMama";

                    string papaName = DataManager.GetLastColumnData("ID", papaID, "Name");
                    string mamaName = DataManager.GetLastColumnData("ID", mamaID, "Name");


                    if (papaName == "")
                    {
                        if (papaID == "00") { papaName = Shared.Smap["Unknown"]; }
                        else { papaName = Shared.Smap["Missing"]; }
                    }
                    if (mamaName == "")
                    {
                        if (mamaID == "00") { mamaName = Shared.Smap["Unknown"]; }
                        else { mamaName = Shared.Smap["Missing"]; }
                    }


                    var t0 = new Label { Text = $"{papaName} + {mamaName}", TextColor = Shared.maleColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
                    AddToGrid(genGrid, t0, rowid++, i, "", false, true);

                    string[] kidsfrompair = DataManager.GetKidsFromPair(selectedClass, pair);
                    foreach (string kid in kidsfrompair)
                    {
                        string kidName = DataManager.GetLastColumnData("ID", kid, "Name");

                        if (kidName == "") { kidName = kid; }

                        var t1 = new Label { Text = $"{kidName}", TextColor = Shared.bottomColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
                        AddToGrid(genGrid, t1, rowid++, i, "", false, true);

                    }


                    var t9 = new Label { Text = $" ", TextColor = Shared.maleColor, FontSize = Shared.fontSize, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
                    AddToGrid(genGrid, t9, rowid++, i, "", false, true);
                }


                i++;
            }



            scrollContent.Children.Add(genGrid);

            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(maingrid, scrollView, 0, 0);
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

            // Create grid to put data in
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
                Source = "crystalegg.png",
                HeightRequest = 200,
                Aspect = Aspect.AspectFit
            };

            // Add the image to the container
            imageContainer.Children.Add(image);

            var label1 = new Label
            {
                Text = "No dinos in here 🔎",
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

    private Label HeaderLabel(string textL, string title)
    {
        Color headerColor = Shared.maleColor;

        int fSize = Shared.headerSize;  // header fontsize

        // add sorting symbol to the sorted column
        string sortChar = "";
        string sortString = "";

        // find out wich table we are sorting
        string tableSort = "";
        if (title == "Male") { tableSort = sortM; headerColor = Shared.maleHeaderColor; }
        else if (title == "Female") { tableSort = sortF; headerColor = Shared.femaleHeaderColor; }
        else if (title == "Bottom") { headerColor = Shared.bottomHeaderColor; }


        if (tableSort.Contains("ASC")) { sortString = tableSort.Substring(0, tableSort.Length - 4); }
        if (tableSort.Contains("DESC")) { sortString = tableSort.Substring(0, tableSort.Length - 5); }

        string upChar = Smap["SortUp"];
        string downChar = Smap["SortDown"];

        if (sortString == $"{StatMap[textL]}")
        {
            if (tableSort.Contains("ASC"))
            {
                sortChar = " " + upChar;
            }
            if (tableSort.Contains("DESC"))
            {
                sortChar = " " + downChar;
            }
        }

        if (textL == "Mama") { headerColor = Shared.femaleColor; }
        if (textL == "Papa") { headerColor = Shared.maleColor; }

        Label nameH = new Label { Text = $"{StatMap[textL]}{sortChar}", FontAttributes = FontAttributes.Bold, TextColor = headerColor, FontSize = fSize };

        if (title != "Bottom")
        {
            SortColumn(nameH, title);
        }

        return nameH;
    }

    private Label RowLabel(string column, string title, DataRow row, int boxID = 0, string id = "")
    {
        // Set the text color of the label based on the title
        Color RowLabelColor = Shared.bottomColor;
        if (title == "Male") { RowLabelColor = Shared.maleColor; }
        else if (title == "Female") { RowLabelColor = Shared.femaleColor; }

        string rowText = row[column].ToString();

        if (rowText == "")
        {
            // this puts missing symbol in bottom panel
            //  rowText = Smap["Missing"]; 
        }
        else
        {

            // Override coloring on breeding stats
            if (column == "Hp") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.HpMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Stamina") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.StaminaMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "O2") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.O2Max) { RowLabelColor = Shared.goodColor; } }
            if (column == "Food") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.FoodMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Weight") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.WeightMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Damage") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.DamageMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "CraftSkill") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.CraftMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Regen") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.RegenMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Capacity") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.CapacityMax) { RowLabelColor = Shared.goodColor; } }
            if (column == "Emission") { if (DataManager.ToDouble(rowText) + statViewOffset >= DataManager.EmissionMax) { RowLabelColor = Shared.goodColor; } }


            if (title == "Bottom")
            {
                // this column only exist in bottom table
                string IDC = row["Res"].ToString();

                if (IDC.Length >= 10)
                {
                    string aC = IDC.Substring(0, 1);
                    string bC = IDC.Substring(1, 1);
                    string cC = IDC.Substring(2, 1);
                    string dC = IDC.Substring(3, 1);
                    string eC = IDC.Substring(4, 1);
                    string fC = IDC.Substring(5, 1);
                    string gC = IDC.Substring(6, 1);
                    string hC = IDC.Substring(7, 1);
                    string iC = IDC.Substring(8, 1);
                    string jC = IDC.Substring(9, 1);


                    // override offspring colors based on breed points
                    if (column == "Hp") { if (aC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Stamina") { if (bC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "O2") { if (cC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Food") { if (dC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Weight") { if (eC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Damage") { if (fC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "CraftSkill") { if (gC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Regen") { if (hC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Capacity") { if (iC == "2") { RowLabelColor = Shared.bestColor; } }
                    if (column == "Emission") { if (jC == "2") { RowLabelColor = Shared.bestColor; } }


                    if (!hasStamina) { bC = "2"; }
                    if (!hasO2) { cC = "2"; }
                    if (!hasDamage) { fC = "2"; }
                    if (!hasCraft) { gC = "2"; }
                    if (!hasCharge) { hC = "2"; iC = "2"; }
                    if (!hasEmission) { jC = "2"; }

                    if (column == "Hp" || column == "Stamina" || column == "O2" || column == "Food" || column == "Weight" || column == "Damage" || column == "CraftSkill" || column == "Regen" || column == "Capacity" || column == "Emission")
                    {
                        if ((aC + bC + cC + dC + eC + fC + gC + hC + iC + jC) == "2222222222") { RowLabelColor = Shared.goldColor; }
                    }
                }
            }
            else
            {
                if (row["Status"].ToString().Contains(Shared.Smap["Garbage"])) // recolor if its a garbage dino
                {
                    RowLabelColor = garbageColor;
                }

                // this column doesn not excist in bottom table
                string mutes = row["Mutes"].ToString();

                // mutation detection overrides normal coloring -> mutaColor
                if (mutes.Length >= 10 && !CurrentStats) // dont show mutations on current statview
                {
                    double testStat = 0;
                    string aC = "";

                    if (column == "Hp") { aC = mutes.Substring(0, 1); testStat = HpMax; }
                    if (column == "Stamina") { aC = mutes.Substring(1, 1); testStat = StaminaMax; }
                    if (column == "O2") { aC = mutes.Substring(2, 1); testStat = O2Max; }
                    if (column == "Food") { aC = mutes.Substring(3, 1); testStat = FoodMax; }
                    if (column == "Weight") { aC = mutes.Substring(4, 1); testStat = WeightMax; }
                    if (column == "Damage") { aC = mutes.Substring(5, 1); testStat = DamageMax; }
                    if (column == "CraftSkill") { aC = mutes.Substring(6, 1); testStat = CraftMax; }
                    if (column == "Regen") { aC = mutes.Substring(7, 1); testStat = RegenMax; }
                    if (column == "Capacity") { aC = mutes.Substring(8, 1); testStat = CapacityMax; }
                    if (column == "Emission") { aC = mutes.Substring(9, 1); testStat = EmissionMax; }


                    if (aC == "1" && ToDouble(rowText) + statOffset >= testStat) { RowLabelColor = mutaGoodColor; }
                    else if (aC == "1" && ToDouble(rowText) - statOffset < testStat) { RowLabelColor = mutaColor; }
                    if (aC == "2") { RowLabelColor = mutaBadColor; }
                }
            }

        }

        if (column == "Status")
        {
            rowText = rowText.Replace("#", Shared.Smap["Identical"]);
            rowText = rowText.Replace("<", Shared.Smap["LessThan"]);

            string notes = DataManager.GetNotes(id);
            if (notes != "") { rowText += Shared.Smap["Notes"]; }

            if (title != "Bottom")
            {
                string age = row["Age"].ToString();
                if (ToDouble(age) < 100) // recolor if its a garbage dino
                {
                    if (!rowText.Contains(Smap["Baby"]))
                    {
                        rowText = Shared.Smap["Baby"] + rowText;
                    }
                }
            }
        }

        if (column == "Mama") { RowLabelColor = Shared.femaleColor; }
        if (column == "Papa") { RowLabelColor = Shared.maleColor; }


        // override for missing ancestry info
        if (column == "Mama" || column == "Papa" || column == "Gen" || column == "MamaMute" || column == "PapaMute")
        {
            string mama = row["Mama"].ToString(); string papa = row["Papa"].ToString();
            if (mama.Contains(Smap["Warning"]) || papa.Contains(Smap["Warning"]))
            {
                rowText = Smap["Warning"];
            }
        }

        Label OutLabel = new Label { Text = rowText, TextColor = RowLabelColor };

        if (title != "Bottom")
        {
            // make the label selectable if not bottom panel
            SelectDino(OutLabel, id, boxID);
        }

        return OutLabel;
    }

    private Grid CreateDinoGrid(string title)
    {
        var grid = new Grid { RowSpacing = 0, ColumnSpacing = 20, Padding = 3 };

        // Define columns
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0  Name
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1  Level
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 2  Hp
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 3  Stamina / Regen
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4  O2 / Capacity
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 5  Food
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 6  Damage
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 7  CraftSkill
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 8  Status
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 9  Gen
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 10 Papa
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 11 Mama
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 12 pM
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 13 mM
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 14 Imprint
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 15 Imprinter
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 16
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 17


        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 18

        int columnID = 0;
        int rowIndex = 0;

        if (title != "Bottom")
        {
            if (DataManager.MaleTable.Rows.Count > 0)
            {
                title = "Male"; hasSpeed = true;

                // Reset startID for new row
                columnID = 0;

                // Add base header row
                AddToGrid(grid, HeaderLabel("Name", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Level", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Hp", title), rowIndex, columnID++, title);
                if (hasStamina) { AddToGrid(grid, HeaderLabel("Stamina", title), rowIndex, columnID++, title); }
                if (hasO2) { AddToGrid(grid, HeaderLabel("O2", title), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, HeaderLabel("Capacity", title), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, HeaderLabel("Regen", title), rowIndex, columnID++, title); }
                AddToGrid(grid, HeaderLabel("Food", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Weight", title), rowIndex, columnID++, title);
                if (hasEmission) { AddToGrid(grid, HeaderLabel("Emission", title), rowIndex, columnID++, title); }
                if (hasDamage) { AddToGrid(grid, HeaderLabel("Damage", title), rowIndex, columnID++, title); }
                if (hasCraft) { AddToGrid(grid, HeaderLabel("CraftSkill", title), rowIndex, columnID++, title); }
                if (hasSpeed) { AddToGrid(grid, HeaderLabel("Speed", title), rowIndex, columnID++, title); }
                AddToGrid(grid, HeaderLabel("Status", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Gen", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Papa", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Mama", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("pM", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("mM", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Imprint", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Imprinter", title), rowIndex, columnID++, title);
                if (ToggleExcluded == 0)
                {
                    Label groupH = new Label { Text = $"{StatMap["Group"]}", FontAttributes = FontAttributes.Bold, TextColor = maleHeaderColor, FontSize = headerSize };
                    AddToGrid(grid, groupH, rowIndex, columnID++, title);
                }

                // increase row index for header row
                rowIndex++;

                foreach (DataRow row in DataManager.MaleTable.Rows)
                {
                    boxRowID++;

                    string id = row["ID"].ToString();

                    // figure out if we have this dino selected for row coloring purposes
                    bool selected = false;
                    if (id == selectedID) { selected = true; }

                    // Reset startID for new row
                    columnID = 0;

                    AddToGrid(grid, RowLabel("Name", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Level", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Hp", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    if (hasStamina) { AddToGrid(grid, RowLabel("Stamina", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasO2) { AddToGrid(grid, RowLabel("O2", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCharge) { AddToGrid(grid, RowLabel("Capacity", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCharge) { AddToGrid(grid, RowLabel("Regen", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    AddToGrid(grid, RowLabel("Food", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Weight", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    if (hasEmission) { AddToGrid(grid, RowLabel("Emission", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasDamage) { AddToGrid(grid, RowLabel("Damage", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCraft) { AddToGrid(grid, RowLabel("CraftSkill", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasSpeed) { AddToGrid(grid, RowLabel("Speed", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    AddToGrid(grid, RowLabel("Status", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Gen", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Papa", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Mama", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("PapaMute", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("MamaMute", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Imprint", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Imprinter", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);

                    if (ToggleExcluded == 0)
                    {
                        string group = GetGroup(id);
                        if (group == "") { group = "Include"; }
                        Label OutLabel = new Label { Text = $"{Smap[group]}", TextColor = maleColor };
                        AddToGrid(grid, OutLabel, rowIndex, columnID++, title);
                    }


                    rowIndex++;
                }

                boxRowID++;

                // add empty row between tables
                var emptyH = new Label { Text = "" };
                AddToGrid(grid, emptyH, rowIndex, columnID, "Empty");
                rowIndex++; boxRowID++;


                // add empty row between tables
                var emptyH2 = new Label { Text = "" };
                AddToGrid(grid, emptyH2, rowIndex, columnID, "Empty");
                rowIndex++; boxRowID++;
            }

            // add one xtra id for female header row
            rowIndex++; boxRowID++;

            if (DataManager.FemaleTable.Rows.Count > 0)
            {
                title = "Female"; hasSpeed = true;

                // Reset startID for new row
                columnID = 0;

                // Add base header row
                AddToGrid(grid, HeaderLabel("Name", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Level", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Hp", title), rowIndex, columnID++, title);
                if (hasStamina) { AddToGrid(grid, HeaderLabel("Stamina", title), rowIndex, columnID++, title); }
                if (hasO2) { AddToGrid(grid, HeaderLabel("O2", title), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, HeaderLabel("Capacity", title), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, HeaderLabel("Regen", title), rowIndex, columnID++, title); }
                AddToGrid(grid, HeaderLabel("Food", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Weight", title), rowIndex, columnID++, title);
                if (hasEmission) { AddToGrid(grid, HeaderLabel("Emission", title), rowIndex, columnID++, title); }
                if (hasDamage) { AddToGrid(grid, HeaderLabel("Damage", title), rowIndex, columnID++, title); }
                if (hasCraft) { AddToGrid(grid, HeaderLabel("CraftSkill", title), rowIndex, columnID++, title); }
                if (hasSpeed) { AddToGrid(grid, HeaderLabel("Speed", title), rowIndex, columnID++, title); }
                AddToGrid(grid, HeaderLabel("Status", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Gen", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Papa", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Mama", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("pM", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("mM", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Imprint", title), rowIndex, columnID++, title);
                AddToGrid(grid, HeaderLabel("Imprinter", title), rowIndex, columnID++, title);
                if (ToggleExcluded == 0)
                {
                    Label groupH = new Label { Text = $"{StatMap["Group"]}", FontAttributes = FontAttributes.Bold, TextColor = femaleHeaderColor, FontSize = headerSize };
                    AddToGrid(grid, groupH, rowIndex, columnID++, title);
                }


                rowIndex++;

                foreach (DataRow row in DataManager.FemaleTable.Rows)
                {
                    boxRowID++;

                    string id = row["ID"].ToString();

                    // figure out if we have this dino selected for row coloring purposes
                    bool selected = false;
                    if (id == selectedID) { selected = true; }

                    // Reset startID for new row
                    columnID = 0;

                    AddToGrid(grid, RowLabel("Name", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Level", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Hp", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    if (hasStamina) { AddToGrid(grid, RowLabel("Stamina", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasO2) { AddToGrid(grid, RowLabel("O2", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCharge) { AddToGrid(grid, RowLabel("Capacity", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCharge) { AddToGrid(grid, RowLabel("Regen", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    AddToGrid(grid, RowLabel("Food", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Weight", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    if (hasEmission) { AddToGrid(grid, RowLabel("Emission", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasDamage) { AddToGrid(grid, RowLabel("Damage", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasCraft) { AddToGrid(grid, RowLabel("CraftSkill", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    if (hasSpeed) { AddToGrid(grid, RowLabel("Speed", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id); }
                    AddToGrid(grid, RowLabel("Status", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Gen", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Papa", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Mama", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("PapaMute", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("MamaMute", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Imprint", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    AddToGrid(grid, RowLabel("Imprinter", title, row, boxRowID, id), rowIndex, columnID++, title, selected, false, id);
                    if (ToggleExcluded == 0)
                    {
                        string group = GetGroup(id);
                        if (group == "") { group = "Include"; }
                        Label OutLabel = new Label { Text = $"{Smap[group]}", TextColor = femaleColor };
                        AddToGrid(grid, OutLabel, rowIndex, columnID++, title);
                    }

                    rowIndex++;
                }
            }
        }
        else if (title == "Bottom")
        {
            hasSpeed = false; // dont activate for offspring since speed doesnt breed

            // Add base header row
            AddToGrid(grid, HeaderLabel("Name", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Level", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Hp", title), rowIndex, columnID++, title);
            if (hasStamina) { AddToGrid(grid, HeaderLabel("Stamina", title), rowIndex, columnID++, title); }
            if (hasO2) { AddToGrid(grid, HeaderLabel("O2", title), rowIndex, columnID++, title); }
            if (hasCharge) { AddToGrid(grid, HeaderLabel("Capacity", title), rowIndex, columnID++, title); }
            if (hasCharge) { AddToGrid(grid, HeaderLabel("Regen", title), rowIndex, columnID++, title); }
            AddToGrid(grid, HeaderLabel("Food", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Weight", title), rowIndex, columnID++, title);
            if (hasEmission) { AddToGrid(grid, HeaderLabel("Emission", title), rowIndex, columnID++, title); }
            if (hasDamage) { AddToGrid(grid, HeaderLabel("Damage", title), rowIndex, columnID++, title); }
            if (hasCraft) { AddToGrid(grid, HeaderLabel("CraftSkill", title), rowIndex, columnID++, title); }
            if (hasSpeed) { AddToGrid(grid, HeaderLabel("Speed", title), rowIndex, columnID++, title); }
            AddToGrid(grid, HeaderLabel("Status", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Gen", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Papa", title), rowIndex, columnID++, title);
            AddToGrid(grid, HeaderLabel("Mama", title), rowIndex, columnID++, title);



            rowIndex = 1; // Start adding rows below the header
            foreach (DataRow row in DataManager.BottomTable.Rows)
            {
                columnID = 0;
                AddToGrid(grid, RowLabel("Name", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Level", title, row), rowIndex, columnID++, title);

                AddToGrid(grid, RowLabel("Hp", title, row), rowIndex, columnID++, title);
                if (hasStamina) { AddToGrid(grid, RowLabel("Stamina", title, row), rowIndex, columnID++, title); }
                if (hasO2) { AddToGrid(grid, RowLabel("O2", title, row), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, RowLabel("Capacity", title, row), rowIndex, columnID++, title); }
                if (hasCharge) { AddToGrid(grid, RowLabel("Regen", title, row), rowIndex, columnID++, title); }
                AddToGrid(grid, RowLabel("Food", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Weight", title, row), rowIndex, columnID++, title);
                if (hasEmission) { AddToGrid(grid, RowLabel("Emission", title, row), rowIndex, columnID++, title); }
                if (hasDamage) { AddToGrid(grid, RowLabel("Damage", title, row), rowIndex, columnID++, title); }
                if (hasCraft) { AddToGrid(grid, RowLabel("CraftSkill", title, row), rowIndex, columnID++, title); }
                if (hasSpeed) { AddToGrid(grid, RowLabel("Speed", title, row), rowIndex, columnID++, title); }

                AddToGrid(grid, RowLabel("Status", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Gen", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Papa", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Mama", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("PapaMute", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("MamaMute", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Imprint", title, row), rowIndex, columnID++, title);
                AddToGrid(grid, RowLabel("Imprinter", title, row), rowIndex, columnID++, title);

                rowIndex++;
            }

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

    private void DefaultRowColors()
    {

        try
        {
            if (boxViews.Count > 0)
            {
                int rowsM = DataManager.MaleTable.Rows.Count;
                int rowsF = DataManager.FemaleTable.Rows.Count;
                int rowsT = boxViews.Count;

                int i = 0; int z = 0;

                while (i < rowsT)
                {

                    if (i <= rowsM && rowsM > 0)
                    {
                        boxViews[i].Color = i % 2 == 0 ? OddMPanelColor : MainPanelColor;
                    }
                    else if (i <= (rowsM + 2) && rowsM > 0)
                    {
                        boxViews[i].Color = MainPanelColor;
                    }
                    else
                    {
                        boxViews[i].Color = z % 2 == 0 ? MainPanelColor : OddMPanelColor;
                        z++;
                    }
                    i++;
                }

            }
        }
        catch { }

    }

    private void ButtonGroup()
    {
        string group = DataManager.GetGroup(selectedID);
        if (group == "Exclude") { ExcludeBtn.Text = Smap["Include"]; ExcludeBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ExcludeBtn.Text = Smap["Exclude"]; ExcludeBtn.BackgroundColor = Shared.SecondaryColor; }

        if (group == "Archived") { ArchiveBtn.Text = Smap["Include"]; ArchiveBtn.BackgroundColor = Shared.PrimaryColor; }
        else { ArchiveBtn.Text = Smap["Archive"]; ArchiveBtn.BackgroundColor = Shared.TrinaryColor; }

        ExcludeBtn.IsVisible = true;
        ArchiveBtn.IsVisible = true;
    }


    // Button event handlers
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

                // set title to dino name
                this.Title = $"{DataManager.GetLastColumnData("ID", selectedID, "Name")} - {selectedID}";

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

                // set title to dino name
                this.Title = $"{DataManager.GetLastColumnData("ID", selectedID, "Name")} - {selectedID}";

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

    private void ToggleBtnClicked(object? sender, EventArgs e)
    {
        ToggleExcluded++;
        if (ToggleExcluded == 3)
        {
            ToggleExcluded = 0;
        }
        dataValid = false;
        FileManager.Log($"Toggle Exclude {ToggleExcluded}", 0);

        ClearSelection();
        CreateContent();
    }

    private void StatsBtnClicked(object? sender, EventArgs e)
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

        dataValid = false;
        ClearSelection();
        CreateContent();
    }

    private void ExcludeBtnClicked(object? sender, EventArgs e)
    {
        if (selectedID != "")
        {
            string group = DataManager.GetGroup(selectedID);
            if (group == "Exclude") { group = ""; }
            else if (group == "") { group = "Exclude"; FileManager.Log($"Excluded ID: {selectedID}", 0); }
            DataManager.SetGroup(selectedID, group);

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
        levelText = ""; hpText = ""; staminaText = ""; O2Text = "";
        foodText = ""; weightText = ""; damageText = ""; notesText = "";
        speedText = ""; craftText = ""; regenText = ""; capacityText = "";
        emissionText = "";
        dataValid = false;
        isDouble = false; showTree = false;
        ClearSelection();
        CreateContent();
    }

    private void SaveBtnClicked(object? sender, EventArgs e)
    {
        // save data here

        // find the breed stats of
        // selectedID
        // and edit them

        if (editStats)
        {
            DataManager.EditBreedStats(selectedID, levelText, hpText, staminaText, O2Text, foodText, weightText, damageText, notesText, speedText, craftText, regenText, capacityText, emissionText);
            FileManager.needSave = true;
            dataValid = false;
        }

        // reset toggles etc.
        levelText = ""; hpText = ""; staminaText = ""; O2Text = "";
        foodText = ""; weightText = ""; damageText = ""; notesText = "";
        speedText = ""; craftText = ""; regenText = ""; capacityText = "";
        emissionText = "";
        isDouble = false;

        ClearSelection();
        CreateContent();
    }

    private void TreeBtnClicked(object? sender, EventArgs e)
    {
        showTree = true;

        dataValid = false;
        ClearSelection();
        CreateContent();

    }


}