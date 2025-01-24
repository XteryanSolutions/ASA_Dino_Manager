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

        // headers
        public static readonly Dictionary<string, string> StatMap = new Dictionary<string, string>
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
            { "pM", "📘" },
            { "mM", "📙" },
            { "Status", "📑" },
            { "Imprint", "💖" },
            { "Imprinter", "💑" },
            { "Age", "🐣" },
            { "Time", "⌛" },
            { "Rate", "📶" },
            { "Date", "📆" },
            { "Group", "📚" },
        };


        public static readonly Dictionary<string, string> MainMap = new Dictionary<string, string>
        {
            { "mainTitle", "Dino Manager" },
            { "mainText", "Remember to feed your dinos!!!" },
            { "noGameTitle", "Could not find game install location" },
            { "noGame", "Check configs for game install folder 🔎" },
            { "hasGameTitle", "Ready to start importing data" },
            { "hasGame", "Looking for dino exports 🔎" },
        };


        public static readonly Dictionary<string, string> DinoMap = new Dictionary<string, string>
        {
            { "dataError", "Dinos exploded :O" },
            { "dataWarning", "Dinos walked away :(" },
            { "noDino", "No dinos in here 🔎" },

        };


        public static readonly Dictionary<string, string> BabyMap = new Dictionary<string, string>
        {
            { "dataError", "Dinos exploded :O" },
            { "dataWarning", "Dinos walked away :(" },
            { "noDino", "No dinos in here 🔎" },

        };

        public static readonly Dictionary<string, string> ArchiveMap = new Dictionary<string, string>
        {
            { "dataError", "Dinos exploded :O" },
            { "dataWarning", "Dinos walked away :(" },
            { "noDino", "No dinos in here 🔎" },

        };

    }
}
