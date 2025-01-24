using Microsoft.Win32;
using System.Globalization;
using System.Text;
using static Ark_Dino_Manager.Localization;

namespace Ark_Dino_Manager
{

    class FileManager
    {
        private static readonly string configLocation = @"\config.ini";
        private static readonly string dataLocation = @"\Data";
        private static readonly string logsLocation = @"\Logs";

        // Filemanager stuff
        private static DateTime TimeStart = DateTime.Now;
        private static bool RunOnce = true; // toggle off at init

        private static string AppPath = "";
        private static string GamePath = "";

        // do we need to save files
        public static bool needSave = false;

        // Logging
        public static string LogText = "";


        public static bool InitFileManager()
        {
            try
            {
                if (RunOnce) { TimeStart = DateTime.Now; RunOnce = false; }

                // get the users document folder and put data in there
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // root directory of all data
                if (!Directory.Exists(documentsPath + @"\DinoManager")) { Directory.CreateDirectory(documentsPath + @"\DinoManager"); }

                AppPath = documentsPath + @"\DinoManager";


                if (!Directory.Exists(AppPath + logsLocation)) { Directory.CreateDirectory(AppPath + logsLocation); }
                if (!Directory.Exists(AppPath + dataLocation)) { Directory.CreateDirectory(AppPath + dataLocation); }


                if (LoadLocalization()) { FileManager.Log($"Loaded localization file", 0); }

                // load config file check if path works return true
                if (LoadConfig()) // loaded
                {
                    Shared.ImportEnabled = true;
                    return true;
                }
                else
                {
                    string installPath = GetGameInstallPath(Shared.ASAgameID); // appID for Ascended
                    if (installPath != null)
                    {
                        FileManager.Log($"Game is installed at: {installPath}", 0);
                        GamePath = Path.Combine(installPath, @"ShooterGame\Saved\DinoExports");
                        Shared.ImportEnabled = true;
                        return true;
                    }
                    else
                    {
                        FileManager.Log("Ascended is not installed.", 1);
                        string installPath2 = GetGameInstallPath(Shared.ASEgameID); // appID for Evolved
                        if (installPath2 != null)
                        {
                            FileManager.Log($"Game is installed at: {installPath2}", 0);
                            string dinoExportsPath = Path.Combine(installPath2, @"ShooterGame\Saved\DinoExports");

                            if (Directory.Exists(dinoExportsPath))
                            {
                                string[] subfolders = Directory.GetDirectories(GamePath);
                                if (subfolders.Length > 0)
                                {
                                    // Use the first subfolder found
                                    string userID = new DirectoryInfo(subfolders[0]).Name;
                                    string gamePath = Path.Combine(dinoExportsPath, userID);

                                    FileManager.Log($"Game path: {gamePath}", 0);
                                    if (gamePath == "")
                                    {
                                        Shared.ImportEnabled = false;
                                        return true;
                                    }
                                    else
                                    {
                                        Shared.ImportEnabled = true;
                                        return true;
                                    }
                                }
                                else
                                {
                                    FileManager.Log("No subfolder in dino exports", 1);
                                    return false;
                                }
                            }
                            else
                            {
                                FileManager.Log($"The directory {dinoExportsPath} does not exist.", 1);
                                return false;
                            }
                        }
                        else
                        {
                            FileManager.Log("Evolved is not installed.", 1);
                            return false;
                        }
                    }
                }
            }
            catch { }

            Console.WriteLine("Failed to init fileManager");
            return false;
        }

