using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuWeightingWIP;

public class MenuWeightingWIPSettings : ISettings
{
    private static readonly IReadOnlyList<(string Id, string Name)> MobDict = new List<(string, string)>
    {
        ("Mob_1", "Mob 1"),
        ("Mob_2", "Mob 2"),
        ("Mob_3", "Mob 3"),
        ("Mob_4", "Mob 4"),
        ("Mob_5", "Mob 5"),
        ("Mob_6", "Mob 6"),
        ("Mob_7", "Mob 7"),
        ("Mob_8", "Mob 8"),
        ("Mob_9", "Mob 9"),
        ("Mob_10", "Mob 10"),
        ("Mob_11", "Mob 11"),
        ("Mob_12", "Mob 12"),
        ("Mob_13", "Mob 13"),
        ("Mob_14", "Mob 14"),
        ("Mob_15", "Mob 15")
    };

    private static readonly IReadOnlyList<(string Id, string Name)> ModDict = new List<(string, string)>
    {
        ("Mod_1", "Mod_human_text_here 1"),
        ("Mod_2", "Mod_human_text_here 2"),
        ("Mod_3", "Mod_human_text_here 3"),
        ("Mod_4", "Mod_human_text_here 4"),
        ("Mod_5", "Mod_human_text_here 5"),
        ("Mod_6", "Mod_human_text_here 6"),
        ("Mod_7", "Mod_human_text_here 7"),
        ("Mod_8", "Mod_human_text_here 8"),
        ("Mod_9", "Mod_human_text_here 9"),
        ("Mod_10", "Mod_human_text_here 10"),
        ("Mod_11", "Mod_human_text_here 11"),
        ("Mod_12", "Mod_human_text_here 12"),
        ("Mod_13", "Mod_human_text_here 13"),
        ("Mod_14", "Mod_human_text_here 14"),
        ("Mod_15", "Mod_human_text_here 15"),
        ("Mod_16", "Mod_human_text_here 16"),
        ("Mod_17", "Mod_human_text_here 17"),
        ("Mod_18", "Mod_human_text_here 18"),
        ("Mod_19", "Mod_human_text_here 19"),
        ("Mod_20", "Mod_human_text_here 20")
    };

    public Dictionary<string, Dictionary<string, float>> ModMobWeightings = [];

    private string selectedModId;

    public MenuWeightingWIPSettings()
    {
        InitializeModMobWeightings();
        string modFilter = "", mobFilter = "";

        Mods = new CustomNode
        {
            DrawDelegate = () =>
            {
                if (!ImGui.TreeNode("Mod - Mob Weighting Configuration"))
                {
                    return;
                }

                ImGui.InputTextWithHint("Modifier Filter##ModFilter", "Filter Modifiers here", ref modFilter, 100);
                ImGui.InputTextWithHint("Monster Filter##MobFilter", "Filter Monsters here", ref mobFilter, 100);

                if (!ImGui.BeginTable("ModMobConfig", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    return;
                }

                ImGui.TableSetupColumn("Modifiers", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Monsters", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Modifiers column
                ImGui.TableNextColumn();

                ModDict.Where(t => t.Name.Contains(modFilter, StringComparison.InvariantCultureIgnoreCase)).ToList()
                       .ForEach(
                           mod =>
                           {
                               var (modId, modName) = mod;
                               HighlightButton(modId, modName, selectedModId == modId, () => selectedModId = modId);
                           }
                       );

                // Monsters column
                ImGui.TableNextColumn();
                DisplayMobWeightings(mobFilter);
                ImGui.EndTable();
                ImGui.TreePop();
            }
        };
    }

    [JsonIgnore]
    public CustomNode Mods { get; }

    public ToggleNode Enable { get; set; } = new(false);

    private void HighlightButton(string modId, string modName, bool isSelected, Action onClick)
    {
        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonHovered));
        }

        if (ImGui.Button($"{modName}##{modId}"))
        {
            onClick.Invoke();
        }

        if (isSelected)
        {
            ImGui.PopStyleColor();
        }
    }

    private void DisplayMobWeightings(string mobFilter)
    {
        if (string.IsNullOrEmpty(selectedModId) || !ModMobWeightings.TryGetValue(selectedModId, out var mobWeightings))
        {
            return;
        }

        foreach (var (mobId, weight) in mobWeightings)
        {
            var mobName = MobDict.FirstOrDefault(m => m.Id == mobId).Name;

            if (mobName == null || !mobName.Contains(mobFilter, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var refModWeight = mobWeightings[mobId];
            DisplayWeightSlider(mobId, ref refModWeight, mobName, weight);
            mobWeightings[mobId] = refModWeight;
        }
    }

    private void DisplayWeightSlider(string mobId, ref float weight, string mobName, float initialWeight)
    {
        var tempWeight = weight;
        var wasStyled = HighlightWeightChange(weight);
        ImGui.SliderFloat($"##{mobId}", ref tempWeight, -10.0f, 10.0f);

        if (wasStyled)
        {
            ImGui.PopStyleColor();
        }

        if (tempWeight != initialWeight)
        {
            weight = tempWeight;
        }

        ImGui.SameLine();
        ImGui.Text(mobName);
    }

    private bool HighlightWeightChange(float weight)
    {
        if (weight == 0)
        {
            return false;
        }

        var color = weight > 0 ? new Vector4(0.16f, 0.48f, 0.16f, 0.54f) : new Vector4(0.48f, 0.16f, 0.16f, 0.93f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, color.ToVector4Num());
        return true;
    }

    private void InitializeModMobWeightings()
    {
        foreach (var mod in ModDict)
            ModMobWeightings[mod.Id] = MobDict.ToDictionary(mob => mob.Id, mob => 0f);

        selectedModId = ModDict.FirstOrDefault().Id;
    }
}