using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        // This is a comment test yes it is !! BLUB

        public MainPage()
        { 
            InitializeComponent();
            SetText("No dinos to show");

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
                // Update content based on the selected route
                switch (route)
                {
                    case "Toad": // Replace with actual route
                        this.Content = new Label
                        {
                            Text = $"{route}",
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        };
                        break;

                    default:
                        // this.Content = new Label{Text = "Stuff",HorizontalOptions = LayoutOptions.Center,VerticalOptions = LayoutOptions.Center};
                        break;
                }
            }
            
           



        }



    }

}
