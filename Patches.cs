using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using SFS.UI;
using System;
using SFS.World;
using SFS.Parts.Modules;
using _Game.Drawers;

namespace ColorResBar
{
    // 直接在资源条数值更新时设定颜色
    [HarmonyPatch(typeof(ResourceBar), nameof(ResourceBar.UpdatePercent))]
    public static class ResourceBarColorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ResourceBar __instance, float percent)
        {
            try
            {
                // 找到该条上方的标题文本
                int titleIndex;
                string title = FindTitleAbove(__instance, out titleIndex);
                if (string.IsNullOrEmpty(title)) return;

                var rocket = PlayerController.main?.player?.Value as Rocket;
                if (rocket == null || rocket.resources == null) return;

                string titleLower = title.ToLowerInvariant();
                int barIndex = GetBarIndexInSection(__instance, titleIndex);

                var groups = new System.Collections.Generic.List<ResourceModule>();
                if (rocket.resources.localGroups != null)
                {
                    foreach (var g in rocket.resources.localGroups)
                        if (IsTypeMatch(g.resourceType, titleLower)) groups.Add(g);
                }
                if (groups.Count == 0) return;

                int barsCountUnderTitle = CountBarsInSection(__instance, titleIndex);
                int barsPerGroup = 1;
                if (barsCountUnderTitle >= groups.Count && barsCountUnderTitle % groups.Count == 0)
                    barsPerGroup = barsCountUnderTitle / groups.Count;

                bool compressedSingleBarForManyGroups = (groups.Count > 1 && barsCountUnderTitle == 1);

                ResourceModule targetGroup = null;
                if (compressedSingleBarForManyGroups)
                {
                    targetGroup = groups[0];
                }
                else
                {
                    int groupIndex = 0;
                    if (barsPerGroup > 1)
                        groupIndex = Math.Max(0, Math.Min(groups.Count - 1, barIndex / barsPerGroup));
                    else if (groups.Count > 1)
                        groupIndex = Math.Max(0, barIndex % groups.Count);
                    targetGroup = groups[Mathf.Clamp(groupIndex, 0, groups.Count - 1)];
                }
                if (targetGroup == null || targetGroup.resourceType == null) return;

                string key = targetGroup.resourceType.name ?? title;
                string hex = ColorResBar.ColorResBarSettings.GetBarColorHex(key);
                if (string.IsNullOrEmpty(hex))
                {
                    string display = targetGroup.resourceType.displayName.Field ?? title;
                    hex = ColorResBar.ColorResBarSettings.GetBarColorHex(display);
                }
                if (string.IsNullOrEmpty(hex)) return;

                if (ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    if (__instance.bar != null)
                    {
                        __instance.bar.color = color;
                    }

                    // 文字颜色：优先 text_colors；若为空则复用条色
                    string textHex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(key);
                    if (string.IsNullOrEmpty(textHex))
                    {
                        string display = targetGroup.resourceType.displayName.Field ?? title;
                        textHex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(display);
                    }
                    Color textColor = color;
                    if (!string.IsNullOrEmpty(textHex) && ColorUtility.TryParseHtmlString(textHex, out var parsed))
                        textColor = parsed;

                    if (__instance.percentText != null)
                        __instance.percentText.Color = textColor;
                    if (__instance.countText != null)
                        __instance.countText.Color = textColor;
                }
            }
            catch (Exception)
            {
            }
        }

        // 找到当前条正上方的标题文本
        static string FindTitleAbove(ResourceBar bar, out int titleSiblingIndex)
        {
            titleSiblingIndex = -1;
            var parent = bar.transform.parent;
            if (parent == null) return null;
            for (int i = bar.transform.GetSiblingIndex() - 1; i >= 0; i--)
            {
                var node = parent.GetChild(i);
                var t = node.GetComponentInChildren<TextAdapter>();
                if (t != null && !string.IsNullOrEmpty(t.Text) && t.Text.Contains(":"))
                {
                    titleSiblingIndex = i;
                    string title = t.Text.Trim();
                    return title;
                }
            }
            return null;
        }

        static int GetBarIndexInSection(ResourceBar bar, int titleSiblingIndex)
        {
            if (titleSiblingIndex < 0) return 0;
            var parent = bar.transform.parent;
            int target = bar.transform.GetSiblingIndex();
            int index = 0;
            for (int i = titleSiblingIndex + 1; i < target; i++)
            {
                var rb = parent.GetChild(i).GetComponent<ResourceBar>();
                if (rb != null) index++;
            }
            return index;
        }

        static int CountBarsInSection(ResourceBar bar, int titleSiblingIndex)
        {
            if (titleSiblingIndex < 0) return 1;
            var parent = bar.transform.parent;
            int count = 0;
            for (int i = titleSiblingIndex + 1; i < parent.childCount; i++)
            {
                var node = parent.GetChild(i);
                var t = node.GetComponentInChildren<TextAdapter>();
                if (t != null && !string.IsNullOrEmpty(t.Text) && t.Text.Contains(":")) break;
                if (node.GetComponent<ResourceBar>() != null) count++;
            }
            return count;
        }

        static bool IsTypeMatch(ResourceType type, string titleLower)
        {
            string display = (type.displayName.Field ?? type.name ?? "").Trim().ToLowerInvariant();
            return display.Contains(titleLower) || titleLower.Contains(display);
        }
    }

    // 为资源传输界面的条与指示线应用颜色（当前量颜色）
    [HarmonyPatch(typeof(FuelTransferUI), nameof(FuelTransferUI.DrawFuelPercent))]
    public static class FuelTransferUIColorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(FuelTransferUI __instance, SFS.World.Resources.Transfer transfer)
        {
            try
            {
                var group = transfer?.group;
                if (group == null || group.resourceType == null) return;
                string key = group.resourceType.name;
                string hex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(key);
                if (string.IsNullOrEmpty(hex))
                {
                    string display = group.resourceType.displayName.Field;
                    hex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(display);
                }
                if (string.IsNullOrEmpty(hex)) return;
                if (ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    if (__instance.resourceBar != null)
                        __instance.resourceBar.color = color;
                    if (__instance.percentText != null)
                        __instance.percentText.Color = color;
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(FuelTransferDrawer), "LateUpdate")]
    public static class FuelTransferLineColorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(FuelTransferDrawer __instance)
        {
            try
            {
                var res = PlayerController.main?.player?.Value as Rocket;
                if (res == null || res.resources == null) return;
                if (res.resources.transfers.Count != 2) return;
                var t0 = res.resources.transfers[0];
                var t1 = res.resources.transfers[1];
                if (t0.group == null || t1.group == null || t0.group.resourceType != t1.group.resourceType) return;
                string key = t0.group.resourceType.name;
                string hex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(key);
                if (string.IsNullOrEmpty(hex))
                {
                    string display = t0.group.resourceType.displayName.Field;
                    hex = ColorResBar.ColorResBarSettings.GetCurrentColorHex(display);
                }
                if (string.IsNullOrEmpty(hex)) return;
                if (ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    // lines 是私有池，无法直接访问具体 LineRenderer；
                    var prefab = __instance.linePrefab;
                    if (prefab != null)
                    {
                        var lr = prefab.GetComponent<LineRenderer>();
                        if (lr != null)
                        {
                            lr.startColor = color;
                            lr.endColor = color;
                        }
                    }
                }
            }
            catch { }
        }
    }
}