﻿namespace ASA_Dino_Manager
{

    internal class Shared
    {
        ////////////////////    Title version   ////////////////////
        public static string version = "ASA Dino Manager 0.04.40";


        /////////////////////////////////////////////////////////////
        ////////////////////    Shared Variables ////////////////////
        /////////////////////////////////////////////////////////////


        ////////////////////    Shared locks    ////////////////////
        public static readonly object _logLock = new object();
        public static readonly object _dbLock = new object();


        ////////////////////    IMPORTING       ////////////////////
        public static bool ImportEnabled = false;
        // initial import delay
        public static int Delay = 5; 
        // default import delay in seconds
        public static int DefaultDelay = 15; 


        ////////////////////    Navigation      ////////////////////
        public static string setPage = "";
        public static string setRoute = "";
        public static string selectedClass = "";
        public static bool eventDisabled = false;
        public static bool isLoaded = false;


        ////////////////////    Coloring        ////////////////////
        // table colors
        public static Color maleColor = Colors.LightBlue;
        public static Color femaleColor = Colors.Pink;
        public static Color breedColor = Colors.LightYellow;
        public static Color goodColor = Colors.LightGreen;
        public static Color mutaColor = Colors.MediumPurple;

        // button colors
        public static Color noColor = Colors.LightBlue;
        public static Color okColor = Colors.LightGreen;
        public static Color warnColor = Colors.LightYellow;
        public static Color dangerColor = Colors.IndianRed;

        // placeholder (doesnt need ot be here)
        public static Color DefaultColor = Colors.Red;
        public static Color headerColor = Colors.White;

        public static Color MainPanelColor = Color.FromArgb("#222224");
        public static Color SidePanelColor = Color.FromArgb("#312f38");
        public static Color BottomPanelColor = Color.FromArgb("#292a32");



        ////////////////////    Scaling        ////////////////////
        public static int headerSize = 16; // font size on table headers
        public static int rowHeight = 19; // rowheight to determine bottom panel size
        public static int sizeOffset = 20; // things



    }
}
