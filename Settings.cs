using System;
using System.Collections.Generic;
using SFS.IO;
using SFS.Parsers.Json;
using ModLoader;

namespace ColorResBar
{
    [Serializable]
    public class ColorResBarSettingsConfig
    {
        // 新格式：三列 资源名 资源条颜色 当前量颜色
        public Dictionary<string, string> bar_colors = new Dictionary<string, string>
        {
            { "Electronic_Component", "#FF9800" },
            { "Material", "#90CAF9" },
            { "Rocket_Material", "#90A4AE" },
            { "Construction_Material", "#FFC107" },
            { "Electricity_Resource", "#FDD835" },
            { "Fuel", "#FFFFFF" },
            { "Oil", "#8D6E63" },
            { "Ore", "#9E9E9E" },
            { "Oxygen", "#5DB2E8" },
            { "Steel", "#B0BEC5" },
            { "Liquid_Fuel", "#FFFFFF" },
            { "Lithium", "#C0CA33" },
            { "Hydrogen", "#26C6DA" },
            { "Uranium", "#43A047" },
            { "Argon", "#9575CD" },
            { "Aerozine-50", "#F4511E" },
            { "HTP", "#7CB342" },
            { "Hydrazine", "#E53935" },
            { "Nitric Acid-Amine", "#EC407A" },
            { "LOX-Alchohol", "#4DD0E1" },
            { "Aerozine50-N2O4", "#FF8F00" },
            { "Hydrazine-NTO", "#FF6E40" },
            { "Nitrogen", "#42A5F5" },
            { "APCP_SolidFuel", "#FB8C00" },
            { "ACEHydrolox", "#26C6DA" },
            { "Kerolox", "#FF7043" },
            { "Methalox", "#4FC3F7" },
            { "Xenon", "#7E57C2" },
            { "NTO-MON-3", "#FF8A80" },
            { "N2O4-MMH", "#F06292" }
        };
        public Dictionary<string, string> current_colors = new Dictionary<string, string>
        {
            { "Electronic_Component", "#FF9800" },
            { "Material", "#90CAF9" },
            { "Rocket_Material", "#90A4AE" },
            { "Construction_Material", "#FFC107" },
            { "Electricity_Resource", "#FDD835" },
            { "Fuel", "#FFFFFF" },
            { "Oil", "#8D6E63" },
            { "Ore", "#9E9E9E" },
            { "Oxygen", "#5DB2E8" },
            { "Steel", "#B0BEC5" },
            { "Liquid_Fuel", "#FFFFFF" },
            { "Lithium", "#C0CA33" },
            { "Hydrogen", "#26C6DA" },
            { "Uranium", "#43A047" },
            { "Argon", "#9575CD" },
            { "Aerozine-50", "#F4511E" },
            { "HTP", "#7CB342" },
            { "Hydrazine", "#E53935" },
            { "Nitric Acid-Amine", "#EC407A" },
            { "LOX-Alchohol", "#4DD0E1" },
            { "Aerozine50-N2O4", "#FF8F00" },
            { "Hydrazine-NTO", "#FF6E40" },
            { "Nitrogen", "#42A5F5" },
            { "APCP_SolidFuel", "#FB8C00" },
            { "ACEHydrolox", "#26C6DA" },
            { "Kerolox", "#FF7043" },
            { "Methalox", "#4FC3F7" },
            { "Xenon", "#7E57C2" },
            { "NTO-MON-3", "#FF8A80" },
            { "N2O4-MMH", "#F06292" }
        };
    }

    public static class ColorResBarSettings
    {
        public static readonly FilePath Path = new FolderPath("Mods/ColorResBar").ExtendToFile("Settings.txt");
        public static ColorResBarSettingsConfig settings;

        public static void Load()
        {
            // 读取纯文本格式：每行 "资源名 资源条颜色 当前量颜色"（#RRGGBB），不再兼容旧格式
            var settingsPath = new FilePath(GetSettingsPath());
            if (!TryLoadPlainText(settingsPath, out settings))
            {
                settings = new ColorResBarSettingsConfig();
                Save();
            }
        }

