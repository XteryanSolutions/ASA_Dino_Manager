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
        ////////////////////    Title version   ////////////////////
        public static string version = "ASA Dino Manager 0.04.38";


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
        public static int DefaultDelay = 10; 


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


        ////////////////////    Scaling        ////////////////////
        public static int rowHeight = 20;

    }
}
