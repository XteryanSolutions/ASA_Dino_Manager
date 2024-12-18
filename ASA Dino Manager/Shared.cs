using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ASA_Dino_Manager
{

    internal class Shared
    {
        //Shared Variables



        // theese might move later to their proper location

        // Title version string
        public static string version = "ASA Dino Manager 0.04.37";


        // Filemanager stuff
        public static DateTime TimeStart = DateTime.UtcNow;
        public static bool RunOnce = true; // toggle off at init

        public static string AppPath = "";
        public static string GamePath = "";
        public static bool Scanning = false;

        // logging
        public static string LogText = "";
        public static readonly object _logLock = new object();

        public static bool needSave = false;



        // Toggles for viewing stats
        public static int ToggleExcluded = 0;
        public static bool CurrentStats = false;


        // IMPORTING
        public static bool ImportEnabled = false;
        public static int Delay = 5; // initial import delay
        public static int DefaultDelay = 10; // default import delay in seconds


        // database lock
        public static readonly object _dbLock = new object();


        public static int tagSize = 0;


        // Navigation
        public static string selectedID = "";
        public static string selectedClass = "";


        public static string setPage = "";
        public static string setRoute = "";


        public static bool isSelected = false;


        public static bool eventDisabled = false;


        // table sorting
        public static string sortM = "";
        public static string sortF = "";
        public static bool showStats = false;


        // Benchmark stuff
        public static int ImportCount = 0;
        public static double ImportAvg = 0; // keep track of average import time


        // Coloring

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

        // placeholder
        public static Color DefaultColor = Colors.Red;
        public static Color headerColor = Colors.White;


        public static bool isLoaded = false;



        // keep refreshing page when new data arrives

        private static int reloadCount = 0;

        public static string ReLoad()
        {
            string result = "";

            // Increment the reload counter
            reloadCount++;

            result = "" + reloadCount;

            return result;
        }

    }
}