        public static void Save()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (settings == null)
            {
                settings = new ColorResBarSettingsConfig();
            }
            var names = new System.Collections.Generic.HashSet<string>();
            if (settings.bar_colors != null)
                foreach (var kv in settings.bar_colors)
                    names.Add(kv.Key);
            if (settings.current_colors != null)
                foreach (var kv in settings.current_colors)
                    names.Add(kv.Key);
            foreach (var name in names)
            {
                string bar = (settings.bar_colors != null && settings.bar_colors.TryGetValue(name, out var b)) ? b : "#FFFFFF";
                string cur = (settings.current_colors != null && settings.current_colors.TryGetValue(name, out var c)) ? c : "#FFFFFF";
                sb.AppendLine($"{name} {bar} {cur}");
            }
            // 确保目录存在后再写入
            string fullPath = GetSettingsPath();
            string dir = System.IO.Path.GetDirectoryName(fullPath);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            new FilePath(fullPath).WriteText(sb.ToString());
        }

        // 获取 Mod 文件夹路径
        private static string GetSettingsPath()
        {
            string folder = "Mods/ColorResBar";
            if (Loader.main != null)
            {
                foreach (var mod in Loader.main.GetAllMods())
                {
                    if (mod.ModNameID == "ColorResBar")
                    {
                        folder = mod.ModFolder;
                        break;
                    }
                }
            }
            return System.IO.Path.Combine(folder, "Settings.txt");
        }

        public static void SaveSettings(ColorResBarSettingsConfig cfg)
        {
            var settingsPath = new FilePath(GetSettingsPath());
            JsonWrapper.SaveAsJson(settingsPath, cfg, true);
        }

        public static ColorResBarSettingsConfig LoadSettings()
        {
            var settingsPath = new FilePath(GetSettingsPath());
            ColorResBarSettingsConfig cfg;
            if (!JsonWrapper.TryLoadJson(settingsPath, out cfg))
                cfg = new ColorResBarSettingsConfig();
            return cfg;
        }

        // 读取：资源条颜色
        public static string GetBarColorHex(string resourceName)
        {
            if (settings == null) Load();
            if (string.IsNullOrWhiteSpace(resourceName)) return null;
            string key = resourceName; // 不替换空格，直接使用内部名（支持空格和连字符）
            if (settings.bar_colors != null && settings.bar_colors.TryGetValue(key, out var hex))
                return hex;
            // 未指定则全白
            return "#FFFFFF";
        }

        // 读取：当前量颜色
        public static string GetCurrentColorHex(string resourceName)
        {
            if (settings == null) Load();
            if (string.IsNullOrWhiteSpace(resourceName)) return null;
            string key = resourceName; // 不替换空格
            if (settings.current_colors != null && settings.current_colors.TryGetValue(key, out var hex))
                return hex;
            return "#FFFFFF";
        }

        // 解析配置：每行 "资源名 资源条颜色 当前量颜色"（#RRGGBB），忽略空行和以#开头的注释
        private static bool TryLoadPlainText(FilePath path, out ColorResBarSettingsConfig cfg)
        {
            cfg = null;
            try
            {
                if (!path.FileExists()) return false;
                var lines = path.ReadText().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var bars = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var curs = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        // 资源名可能包含空格/连字符：取最后两个作为颜色，其余拼为名字
                        string bar = parts[parts.Length - 2];
                        string cur = parts[parts.Length - 1];
                        string name = string.Join(" ", parts, 0, parts.Length - 2);
                        if (!bar.StartsWith("#") || (cur != null && !cur.StartsWith("#")))
                            continue; // 非法颜色，跳过
                        bars[name] = bar;
                        curs[name] = cur;
                    }
                }
                cfg = new ColorResBarSettingsConfig
                {
                    bar_colors = bars,
                    current_colors = curs
                };
                return true;
            }
            catch
            {
                cfg = null;
                return false;
            }
        }
    }
}

