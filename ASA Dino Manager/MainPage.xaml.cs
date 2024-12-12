using System.Data;
using System.Drawing;

namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        // This is a comment test yes it is !! BLUB
        public static int ToggleExcluded = 0;

        public static bool OnlyExcluded = false;
        public static bool CurrentStats = false;




        public MainPage()
        { 
            InitializeComponent();
            SetText("No dinos here yet");

            Shell.Current.Navigated += OnShellNavigated;
        }


        public void SetText(string text)
        {
            Label1.Text = text;

            //SemanticScreenReader.Announce(Label1.Text);
        }

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Check if the navigation is to the current page
            if (e.Source == ShellNavigationSource.ShellItemChanged || e.Source == ShellNavigationSource.Push)
            {
                UpdateContentBasedOnNavigation();
            }
        }

        private void UpdateContentBasedOnNavigation()
        {
            var route = Shell.Current.CurrentState.Location.ToString();
            route = route.Replace("/","");

            if (route == "Looking for dinos")
            {

            }
            else
            {
                var labels = new List<Label>();


                string dinoTag = DataManager.TagForClass(route);
                DataManager.selectedClass = dinoTag;


                if (DataManager.selectedClass != "")
                {
                    DataManager.GetDinoData(DataManager.selectedClass);
                    DataManager.SetMaxStats();
                    DataManager.SetBinaryStats();
                    DataManager.GetBestPartner();
                }



                // =================================================================================================    Show data   =====================================================

                // Create the main layout
                var mainLayout = new Grid();

                // Define row definitions
                mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
                mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


                var mainStack = new StackLayout
                {
                    Spacing = 20,
                    Padding = 10
                };


            
                // Add the button grid
                AddToGrid(mainLayout, CreateButtonGrid(), 0, 0);



                // Create scrollable content
                var scrollContent = new StackLayout
                {
                    Spacing = 20,
                    Padding = 10
                };

                // Add male and female tables
                scrollContent.Children.Add(CreateTableGrid(DataManager.MaleTable, "Male"));
                scrollContent.Children.Add(CreateTableGrid(DataManager.FemaleTable, "Female"));
               

                // Wrap the scrollable content in a ScrollView and add it to the second row
                var scrollView = new ScrollView { Content = scrollContent };


                AddToGrid(mainLayout, scrollView, 1, 0);



                // Create scrollable content
                var scrollContent2 = new StackLayout
                {
                    Spacing = 20,
                    Padding = 10
                };

                scrollContent2.Children.Add(CreateTableGrid(DataManager.BottomTable, "Bottom"));
                var scrollView2 = new ScrollView { Content = scrollContent2 };


                AddToGrid(mainLayout, scrollView2, 2, 0);


                this.Content = mainLayout;

            }
        }
        private Grid CreateButtonGrid()
        {
            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10,
                Padding = 10
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1

            var bColor1 = Colors.LightBlue;
            var bColor2 = Colors.LightBlue;


            if (MainPage.CurrentStats)
            {
                bColor2 = Colors.LightGreen;
            }

            if (MainPage.ToggleExcluded == 0)
            {
                bColor1 = Colors.LightBlue;
            }
            else if (MainPage.ToggleExcluded == 1)
            {
                bColor1 = Colors.LightGreen;
            }
            else if (MainPage.ToggleExcluded == 2)
            {
                bColor1 = Colors.LightYellow;
            }



            var topButton1 = new Button { Text = "Toggle Excluded" ,BackgroundColor = bColor1 };
            var topButton2 = new Button { Text = "Current stats" ,BackgroundColor = bColor2 };



            AddToGrid(grid, topButton1 , 0, 0);
            AddToGrid(grid, topButton2, 0, 1);



            // Create the fixed button


            topButton1.Clicked += OnTopButton1Clicked;
            topButton2.Clicked += OnTopButton2Clicked;


            return grid;
        }

        private Grid CreateTableGrid(DataTable table, string title)
        {
            var grid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10,
                Padding = 10
            };

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 0
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 1
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 2
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 3
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 4
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 5
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 6
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 7
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 8



            var maleColor = Colors.LightBlue;
            var femaleColor = Colors.Pink;
            var breedColor = Colors.LightYellow;
            var goodColor = Colors.LightGreen;




            var headerColor = maleColor;
            var DefaultColor = maleColor;



            if (title == "Male") { DefaultColor = maleColor; }
            else if (title == "Female") { DefaultColor = femaleColor; }
            else {  DefaultColor = breedColor; }


            headerColor = DefaultColor;


            // Add header row
            AddToGrid(grid, new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 0);
            AddToGrid(grid, new Label { Text = "Level", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 1);
            AddToGrid(grid, new Label { Text = "Hp", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 2);
            AddToGrid(grid, new Label { Text = "Stamina", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 3);
            AddToGrid(grid, new Label { Text = "Oxygen", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 4);
            AddToGrid(grid, new Label { Text = "Food", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 5);
            AddToGrid(grid, new Label { Text = "Weight", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 6);
            AddToGrid(grid, new Label { Text = "Damage", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 7);
            AddToGrid(grid, new Label { Text = "Status", FontAttributes = FontAttributes.Bold, TextColor = headerColor }, 0, 8);





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


                string name = row["Name"].ToString();
                string level = row["Level"].ToString();
                string hp = row["Hp"].ToString();
                string stamina = row["Stamina"].ToString();
                string oxygen = row["Oxygen"].ToString();
                string food = row["Food"].ToString();
                string weight = row["Weight"].ToString();
                string damage = row["Damage"].ToString();
                string status = row["Status"].ToString();




                if (DataManager.ToDouble(hp) >= DataManager.HpMax) { cellColor2 = goodColor; }
                if (DataManager.ToDouble(stamina) >= DataManager.StaminaMax) { cellColor3 = goodColor; }
                if (DataManager.ToDouble(oxygen) >= DataManager.OxygenMax) { cellColor4 = goodColor; }
                if (DataManager.ToDouble(food) >= DataManager.FoodMax) { cellColor5 = goodColor; }
                if (DataManager.ToDouble(weight) >= DataManager.WeightMax) { cellColor6 = goodColor; }
                if (DataManager.ToDouble(damage) >= DataManager.DamageMax) { cellColor7 = goodColor; }




                // Add data to the grid
                AddToGrid(grid, new Label { Text = name, TextColor = cellColor0 }, rowIndex, 0);
                AddToGrid(grid, new Label { Text = level, TextColor = cellColor1 }, rowIndex, 1);
                AddToGrid(grid, new Label { Text = hp, TextColor = cellColor2 }, rowIndex, 2);
                AddToGrid(grid, new Label { Text = stamina, TextColor = cellColor3 }, rowIndex, 3);
                AddToGrid(grid, new Label { Text = oxygen, TextColor = cellColor4 }, rowIndex, 4);
                AddToGrid(grid, new Label { Text = food, TextColor = cellColor5 }, rowIndex, 5);
                AddToGrid(grid, new Label { Text = weight, TextColor = cellColor6 }, rowIndex, 6);
                AddToGrid(grid, new Label { Text = damage, TextColor = cellColor7 }, rowIndex, 7);
                AddToGrid(grid, new Label { Text = status, TextColor = cellColor8 }, rowIndex, 8);


                rowIndex++;
            }

            return grid;
        }

        private void OnTopButton1Clicked(object? sender, EventArgs e)
        {
            ToggleExcluded++;
            if (ToggleExcluded == 3)
            {
                ToggleExcluded = 0;
            }

            // reload stuff
            UpdateContentBasedOnNavigation();
        }

        private void OnTopButton2Clicked(object? sender, EventArgs e)
        {
            if (CurrentStats)
            {
                CurrentStats = false;
            }
            else
            {
                CurrentStats = true;
            }
            // reload stuff
            UpdateContentBasedOnNavigation();
        }


        private void AddToGrid(Grid grid, View view, int row, int column)
        {
            // Ensure rows exist up to the specified index
            while (grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Set the row and column for the view
            Grid.SetRow(view, row);
            Grid.SetColumn(view, column);

            // Add the view to the grid
            grid.Children.Add(view);
        }

    }

}
