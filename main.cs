using HarmonyLib;
using ModLoader;

namespace ColorResBar
{
    public class Main : Mod
    {
        public override string ModNameID => "ColorResBar";
        public override string DisplayName => "ColorResBar";
        public override string Author => "SFSGamer";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v1.0.0";
        public override string Description => "Add different colors to different resources.";

        static Harmony patcher;

        public override void Load()
        {
            ColorResBar.ColorResBarSettings.Load();
            patcher = new Harmony("ColorResBar.Main");
            patcher.PatchAll();
        }
    }
} 