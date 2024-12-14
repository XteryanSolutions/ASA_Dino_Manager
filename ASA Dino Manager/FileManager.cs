using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace ASA_Dino_Manager
{

    class FileManager
    {

        public static DateTime TimeStart = DateTime.UtcNow;
        public static bool RunOnce = true; // toggle off at init

        public static string AppPath = "";
        public static string GamePath = "";
        public static bool Scanning = false;
        public static string LogText = "";

        public static string ColorString = "";
        public static string DefaultColor = "#8cabff;#a1c5f7;#6378ab;#f77777;#f7ada1;#e3d788;#7b818a;#b0b0b0;#dbb172;#65e05a;#23a11a;#b267cf;";
        public static bool needSave = false;

        public static bool InitFileManager()
        {
            try
            {
                if (RunOnce) { TimeStart = DateTime.UtcNow; RunOnce = false; }

                //AppPath = Path.GetDirectoryName(Application.ExecutablePath).ToString();

                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                AppPath = Path.GetDirectoryName(assemblyPath);

                if (!Directory.Exists(AppPath + @"\Logs")) { Directory.CreateDirectory(AppPath + @"\Logs"); }
                if (!Directory.Exists(AppPath + @"\Data")) { Directory.CreateDirectory(AppPath + @"\Data"); }

                // if (!LoadColorFile()) { return false; }

                if (LoadPath()) // loaded
                {
                    if (CheckPath(GamePath)) { return true; }
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
                    return true;
                }
            }
            catch { }
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

        public static bool LoadColorFile()
        {
            try
            {
                if (File.Exists(AppPath + @"\Data\colors.hrv"))
                {
                    using (StreamReader read = new StreamReader(AppPath + @"\Data\colors.hrv"))
                    {
                        string line; 
                        while ((line = read.ReadLine()) != null)
                        {
                            ColorString = line; 
                        }
                    }
                    return true;
                }
                else
                {
                    ColorString = DefaultColor;
                    if (SaveColorFile()) { return true; }
                    else { return false; }
                    }
            }
            catch { }
            return false;
        }

        public static bool SaveColorFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(AppPath + @"\Data\colors.hrv"))
                {
                    writer.WriteLine(ColorString);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ScanPath()
        {
            if (!Scanning)
            {
                bool result = true;
                FileManager.Log("Scanning for gamePath...");
                
                Thread thread = new Thread(delegate () { // start calculation thread
                    try
                    {
                        Scanning = true;

                        string filepath = FileManager.FindFilePath(@"ShooterGame\Saved\DinoExports");

                        if (CheckPath(filepath)) // check if the path we found works
                        {
                            GamePath = filepath;
                            using (StreamWriter writer = new StreamWriter(AppPath + @"\Data\config.hrv"))
                            {
                                writer.WriteLine(filepath);
                            }
                            FileManager.Log("Set New gamePath");
                            Scanning = false;
                        }
                        else
                        {
                            // Application.Exit();
                            FileManager.Log("Didnt find gamePath");
                            Application.Current.Quit();
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
                needSave = true;
                SaveFiles();
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
                    FileManager.Log("Saved DataBase");
                    needSave = false;
                }
                catch
                {
                    Console.WriteLine("File write failure");
                }
            }
           
        }

        public static void Log(string text)
        {
            LogText += text + Environment.NewLine;
            //Console.WriteLine(text);
            System.Diagnostics.Debug.WriteLine(text);
        }

        public static void WriteLog()
        {
            try
            {
                if (LogText != "")
                {
                    File.AppendAllText(AppPath + @"\Logs\log_" + TimeStart.ToString().Replace(@"/", ".").Replace(@":", ".") + ".txt", LogText);
                    LogText = "";
                }
            }
            catch
            {
                Console.WriteLine("Could not write to log file");
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
                FileManager.Log("Parse Error!!");
            }
            return resultSet;
        }

        public static string FindFilePath(string FolderName = @"ShooterGame\Saved\DinoExports")
        {
            string result = "";
            bool found = false;
            foreach (var drive in DriveInfo.GetDrives())
            {
                FileManager.Log("Scanning Drive " + drive.Name);
                var directories = GetDirectories(drive.Name);

                foreach (var data in directories)
                {
                    string dirName = data.ToString();
                    if (dirName.ToUpper().Contains(FolderName.ToUpper()))
                    {
                        result = dirName; found = true; FileManager.Log("found it"); break;
                    }
                }
                if (found) { break; }
            }
            if (!found)
            {
                FileManager.Log("didnt find anything");
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
