using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
//using static System.Net.Mime.MediaTypeNames;
//using System.Windows.Forms;

namespace ASA_Dino_Manager
{

    class FileManager
    {
        private static readonly string configLocation = @"\config.ini";
        private static readonly string dataLocation = @"\Data";
        private static readonly string logsLocation = @"\Logs";

        private static readonly string exportPattern = @"ShooterGame\Saved\DinoExports";


        // Filemanager stuff
        private static DateTime TimeStart = DateTime.UtcNow;
        private static bool RunOnce = true; // toggle off at init

        private static string AppPath = "";
        private static string GamePath = "";


        // scanning for Gamepath
        public static bool Scanning = false;

        // do we need to save files
        public static bool needSave = false;

        // Logging
        public static string LogText = "";



        public static bool InitFileManager()
        {
            try
            {
                if (RunOnce) { TimeStart = DateTime.UtcNow; RunOnce = false; }

                //AppPath = Path.GetDirectoryName(Application.ExecutablePath).ToString();
                //AppPath = Path.GetDirectoryName(assemblyPath);
                //string assemblyPath = Assembly.GetExecutingAssembly().Location;
                // AppPath = Path.GetDirectoryName(documentsPath);


                // get the users document folder and put data in there
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // root directory of all data
                if (!Directory.Exists(documentsPath + @"\DinoManager")) { Directory.CreateDirectory(documentsPath + @"\DinoManager"); }

                AppPath = documentsPath + @"\DinoManager";


                if (!Directory.Exists(AppPath + logsLocation)) { Directory.CreateDirectory(AppPath + logsLocation); }
                if (!Directory.Exists(AppPath + dataLocation)) { Directory.CreateDirectory(AppPath + dataLocation); }

                // if (!LoadColorFile()) { return false; }

                if (LoadConfig()) // loaded
                {
                    if (CheckPath()) { return true; }
                    else
                    {
                        if (ScanPath()) { return true; }
                    }
                }
                else
                {
                    if (ScanPath()) { return true; }
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
                    if (!Shared.ImportEnabled) { Shared.ImportEnabled = true; FileManager.Log("Enabled Importing (Path checks out)", 0); Shared.setPage = "ASA"; }
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
                    writer.WriteLine("BestStatColor=" + ColorToHex(Shared.bestColor));
                    writer.WriteLine("GoldStatColor=" + ColorToHex(Shared.goldColor));
                    writer.WriteLine("MutatedColor=" + ColorToHex(Shared.mutaColor));
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
                }

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

        public static bool LoadConfig()
        {
            try
            {
                bool fail = false;
                string filename = AppPath + configLocation;
                if (File.Exists(filename))
                {
                    var iniData = IniParser.ParseIniFile(filename);
                    foreach (var section in iniData)
                    {
                        if (section.Key.ToUpper() == "GAME LOCATION")
                        {
                            foreach (var key in section.Value)
                            {
                                if (key.Key.ToUpper() == "GAMEPATH")
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
                            if (section.Key.ToUpper() == "[TABLE COLORS]")
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.ToUpper() == "MALEHEADERCOLOR")
                                    {
                                        Shared.maleHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "MALECOLOR")
                                    {
                                        Shared.maleColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "FEMALEHEADERCOLOR")
                                    {
                                        Shared.femaleHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "FEMALECOLOR")
                                    {
                                        Shared.femaleColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "BOTTOMHEADERCOLOR")
                                    {
                                        Shared.bottomHeaderColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "BOTTOMCOLOR")
                                    {
                                        Shared.bottomColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "GOODSTATCOLOR")
                                    {
                                        Shared.goodColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "BESTSTATCOLOR")
                                    {
                                        Shared.bestColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "GOLDSTATCOLOR")
                                    {
                                        Shared.goldColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "MUTATEDSTATCOLOR")
                                    {
                                        Shared.mutaColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                            if (section.Key.ToUpper() == "[BUTTON COLORS]")
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.ToUpper() == "DEFAULTCOLOR")
                                    {
                                        Shared.DefaultBColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "PRIMARYCOLOR")
                                    {
                                        Shared.PrimaryColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "SECONDARYCOLOR")
                                    {
                                        Shared.SecondaryColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "TRINARYCOLOR")
                                    {
                                        Shared.TrinaryColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                            if (section.Key.ToUpper() == "[LAYOUT COLORS]")
                            {
                                foreach (var key in section.Value)
                                {
                                    if (key.Key.ToUpper() == "SELECTEDCOLOR")
                                    {
                                        Shared.SelectedColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "SIDEPANELCOLOR")
                                    {
                                        Shared.SidePanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "MAINPANELCOLOR")
                                    {
                                        Shared.MainPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "ODDMPANELCOLOR")
                                    {
                                        Shared.OddMPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "BOTTOMPANELCOLOR")
                                    {
                                        Shared.BottomPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "ODDBPANELCOLOR")
                                    {
                                        Shared.OddBPanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "ARCHIVEPANELCOLOR")
                                    {
                                        Shared.ArchivePanelColor = Color.FromArgb(key.Value);
                                    }
                                    if (key.Key.ToUpper() == "ODDAPANELCOLOR")
                                    {
                                        Shared.OddAPanelColor = Color.FromArgb(key.Value);
                                    }
                                }
                            }
                        }
                        catch {
                        
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

        public static bool ScanPath()
        {
            if (!Scanning)
            {
                bool result = true;
                FileManager.Log("Scanning for gamePath...", 0);

                Thread thread = new Thread(delegate ()
                { // start calculation thread
                    try
                    {
                        Scanning = true;

                        string filepath = FileManager.FindFilePath();
                        GamePath = filepath;

                        if (CheckPath()) // check if the path we found works
                        {

                            // save config here ======================================================================
                            SaveConfig();

                            Scanning = false;
                            if (!Shared.ImportEnabled) { Shared.ImportEnabled = true; FileManager.Log("Enabled Importing (Found GamePath)", 0); }
                        }
                        else
                        {
                            Shared.ImportEnabled = false;
                            FileManager.Log("Didnt find gamePath", 3);
                        }
                    }
                    catch
                    {
                        Scanning = false;
                        result = false;
                    }
                });
                thread.Start();
                return result;
            }
            return false;
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
                    string formattedTime = "[" + DateTime.UtcNow.ToString("HH:ss:fff") + "]";

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

        public static string FindFilePath()
        {
            string result = "";
            bool found = false;
            foreach (var drive in DriveInfo.GetDrives())
            {
                FileManager.Log("Scanning Drive " + drive.Name, 0);
                var directories = GetDirectories(drive.Name);

                foreach (var data in directories)
                {
                    string dirName = data.ToString();
                    if (dirName.ToUpper().Contains(exportPattern.ToUpper()))
                    {
                        result = dirName; found = true; FileManager.Log("Found GamePath", 0); break;
                    }
                }
                if (found) { break; }
            }
            if (!found)
            {
                FileManager.Log("didnt find anything", 2);
            }
            return result;
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*",
        SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                if (path.Length < 248)
                {
                    return Directory.GetDirectories(path, searchPattern).ToList();
                }
                else
                {
                    return new List<string>();
                }

            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

    }
}
