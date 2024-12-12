using Microsoft.Maui.Controls;

namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        public string version = "ASA Dino Manager 0.04.30";

        // This is a comment test yes it is !! BLUB
        // i blub more and more and more





        public MainPage()
        {
            InitializeComponent();

           

            //string[] tagList = DataManager.GetAllDistinctColumnData("Tag");


        }




        private void OnCounterClicked(object sender, EventArgs e)
        {
            AppShell.StartImport();
        }
    }

}
