using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using static System.Net.Mime.MediaTypeNames;
//using System.Windows.Forms;

namespace ASA_Dino_Manager
{

    class FileManager
    {
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

                if (!Directory.Exists(documentsPath + @"\DinoManager")) { Directory.CreateDirectory(documentsPath + @"\DinoManager"); }

                AppPath = documentsPath + @"\DinoManager";


                if (!Directory.Exists(AppPath + @"\Logs")) { Directory.CreateDirectory(AppPath + @"\Logs"); }
                if (!Directory.Exists(AppPath + @"\Data")) { Directory.CreateDirectory(AppPath + @"\Data"); }

                // if (!LoadColorFile()) { return false; }

                if (LoadPath()) // loaded
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

        public static bool LoadPath()
        {
            try
            {
                if (File.Exists(AppPath + @"\Data\config.hrv"))
                {
                    using (StreamReader read = new StreamReader(AppPath + @"\Data\config.hrv"))
                    {
                        string line;
                        while ((line = read.ReadLine()) != null)
                        {
                            GamePath = line;
                        }
                    }
                    //fileManager.log("gamePath Loaded");
                    return true;
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

                        string filepath = FileManager.FindFilePath(@"ShooterGame\Saved\DinoExports");
                        GamePath = filepath;

                        if (CheckPath()) // check if the path we found works
                        {
                            
                            using (StreamWriter writer = new StreamWriter(AppPath + @"\Data\config.hrv"))
                            {
                                writer.WriteLine(filepath);
                            }
                            FileManager.Log("Set New gamePath", 0);
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
                DataManager.ImportsTable.ReadXml(AppPath + @"\Data\dinos.hrv");

                DataManager.StatTable.Clear();
                DataManager.StatTable.ReadXml(AppPath + @"\Data\data.hrv");
            }
            catch
            {
                Console.WriteLine("no import dataBase! creating a new one");
                needSave = true; SaveFiles();
            }
        }

        public static void SaveFiles()
        {
            if (needSave)
            {
                try
                {
                    DataManager.ImportsTable.WriteXml(AppPath + @"\Data\dinos.hrv");
                    DataManager.StatTable.WriteXml(AppPath + @"\Data\data.hrv");
                    FileManager.Log("==== Saved DataBase ====", 0);
                    needSave = false;
                }
                catch
                {
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
                        File.AppendAllText(AppPath + @"\Logs\log_" + TimeStart.ToString().Replace(@"/", ".").Replace(@":", ".") + ".txt", LogText);
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

        public static string FindFilePath(string FolderName = @"ShooterGame\Saved\DinoExports")
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
                    if (dirName.ToUpper().Contains(FolderName.ToUpper()))
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
