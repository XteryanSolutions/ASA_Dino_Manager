using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark_Dino_Manager
{
    internal class Localization
    {

        // symbols (maybe not localize theese)
        public static readonly Dictionary<string, string> Smap = new Dictionary<string, string>
        {
            { "Unknown", "❔" }, // Wild Tame
            { "Missing", "⚠" }, // Link info missing from database
            { "Warning", "❓" }, // need to import ancestry
            { "Garbage", "💩" },
            { "NewTame", "🧬" },
            { "Baby", "🐣" },
            { "Grown", "🦖" },
            { "LessThan", "📉" },
            { "Identical", "🔀" },
            { "Notes", "📋" },
            { "SortUp", "▲" },
            { "SortDown", "▼" },
            { "Breeding", "🐣" },
            { "Current", "🐓" },
            { "Back", "🔙" },
            { "Save", "💾" },
            { "Heritage", "🌳" },
            { "All", "📚" },
            { "Include", "📗" },
            { "Exclude", "📘" },
            { "Archive", "🧨" },
            { "Restore", "📗" },
            { "Purge", "🧨" },
            { "PurgeAll", "⛔" },
            { "Import", "📩" },
        };

        // symbols (DO NOT MODIFY)
        public static readonly Dictionary<string, string> SymMap = new Dictionary<string, string>
        {
            { "ID", "💳" },
            { "Tag", "🔖" },
            { "Class", "🦖" },
            { "Name", "🔤" },
            { "Level", "💯" },
            { "Hp", "💚" },
            { "Stamina", "⚡" },
            { "O2", "🤿" },
            { "Food", "🥩" },
            { "Weight", "📦" },
            { "Damage", "💪" },
            { "CraftSkill", "🔨" },
            { "ChargeCapacity", "🔋" },
            { "ChargeRegen", "🔌" },
            { "Emission", "🔆" },
            { "Speed", "🐌" },
            { "Gen", "👪" },
            { "Papa", "♂" },
            { "Mama", "♀" },
            { "papaMute", "📘" },
            { "mamaMute", "📙" },
            { "Status", "📑" },
            { "Imprinter", "💑" },
            { "Imprint", "💖" },
            { "Age", "🐣" },
            { "Time", "⌛" },
            { "Rate", "📶" },
            { "Date", "📆" },
            { "Group", "📚" },
        };



        // headers (empty values as default shows only symbols instead of symbol + name)
        public static readonly Dictionary<string, string> HeaderMap = new Dictionary<string, string>
        {
            { "ID", "" },
            { "Tag", "" },
            { "Class", "" },
            { "Name", "" },
            { "Level", "" },
            { "Hp", "" },
            { "Stamina", "" },
            { "O2", "" },
            { "Food", "" },
            { "Weight", "" },
            { "Damage", "" },
            { "CraftSkill", "" },
            { "ChargeCapacity", "" },
            { "ChargeRegen", "" },
            { "Emission", "" },
            { "Speed", "" },
            { "Gen", "" },
            { "Papa", "" },
            { "Mama", "" },
            { "papaMute", "" },
            { "mamaMute", "" },
            { "Status", "" },
            { "Imprinter", "" },
            { "Imprint", "" },
            { "Age", "" },
            { "Time", "" },
            { "Rate", "" },
            { "Date", "" },
            { "Group", "" },
        };

        public static readonly Dictionary<string, string> StringMap = new Dictionary<string, string>
        {
            { "mainTitle", "Dino Manager" },
            { "mainText", "Remember to feed your dinos!!!" },
            { "noGameTitle", "Could not find game install" },
            { "noGame", "Check configs for game install folder 🔎" },
            { "hasGameTitle", "Ready to start importing data" },
            { "hasGame", "Looking for dino exports 🔎" },
            { "dataError", "Dinos exploded :O" },
            { "dataWarning", "Dinos walked away :(" },
            { "noDino", "No dinos in here 🔎" },
            { "purgeAllTitle", "Purge All dinos from DataBase" },
            { "purgeAllMessage", "Do you want to proceed?" },
            { "purgeTitle", "Purge dino from DataBase" },
            { "purgeMessage", "Do you want to proceed?" },
            { "purgeYes", "Yes" },
            { "purgeNo", "No" },
            { "openFileTitle", "Open File" },
            { "openFileMessage", "Select dataBase to merge" },
            { "okBtnText", "OK" },
            { "invalidTitle", "Invalid File" },
            { "invalidMessage", "Please select a file with the .hrv extension." },
            { "babyTitle", "Baby Dinos" },
            { "ageRateTitle", "Aging Rate" },
            { "ageRateText", "Age %/hr" },
            { "editStats", "Edit Stats" },
        };


    }
}
