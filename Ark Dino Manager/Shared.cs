namespace Ark_Dino_Manager
{

    internal class Shared
    {
        ////////////////////    Title version   ////////////////////
        public static string version = "Ark Dino Manager 0.04.64";


        ////////////////////////////////////////////////////////////
        ////////////////////    Shared Variables ///////////////////
        ////////////////////////////////////////////////////////////

        ////////////////////    Default values         ////////////////////
        // Sorting
        public static string DefaultSortA = ""; // default sorting for archive
        public static string DefaultSortM = ""; // default sorting for male dinos
        public static string DefaultSortF = ""; // default sorting for female dinos
        // Navigation
        public static int DefaultToggle = 1;
        public static int DefaultToggleB = 0; // default toggle for babypage
        public static bool DefaultStat = false;
        public static int doubleClick = 500;


        // symbols
        public static readonly Dictionary<string, string> Smap = new Dictionary<string, string>
    {
        { "Unknown", "👽" },
        { "Warning", "❗" },
        { "Missing", "❓" },
        { "Garbage", "💩" },
        { "NewTame", "🧬" },
        { "Age", "🐣" },
        { "Grown", "🦖" },
        { "LessThan", "📉" },
        { "Identical", "🔀" },
        { "Notes", "📋" },
        { "Time", "⌛" },
        { "Rate", "📶" },
        { "Date", "📆" },
        { "Name", "🔤" },
        { "Level", "🔎" },
        { "Hp", "💚" },
        { "Stamina", "⚡" },
        { "O2", "🤿" },
        { "Food", "🥩" },
        { "Weight", "📦" },
        { "Damage", "💪" },
        { "Crafting", "🔨" },
        { "Speed", "🐌" },
        { "Gen", "👪" },
        { "Papa", "👨" },
        { "Mama", "👩" },
        { "Mutation", "🧪" },
        { "Status", "📑" },
        { "Imprint", "💖" },
        { "Imprinter", "💑" },
        { "Class", "🦖" },
        { "SortUp", "▲" },
        { "SortDown", "▼" },
        { "Save", "💾" },
        { "ID", "💳" },
        { "Tag", "🔖" },
        { "Regen", "⚡" },
        { "Capacity", "🔋" },
    };


        //offset for preventing excessive mutation triggers
        public static int muteOffset = 10;

        // offset for a stat considered good enough
        public static int statOffset = 1;

        // Benchmark stuff
        public static int loadCount = 0;
        public static double loadAvg = 0; // keep track of average time


        ////////////////////    Shared locks    ////////////////////
        public static readonly object _logLock = new object();
        public static readonly object _dbLock = new object();


        ////////////////////    IMPORTING       ////////////////////
        public static bool ImportEnabled = false;
        public static bool firstImport = false;
        public static int Delay = 5; // initial import delay
        public static int DefaultDelay = 10; // default import delay in seconds
        public static int CurrentDelay = 10;
        public static int MaxDelay = 60;


        ////////////////////    Navigation      ////////////////////
        public static string setPage = "";
        public static string selectedClass = "";


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
        public static Color mutaColor = Color.FromArgb("#ffdb7a"); //
        public static Color mutaBadColor = Color.FromArgb("#ff1803"); // ADD THIS TO CONFIG


        // Button colors
        public static Color DefaultBColor = Color.FromArgb("#ADD8E6"); // LightBlue
        public static Color PrimaryColor = Color.FromArgb("#90EE90"); // LightGreen
        public static Color SecondaryColor = Color.FromArgb("#FFFFE0"); // LightYellow
        public static Color TrinaryColor = Color.FromArgb("#CD5C5C"); // IndianRed

        // Layout colors
        public static Color SelectedColor = Color.FromArgb("#85837d");
        public static Color SidePanelColor = Color.FromArgb("#312f38");

        public static Color MainPanelColor = Color.FromArgb("#222222");
        public static Color OddMPanelColor = Color.FromArgb("#303030");

        public static Color BottomPanelColor = Color.FromArgb("#292a32");
        public static Color OddBPanelColor = Color.FromArgb("#2e2f37");

        public static Color ArchivePanelColor = Color.FromArgb("#222222");
        public static Color OddAPanelColor = Color.FromArgb("#303030");


    }
}
