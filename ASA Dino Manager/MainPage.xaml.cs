using Microsoft.Maui.Controls;

namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        // This is a comment test yes it is !! BLUB

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            AppShell.StartImport();
        }

        public static void SetText(string text)
        {
            Label1.Text = text;

            SemanticScreenReader.Announce(Label1.Text);


        }


    }

}
