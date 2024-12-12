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
        }


        public void SetText(string text)
        {
            Label1.Text = text;

            //SemanticScreenReader.Announce(Label1.Text);
        }


    }

}
