namespace ASA_Dino_Manager
{
    public partial class MainPage : ContentPage
    {
        public string version = "ASA Dino Manager 0.03.21";

        // This is a comment test yes it is !! BLUB
        // i blub more and more and more

        // IMPORTING
        public static bool Importing = false;
        public bool ImportEnabled = true;
        public int Delay = 5;
        public int DefaultDelay = 30; // default import delay in seconds


        // benchmark stuff
        private int ImportCount = 0;
        private double ImportAvg = 0; // keep track of average import time


        public MainPage()
        {
            InitializeComponent();

            if (!FileManager.InitFileManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
            if (!DataManager.InitDataManager())
            {
                // Exit app here
                Application.Current.Quit();
            }
        }



        private void StartImport()
        {
            if (ImportEnabled)
            {
                if (FileManager.CheckPath(FileManager.GamePath))
                {
                    if (!Importing)
                    {
                        FileManager.Log("Starting Import thread");
                        Thread thread = new Thread(delegate () { // start new thread
                            try
                            {
                                Importing = true;
                                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                                DataManager.Import();
                                stopwatch.Stop();
                                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                                Importing = false;
                                FileManager.SaveFiles();
                                ImportCount++;
                                double outAVG = 0;
                                if (ImportCount < 2) { ImportAvg = elapsedMilliseconds; outAVG = ImportAvg; }
                                else { ImportAvg += elapsedMilliseconds; outAVG = ImportAvg / ImportCount; }

                                FileManager.Log("Imported files in " + elapsedMilliseconds + "ms" + " Avg: " + outAVG);
                                Delay = DefaultDelay; // only start up timer after scanning is done 
                            }
                            catch
                            {
                                FileManager.Log("Import thread failure");
                                Delay = DefaultDelay; // infinite retries????????
                            }
                        });
                        thread.Start();
                    }
                    else
                    {
                        FileManager.Log("DataBase locked");
                        // restart timer if database is locked
                        Delay = DefaultDelay; 
                    }
                }
                else
                {
                    // scan for a new path and save it
                    FileManager.ScanPath(); 
                    Delay = DefaultDelay;
                }
            }
        }


        private void OnCounterClicked(object sender, EventArgs e)
        {
            StartImport();
        }
    }

}