        public static bool CheckPath()
        {
            try
            {
                string dir = GamePath;

                if (Directory.Exists(dir))
                {
                    string[] exports = Directory.GetFiles(dir + @"\", "*.ini", SearchOption.TopDirectoryOnly);
                    if (!Shared.ImportEnabled) { Shared.ImportEnabled = true; FileManager.Log("Enabled Importing (Path checks out)", 0); }
                    return true;
                }
                else { if (Shared.ImportEnabled) { Shared.ImportEnabled = false; FileManager.Log("Disabled Importing (Path not found)", 1); } }
            }
            catch
            {
                if (Shared.ImportEnabled) { Shared.ImportEnabled = false; FileManager.Log("Disabled Importing (Error)", 2); }
            }
            return false;
        }

        public static bool SaveConfig()
        {
            try
            {
                string filename = AppPath + configLocation;

                using (StreamWriter writer = new StreamWriter(filename))
                {
                    // compile ini file from set values
                    writer.WriteLine("[Game Location]");
                    writer.WriteLine("GamePath=" + GamePath);
                    writer.WriteLine("");
                    writer.WriteLine("[Table Colors]");
                    writer.WriteLine("MaleHeaderColor=" + ColorToHex(Shared.maleHeaderColor));
                    writer.WriteLine("MaleColor=" + ColorToHex(Shared.maleColor));
                    writer.WriteLine("FemaleHeaderColor=" + ColorToHex(Shared.femaleHeaderColor));
                    writer.WriteLine("FemaleColor=" + ColorToHex(Shared.femaleColor));
                    writer.WriteLine("BottomHeaderColor=" + ColorToHex(Shared.bottomHeaderColor));
                    writer.WriteLine("BottomColor=" + ColorToHex(Shared.bottomColor));
                    writer.WriteLine("GoodStatColor=" + ColorToHex(Shared.goodColor));
                    writer.WriteLine("GarbageColor=" + ColorToHex(Shared.garbageColor));
                    writer.WriteLine("BestStatColor=" + ColorToHex(Shared.bestColor));
                    writer.WriteLine("GoldStatColor=" + ColorToHex(Shared.goldColor));
                    writer.WriteLine("MutatedStatColor=" + ColorToHex(Shared.mutaColor));
                    writer.WriteLine("MutatedGoodColor=" + ColorToHex(Shared.mutaGoodColor));
                    writer.WriteLine("MutatedBadColor=" + ColorToHex(Shared.mutaBadColor));
                    writer.WriteLine("");
                    writer.WriteLine("[Button Colors]");
                    writer.WriteLine("DefaultColor=" + ColorToHex(Shared.DefaultBColor));
                    writer.WriteLine("PrimaryColor=" + ColorToHex(Shared.PrimaryColor));
                    writer.WriteLine("SecondaryColor=" + ColorToHex(Shared.SecondaryColor));
                    writer.WriteLine("TrinaryColor=" + ColorToHex(Shared.TrinaryColor));
                    writer.WriteLine("");
                    writer.WriteLine("[Layout Colors]");
                    writer.WriteLine("SelectedColor=" + ColorToHex(Shared.SelectedColor));
                    writer.WriteLine("SidePanelColor=" + ColorToHex(Shared.SidePanelColor));
                    writer.WriteLine("MainPanelColor=" + ColorToHex(Shared.MainPanelColor));
                    writer.WriteLine("OddMPanelColor=" + ColorToHex(Shared.OddMPanelColor));
                    writer.WriteLine("BottomPanelColor=" + ColorToHex(Shared.BottomPanelColor));
                    writer.WriteLine("OddBPanelColor=" + ColorToHex(Shared.OddBPanelColor));
                    writer.WriteLine("ArchivePanelColor=" + ColorToHex(Shared.ArchivePanelColor));
                    writer.WriteLine("OddAPanelColor=" + ColorToHex(Shared.OddAPanelColor));
                    writer.WriteLine("");
                }
                FileManager.Log("Config Saved/Updated", 0);
                return true;
            }
            catch { }
            return false;
        }




        private static string ColorToHex(Color color)
        {
            int r = (int)(color.Red * 255);
            int g = (int)(color.Green * 255);
            int b = (int)(color.Blue * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }


        public static bool LoadLocalization()
        {
            try
            {
                string keyName = "";
                var cci = StringComparison.CurrentCultureIgnoreCase;

                // get current culture
                CultureInfo currentCulture = CultureInfo.CurrentCulture;

                // Get the language (e.g., "en" for English, "fr" for French)
                string language = currentCulture.TwoLetterISOLanguageName;

                // Get the full locale (e.g., "en-US" for English (United States))
                string locale = currentCulture.Name;

                string locPath = AppContext.BaseDirectory;
                string filename = locPath + @$"\Localization\{language}.ini";

                if (Directory.Exists(locPath + @"\Localization"))
                {
                    if (File.Exists(filename)) // load localization file and set values
                    {
                        var iniData = IniParser.ParseIniFile(filename);
                        foreach (var section in iniData)
                        {

                            if (section.Key.Equals("Headers", cci)) // header section with unique key values
                            {
                                foreach (var key in section.Value)
                                {
                                    foreach (var d in StatMap)
                                    {
                                        keyName = d.Key;
                                        if (key.Key.Equals(keyName, cci))
                                        {
                                            if (!StatMap.ContainsValue(key.Value) && key.Value != "")
                                            {
                                                StatMap[keyName] = key.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            if (section.Key.Equals("Main", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    foreach (var d in MainMap)
                                    {
                                        keyName = d.Key;
                                        if (key.Key.Equals(keyName, cci))
                                        {
                                            if (key.Value != "")
                                            {
                                                MainMap[keyName] = key.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            if (section.Key.Equals("Dino", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    foreach (var d in DinoMap)
                                    {
                                        keyName = d.Key;
                                        if (key.Key.Equals(keyName, cci))
                                        {
                                            if (key.Value != "")
                                            {
                                                DinoMap[keyName] = key.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            if (section.Key.Equals("Baby", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    foreach (var d in BabyMap)
                                    {
                                        keyName = d.Key;
                                        if (key.Key.Equals(keyName, cci))
                                        {
                                            if (key.Value != "")
                                            {
                                                BabyMap[keyName] = key.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            if (section.Key.Equals("Archive", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    foreach (var d in ArchiveMap)
                                    {
                                        keyName = d.Key;
                                        if (key.Key.Equals(keyName, cci))
                                        {
                                            if (key.Value != "")
                                            {
                                                ArchiveMap[keyName] = key.Value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool LoadConfig()
        {
            var cci = StringComparison.CurrentCultureIgnoreCase;
            try
            {
                bool fail = false;
                string filename = AppPath + configLocation;
                if (File.Exists(filename))
                {
                    var iniData = IniParser.ParseIniFile(filename);
                    foreach (var section in iniData)
                    {
                        if (section.Key.Equals("Game Location", cci))
                        {
                            foreach (var key in section.Value)
                            {
                                if (key.Key.Equals("GamePath", cci))
                                {
                                    GamePath = key.Value;
                                }
                            }
                        }
                        // fail later on once colors are checked
                        // and loaded default if those fail so we can continue to look for a gamepath
                        if (GamePath == "") { fail = true; }

                        // try all colors at once for now and return to default if any fails
                        try
                        {
                            // if (key.Key.Equals("OddAPanelColor", cci))
                            if (section.Key.Equals("Table Colors", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.Equals("MaleHeaderColor", cci))
                                    {
                                        Shared.maleHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("MaleColor", cci))
                                    {
                                        Shared.maleColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("FemaleHeaderColor", cci))
                                    {
                                        Shared.femaleHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("FemaleColor", cci))
                                    {
                                        Shared.femaleColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("BottomHeaderColor", cci))
                                    {
                                        Shared.bottomHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("BottomColor", cci))
                                    {
                                        Shared.bottomColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("GoodStatColor", cci))
                                    {
                                        Shared.goodColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("BestStatColor", cci))
                                    {
                                        Shared.bestColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("GoldStatColor", cci))
                                    {
                                        Shared.goldColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("MutatedStatColor", cci))
                                    {
                                        Shared.mutaColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("MutatedBadColor", cci))
                                    {
                                        Shared.mutaColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("MutatedGoodColor", cci))
                                    {
                                        Shared.mutaGoodColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("GarbageColor", cci))
                                    {
                                        Shared.garbageColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                            if (section.Key.Equals("Button Colors", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.Equals("DefaultColor", cci))
                                    {
                                        Shared.DefaultBColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("PrimaryColor", cci))
                                    {
                                        Shared.PrimaryColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("SecondaryColor", cci))
                                    {
                                        Shared.SecondaryColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("TrinaryColor", cci))
                                    {
                                        Shared.TrinaryColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                            if (section.Key.Equals("Layout Colors", cci))
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.Equals("SelectedColor", cci))
                                    {
                                        Shared.SelectedColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("SidePanelColor", cci))
                                    {
                                        Shared.SidePanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("MainPanelColor", cci))
                                    {
                                        Shared.MainPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("OddMPanelColor", cci))
                                    {
                                        Shared.OddMPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("BottomPanelColor", cci))
                                    {
                                        Shared.BottomPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("OddBPanelColor", cci))
                                    {
                                        Shared.OddBPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("ArchivePanelColor", cci))
                                    {
                                        Shared.ArchivePanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.Equals("OddAPanelColor", cci))
                                    {
                                        Shared.OddAPanelColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                        }
                        catch
                        {

                        } // no need to set false on fail its just simply going to keep hardcoded colors
                    }
                    if (!fail)
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static string[] GetExportFiles()
        {
            // retrieve all ini files in dinoexport folder
            string[] exports = Directory.GetFiles(GamePath + @"\", "*.ini", SearchOption.TopDirectoryOnly);

            return exports;
        }

        public static bool DeleteFile(string id)
        {
            string file = GamePath + @"\DinoExport_" + id + ".ini";
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    FileManager.Log($"Deleted: {file}", 1);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void LoadFiles()
        {
            try
            {
                DataManager.ImportsTable.Clear();
                DataManager.ImportsTable.ReadXml(AppPath + dataLocation + @"\dinos.hrv");

                DataManager.StatTable.Clear();
                DataManager.StatTable.ReadXml(AppPath + dataLocation + @"\data.hrv");
            }
            catch
            {
                FileManager.Log("==== File Load failure (Creating new files) ====", 2); // error
                needSave = true; SaveFiles();
            }
        }

        public static void SaveFiles()
        {
            if (needSave)
            {
                try
                {
                    DataManager.ImportsTable.WriteXml(AppPath + dataLocation + @"\dinos.hrv");
                    DataManager.StatTable.WriteXml(AppPath + dataLocation + @"\data.hrv");
                    FileManager.Log("==== Saved DataBase ====", 0);
                    needSave = false;

                }
                catch
                {
                    FileManager.Log("==== File write failure ====", 2); // error
                    Console.WriteLine("File write failure");
                }
            }
        }

        public static async Task SaveDataToUrl(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Convert the ImportsTable to XML
                using (StringWriter writer = new StringWriter())
                {
                    DataManager.ImportsTable.WriteXml(writer);
                    string xmlData = writer.ToString();

                    // Upload the data (POST request)
                    var content = new StringContent(xmlData, Encoding.UTF8, "application/xml");
                    await client.PostAsync(url, content);
                }
            }
        }

        public static async Task LoadDataFromUrl(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string xmlData = await client.GetStringAsync(url);

                // Load the XML data into the ImportsTable
                using (StringReader reader = new StringReader(xmlData))
                {
                    DataManager.ImportsTable.ReadXml(reader);
                }
            }
        }

        public static void Log(string text, int logLevel)
        {
            string levelText = "";
            if (logLevel == 0) { levelText = "[INFO] "; }
            else if (logLevel == 1) { levelText = "[WARNING] "; }
            else if (logLevel == 2) { levelText = "[ERROR] "; }
            else if (logLevel == 3) { levelText = "[CRITICAL] "; }

            if (Monitor.TryEnter(Shared._logLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    // Format dateTime as string
                    string formattedTime = "[" + DateTime.Now.ToString("HH:ss:fff") + "]";

                    string logString = $"{formattedTime}{levelText}{text}";

                    // add logged text to buffer
                    LogText += logString + Environment.NewLine;

                    // Output logged text in console
                    System.Diagnostics.Debug.WriteLine(logString);
                }
                finally
                {
                    Monitor.Exit(Shared._logLock);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Log failed");
            }
            if (logLevel == 3) // critical error we need to write log and quit
            {
                WriteLog(true);
            }
        }

        public static void WriteLog(bool critical = false)
        {
            if (Monitor.TryEnter(Shared._logLock, TimeSpan.FromSeconds(5)))
            {
                try
                {
                    if (LogText != "")
                    {
                        // empty log buffer to file
                        File.AppendAllText(AppPath + logsLocation + @"\log_" + TimeStart.ToString().Replace(@"/", ".").Replace(@":", ".") + ".txt", LogText);
                        LogText = "";
                    }
                    if (critical) { Application.Current.Quit(); } // shutdown after logfile has been written even if no new text was added
                }
                catch
                {
                    Console.WriteLine("Could not write to log file");
                }
                finally
                {
                    Monitor.Exit(Shared._logLock);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Log write failed");
            }
        }

        public static string GetSteamPath()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
                   ?? throw new Exception("Steam is not installed or the path could not be found.");
        }

        public static string GetGameInstallPath(string appId)
        {
            string steamPath = GetSteamPath();
            string libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile))
            {
                throw new Exception("Steam library file not found.");
            }

            // Read all library folders
            var libraries = new System.Collections.Generic.List<string> { Path.Combine(steamPath, "steamapps") };
            foreach (var line in File.ReadLines(libraryFile))
            {
                if (line.Contains("path"))
                {
                    string path = line.Split('"')[3]; // Get the path value
                    libraries.Add(Path.Combine(path, "steamapps"));
                }
            }

            // Search for the game in each library
            foreach (var library in libraries)
            {
                string acfPath = Path.Combine(library, $"appmanifest_{appId}.acf");
                if (File.Exists(acfPath))
                {
                    // Parse the ACF file to find installdir
                    foreach (var line in File.ReadLines(acfPath))
                    {
                        if (line.Contains("installdir"))
                        {
                            string installDir = line.Split('"')[3]; // Get the installdir value
                            return Path.Combine(library, "common", installDir);
                        }
                    }
                }
            }

            return null; // Game not found
        }

    }
}
