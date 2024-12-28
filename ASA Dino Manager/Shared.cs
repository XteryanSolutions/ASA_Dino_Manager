namespace ASA_Dino_Manager
{

    internal class Shared
    {
        ////////////////////    Title version   ////////////////////
        public static string version = "ASA Dino Manager 0.04.49";


        ////////////////////////////////////////////////////////////
        ////////////////////    Shared Variables ///////////////////
        ////////////////////////////////////////////////////////////

        ////////////////////    Default values         ////////////////////
        // Sorting
        public static string DefaultSortA = "Class ASC"; // default sorting for archive
        public static string DefaultSortM = ""; // default sorting for male dinos
        public static string DefaultSortF = ""; // default sorting for female dinos
        // Navigation
        public static int DefaultToggle = 1;
        public static bool DefaultStat = false;
        public static int doubleClick = 500;


        // symbols
        public static string sortUp = "▲";
        public static string sortDown = "▼";

        public static string tameSym = "🧬";
        public static string breedSym = "🐣";
        public static string noBabySym = "🐤";
        public static string grownSym = "🦖";
        public static string garbageSym = "💩";
        public static string missingSym = "❗";
        public static string worseSym = "📉";
        public static string noteSym = "📋";
        public static string timeSym = "⌛";
        public static string speedSym = "📶";
        public static string dateSym = "📆";
        public static string starSym = "✨";
        public static string noSym = "❓";


        ////////////////////    Shared locks    ////////////////////
        public static readonly object _logLock = new object();
        public static readonly object _dbLock = new object();


        ////////////////////    IMPORTING       ////////////////////
        public static bool ImportEnabled = false; // initial import delay
        public static int Delay = 5; 
        public static int DefaultDelay = 10; // default import delay in seconds
        public static int CurrentDelay = 10;
        public static int MaxDelay = 60;


        ////////////////////    Navigation      ////////////////////
        public static string setPage = "";
        //public static string setRoute = "";
        public static string selectedClass = "";
        public static bool eventDisabled = false;
        public static bool isLoaded = false;



        ////////////////////    Scaling         ////////////////////
        public static int headerSize = 16; // FontSize on table headers
        public static int tableSize = 16; // FontSize on tables
        public static int fontSize = 16; // General FontSize
        public static int fontHSize = 20; // General Header FontSize

        public static int rowHeight = 24; // rowheight to determine bottom panel size
        public static int sizeOffset = 5; // Extra buffer to prevent scrolling


        public static int startupX = 1180;   // 16:9
        public static int startupY = 664;


        ////////////////////////////////////////////////////////////
        ////////////////////    Configurable    ////////////////////

        ////////////////////    Coloring        ////////////////////
        // Table colors
        public static Color maleHeaderColor = Color.FromArgb("#ADD8E6"); // LightBlue
        public static Color maleColor = Color.FromArgb("#ADD8E6"); // LightBlue
        public static Color femaleHeaderColor = Color.FromArgb("#FFC0CB"); // Pink
        public static Color femaleColor = Color.FromArgb("#FFC0CB"); // Pink
        public static Color bottomHeaderColor = Color.FromArgb("#FFFFE0"); // LightYellow
        public static Color bottomColor = Color.FromArgb("#FFFFE0"); // LightYellow
        public static Color goodColor = Color.FromArgb("#90EE90"); // LightGreen
        public static Color bestColor = Color.FromArgb("#008000"); // Green
        public static Color goldColor = Color.FromArgb("#FFFF00"); // Yellow
        public static Color mutaColor = Color.FromArgb("#9370DB"); // MediumPurple

        // add theese to config (maybe not yet)
        public static Color tameColor = Color.FromArgb("#9370DB"); // MediumPurple
        public static Color breedColor = Color.FromArgb("#FFFFE0"); // LightYellow
        public static Color grownColor = Color.FromArgb("#90EE90"); // LightGreen
        // 3 values


        // Button colors
        public static Color DefaultBColor = Color.FromArgb("#ADD8E6"); // LightBlue
        public static Color PrimaryColor = Color.FromArgb("#90EE90"); // LightGreen
        public static Color SecondaryColor = Color.FromArgb("#FFFFE0"); // LightYellow
        public static Color TrinaryColor = Color.FromArgb("#CD5C5C"); // IndianRed

        // Layout colors
        public static Color SelectedColor = Color.FromArgb("#545461");
        public static Color SidePanelColor = Color.FromArgb("#312f38");

        public static Color MainPanelColor = Color.FromArgb("#222222");
        public static Color OddMPanelColor = Color.FromArgb("#272727");

        public static Color BottomPanelColor = Color.FromArgb("#292a32");
        public static Color OddBPanelColor = Color.FromArgb("#2e2f37");

        public static Color ArchivePanelColor = Color.FromArgb("#222222");
        public static Color OddAPanelColor = Color.FromArgb("#272727");


    }
}
