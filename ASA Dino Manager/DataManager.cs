using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Formats.Asn1;
using System.Globalization;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ASA_Dino_Manager
{
    class DataManager
    {
        // how many different dino classes do we have
        public static int classSize = 0;


        public static DataSet ImportsDataSet = new DataSet("importsDataSet");
        public static DataTable ImportsTable = new DataTable("importsList");

        public static DataSet FemaleDataSet = new DataSet("dinoDataSet");
        public static DataTable FemaleTable = new DataTable("dinoList");

        public static DataSet MaleDataSet = new DataSet("dinoDataSet");
        public static DataTable MaleTable = new DataTable("dinoList");

        public static DataSet BottomDataSet = new DataSet("dinoDataSet");
        public static DataTable BottomTable = new DataTable("dinoList");

        public static DataSet StatDataSet = new DataSet("dinoDataSet");
        public static DataTable StatTable = new DataTable("dinoList");

        public static DataSet ComboDataSet = new DataSet("comboDataSet"); // used for combining the best breed
        public static DataTable ComboTable = new DataTable("comboList");

        public static DataSet ArchiveDataSet = new DataSet("dinoDataSet");
        public static DataTable ArchiveTable = new DataTable("dinoList");

        public static int ModC = 0;  // keep track of updated dinos
        public static int AddC = 0;  // keep track of added dinos

        public static string DecimalSeparator = "";
        private static string ThousandsSeparator = "";

        private static readonly CultureInfo Culture = Thread.CurrentThread.CurrentCulture;

        // dino stats
        public static double LevelMax = 0;
        public static double HpMax = 0;
        public static double StaminaMax = 0;
        public static double OxygenMax = 0;
        public static double FoodMax = 0;
        public static double WeightMax = 0;
        public static double DamageMax = 0;
        public static double SpeedMax = 0;
        public static string[] BinaryM = new string[1]; //keeping track of max stats for breeding
        public static string[] BinaryF = new string[1]; //keeping track of max stats for breeding


        public static bool InitDataManager()
        {
            try
            {
                DecimalSeparator = Culture.NumberFormat.CurrencyDecimalSeparator;
                ThousandsSeparator = Culture.NumberFormat.CurrencyGroupSeparator;


                ImportsDataSet.Tables.Add(ImportsTable);
                FemaleDataSet.Tables.Add(FemaleTable);
                MaleDataSet.Tables.Add(MaleTable);
                BottomDataSet.Tables.Add(BottomTable);
                StatDataSet.Tables.Add(StatTable);
                ComboDataSet.Tables.Add(ComboTable);
                ArchiveDataSet.Tables.Add(ArchiveTable);


                ImportsTable.Clear();
                ImportsTable.Columns.Add("ID", typeof(string));
                ImportsTable.Columns.Add("Tag", typeof(string));
                ImportsTable.Columns.Add("Sex", typeof(string));
                ImportsTable.Columns.Add("Name", typeof(string));
                ImportsTable.Columns.Add("Level", typeof(string));
                ImportsTable.Columns.Add("HP", typeof(string));
                ImportsTable.Columns.Add("Stamina", typeof(string));
                ImportsTable.Columns.Add("Oxygen", typeof(string));
                ImportsTable.Columns.Add("Food", typeof(string));
                ImportsTable.Columns.Add("Weight", typeof(string));
                ImportsTable.Columns.Add("Damage", typeof(string));
                ImportsTable.Columns.Add("Speed", typeof(string));
                ImportsTable.Columns.Add("Mama", typeof(string));
                ImportsTable.Columns.Add("Papa", typeof(string));
                ImportsTable.Columns.Add("MamaMute", typeof(string));
                ImportsTable.Columns.Add("PapaMute", typeof(string));
                ImportsTable.Columns.Add("Gen", typeof(string));
                ImportsTable.Columns.Add("GenM", typeof(string));
                ImportsTable.Columns.Add("BabyAge", typeof(string));
                ImportsTable.Columns.Add("Neutered", typeof(string));
                ImportsTable.Columns.Add("Imprint", typeof(string));
                ImportsTable.Columns.Add("Imprinter", typeof(string));
                ImportsTable.Columns.Add("Tribe", typeof(string));
                ImportsTable.Columns.Add("Class", typeof(string));
                ImportsTable.Columns.Add("Time", typeof(string));
                ImportsTable.Columns.Add("Colors", typeof(string));




                FemaleTable.Clear();
                FemaleTable.Columns.Add("Status", typeof(string));
                FemaleTable.Columns.Add("Name", typeof(string));
                FemaleTable.Columns.Add("Level", typeof(double));
                // ==============
                FemaleTable.Columns.Add("HP", typeof(double));
                FemaleTable.Columns.Add("Stamina", typeof(double));
                FemaleTable.Columns.Add("Oxygen", typeof(double));
                FemaleTable.Columns.Add("Food", typeof(double));
                FemaleTable.Columns.Add("Weight", typeof(double));
                FemaleTable.Columns.Add("Damage", typeof(double));
                // ==============
                FemaleTable.Columns.Add("Speed", typeof(double));
                FemaleTable.Columns.Add("Gen", typeof(double));
                FemaleTable.Columns.Add("Papa", typeof(string));
                FemaleTable.Columns.Add("Mama", typeof(string));
                FemaleTable.Columns.Add("MamaMute", typeof(double));
                FemaleTable.Columns.Add("PapaMute", typeof(double));
                FemaleTable.Columns.Add("Age", typeof(double));
                FemaleTable.Columns.Add("Imprint", typeof(double));
                FemaleTable.Columns.Add("Imprinter", typeof(string));
                FemaleTable.Columns.Add("ID", typeof(string));



                MaleTable.Clear();
                MaleTable.Columns.Add("Status", typeof(string));
                MaleTable.Columns.Add("Name", typeof(string));
                MaleTable.Columns.Add("Level", typeof(double));
                // ==============
                MaleTable.Columns.Add("HP", typeof(double));
                MaleTable.Columns.Add("Stamina", typeof(double));
                MaleTable.Columns.Add("Oxygen", typeof(double));
                MaleTable.Columns.Add("Food", typeof(double));
                MaleTable.Columns.Add("Weight", typeof(double));
                MaleTable.Columns.Add("Damage", typeof(double));
                // ==============
                MaleTable.Columns.Add("Speed", typeof(double));
                MaleTable.Columns.Add("Gen", typeof(double));
                MaleTable.Columns.Add("Papa", typeof(string));
                MaleTable.Columns.Add("Mama", typeof(string));
                MaleTable.Columns.Add("MamaMute", typeof(double));
                MaleTable.Columns.Add("PapaMute", typeof(double));
                MaleTable.Columns.Add("Age", typeof(double));
                MaleTable.Columns.Add("Imprint", typeof(double));
                MaleTable.Columns.Add("Imprinter", typeof(string));
                MaleTable.Columns.Add("ID", typeof(string));




                BottomTable.Clear();
                BottomTable.Columns.Add("Status", typeof(string));
                BottomTable.Columns.Add("Name", typeof(string));
                BottomTable.Columns.Add("Level", typeof(double));
                // ==============
                BottomTable.Columns.Add("HP", typeof(double));
                BottomTable.Columns.Add("Stamina", typeof(double));
                BottomTable.Columns.Add("Oxygen", typeof(double));
                BottomTable.Columns.Add("Food", typeof(double));
                BottomTable.Columns.Add("Weight", typeof(double));
                BottomTable.Columns.Add("Damage", typeof(double));
                // ==============
                BottomTable.Columns.Add("Speed", typeof(double));
                BottomTable.Columns.Add("Gen", typeof(double));
                BottomTable.Columns.Add("Papa", typeof(string));
                BottomTable.Columns.Add("Mama", typeof(string));
                BottomTable.Columns.Add("MamaMute", typeof(double));
                BottomTable.Columns.Add("PapaMute", typeof(double));
                BottomTable.Columns.Add("Age", typeof(double));
                BottomTable.Columns.Add("Imprint", typeof(double));
                BottomTable.Columns.Add("Imprinter", typeof(string));
                BottomTable.Columns.Add("ID", typeof(string));




                ComboTable.Clear();
                ComboTable.Columns.Add("#", typeof(int));
                ComboTable.Columns.Add("P", typeof(string));
                ComboTable.Columns.Add("M", typeof(string));
                ComboTable.Columns.Add("gP", typeof(int));
                ComboTable.Columns.Add("aP", typeof(int));
                ComboTable.Columns.Add("agP", typeof(int));
                ComboTable.Columns.Add("bP", typeof(int));
                ComboTable.Columns.Add("res", typeof(string));



                StatTable.Clear();
                StatTable.Columns.Add("ID", typeof(string));
                StatTable.Columns.Add("Status", typeof(string));
                StatTable.Columns.Add("Notes", typeof(string));
                StatTable.Columns.Add("Mutes", typeof(string));




                ArchiveTable.Clear();
                ArchiveTable.Columns.Add("ID", typeof(string));
                ArchiveTable.Columns.Add("Tag", typeof(string));
                ArchiveTable.Columns.Add("Name", typeof(string));
                ArchiveTable.Columns.Add("Level", typeof(double));
                ArchiveTable.Columns.Add("Class", typeof(string));



                FileManager.LoadFiles();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string LongClassToShort(string longClass)
        {
            string dinoClass = "";
            if (longClass.ToUpper().Contains("MAGIC"))
            {
                // magicland stuff
                // <Class>/Forglar/Forglar_All/Dinos/MagicLand/Therizino_Character_BP_Magic.Therizino_Character_BP_Magic_C</Class>

                var split = longClass.Split(new[] { @"_Character_BP" }, StringSplitOptions.RemoveEmptyEntries);

                // <Class>/Forglar/Forglar_All/Dinos/MagicLand/Therizino    _Magic.Therizino_       _Magic_C</Class>

                dinoClass = split[1].Replace("_", "").Trim();// Magic.Therizino
                dinoClass = dinoClass.Replace(".", "_");// Magic_Therizino

            }
            else if (longClass.ToUpper().Contains("BIONIC"))
            {
                // bionic stuff (TEK)
                // <Class>/Game/PrimalEarth/Dinos/Stego/BionicStego_Character_BP.BionicStego_Character_BP_C</Class>
                var classSplit = longClass.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

                // <Class>  Game    PrimalEarth     Dinos   Stego   BionicStego_Character_BP.BionicStego_Character_BP_C<    Class>
                if (classSplit.Length > 2)
                {
                    dinoClass = $"TEK_{classSplit[3].Trim()}";
                }
            }
            else if (longClass.ToUpper().Contains("ABERRANT"))
            {
                // aberrant stuff
                // <Class>/Game/PrimalEarth/Dinos/Lystrosaurus/Lystro_Character_BP_Aberrant.Lystro_Character_BP_Aberrant_C</Class>

                var classSplit = longClass.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

                // <Class>  Game    PrimalEarth     Dinos   Lystrosaurus    Lystro_Character_BP_Aberrant.Lystro_Character_BP_Aberrant_C<    Class>

                if (classSplit.Length > 2)
                {
                    dinoClass = $"Aberrant_{classSplit[3].Trim()}";
                }
            }
            else
            {
                // normal stuff
                // <Class>/Game/PrimalEarth/Dinos/Doedicurus/Doed_Character_BP.Doed_Character_BP_C</Class>
                // <Class>/Game/PrimalEarth/Dinos/Lystrosaurus/Lystro_Character_BP.Lystro_Character_BP_C</Class>
                // <Class>/Game/PrimalEarth/Dinos/Ankylo/Ankylo_Character_BP.Ankylo_Character_BP_C</Class>

                var classSplit = longClass.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

                // <Class>  Game    PrimalEarth     Dinos   Doedicurus      Doed_Character_BP.Doed_Character_BP_C<      Class>
                // <Class>  Game    PrimalEarth     Dinos   Lystrosaurus    Lystro_Character_BP.Lystro_Character_BP_C<  Class>
                // <Class>  Game    PrimalEarth     Dinos   Ankylo          Ankylo_Character_BP.Ankylo_Character_BP_C<  Class>

                if (classSplit.Length > 2)
                {
                    dinoClass = classSplit[3].Trim();
                }
            }
            return dinoClass;
        }

        public static string[] GetAllClassesShort(string exclude = "")
        {
            // get the raw unprocessed classlist
            string[] classList = DataManager.GetAllDistinctColumnData("Class");

            // Define exclusions
            string[] excludes = { "N/A", "#", exclude };

            // Use a HashSet to store distinct values
            HashSet<string> resultSet = new HashSet<string>();

            // Iterate through the rows of the list
            foreach (string rawClass in classList)
            {
                string dinoClass = LongClassToShort(rawClass);

                // Check if the value is not excluded and is not null/empty
                if (!excludes.Contains(dinoClass) && !string.IsNullOrWhiteSpace(dinoClass))
                {
                    // Add the value to the result set (distinct)
                    resultSet.Add(dinoClass);
                }
            }

            // Return the distinct values as an array
            return resultSet.ToArray();
        }

        public static string ShortClassToLong(string shortClass)
        {
            string result = "";

            foreach (DataRow row in ImportsTable.Rows)
            {
                string longClass = row["Class"].ToString();


                if (shortClass.ToUpper().Contains("MAGIC"))
                {
                    // <Class>/Forglar/Forglar_All/Dinos/MagicLand/Therizino_Character_BP_Magic.Therizino_Character_BP_Magic_C</Class>
                    if (longClass.Contains(shortClass.Replace("_", ".")))
                    {
                        return row["Class"].ToString();
                    }
                }
                else if (shortClass.ToUpper().Contains("TEK"))
                {
                    // <Class>/Game/PrimalEarth/Dinos/Stego/BionicStego_Character_BP.BionicStego_Character_BP_C</Class>
                    if (longClass.Contains(shortClass.Replace("TEK_", "Bionic")))
                    {
                        return row["Class"].ToString();
                    }
                }
                else if (shortClass.ToUpper().Contains("ABERRANT"))
                {
                    // <Class>/Game/PrimalEarth/Dinos/Lystrosaurus/Lystro_Character_BP_Aberrant.Lystro_Character_BP_Aberrant_C</Class>
                    if (longClass.Contains("Aberrant"))
                    {
                        if (longClass.Contains(shortClass.Replace("Aberrant_", "")))
                        {
                            return row["Class"].ToString();
                        }
                    }
                }
                else
                {
                    // <Class>/Game/PrimalEarth/Dinos/Doedicurus/Doed_Character_BP.Doed_Character_BP_C</Class>
                    if (longClass.Contains(shortClass))
                    {
                        return row["Class"].ToString();
                    }
                }
            }
            return result;
        }

        public static string[] GetDistinctFilteredColumnData(string inColumn1, string inData, string inColumn2, string inData2, string outData, string exclude = "")
        {
            // Define exclusions
            string[] excludes = { "N/A", "#", exclude };

            // Use a HashSet to store distinct values
            HashSet<string> resultSet = new HashSet<string>();

            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Retrieve the values from the columns
                string value1 = row[inColumn1]?.ToString();
                string value2 = row[inColumn2]?.ToString();
                string outputValue = row[outData]?.ToString();

                // Apply the filtering conditions
                if (value1 == inData &&
                    value2 == inData2 &&
                    !excludes.Contains(value1) &&
                    !excludes.Contains(value2) &&
                    !string.IsNullOrWhiteSpace(outputValue))  // Ensure output is not empty or null
                {
                    // Add the output value to the result set (distinct)
                    resultSet.Add(outputValue);
                }
            }

            // Return the distinct values as an array
            return resultSet.ToArray();
        }

        public static string[] GetDistinctFilteredColumnDataFull(string outData, string exclude = "")
        {
            // Define exclusions
            string[] excludes = { "N/A", "#", exclude };

            // Use a HashSet to store distinct values
            HashSet<string> resultSet = new HashSet<string>();

            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Retrieve the values from the columns
                string value1 = row["BabyAge"]?.ToString();
                string outputValue = row[outData]?.ToString();

                // Apply the filtering conditions
                if (ToDouble(value1) == 1) // get the fullgrowns
                {
                    if (!string.IsNullOrWhiteSpace(outputValue))  // Ensure output is not empty or null
                    {
                        // Add the output value to the result set (distinct)
                        resultSet.Add(outputValue);
                    }
                }
            }

            // Return the distinct values as an array
            return resultSet.ToArray();
        }

        public static string[] GetDistinctFilteredColumnDataB(string inColumn2, string inData2, string outData, string exclude = "")
        {
            // Define exclusions
            string[] excludes = { "N/A", "#", exclude };

            // Use a HashSet to store distinct values
            HashSet<string> resultSet = new HashSet<string>();


            // make a list of id's to exclude that has fullgrown
            string[] grownUps = DataManager.GetDistinctFilteredColumnDataFull("ID");


            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Retrieve the values from the columns
                string value1 = row["BabyAge"]?.ToString();
                string value2 = row[inColumn2]?.ToString();
                string outputValue = row[outData]?.ToString();

                // Apply the filtering conditions
                if (ToDouble(value1) < 1) // get only the babies
                {
                    if (value2 == inData2 &&
                        !excludes.Contains(value2) &&
                        !grownUps.Contains(outputValue) && // make sure we exclude grownUps
                        !string.IsNullOrWhiteSpace(outputValue))  // Ensure output is not empty or null
                    {
                        // Add the output value to the result set (distinct)
                        resultSet.Add(outputValue);
                    }
                }
            }

            // Return the distinct values as an array
            return resultSet.ToArray();
        }

        public static string[] GetAllDistinctColumnData(string inColumn, string exclude = "")
        {
            // Define exclusions
            string[] excludes = { "N/A", "#", exclude };

            // Use a HashSet to store distinct values
            HashSet<string> resultSet = new HashSet<string>();

            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Retrieve the value from the specified column
                string columnValue = row[inColumn]?.ToString();

                // Check if the value is not excluded and is not null/empty
                if (!excludes.Contains(columnValue) && !string.IsNullOrWhiteSpace(columnValue))
                {
                    // Add the value to the result set (distinct)
                    resultSet.Add(columnValue);
                }
            }

            // Return the distinct values as an array
            return resultSet.ToArray();
        }

        public static string GetFirstColumnData(string inColumn, string inData, string outColumn, string exclude = "")
        {
            // get first non empty field
            string[] excludes = { "N/A", "#", exclude };
            string result = "";

            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Match the input column value
                if (row[inColumn]?.ToString() == inData && !excludes.Contains(row[inColumn]?.ToString()))
                {
                    // Get result if the output column value is not excluded
                    if (!excludes.Contains(row[outColumn]?.ToString()))
                    {
                        return row[outColumn]?.ToString();
                    }
                }
            }
            return result;
        }

        public static string GetLastColumnData(string inColumn, string inData, string outColumn, string exclude = "")
        {
            // Define excluded values
            string[] excludes = { "N/A", "#", exclude };
            string result = "";

            // Iterate through the rows of the table
            foreach (DataRow row in ImportsTable.Rows)
            {
                // Match the input column value
                if (row[inColumn]?.ToString() == inData && !excludes.Contains(row[inColumn]?.ToString()))
                {
                    // Update result if the output column value is not excluded
                    if (!excludes.Contains(row[outColumn]?.ToString()) &&
                        !string.IsNullOrWhiteSpace(row[outColumn]?.ToString())) // Avoid empty/whitespace values
                    {
                        result = row[outColumn]?.ToString();
                    }
                }
            }
            return result;
        }

        public static List<string[]> GetFirstStats(string[] dinos)
        {
            var results = new List<string[]>();
            foreach (string id in dinos)
            {
                string[] columns = {
                    "Name", "Level", "HP", "Stamina", "Oxygen", "Food", "Weight", "Damage", "Speed"
                    , "Mama", "Papa", "MamaMute", "PapaMute", "Gen", "GenM", "BabyAge", "Neutered", "Imprint"
                    , "Imprinter", "Tribe"
                };
                string[] stats = new string[columns.Count()];
                int i = 0;
                while (i < columns.Count())
                {
                    stats[i] = DataManager.GetFirstColumnData("ID", id, columns[i]);
                    i++;
                }
                results.Add(stats);
            }
            return results;
        }

        public static List<string[]> GetLastStats(string[] dinos)
        {
            var results = new List<string[]>();
            foreach (string id in dinos)
            {
                string[] columns = {
                    "Name", "Level", "HP", "Stamina", "Oxygen", "Food", "Weight", "Damage", "Speed"
                    , "Mama", "Papa", "MamaMute", "PapaMute", "Gen", "GenM", "BabyAge", "Neutered", "Imprint"
                    , "Imprinter", "Tribe"
                };
                string[] stats = new string[columns.Count()];
                int i = 0;
                while (i < columns.Count())
                {
                    stats[i] = DataManager.GetLastColumnData("ID", id, columns[i], "0");
                    i++;
                }
                results.Add(stats);
            }
            return results;
        }

        public static List<Tuple<double, string, double, bool, bool, double, double>> GetDinoAgingData(string id)
        {
            var list = new List<Tuple<double, string, double, bool, bool, double, double>>();

            // all time values are in seconds to retain accuracy
            /////////////////////////////////////////////////////////////////////////
            // make a tuple containing -> agingRate, fullGrownDate, growUpTime, isBaby, beenBaby, fullTime, currentAge
            ////////////////////////////////////////////////////////////////////////
            ///  3,45%/hr , 
            try
            {
                double currentAge = 0;
                double agingRate = GetGrowthRate(id); // get agingRate
                string fullGrown = "N/A";
                double growUpTime = 0;
                double fullTime = 0;

                // get last known age and time
                double lastAge = ToDouble(GetLastColumnData("ID", id, "BabyAge"));
                string lastTime = GetLastColumnData("ID", id, "Time");

                // get first known age and time
                double firstAge = ToDouble(GetFirstColumnData("ID", id, "BabyAge"));
                string firstTime = GetFirstColumnData("ID", id, "Time");

                // convert units
                double ageRateHr = agingRate * 3600; // age rate to % per hr
                double lastAgeP = lastAge * 100;
                double firstAgeP = firstAge * 100;


                DateTime startTime1 = DateTime.ParseExact(firstTime, "dd/MM/yyyy HH:mm:ss", null);
                DateTime endTime1 = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);
                // Calculate differences
                double timeDiffSeconds = (endTime1 - startTime1).TotalSeconds;
                double ageDiff = (lastAge - firstAge) * 100;

                if (timeDiffSeconds > 0)
                {
                    // get accurate unRounded ageRate
                    agingRate = ageDiff / timeDiffSeconds;
                }

                // do we or do we not have a Xanax detector (baby detector)
                // figure out if it has ever been a baby or do we just mark it as tamed
                bool isBaby = false; bool beenBaby = false;
                if (firstAge < 1) // we have an entry with age younger than one
                {
                    beenBaby = true;
                    if (lastAge < 1) // and the last known age is also under 1
                    {
                        isBaby = true; // so baby it is (at this point in time)
                    }
                }

                if (!beenBaby) // never been a baby so this is probably a fresh Tame
                {
                    fullGrown = firstTime; // set fullgrown to the time it was tamed
                }
                else
                {
                    // if we have aging rate here we can calculate how long it takes for this dino to grow from 0 to 100%
                    if (agingRate > 0)
                    {
                        growUpTime = 1 / agingRate;

                        // if its still a baby we need to calculate when its about to grow up
                        if (isBaby) // wich we also need the agingRate for
                        {
                            // 100% - currentAge and we get how many % left to grow
                            double AgeLeft = 100 - lastAgeP;

                            double timeleft = AgeLeft / ageRateHr; // time left at last data

                            // Convert seconds to TimeSpan
                            TimeSpan timeL = TimeSpan.FromHours(timeleft);

                            // Parse lastTime string to DateTime using the exact known format
                            DateTime lastTimeD = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);

                            // Add the TimeSpan to DateTime
                            DateTime fullGrownDate = lastTimeD.Add(timeL);

                            // Convert the result back to a string in the same format
                            fullGrown = fullGrownDate.ToString("dd/MM/yyyy HH:mm:ss");


                            DateTime endTime = DateTime.ParseExact(fullGrown, "dd/MM/yyyy HH:mm:ss", null);
                            DateTime startTime = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);
                            DateTime nowTime = DateTime.Now;

                            // timepassed since data = nowTime - lastTimeD
                            double timePassed = (nowTime - lastTimeD).TotalSeconds;

                            fullTime = (endTime - startTime).TotalSeconds;
                            fullTime = fullTime - timePassed;

                            double passedAge = timePassed * agingRate;

                            currentAge = lastAgeP + passedAge;

                            double leftToGrow = 100 - currentAge;
                            double time = leftToGrow / ageRateHr;
                        }
                        else
                        {
                            // when did it reach fullGrown
                            fullGrown = GrowUpTime(id);

                            DateTime endTime = DateTime.ParseExact(fullGrown, "dd/MM/yyyy HH:mm:ss", null);
                            DateTime startTime = DateTime.ParseExact(firstTime, "dd/MM/yyyy HH:mm:ss", null);

                            fullTime = (endTime - startTime).TotalSeconds;
                        }
                    }
                    else // if we dont have aging rate we can atleast show the last known % and calc from there
                    {
                        currentAge = lastAgeP;

                        DateTime lastTimeD = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);
                        fullGrown = lastTimeD.ToString("dd/MM/yyyy HH:mm:ss");

                    }
                }


                list.Add(new Tuple<double, string, double, bool, bool, double, double>(agingRate, fullGrown, growUpTime, isBaby, beenBaby, fullTime, currentAge));
            }
            catch
            {
                list.Add(new Tuple<double, string, double, bool, bool, double, double>(0, "N/A", 0, false, false, 0, 1));
            }

            // FileManager.Log($"HUPP", 0);

            return list;
        }

        public static string GrowUpTime(string id)
        {
            string result = "N/A";
            foreach (DataRow row in ImportsTable.Rows)
            {
                if (row["ID"]?.ToString() == id)
                {
                    string timeValue = row["Time"].ToString();
                    string ageValue = row["BabyAge"].ToString();
                    // make sure both fields have data
                    if (timeValue != "" && ToDouble(ageValue) > 0)
                    {
                        if (ToDouble(ageValue) == 1)
                        {
                            // time when dino reached fullgrown
                            result = timeValue;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static double GetGrowthRate(string id)
        {
            try
            {
                // Filter rows by ID and ensure valid data
                var filteredRows = ImportsTable.AsEnumerable()
                .Where(row => row["ID"]?.ToString() == id &&
                              !string.IsNullOrEmpty(row["Time"]?.ToString()) &&
                              !string.IsNullOrEmpty(row["BabyAge"]?.ToString()) &&
                              ToDouble(row["BabyAge"].ToString()) > 0 &&
                              ToDouble(row["BabyAge"].ToString()) < 1)
                .OrderBy(row => DateTime.ParseExact(row["Time"].ToString(), "dd/MM/yyyy HH:mm:ss", null))
                .ToList();

                if (filteredRows.Count < 2)
                {
                    // Not enough data to calculate aging rate
                    return 0;
                }

                // Get the last two rows (most recent entries)
                var secondLastRow = filteredRows[filteredRows.Count - 2];
                var lastRow = filteredRows[filteredRows.Count - 1];

                // Parse times and ages
                DateTime secondLastTime = DateTime.ParseExact(secondLastRow["Time"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                DateTime lastTime = DateTime.ParseExact(lastRow["Time"].ToString(), "dd/MM/yyyy HH:mm:ss", null);

                double secondLastAge = ToDouble(secondLastRow["BabyAge"].ToString());
                double lastAge = ToDouble(lastRow["BabyAge"].ToString());

                // Ensure chronological order and valid data
                if (lastTime > secondLastTime && lastAge >= secondLastAge)
                {
                    // Calculate differences
                    double timeDiffSeconds = (lastTime - secondLastTime).TotalSeconds;
                    double ageDiff = (lastAge - secondLastAge) * 100;

                    if (timeDiffSeconds > 0)
                    {
                        // get accurate unRounded ageRate
                        double ageRatePerSecond = ageDiff / timeDiffSeconds;
                        return ageRatePerSecond;
                    }
                }

                // Handle invalid data (e.g., younger age or incorrect time order)
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static void EditBreedStats(string id, string level, string hp, string st, string ox, string fo, string we, string da, string notes)
        {
            if (id != "")
            {
                SetNotes(id, notes);
                int rowID = 0;
                foreach (DataRow row in ImportsTable.Rows)
                {
                    if (id == row["ID"].ToString()) // found dino to edit
                    {
                        ImportsTable.Rows[rowID].SetField("Level", level);
                        ImportsTable.Rows[rowID].SetField("Hp", hp);
                        ImportsTable.Rows[rowID].SetField("Stamina", st);
                        ImportsTable.Rows[rowID].SetField("Oxygen", ox);
                        ImportsTable.Rows[rowID].SetField("Food", fo);
                        ImportsTable.Rows[rowID].SetField("Weight", we);
                        ImportsTable.Rows[rowID].SetField("Damage", da);

                        break;
                    }
                    rowID++;
                }
            }
        }

        public static int DinoCount(string dinoTag, int toggle = 0)
        {
            // count dinos and only include based on their status and what view is toggled 0 to count all
            int count = 0;
            try
            {
                // Retrieve female data
                string[] females = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Female", "ID");
                foreach (string dino in females)
                {
                    string group = GetGroup(dino);
                    if (toggle == 0) { count++; }
                    else if (toggle == 1) { if (group != "Exclude" && group != "Archived") { count++; } }
                    else if (toggle == 2) { if (group != "" && group != "Archived") { count++; } }
                    else if (toggle == 3) { if (group != "Exclude" && group != "") { count++; } }
                }
                // Retrieve male data
                string[] males = DataManager.GetDistinctFilteredColumnData("Class", dinoTag, "Sex", "Male", "ID");
                foreach (string dino in males)
                {
                    string group = GetGroup(dino);
                    if (toggle == 0) { count++; }
                    else if (toggle == 1) { if (group != "Exclude" && group != "Archived") { count++; } }
                    else if (toggle == 2) { if (group != "" && group != "Archived") { count++; } }
                    else if (toggle == 3) { if (group != "Exclude" && group != "") { count++; } }
                }
            }
            catch { }

            return count;
        }

        public static string GetMutes(string id)
        {
            foreach (DataRow row in DataManager.StatTable.Rows)
            {
                if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                {
                    return row["Mutes"].ToString();
                }
            }
            return "";
        }

        public static string GetGroup(string id)
        {
            foreach (DataRow row in DataManager.StatTable.Rows)
            {
                if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                {
                    return row["Status"].ToString();
                }
            }
            return "";
        }

        public static string GetNotes(string id)
        {
            foreach (DataRow row in DataManager.StatTable.Rows)
            {
                if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                {
                    return row["Notes"].ToString();
                }
            }
            return "";
        }

        public static void SetGroup(string id, string group)
        {
            if (id != "")
            {
                int rowid = 0; bool found = false;
                foreach (DataRow row in DataManager.StatTable.Rows)
                {
                    if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                    {
                        StatTable.Rows[rowid].SetField("Status", group);
                        FileManager.Log($"Set group for id: {id} to: {group}", 0);
                        found = true; break;
                    }
                    rowid++;
                }
                if (!found)
                {
                    DataRow dr = DataManager.StatTable.NewRow();
                    dr["ID"] = id;
                    dr["Status"] = group;
                    dr["Mutes"] = "";
                    dr["Notes"] = "";
                    DataManager.StatTable.Rows.Add(dr);
                }
                // request a save after modifying data
                FileManager.needSave = true;
            }
        }

        public static void SetNotes(string id, string notes)
        {
            int rowid = 0; bool found = false;
            foreach (DataRow row in DataManager.StatTable.Rows)
            {
                if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                {
                    StatTable.Rows[rowid].SetField("Notes", notes);
                    found = true; break;
                }
                rowid++;
            }
            if (!found)
            {
                DataRow dr = DataManager.StatTable.NewRow();
                dr["ID"] = id;
                dr["Notes"] = notes;
                dr["Status"] = "";
                dr["Mutes"] = "";
                DataManager.StatTable.Rows.Add(dr);
            }
            // request a save after modifying data
            FileManager.needSave = true;
        }

        public static void SetMutes(string id, string mutes)
        {
            int rowid = 0; bool found = false;
            foreach (DataRow row in DataManager.StatTable.Rows)
            {
                if (id == row["ID"].ToString()) // did we find our dino in dinoData file
                {
                    StatTable.Rows[rowid].SetField("Mutes", mutes);
                    found = true; break;
                }
                rowid++;
            }
            if (!found)
            {
                DataRow dr = DataManager.StatTable.NewRow();
                dr["ID"] = id;
                dr["Mutes"] = mutes;
                dr["Status"] = "";
                dr["Notes"] = "";
                DataManager.StatTable.Rows.Add(dr);
            }
        }

        private static void ProcessDinos(string[] dinos, List<string[]> MainStats, List<string[]> BrStats, DataTable table)
        {
            int rowID = 0;
            foreach (var dino in dinos)
            {
                string group = GetGroup(dino);


                //MUTATION DETECTION SYSTEM HERE
                string mamaID = BrStats[rowID][9].ToString();
                string papaID = BrStats[rowID][10].ToString();

                string a = "0"; string b = "0"; string c = "0";
                string d = "0"; string e = "0"; string f = "0";

                if (mamaID != "" && papaID != "")
                {
                    double dinoHP = Math.Round(ToDouble(MainStats[rowID][2].ToString()));
                    double mamaHP = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "HP")));
                    double papaHP = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "HP")));
                    if ((mamaHP != 0 && papaHP != 0) && (dinoHP != papaHP && dinoHP != mamaHP))
                    { a = "1"; }
                    double dinoStamina = Math.Round(ToDouble(MainStats[rowID][3].ToString()));
                    double mamaStamina = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "Stamina")));
                    double papaStamina = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "Stamina")));
                    if ((mamaStamina != 0 && papaStamina != 0) && (dinoStamina != papaStamina && dinoStamina != mamaStamina))
                    { b = "1"; }
                    double dinoOxygen = Math.Round(ToDouble(MainStats[rowID][4].ToString()));
                    double mamaOxygen = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "Oxygen")));
                    double papaOxygen = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "Oxygen")));
                    if ((mamaOxygen != 0 && papaOxygen != 0) && (dinoOxygen != papaOxygen && dinoOxygen != mamaOxygen))
                    { c = "1"; }
                    double dinoFood = Math.Round(ToDouble(MainStats[rowID][5].ToString()));
                    double mamaFood = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "Food")));
                    double papaFood = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "Food")));
                    if ((mamaFood != 0 && papaFood != 0) && (dinoFood != papaFood && dinoFood != mamaFood))
                    { d = "1"; }
                    double dinoWeight = Math.Round(ToDouble(MainStats[rowID][6].ToString()));
                    double mamaWeight = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "Weight")));
                    double papaWeight = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "Weight")));
                    if ((mamaWeight != 0 && papaWeight != 0) && (dinoWeight != papaWeight && dinoWeight != mamaWeight))
                    { e = "1"; }
                    double dinoDamage = Math.Round(ToDouble(MainStats[rowID][7].ToString()), 2);
                    double mamaDamage = Math.Round(ToDouble(GetFirstColumnData("ID", mamaID, "Damage")), 2);
                    double papaDamage = Math.Round(ToDouble(GetFirstColumnData("ID", papaID, "Damage")), 2);
                    if ((mamaDamage != 0 && papaDamage != 0) && (dinoDamage != papaDamage && dinoDamage != mamaDamage))
                    {
                        f = "1";
                    }
                }
                string mutes = a + b + c + d + e + f;

                SetMutes(dino, mutes);
                int toggle = DinoPage.ToggleExcluded;


                bool addIT = false;
                if (toggle == 0)
                {
                    if (group != "Archived")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 1)
                {
                    if (group != "Archived" && group != "Exclude")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 2)
                {
                    if (group != "Archived" && group == "Exclude")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 3)
                {
                    if (group == "Archived")
                    {
                        addIT = true;
                    }
                }

                if (addIT)
                {
                    // Fill the DataRow
                    DataRow dr = table.NewRow();
                    dr["ID"] = dino;
                    dr["Name"] = BrStats[rowID][0].ToString();
                    dr["Mama"] = GetLastColumnData("ID", BrStats[rowID][9].ToString(), "Name", "");
                    dr["Papa"] = GetLastColumnData("ID", BrStats[rowID][10].ToString(), "Name", "");
                    dr["Imprinter"] = BrStats[rowID][18].ToString();
                    dr["Level"] = ToDouble(MainStats[rowID][1].ToString());
                    dr["Hp"] = Math.Round(ToDouble(MainStats[rowID][2].ToString()), 1);
                    dr["Stamina"] = Math.Round(ToDouble(MainStats[rowID][3].ToString()), 1);
                    dr["Oxygen"] = Math.Round(ToDouble(MainStats[rowID][4].ToString()), 1);
                    dr["Food"] = Math.Round(ToDouble(MainStats[rowID][5].ToString()), 1);
                    dr["Weight"] = Math.Round(ToDouble(MainStats[rowID][6].ToString()), 1);
                    dr["Damage"] = Math.Round((ToDouble(MainStats[rowID][7].ToString()) + 1) * 100, 1);
                    dr["Speed"] = Math.Round((ToDouble(BrStats[rowID][8].ToString()) + 1) * 100);
                    dr["Gen"] = ToDouble(BrStats[rowID][13].ToString());
                    dr["MamaMute"] = ToDouble(BrStats[rowID][11].ToString());
                    dr["PapaMute"] = ToDouble(BrStats[rowID][12].ToString());
                    dr["Age"] = Math.Round(ToDouble(BrStats[rowID][15].ToString()) * 100);
                    dr["Imprint"] = Math.Round(ToDouble(BrStats[rowID][17].ToString()) * 100);
                    dr["Status"] = CalcStatus(dino);


                    table.Rows.Add(dr);
                }

                rowID++;
            }
        }

        public static string CalcStatus(string id)
        {
            string status = "";

            // get first known age and time
            string firstTime = DataManager.GetFirstColumnData("ID", id, "Time");
            double firstAge = DataManager.ToDouble(DataManager.GetFirstColumnData("ID", id, "BabyAge")) * 100;

            // get last known age and time
            double lastAge = DataManager.ToDouble(DataManager.GetLastColumnData("ID", id, "BabyAge")) * 100;
            string lastTime = DataManager.GetLastColumnData("ID", id, "Time");

            // convert units   
            //DateTime lastTimeD = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);

            bool isBaby = false; bool beenBaby = false;
            if (firstAge < 100) // get the growth stage of the dino
            {
                beenBaby = true;
            }
            if (lastAge < 100)
            {
                isBaby = true;
            }

            if (!beenBaby) // its a tame just add info on when it was tamed
            {
                DateTime firstTimeD = DateTime.ParseExact(firstTime, "dd/MM/yyyy HH:mm:ss", null);
                status = "[tameSym]" + firstTimeD.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                if (isBaby)
                {
                    status = $"[breedSym]{Math.Round(lastAge, 1)}%";
                }
                else
                {
                    status = "[grownSym]" + GrowUpTime(id);
                }
            }


            return status;
        }

        public static void GetDinoData(string DinoClass, string sortiM = "", string sortiF = "")
        {
            if (string.IsNullOrEmpty(DinoClass))
            {
                return; // Exit early if tag is empty
            }

            // Clear the tables before populating them
            DataManager.MaleTable.Clear();
            DataManager.FemaleTable.Clear();

            // Retrieve female data
            string[] females = DataManager.GetDistinctFilteredColumnData("Class", DinoClass, "Sex", "Female", "ID");

            // Retrieve male data
            string[] males = DataManager.GetDistinctFilteredColumnData("Class", DinoClass, "Sex", "Male", "ID");


            if (DinoPage.CurrentStats)
            {
                // Process females
                List<string[]> MainStatsF = DataManager.GetLastStats(females);
                List<string[]> BrStatsF = DataManager.GetLastStats(females);
                ProcessDinos(females, MainStatsF, BrStatsF, DataManager.FemaleTable);

                // Process males
                List<string[]> MainStatsM = DataManager.GetLastStats(males);
                List<string[]> BrStatsM = DataManager.GetLastStats(males);
                ProcessDinos(males, MainStatsM, BrStatsM, DataManager.MaleTable);
            }
            else
            {
                // Process females
                List<string[]> MainStatsF = DataManager.GetFirstStats(females);
                List<string[]> BrStatsF = DataManager.GetLastStats(females);
                ProcessDinos(females, MainStatsF, BrStatsF, DataManager.FemaleTable);

                // Process males
                List<string[]> MainStatsM = DataManager.GetFirstStats(males);
                List<string[]> BrStatsM = DataManager.GetLastStats(males);
                ProcessDinos(males, MainStatsM, BrStatsM, DataManager.MaleTable);
            }



            // Sort the MaleTable based on the desired column
            DataView view1 = new DataView(DataManager.MaleTable);
            view1.Sort = sortiM;
            DataManager.MaleTable = view1.ToTable();


            // Sort the FemaleTable based on the desired column
            DataView view2 = new DataView(DataManager.FemaleTable);
            view2.Sort = sortiF;
            DataManager.FemaleTable = view2.ToTable();


            //  FileManager.Log("updated data");
        }

        public static void GetDinoBabies(string sortiM = "", string sortiF = "")
        {
            // Clear the tables before populating them
            DataManager.MaleTable.Clear();
            DataManager.FemaleTable.Clear();

            // Retrieve female babies data
            string[] females = DataManager.GetDistinctFilteredColumnDataB("Sex", "Female", "ID");

            // Retrieve male babies data
            string[] males = DataManager.GetDistinctFilteredColumnDataB("Sex", "Male", "ID");


            // Process females
            List<string[]> MainStatsF = DataManager.GetFirstStats(females);
            List<string[]> BrStatsF = DataManager.GetLastStats(females);
            ProcessDinoBabies(females, MainStatsF, BrStatsF, DataManager.FemaleTable);

            // Process males
            List<string[]> MainStatsM = DataManager.GetFirstStats(males);
            List<string[]> BrStatsM = DataManager.GetLastStats(males);
            ProcessDinoBabies(males, MainStatsM, BrStatsM, DataManager.MaleTable);


            // replace column names with the real ones
            sortiM = sortiM.Replace($"{Shared.breedSym}Age", "Hp");
            sortiF = sortiF.Replace($"{Shared.breedSym}Age", "Hp");

            sortiM = sortiM.Replace($"{Shared.timeSym}Time", "Stamina");
            sortiF = sortiF.Replace($"{Shared.timeSym}Time", "Stamina");

            sortiM = sortiM.Replace($"{Shared.speedSym}Rate", "Oxygen");
            sortiF = sortiF.Replace($"{Shared.speedSym}Rate", "Oxygen");



            // Sort the MaleTable based on the desired column
            DataView view1 = new DataView(DataManager.MaleTable);
            view1.Sort = sortiM;
            DataManager.MaleTable = view1.ToTable();


            // Sort the FemaleTable based on the desired column
            DataView view2 = new DataView(DataManager.FemaleTable);
            view2.Sort = sortiF;
            DataManager.FemaleTable = view2.ToTable();


            //  FileManager.Log("updated data");
        }

        private static void ProcessDinoBabies(string[] dinos, List<string[]> MainStats, List<string[]> BrStats, DataTable table)
        {
            int rowID = 0;
            foreach (var dino in dinos)
            {
                string group = GetGroup(dino);
                int toggle = BabyPage.ToggleExcluded;

                bool addIT = false;
                if (toggle == 0)
                {
                    if (group != "Archived")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 1)
                {
                    if (group != "Archived" && group != "Exclude")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 2)
                {
                    if (group != "Archived" && group == "Exclude")
                    {
                        addIT = true;
                    }
                }
                else if (toggle == 3)
                {
                    if (group == "Archived")
                    {
                        addIT = true;
                    }
                }




                if (addIT)
                {
                    string id = dino;
                    // get first known age and time
                    double firstAge = ToDouble(GetFirstColumnData("ID", id, "BabyAge")) * 100;
                    string firstTime = GetFirstColumnData("ID", id, "Time");


                    // get last known age and time
                    double lastAge = ToDouble(GetLastColumnData("ID", id, "BabyAge")) * 100;
                    string lastTime = GetLastColumnData("ID", id, "Time");
                    double knownAgeLeft = 100 - lastAge;

                    // convert units
                    DateTime firstTimeD = DateTime.ParseExact(firstTime, "dd/MM/yyyy HH:mm:ss", null);
                    DateTime lastTimeD = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", null);


                    // Calculate differences
                    double timeDiffMinutes = (lastTimeD - firstTimeD).TotalMinutes;
                    double ageDiff = lastAge - firstAge;



                    // Calculate rates age % per minute
                    double ageRate = ageDiff / timeDiffMinutes;


                    // Estimate data
                    DateTime nowTime = DateTime.Now;

                    double timePassed = (nowTime - lastTimeD).TotalMinutes;
                    double agePassed = timePassed * ageRate;


                    // Convert minutes to TimeSpan
                    TimeSpan timePassedD = TimeSpan.FromMinutes(timePassed);

                    double estimatedAge = lastAge + agePassed;
                    double estAgeLeft = 100 - estimatedAge;


                    double estTimeLeft = estAgeLeft / ageRate;


                    double LastTimeLeft = knownAgeLeft / ageRate; // time left at last data

                    DateTime dateT = DateTime.Now;

                    if (ageRate > 0)
                    {
                        if (!double.IsNaN(LastTimeLeft))
                        {
                            TimeSpan timePa = TimeSpan.FromMinutes(LastTimeLeft);
                            dateT = lastTimeD + timePa;
                        }
                    }




                    string ageT = "0"; // hp column
                    string timeT = "0"; // stamina column    // time left in minutes
                    string rateT = "0";  // oxygen column
                    //string dateT = "N/A";  // food column



                    // Fill the DataRow
                    DataRow dr = table.NewRow();
                    dr["ID"] = dino;
                    dr["Name"] = BrStats[rowID][0].ToString();
                    dr["Mama"] = GetLastColumnData("ID", BrStats[rowID][9].ToString(), "Name", "");
                    dr["Papa"] = GetLastColumnData("ID", BrStats[rowID][10].ToString(), "Name", "");
                    dr["Imprinter"] = BrStats[rowID][18].ToString();
                    dr["Level"] = ToDouble(MainStats[rowID][1].ToString());


                    // change theese stats to baby tracking stuff
                    dr["Hp"] = estimatedAge;
                    dr["Stamina"] = estTimeLeft;
                    dr["Oxygen"] = ageRate;
                    dr["Status"] = dateT.ToString("dd/MM/yyyy HH:mm:ss"); // have to use status here because date is a string

                    dr["Food"] = 0;

                    dr["Weight"] = Math.Round(ToDouble(MainStats[rowID][6].ToString()), 1);
                    dr["Damage"] = Math.Round((ToDouble(MainStats[rowID][7].ToString()) + 1) * 100, 1);
                    dr["Speed"] = Math.Round((ToDouble(BrStats[rowID][8].ToString()) + 1) * 100);

                    dr["Gen"] = ToDouble(BrStats[rowID][13].ToString());
                    dr["MamaMute"] = ToDouble(BrStats[rowID][11].ToString());
                    dr["PapaMute"] = ToDouble(BrStats[rowID][12].ToString());
                    dr["Age"] = Math.Round(ToDouble(BrStats[rowID][15].ToString()) * 100);
                    dr["Imprint"] = Math.Round(ToDouble(BrStats[rowID][17].ToString()) * 100);


                    table.Rows.Add(dr);
                }

                rowID++;
            }
        }

        public static void CompileDinoArchive(string sortC = "")
        {
            // Retrieve distinct IDs
            string[] idList = DataManager.GetAllDistinctColumnData("ID");

            ArchiveTable.Clear();

            // now check the status of each id and add them to ArchiveTable if status = Archived
            foreach (string dino in idList)
            {
                string status = GetGroup(dino);
                if (status == "Archived")
                {
                    DataRow dr = ArchiveTable.NewRow();
                    dr["ID"] = dino;
                    dr["Tag"] = GetLastColumnData("ID", dino, "Tag");
                    dr["Name"] = GetLastColumnData("ID", dino, "Name");
                    dr["Level"] = GetLastColumnData("ID", dino, "Level");
                    dr["Class"] = GetLastColumnData("ID", dino, "Class");



                    ArchiveTable.Rows.Add(dr);
                }
            }

            // Sort the MaleTable based on the desired column
            DataView view1 = new DataView(DataManager.ArchiveTable);
            view1.Sort = sortC;
            DataManager.ArchiveTable = view1.ToTable();

            FileManager.Log("Archive compiled", 0);
        }

        public static void SetMaxStats(int toggle = 0)
        {
            LevelMax = 0;
            HpMax = 0;
            StaminaMax = 0;
            OxygenMax = 0;
            FoodMax = 0;
            WeightMax = 0;
            DamageMax = 0;
            SpeedMax = 0;
            // look trough males for highest stats
            foreach (DataRow rowM in MaleTable.Rows)
            {

                string id = rowM["id"].ToString();
                string group = GetGroup(id);

                bool include = false;
                if (toggle == 0)// include dinos only by set toggle
                {
                    if (group != "Archived") { include = true; }
                }
                else if (toggle == 1)
                {
                    if (group != "Archived" && group != "Exclude") { include = true; }
                }
                else if (toggle == 2)
                {
                    if (group != "Archived" && group != "") { include = true; }
                }
                else if (toggle == 3)
                {
                    if (group != "" && group != "Exclude") { include = true; }
                }


                if (include)
                {
                    double LevelM = ToDouble(rowM["Level"].ToString());
                    double HpM = ToDouble(rowM["HP"].ToString());
                    double StaminaM = ToDouble(rowM["Stamina"].ToString());
                    double OxygenM = ToDouble(rowM["Oxygen"].ToString());
                    double FoodM = ToDouble(rowM["Food"].ToString());
                    double WeightM = ToDouble(rowM["Weight"].ToString());
                    double DamageM = ToDouble(rowM["Damage"].ToString());
                    double SpeedM = ToDouble(rowM["Speed"].ToString());

                    if (LevelM >= LevelMax) { LevelMax = LevelM; }
                    if (HpM >= HpMax) { HpMax = HpM; }
                    if (StaminaM >= StaminaMax) { StaminaMax = StaminaM; }
                    if (OxygenM >= OxygenMax) { OxygenMax = OxygenM; }
                    if (FoodM >= FoodMax) { FoodMax = FoodM; }
                    if (WeightM >= WeightMax) { WeightMax = WeightM; }
                    if (DamageM >= DamageMax) { DamageMax = DamageM; }
                    if (SpeedM >= SpeedMax) { SpeedMax = SpeedM; }
                }
            }

            // look trough females for highest stats
            foreach (DataRow rowM in FemaleTable.Rows)
            {

                string id = rowM["id"].ToString();
                string group = GetGroup(id);

                bool include = false;
                if (toggle == 0)// include dinos only by set toggle
                {
                    if (group != "Archived") { include = true; }
                }
                else if (toggle == 1)
                {
                    if (group != "Archived" && group != "Exclude") { include = true; }
                }
                else if (toggle == 2)
                {
                    if (group != "Archived" && group != "") { include = true; }
                }
                else if (toggle == 3)
                {
                    if (group != "" && group != "Exclude") { include = true; }
                }


                if (include)
                {
                    double LevelM = ToDouble(rowM["Level"].ToString());
                    double HpM = ToDouble(rowM["HP"].ToString());
                    double StaminaM = ToDouble(rowM["Stamina"].ToString());
                    double OxygenM = ToDouble(rowM["Oxygen"].ToString());
                    double FoodM = ToDouble(rowM["Food"].ToString());
                    double WeightM = ToDouble(rowM["Weight"].ToString());
                    double DamageM = ToDouble(rowM["Damage"].ToString());
                    double SpeedM = ToDouble(rowM["Speed"].ToString());

                    if (LevelM >= LevelMax) { LevelMax = LevelM; }
                    if (HpM >= HpMax) { HpMax = HpM; }
                    if (StaminaM >= StaminaMax) { StaminaMax = StaminaM; }
                    if (OxygenM >= OxygenMax) { OxygenMax = OxygenM; }
                    if (FoodM >= FoodMax) { FoodMax = FoodM; }
                    if (WeightM >= WeightMax) { WeightMax = WeightM; }
                    if (DamageM >= DamageMax) { DamageMax = DamageM; }
                    if (SpeedM >= SpeedMax) { SpeedMax = SpeedM; }
                }
            }
            //FileManager.Log("updated stats");
        }

        public static void SetBinaryStats(int toggle = 0)
        {
            BinaryM = new string[MaleTable.Rows.Count];
            BinaryF = new string[FemaleTable.Rows.Count];

            // look trough males
            int rowIDC = 0;
            foreach (DataRow rowC in MaleTable.Rows)
            {
                string compare = rowC["ID"].ToString();
                string compareStatus = rowC["Status"].ToString();

                string groupC = GetGroup(compare);

                bool includeC = false;
                if (toggle == 0)// include dinos only by set toggle
                {
                    if (groupC != "Archived") { includeC = true; }
                }
                else if (toggle == 1)
                {
                    if (groupC != "Archived" && groupC != "Exclude") { includeC = true; }
                }
                else if (toggle == 2)
                {
                    if (groupC != "Archived" && groupC != "") { includeC = true; }
                }
                else if (toggle == 3)
                {
                    if (groupC != "" && groupC != "Exclude") { includeC = true; }
                }

                if (includeC)
                {
                    string aC = "0"; string bC = "0"; string cC = "0";
                    string dC = "0"; string eC = "0"; string fC = "0";
                    string binaryC = "000000";

                    double HpC = ToDouble(rowC["HP"].ToString());
                    double StaminaC = ToDouble(rowC["Stamina"].ToString());
                    double OxygenC = ToDouble(rowC["Oxygen"].ToString());
                    double FoodC = ToDouble(rowC["Food"].ToString());
                    double WeightC = ToDouble(rowC["Weight"].ToString());
                    double DamageC = ToDouble(rowC["Damage"].ToString());

                    if (HpC >= HpMax) { aC = "1"; }
                    if (StaminaC >= StaminaMax) { bC = "1"; }
                    if (OxygenC >= OxygenMax) { cC = "1"; }
                    if (FoodC >= FoodMax) { dC = "1"; }
                    if (WeightC >= WeightMax) { eC = "1"; }
                    if (DamageC >= DamageMax) { fC = "1"; }

                    binaryC = aC + bC + cC + dC + eC + fC;
                    BinaryM[rowIDC] = binaryC;
                    string outStatus = "";
                    foreach (DataRow rowW in MaleTable.Rows) // compare males to put useles males in reserve
                    {
                        string with = rowW["ID"].ToString();
                        string withStatus = rowW["Status"].ToString();

                        string groupW = GetGroup(with);

                        bool includeW = false;
                        if (toggle == 0)// include dinos only by set toggle
                        {
                            if (groupW != "Archived") { includeW = true; }
                        }
                        else if (toggle == 1)
                        {
                            if (groupW != "Archived" && groupW != "Exclude") { includeW = true; }
                        }
                        else if (toggle == 2)
                        {
                            if (groupW != "Archived" && groupW != "") { includeW = true; }
                        }
                        else if (toggle == 3)
                        {
                            if (groupW != "" && groupW != "Exclude") { includeW = true; }
                        }

                        if (compare != with && includeW) // not with eachother
                        {
                            string aW = "0"; string bW = "0"; string cW = "0";
                            string dW = "0"; string eW = "0"; string fW = "0";

                            double HpW = ToDouble(rowW["HP"].ToString());
                            double StaminaW = ToDouble(rowW["Stamina"].ToString());
                            double OxygenW = ToDouble(rowW["Oxygen"].ToString());
                            double FoodW = ToDouble(rowW["Food"].ToString());
                            double WeightW = ToDouble(rowW["Weight"].ToString());
                            double DamageW = ToDouble(rowW["Damage"].ToString());

                            if (HpW >= DataManager.HpMax) { aW = "1"; }
                            if (StaminaW >= DataManager.StaminaMax) { bW = "1"; }
                            if (OxygenW >= DataManager.OxygenMax) { cW = "1"; }
                            if (FoodW >= DataManager.FoodMax) { dW = "1"; }
                            if (WeightW >= DataManager.WeightMax) { eW = "1"; }
                            if (DamageW >= DataManager.DamageMax) { fW = "1"; }

                            string binaryW = aW + bW + cW + dW + eW + fW;

                            // now that we have both binary strings compare them to figure out if the compare is superceeded or not
                            string aA = "0"; string bA = "0"; string cA = "0";
                            string dA = "0"; string eA = "0"; string fA = "0";

                            // add up the binary shiz with magical ways known only to the gods of blubs
                            if (aC == "0" && aW == "0") { aA = "0"; } else if (aC == "0" && aW == "1") { aA = "1"; } else if (aC == "1" && aW == "0") { aA = "2"; } else if (aC == "1" && aW == "1") { aA = "3"; }
                            if (bC == "0" && bW == "0") { bA = "0"; } else if (bC == "0" && bW == "1") { bA = "1"; } else if (bC == "1" && bW == "0") { bA = "2"; } else if (bC == "1" && bW == "1") { bA = "3"; }
                            if (cC == "0" && cW == "0") { cA = "0"; } else if (cC == "0" && cW == "1") { cA = "1"; } else if (cC == "1" && cW == "0") { cA = "2"; } else if (cC == "1" && cW == "1") { cA = "3"; }
                            if (dC == "0" && dW == "0") { dA = "0"; } else if (dC == "0" && dW == "1") { dA = "1"; } else if (dC == "1" && dW == "0") { dA = "2"; } else if (dC == "1" && dW == "1") { dA = "3"; }
                            if (eC == "0" && eW == "0") { eA = "0"; } else if (eC == "0" && eW == "1") { eA = "1"; } else if (eC == "1" && eW == "0") { eA = "2"; } else if (eC == "1" && eW == "1") { eA = "3"; }
                            if (fC == "0" && fW == "0") { fA = "0"; } else if (fC == "0" && fW == "1") { fA = "1"; } else if (fC == "1" && fW == "0") { fA = "2"; } else if (fC == "1" && fW == "1") { fA = "3"; }

                            string binaryA = aA + bA + cA + dA + eA + fA;


                            if (binaryC == binaryW && !withStatus.Contains("<") && !withStatus.Contains("#"))
                            {
                                // both have same stats   MARK IT
                                outStatus = "# " + rowW["Name"].ToString();  // identical   #with
                            }

                            if (binaryA.Contains("1") && binaryA.Contains("3") && !binaryA.Contains("2")) // has 1 and 3 but not 2
                            {
                                // both have same stats   MARK IT
                                outStatus = "< " + rowW["Name"].ToString();  // superceeded
                            }

                        }
                    }

                    string finalStatus = compareStatus;


                    if (outStatus.Contains("<") || outStatus.Contains("#"))
                    {
                        finalStatus = outStatus;
                    }
                    // mark as garbage if they have none of the best stats
                    if (binaryC == "000000") 
                    { 
                        finalStatus = $"{compareStatus}[garbageSym]";
                    }

                    // edit the row that we show
                    MaleTable.Rows[rowIDC].SetField("Status", finalStatus);
                }

                rowIDC++;
            }

            // look trough females
            rowIDC = 0;
            foreach (DataRow rowC in FemaleTable.Rows)
            {
                string compare = rowC["ID"].ToString();
                string compareStatus = rowC["Status"].ToString();

                string groupC = GetGroup(compare);

                bool includeC = false;
                if (toggle == 0)// include dinos only by set toggle
                {
                    if (groupC != "Archived") { includeC = true; }
                }
                else if (toggle == 1)
                {
                    if (groupC != "Archived" && groupC != "Exclude") { includeC = true; }
                }
                else if (toggle == 2)
                {
                    if (groupC != "Archived" && groupC != "") { includeC = true; }
                }
                else if (toggle == 3)
                {
                    if (groupC != "" && groupC != "Exclude") { includeC = true; }
                }


                string outStatus = "";
                string aC = "0"; string bC = "0"; string cC = "0";
                string dC = "0"; string eC = "0"; string fC = "0";
                string binaryC = "000000";

                double HpC = ToDouble(rowC["HP"].ToString());
                double StaminaC = ToDouble(rowC["Stamina"].ToString());
                double OxygenC = ToDouble(rowC["Oxygen"].ToString());
                double FoodC = ToDouble(rowC["Food"].ToString());
                double WeightC = ToDouble(rowC["Weight"].ToString());
                double DamageC = ToDouble(rowC["Damage"].ToString());



                if (HpC >= HpMax) { aC = "1"; }
                if (StaminaC >= StaminaMax) { bC = "1"; }
                if (OxygenC >= OxygenMax) { cC = "1"; }
                if (FoodC >= FoodMax) { dC = "1"; }
                if (WeightC >= WeightMax) { eC = "1"; }
                if (DamageC >= DamageMax) { fC = "1"; }

                binaryC = aC + bC + cC + dC + eC + fC;

                BinaryF[rowIDC] = binaryC;


                if (includeC)
                {
                    string finalStatus = compareStatus;

                    // mark as garbage if they have none of the best stats
                    if (binaryC == "000000")
                    {
                        finalStatus = $"{compareStatus}[garbageSym]";
                    }

                    // edit the row we show
                    FemaleTable.Rows[rowIDC].SetField("Status", finalStatus);
                }

                rowIDC++;
            }
            // FileManager.Log("updated binary");
        }

        public static void GetBestPartner()
        {
            BottomTable.Clear();
            string aB = "0"; string bB = "0"; string cB = "0";
            string dB = "0"; string eB = "0"; string fB = "0";
            // colorDinos();
            // get best pairing

            // ==================================================================================================
            string maleO1 = ""; string femaleO1 = "";
            string point = "";
            // bPointsMax = 0;
            // rowFID = 0;

            DataManager.ComboTable.Clear();


            int superID = 0;
            int superID2 = 1000;

            int p0 = 6;
            while (p0 >= 0)
            {
                int p1 = 6;
                while (p1 >= 0)
                {
                    int p2 = 6;
                    while (p2 >= 0)
                    {
                        int roMID = 0;
                        foreach (DataRow rowM in DataManager.MaleTable.Rows)
                        {
                            string IDM1 = DataManager.BinaryM[roMID];
                            string papaID = rowM["ID"].ToString();
                            string statusM = rowM["Status"].ToString();


                            if (!statusM.Contains("<") && !statusM.Contains("#") && !statusM.Contains("Exclude")) // dont include theese males
                            {
                                int roFID = 0;
                                foreach (DataRow rowF in DataManager.FemaleTable.Rows)
                                {
                                    string IDF1 = DataManager.BinaryF[roFID];
                                    string mamaID = rowF["ID"].ToString();
                                    string statusF = rowF["Status"].ToString();
                                    if (statusF != "Baby" && statusF != "Exclude")// && nameF != femaleO)
                                    {
                                        string aM = IDM1.Substring(0, 1); string bM = IDM1.Substring(1, 1); string cM = IDM1.Substring(2, 1);
                                        string dM = IDM1.Substring(3, 1); string eM = IDM1.Substring(4, 1); string fM = IDM1.Substring(5, 1);

                                        string aF = IDF1.Substring(0, 1); string bF = IDF1.Substring(1, 1); string cF = IDF1.Substring(2, 1);
                                        string dF = IDF1.Substring(3, 1); string eF = IDF1.Substring(4, 1); string fF = IDF1.Substring(5, 1);


                                        int gPoints = 0;
                                        int aPoints = 0;
                                        int nPoints = 0;

                                        if (aM == "1" && aF == "1") { gPoints++; aB = "2"; } else if (aM == "1" || aF == "1") { aPoints++; aB = "1"; } else { aB = "0"; nPoints++; }
                                        if (bM == "1" && bF == "1") { gPoints++; bB = "2"; } else if (bM == "1" || bF == "1") { aPoints++; bB = "1"; } else { bB = "0"; nPoints++; }
                                        if (cM == "1" && cF == "1") { gPoints++; cB = "2"; } else if (cM == "1" || cF == "1") { aPoints++; cB = "1"; } else { cB = "0"; nPoints++; }
                                        if (dM == "1" && dF == "1") { gPoints++; dB = "2"; } else if (dM == "1" || dF == "1") { aPoints++; dB = "1"; } else { dB = "0"; nPoints++; }
                                        if (eM == "1" && eF == "1") { gPoints++; eB = "2"; } else if (eM == "1" || eF == "1") { aPoints++; eB = "1"; } else { eB = "0"; nPoints++; }
                                        if (fM == "1" && fF == "1") { gPoints++; fB = "2"; } else if (fM == "1" || fF == "1") { aPoints++; fB = "1"; } else { fB = "0"; nPoints++; }


                                        int agPoints = gPoints + aPoints;

                                        if (p0 == agPoints)
                                        {
                                            if (p1 == gPoints)
                                            {
                                                if (p2 == aPoints)
                                                {
                                                    bool fnd = false;
                                                    foreach (DataRow rowC in DataManager.ComboTable.Rows)
                                                    {
                                                        if (rowC["M"].ToString() == mamaID) { fnd = true; break; }
                                                    }
                                                    if (!fnd)
                                                    {
                                                        if (rowF["Status"].ToString() != "Baby")
                                                        {
                                                            if (rowM["Status"].ToString() != "Baby")
                                                            {

                                                                if (aPoints > 0)
                                                                {
                                                                    DataRow dr = DataManager.ComboTable.NewRow(); // add to combine list sorted by bPoints
                                                                    dr["#"] = superID;
                                                                    dr["P"] = papaID; // papa
                                                                    dr["M"] = mamaID; // mama
                                                                    dr["gP"] = gPoints;
                                                                    dr["aP"] = aPoints;
                                                                    dr["agP"] = agPoints;
                                                                    dr["res"] = (aB + bB + cB + dB + eB + fB);
                                                                    DataManager.ComboTable.Rows.Add(dr);
                                                                    superID++;
                                                                }
                                                                else if (agPoints > 5)
                                                                {
                                                                    DataRow dr = DataManager.ComboTable.NewRow(); // add to combine list sorted by bPoints
                                                                    dr["#"] = superID;
                                                                    dr["P"] = papaID; // papa
                                                                    dr["M"] = mamaID; // mama
                                                                    dr["gP"] = gPoints;
                                                                    dr["aP"] = aPoints;
                                                                    dr["agP"] = agPoints;
                                                                    dr["res"] = (aB + bB + cB + dB + eB + fB);
                                                                    DataManager.ComboTable.Rows.Add(dr);
                                                                    superID++;
                                                                }
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        DataRow dr = DataManager.ComboTable.NewRow(); // add to combine list sorted by bPoints
                                                        dr["#"] = superID2;
                                                        dr["P"] = papaID; // papa
                                                        dr["M"] = mamaID; // mama
                                                        dr["gP"] = gPoints;
                                                        dr["aP"] = aPoints;
                                                        dr["agP"] = agPoints;
                                                        dr["res"] = (aB + bB + cB + dB + eB + fB);
                                                        //   comboTable.Rows.Add(dr);
                                                        superID2++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    roFID++;
                                }
                            }
                            roMID++;
                        }
                        p2--;
                    }
                    p1--;
                }
                p0--;
            }


            // now we have a list of all the combinations and their points
            // sort by highest bP then gP with most aP

            //string combo = "";
            int nr = 1;
            foreach (DataRow rowC in DataManager.ComboTable.Rows) // now find the best combination gp + ap
            {
                //combo = rowC["M"].ToString() + "+" + rowC["P"].ToString();
                int gP = Convert.ToInt32(rowC["gP"].ToString());
                int aP = Convert.ToInt32(rowC["aP"].ToString());
                int agP = gP + aP;


                if (aP > 0 || gP == 6)
                {
                    maleO1 = rowC["P"].ToString(); femaleO1 = rowC["M"].ToString(); // the est one
                    point = gP + " + " + aP + " = " + agP;

                    MakeOffspring(maleO1, femaleO1, "Breed #" + nr, point);
                    nr++;
                }
            }
            //FileManager.Log("Updated BreedPairs",0);
        }

        public static void MakeOffspring(string male, string female, string offspring, string point)
        {

            if (male != "" && female != "")
            {
                double Hp = 0; double Stamina = 0; double Oxygen = 0;
                double Food = 0; double Weight = 0; double Damage = 0;
                double Gen = 0; double LevelB = 0;
                string mama = ""; string papa = "";
                int MrowID = 0;

                foreach (DataRow rowM in DataManager.MaleTable.Rows)
                {
                    string idM = rowM["ID"].ToString();
                    string namM = rowM["Name"].ToString();
                    if (idM == male)
                    {
                        papa = namM;
                        double HpM = ToDouble(rowM["HP"].ToString());
                        double StaminaM = ToDouble(rowM["Stamina"].ToString());
                        double OxygenM = ToDouble(rowM["Oxygen"].ToString());
                        double FoodM = ToDouble(rowM["Food"].ToString());
                        double WeightM = ToDouble(rowM["Weight"].ToString());
                        double DamageM = ToDouble(rowM["Damage"].ToString());
                        double GenM = ToDouble(rowM["Gen"].ToString());
                        double levelM = ToDouble(rowM["Level"].ToString());
                        int FrowID = 0;
                        foreach (DataRow rowF in DataManager.FemaleTable.Rows)
                        {
                            string nameF = rowF["ID"].ToString();
                            string namF = rowF["Name"].ToString();
                            if (nameF == female)
                            {
                                mama = namF;
                                double HpF = ToDouble(rowF["HP"].ToString());
                                double StaminaF = ToDouble(rowF["Stamina"].ToString());
                                double OxygenF = ToDouble(rowF["Oxygen"].ToString());
                                double FoodF = ToDouble(rowF["Food"].ToString());
                                double WeightF = ToDouble(rowF["Weight"].ToString());
                                double DamageF = ToDouble(rowF["Damage"].ToString());
                                double GenF = ToDouble(rowF["Gen"].ToString());
                                double levelF = ToDouble(rowF["Level"].ToString());

                                if (GenM >= GenF)
                                {
                                    Gen = GenM + 1; // add up generations
                                }
                                else
                                {
                                    Gen = GenF + 1; // add up generations
                                }

                                if (HpM > HpF) { Hp = HpM; } else { Hp = HpF; }
                                if (StaminaM > StaminaF) { Stamina = StaminaM; } else { Stamina = StaminaF; }
                                if (OxygenM > OxygenF) { Oxygen = OxygenM; } else { Oxygen = OxygenF; }
                                if (FoodM > FoodF) { Food = FoodM; } else { Food = FoodF; }
                                if (WeightM > WeightF) { Weight = WeightM; } else { Weight = WeightF; }
                                if (DamageM > DamageF) { Damage = DamageM; } else { Damage = DamageF; }


                                if ((levelM + levelF) > 0) { LevelB = Math.Round((levelM + levelF) / 2); }

                            }
                            FrowID++;
                        }
                    }
                    MrowID++;
                }


                DataRow dr = DataManager.BottomTable.NewRow();
                dr["Name"] = offspring;
                dr["Level"] = LevelB;
                dr["Hp"] = Hp;
                dr["Stamina"] = Stamina;
                dr["Oxygen"] = Oxygen;
                dr["Food"] = Food;
                dr["Weight"] = Weight;
                dr["Damage"] = Damage;
                dr["Gen"] = Gen;
                dr["Mama"] = mama;
                dr["Papa"] = papa;
                dr["Status"] = point;


                DataManager.BottomTable.Rows.Add(dr);
            }
        }

        public static void GetOneDinoData(string id)
        {
            //fill the bTable with all the rows that this id gets
            BottomTable.Clear();


            foreach (DataRow row in ImportsTable.Rows)
            {
                if (row["ID"].ToString() == id)
                {
                    DataRow dr = DataManager.BottomTable.NewRow();

                    dr["Name"] = row["Name"].ToString();
                    dr["Level"] = ToDouble(row["Level"].ToString());
                    dr["Hp"] = Math.Round(ToDouble(row["HP"].ToString()), 1);
                    dr["Stamina"] = Math.Round(ToDouble(row["Stamina"].ToString()), 1);
                    dr["Oxygen"] = Math.Round(ToDouble(row["Oxygen"].ToString()), 1);
                    dr["Food"] = Math.Round(ToDouble(row["Food"].ToString()), 1);
                    dr["Weight"] = Math.Round(ToDouble(row["Weight"].ToString()), 1);
                    dr["Damage"] = Math.Round((ToDouble(row["Damage"].ToString()) + 1) * 100, 1);
                    dr["Speed"] = Math.Round((ToDouble(row["Speed"].ToString()) + 1) * 100, 1);
                    dr["Gen"] = ToDouble(row["Gen"].ToString());
                    dr["Mama"] = GetLastColumnData("ID", row["Mama"].ToString(), "Name", "");
                    dr["Papa"] = GetLastColumnData("ID", row["Papa"].ToString(), "Name", "");
                    dr["MamaMute"] = row["MamaMute"].ToString();
                    dr["PapaMute"] = row["PapaMute"].ToString();
                    dr["Imprint"] = Math.Round(ToDouble(row["Imprint"].ToString()) * 100);
                    dr["Imprinter"] = row["Imprinter"].ToString();
                    dr["Age"] = Math.Round(ToDouble(row["BabyAge"].ToString()) * 100);
                    dr["ID"] = row["Time"].ToString();

                    DataManager.BottomTable.Rows.Add(dr);
                }

            }

            // Sort the BottomTable based on the desired column
            DataView view = new DataView(DataManager.BottomTable);
            view.Sort = "ID DESC"; // Replace "Level" with the desired column and sorting order (e.g., "Name ASC", "ID DESC")
            DataManager.BottomTable = view.ToTable();
            //FileManager.Log("Retrieved Dino Stats");
        }

        public static void CleanDataBaseByID()
        {
            // Get all distinct IDs from the database.
            string[] idList = DataManager.GetAllDistinctColumnData("ID");

            // Counter for deleted rows.
            int totalDeletedRows = 0;

            // Dictionary to store deleted row counts per ID.
            var deletedRowsPerID = new Dictionary<string, int>();

            foreach (string id in idList)
            {
                // Get rows for the current ID, sorted by Time in descending order.
                var rows = ImportsTable.Select($"ID = '{id}'", "Time DESC");

                // Reset the HashSet to track unique rows excluding "Time".
                var seenRows = new HashSet<string>();

                // Counter for this ID's deleted rows.
                int deletedRows = 0;

                foreach (var row in rows)
                {
                    // Generate a unique key excluding the "Time" column.
                    var rowKey = string.Join("|", row.Table.Columns.Cast<DataColumn>()
                        .Where(col => col.ColumnName != "Time") // Exclude the "Time" column
                        .Select(col => row[col]));

                    if (seenRows.Contains(rowKey))
                    {
                        // Remove the duplicate row and increment the counters.
                        ImportsTable.Rows.Remove(row);
                        deletedRows++;
                        totalDeletedRows++;
                    }
                    else
                    {
                        // Add the row key to the set if it is unique.
                        seenRows.Add(rowKey);
                    }
                }

                // Record deleted row count for this ID.
                if (deletedRows > 0)
                {
                    deletedRowsPerID[id] = deletedRows;
                }
            }

            // Accept changes to finalize modifications.
            ImportsTable.AcceptChanges();

            if (totalDeletedRows > 0)
            {
                FileManager.Log("DataBase cleaned", 0);
            }

        }

        public static void DeleteRowsByID(string id)
        {
            // Select rows matching the ID.
            var rowsToDelete = ImportsTable.Select($"ID = '{id}'");

            if (rowsToDelete.Length == 0)
            {
                FileManager.Log($"No rows found for ID: {id}", 1);
                return;
            }

            // Track how many rows are deleted.
            int deletedCount = 0;

            foreach (var row in rowsToDelete)
            {
                //Console.WriteLine($"Deleting row: ID={row["ID"]}, Name={row["Name"]}");
                row.Delete();
                deletedCount++;
            }

            // Commit the deletions to the DataTable.
            ImportsTable.AcceptChanges();

            if (deletedCount > 0)
            {
                //Console.WriteLine($"Deleted {deletedCount} rows for ID: {id}");

                DataManager.SetGroup(id, "");
                // maybe delete the file too to prevent reimport

                // delete associated ini file to prevent reimport
                // otherwise we have to mark it for exclusion during import
                // maybe as a failsafe?!?!
                FileManager.DeleteFile(id);


                FileManager.Log($"Purged ID: {id}", 0);
                // request a save after modifying data
                FileManager.needSave = true;
            }
        }

        public static void PurgeAll()
        {
            string[] idList = DataManager.GetAllDistinctColumnData("ID");

            foreach (string id in idList)
            {
                string group = GetGroup(id);
                if (group == "Archived")
                {
                    DeleteRowsByID(id);
                }
            }
            FileManager.needSave = true;
        }

        public static void Import()
        {
            // get a string array of exported dino files
            string[] exports = FileManager.GetExportFiles();

            AddC = 0; ModC = 0;
            foreach (string file in exports) // loop trough each file to look for data in all of them
            {
                var split1 = file.Split(new[] { @"DinoExport_" }, StringSplitOptions.RemoveEmptyEntries);
                var split2 = split1[1].Split(new[] { @".ini" }, StringSplitOptions.RemoveEmptyEntries);
                string id = split2[0].ToString();

                string[] importedNew = FilterDinoStats(file);
                string Time = File.GetLastWriteTime(file).ToString("dd/MM/yyyy HH:mm:ss");

                if (importedNew.Count() > 0)
                {

                    bool found = false;
                    foreach (DataRow row in ImportsTable.Rows)
                    {
                        if (row["ID"].ToString() == id)
                        {
                            found = true; break;
                        }
                    }
                    // never added dino before add first entry
                    DataRow dr = ImportsTable.NewRow();
                    dr["ID"] = id;
                    dr["Tag"] = importedNew[0];
                    dr["Sex"] = importedNew[1];
                    dr["Name"] = importedNew[2];
                    dr["Level"] = importedNew[3];
                    dr["Hp"] = importedNew[4];
                    dr["Stamina"] = importedNew[5];
                    dr["Oxygen"] = importedNew[6];
                    dr["Food"] = importedNew[7];
                    dr["Weight"] = importedNew[8];
                    dr["Damage"] = importedNew[9];
                    dr["Speed"] = importedNew[10];
                    dr["Mama"] = importedNew[11];
                    dr["Papa"] = importedNew[12];
                    dr["MamaMute"] = importedNew[13];
                    dr["PapaMute"] = importedNew[14];
                    dr["Gen"] = importedNew[15];
                    dr["GenM"] = importedNew[16];
                    dr["BabyAge"] = importedNew[17];
                    dr["Neutered"] = importedNew[18];
                    dr["Imprint"] = importedNew[19];
                    dr["Imprinter"] = importedNew[20];
                    dr["Tribe"] = importedNew[21];
                    dr["Class"] = importedNew[22];
                    dr["Time"] = Time;
                    dr["Colors"] = importedNew[23];

                    if (!found)
                    {
                        AddC++;
                        //fileManager.log("Added dino: " + id);
                        ImportsTable.Rows.Add(dr);
                    }
                    else // we got a hit on the id
                    {
                        bool change = false; string lastTime = "";
                        lastTime = DataManager.GetLastColumnData("ID", id, "Time", "#");
                        DateTime LastTime = DateTime.ParseExact(lastTime, "dd/MM/yyyy HH:mm:ss", Culture);
                        DateTime NowTime = DateTime.ParseExact(Time, "dd/MM/yyyy HH:mm:ss", Culture);

                        if (NowTime > LastTime) { change = true; }

                        if (change)
                        {
                            ModC++;
                            //fileManager.log("Updated dino: " + id);
                            ImportsTable.Rows.Add(dr);
                        }
                    }
                }
            }
            if (AddC > 0)
            {
                FileManager.Log("Added " + AddC + " dinos", 0);
            }
            if (ModC > 0)
            {
                FileManager.Log("Updated " + ModC + " dinos", 0);
            }
        }

        private static string[] FilterDinoStats(string filename, string emptyString = "N/A")
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

        public static double ToDouble(string input, double def = 0)
        {
            if (input == "")
            {
                return def;
            }
            else
            {
                try
                {
                    input = input.Replace(",", DecimalSeparator);
                    input = input.Replace(".", DecimalSeparator);

                    return Convert.ToDouble(input);
                }
                catch
                {
                    return def;
                }
            }
        }
    }
}
