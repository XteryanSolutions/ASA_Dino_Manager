namespace Ark_Dino_Manager
{
    internal class StatPoints
    {

        public static readonly Dictionary<string, Dictionary<string, (double BaseStat, double StatRate)>> Groups =
            new Dictionary<string, Dictionary<string, (double BaseStat, double StatRate)>>
            {
                {
                   "Gacha", new Dictionary<string, (double BaseStat, double StatRate)>
                   {
                        { "Hp", (750, 150) },
                        { "Stamina", (325, 32.5) },
                        { "O2", (150, 15) },
                        { "Food", (3000, 300) },
                        { "Weight", (550, 22) },
                        { "Damage", (32, 1.6) },
                        { "CraftSkill", (100, 1.5) },
                        { "Regen", (0, 0) },
                        { "Capacity", (0, 0) }
                   }
                },
                {
                    "Owl", new Dictionary<string, (double BaseStat, double StatRate)>
                    {
                        { "Hp", (325, 65) },
                        { "Stamina", (350, 35) },
                        { "O2", (150, 15) },
                        { "Food", (2000, 200) },
                        { "Weight", (375, 7.5) },
                        { "Damage", (100, 5) },
                        { "CraftSkill", (0, 0) },
                        { "Regen", (0, 0) },
                        { "Capacity", (0, 0) }
                    }
                }
            };
    }
}
