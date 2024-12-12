using System.Data;
using System.Drawing;

namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        // This is a comment test yes it is !! BLUB

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

                DataManager.forceLoad = true;
                DataManager.GetDinoData(dinoTag);
                DataManager.SetMaxStats();
                DataManager.SetBinaryStats();
                DataManager.GetBestPartner();

                // Show data
                var mainStack = new StackLayout
                {
                    Spacing = 20,
                    Padding = 10
                };

                // Add male table
                mainStack.Children.Add(CreateTableGrid(DataManager.MaleTable, "Male Table"));

                // Add female table
                mainStack.Children.Add(CreateTableGrid(DataManager.FemaleTable, "Female Table"));

                // Add Bottom table
                mainStack.Children.Add(CreateTableGrid(DataManager.BottomTable, "Bottom Table"));



                this.Content = new ScrollView { Content = mainStack }; // Wrap in a 

            }
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



            // Add table title
            var titleLabel = new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Black
            };
            //Grid.SetColumnSpan(titleLabel, 3); // Span across all 3 columns
            //AddToGrid(grid, titleLabel, 0, 0);

            var headerColor = Colors.LightBlue;
            var cellColor = Colors.LightBlue;

            if (title == "") { }
            else if (title == "") { }



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
                string name = row["Name"].ToString();
                string level = row["Level"].ToString();
                string hp = row["Hp"].ToString();
                string stamina = row["Stamina"].ToString();
                string oxygen = row["Oxygen"].ToString();
                string food = row["Food"].ToString();
                string weight = row["Weight"].ToString();
                string damage = row["Damage"].ToString();
                string status = row["Status"].ToString();



                // Add data to the grid
                AddToGrid(grid, new Label { Text = name, TextColor = cellColor }, rowIndex, 0);
                AddToGrid(grid, new Label { Text = level, TextColor = cellColor }, rowIndex, 1);
                AddToGrid(grid, new Label { Text = hp, TextColor = cellColor }, rowIndex, 2);
                AddToGrid(grid, new Label { Text = stamina, TextColor = cellColor }, rowIndex, 3);
                AddToGrid(grid, new Label { Text = oxygen, TextColor = cellColor }, rowIndex, 4);
                AddToGrid(grid, new Label { Text = food, TextColor = cellColor }, rowIndex, 5);
                AddToGrid(grid, new Label { Text = weight, TextColor = cellColor }, rowIndex, 6);
                AddToGrid(grid, new Label { Text = damage, TextColor = cellColor }, rowIndex, 7);
                AddToGrid(grid, new Label { Text = status, TextColor = cellColor }, rowIndex, 8);




                rowIndex++;
            }

            return grid;
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
