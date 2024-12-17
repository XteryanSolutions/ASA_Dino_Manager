using System;
using System.Collections.Generic;
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



        public static bool InitFileManager()
        {
            try
            {
                if (Shared.RunOnce) { Shared.TimeStart = DateTime.UtcNow; Shared.RunOnce = false; }

                //AppPath = Path.GetDirectoryName(Application.ExecutablePath).ToString();

                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                Shared.AppPath = Path.GetDirectoryName(assemblyPath);

                if (!Directory.Exists(Shared.AppPath + @"\Logs")) { Directory.CreateDirectory(Shared.AppPath + @"\Logs"); }
                if (!Directory.Exists(Shared.AppPath + @"\Data")) { Directory.CreateDirectory(Shared.AppPath + @"\Data"); }

                // if (!LoadColorFile()) { return false; }

                if (LoadPath()) // loaded
                {
                    if (CheckPath(Shared.GamePath)) { return true; }
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

        public static bool CheckPath(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    string[] exports = Directory.GetFiles(dir + @"\", "*.ini", SearchOption.TopDirectoryOnly);
                    if (!Shared.ImportEnabled) { Shared.ImportEnabled = true; FileManager.Log("Enabled Importing (Path checks out)", 0); Shared.setRoute = "ASA"; }
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
                if (File.Exists(Shared.AppPath + @"\Data\config.hrv"))
                {
                    using (StreamReader read = new StreamReader(Shared.AppPath + @"\Data\config.hrv"))
                    {
                        string line;
                        while ((line = read.ReadLine()) != null)
                        {
                            Shared.GamePath = line;
                        }
                    }
                    //fileManager.log("gamePath Loaded");
                    return true;
                }
            }
            catch { }
            return false;
        }


        public static bool ScanPath()
        {
            if (!Shared.Scanning)
            {
                bool result = true;
                FileManager.Log("Scanning for gamePath...", 0);

                Thread thread = new Thread(delegate ()
                { // start calculation thread
                    try
                    {
                        Shared.Scanning = true;

                        string filepath = FileManager.FindFilePath(@"ShooterGame\Saved\DinoExports");

                        if (CheckPath(filepath)) // check if the path we found works
                        {
                            Shared.GamePath = filepath;
                            using (StreamWriter writer = new StreamWriter(Shared.AppPath + @"\Data\config.hrv"))
                            {
                                writer.WriteLine(filepath);
                            }
                            FileManager.Log("Set New gamePath", 0);
                            Shared.Scanning = false;
                            if (!Shared.ImportEnabled) { Shared.ImportEnabled = true; FileManager.Log("Enabled Importing (Found GamePath)", 0); }
                        }
                        else
                        {
                            FileManager.Log("Didnt find gamePath", 3);
                        }
                    }
                    catch
                    {
                        Shared.Scanning = false;
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
                DataManager.ImportsTable.ReadXml(Shared.AppPath + @"\Data\dinos.hrv");

                DataManager.StatTable.Clear();
                DataManager.StatTable.ReadXml(Shared.AppPath + @"\Data\data.hrv");
            }
            catch
            {
                Console.WriteLine("no import dataBase! creating a new one");
                Shared.needSave = true; SaveFiles();
            }
        }


        public static void SaveFiles()
        {
            if (Shared.needSave)
            {
                try
                {
                    DataManager.ImportsTable.WriteXml(Shared.AppPath + @"\Data\dinos.hrv");
                    DataManager.StatTable.WriteXml(Shared.AppPath + @"\Data\data.hrv");
                    FileManager.Log("==== Saved DataBase ====", 0);
                    Shared.needSave = false;
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
                    Shared.LogText += logString + Environment.NewLine;

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
                    if (Shared.LogText != "")
                    {
                        // empty log buffer to file
                        File.AppendAllText(Shared.AppPath + @"\Logs\log_" + Shared.TimeStart.ToString().Replace(@"/", ".").Replace(@":", ".") + ".txt", Shared.LogText);
                        Shared.LogText = "";
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

        public static string[] FilterDinoStats(string filename, string emptyString = "N/A")
        {
            string[] resultSet = Enumerable.Repeat(emptyString, 24).ToArray();

            try
            {
                var iniData = IniParser.ParseIniFile(filename);

                foreach (var section in iniData)
                {
                    if (section.Key.ToUpper() == "DINOANCESTORS") // look for parents here
                    {
                        try // catch parents separately to leave empty just incase it fails
                        {
                            foreach (var key in section.Value)
                            {
                                var split = key.Value.ToString().Split(new[] { @";" }, StringSplitOptions.RemoveEmptyEntries);

                                var id1P = split[1].Split(new[] { @"=" }, StringSplitOptions.RemoveEmptyEntries);
                                var id2P = split[2].Split(new[] { @"=" }, StringSplitOptions.RemoveEmptyEntries);

                                resultSet[12] = id1P[1] + id2P[1];

                                var id1M = split[4].Split(new[] { @"=" }, StringSplitOptions.RemoveEmptyEntries);
                                var id2M = split[5].Split(new[] { @"=" }, StringSplitOptions.RemoveEmptyEntries);

                                resultSet[11] = id1M[1] + id2M[1];
                            }
                        }
                        catch
                        {
                            resultSet[11] = emptyString;
                            resultSet[12] = emptyString;
                        }
                    }
                    else if (section.Key.ToUpper() == "COLORIZATION")
                    {
                        try // catch parents separately to leave empty just incase it fails
                        {
                            string[] outst = new string[section.Value.Count];
                            int rid = 0;
                            foreach (var key in section.Value) { outst[rid] = key.Value; rid++; }
                            rid = 0; string output = "";
                            foreach (string t in outst) { output += t + ";"; rid++; }
                            resultSet[23] = output;
                        }
                        catch
                        {
                            resultSet[23] = emptyString;
                        }
                    }
                    else
                    {
                        foreach (var key in section.Value)
                        {
                            if (key.Key.ToUpper() == "DINONAMETAG")
                            {
                                resultSet[0] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "BISFEMALE")
                            {
                                if (key.Value.ToUpper() == "TRUE")
                                {
                                    resultSet[1] = "Female";
                                }
                                else
                                {
                                    resultSet[1] = "Male";
                                }
                            }
                            else if (key.Key.ToUpper() == "TAMEDNAME")
                            {
                                resultSet[2] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "CHARACTERLEVEL")
                            {
                                resultSet[3] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "HEALTH")
                            {
                                resultSet[4] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "STAMINA")
                            {
                                resultSet[5] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "OXYGEN")
                            {
                                resultSet[6] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "FOOD")
                            {
                                resultSet[7] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "WEIGHT")
                            {
                                resultSet[8] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "MELEE DAMAGE")
                            {
                                resultSet[9] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "MOVEMENT SPEED")
                            {
                                resultSet[10] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "RANDOMMUTATIONSFEMALE")
                            {
                                resultSet[13] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "RANDOMMUTATIONSMALE")
                            {
                                resultSet[14] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "DINOANCESTORSCOUNT")
                            {
                                resultSet[15] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "DINOANCESTORSMALE")
                            {
                                resultSet[16] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "BABYAGE")
                            {
                                resultSet[17] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "BNEUTERED")
                            {
                                if (key.Value.ToUpper() == "TRUE")
                                {
                                    resultSet[18] = "Y";
                                }
                                else
                                {
                                    resultSet[18] = "N";
                                }
                            }
                            else if (key.Key.ToUpper() == "DINOIMPRINTINGQUALITY")
                            {
                                resultSet[19] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "IMPRINTERNAME")
                            {
                                resultSet[20] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "TAMERSTRING")
                            {
                                resultSet[21] = key.Value;
                            }
                            else if (key.Key.ToUpper() == "DINOCLASS")
                            {
                                resultSet[22] = key.Value;
                            }
                        }

                    }

                }
            }
            catch
            {
                resultSet = new string[0];
                FileManager.Log("Parse Error!!", 2);
            }
            return resultSet;
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
