namespace Ark_Dino_Manager
{


    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            FileManager.Log($"Loaded: {Shared.setPage}", 0);

            // set page title
            this.Title = $"{Shared.setPage}";
            CreateContent();
        }

        public void CreateContent()
        {
            if (Monitor.TryEnter(Shared._dbLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    FileManager.Log("Updating GUI -> " + Shared.setPage, 0);


                    if (Shared.setPage == @"Looking_for_dinos")
                    {
                        this.Title = "Ready to start importing data";
                        DefaultView("Looking for dino exports 🔎", "egg200.png");
                        // redirect to start page once first import is run
                        Shared.firstImport = true; Shared.setPage = "ASA";
                    }
                    else
                    {
                        this.Title = "Dino Manager";
                        DefaultView("Remember to feed your dinos!!!");
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

        private void DefaultView(string labelText, string imageSource = "dino400.png")
        {
            var mainLayout = new Grid
            {
                BackgroundColor = Shared.MainPanelColor
            };

            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Fixed button row
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Scrollable content


            var scrollContent = new StackLayout
            {
                Spacing = 20,
                Padding = 3
            };

            var image1 = new Image { Source = imageSource, HeightRequest = 255, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start };

            var label1 = new Label { Text = labelText, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Shared.goodColor, FontSize = 22 };


            AddToGrid(mainLayout, image1, 0, 0);
            AddToGrid(mainLayout, label1, 1, 0);


            // Wrap the scrollable content in a ScrollView and add it to the second row
            var scrollView = new ScrollView { Content = scrollContent };

            AddToGrid(mainLayout, scrollView, 0, 0);

            // only attach the tapgesture if we have something selected
            //UnSelectAll(mainLayout);


            this.Content = null;
            this.Content = mainLayout;

            if (Shared.setPage != @"Looking_for_dinos")
            {
                // https://www.dododex.com/
                // https://github.com/XteryanSolutions/ASA_Dino_Manager/releases
                var loadedView = new WebView
                {
                    Source = "https://github.com/XteryanSolutions/ASA_Dino_Manager/releases", // Replace with your desired URL
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };


                LoadWebView(loadedView);
            }
        }

        private async Task LoadWebView(WebView loadedView)
        {
            await Task.Delay(5000);
            this.Content = loadedView;
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

        void UnSelectAll(Grid grid)
        {
            // Create a TapGestureRecognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle the click event
                CreateContent();
            };

            // Attach the TapGestureRecognizer to the label
            grid.GestureRecognizers.Add(tapGesture);
        }

    }
}
