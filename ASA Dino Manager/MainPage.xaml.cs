using System.Data;
//using System.Drawing;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Controls.StyleSheets;
using System.Xml.Linq;
using Microsoft.Maui.Graphics.Text;
using System.Text;

namespace ASA_Dino_Manager
{

    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            FileManager.Log($"Loading: {Shared.setPage}", 0);

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


                    string test = Shared.setPage;
                    if (Shared.setPage == @"Looking.for.dinos")
                    {
                        this.Title = "No dinos around here!";
                        DefaultView("Looking for dinos =/");
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
            UnSelectAll(mainLayout);


            this.Content = null;
            this.Content = mainLayout;
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
