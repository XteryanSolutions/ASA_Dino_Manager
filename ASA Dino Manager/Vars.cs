using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_Dino_Manager
{

    internal class Vars
    {
        public static string version = "ASA Dino Manager 0.04.37";



        // Toggles for viewing stats
        public static int ToggleExcluded = 0;
        public static bool CurrentStats = false;


        // IMPORTING
        public static bool ImportEnabled = false;
        public static int Delay = 5;
        public static int DefaultDelay = 10; // default import delay in seconds

        public static int tagSize = 0;


        public static readonly object _dbLock = new object();

        public static string setRoute = "";
        public static bool eventDisabled = false;


        // Benchmark stuff
        public static int RefreshCount = 0;
        public static double RefreshAvg = 0; // keep track of average import time


        //public static bool isLoaded = false; // tag to prevent extra navigation triggers



        public static string selectedID = "";
        public static string selectedClass = "";


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

        public static Color DefaultColor = Colors.Red; // placeholder
        public static Color headerColor = Colors.White; // placeholder

        public static string sortM = "";
        public static string sortF = "";




    }
}
