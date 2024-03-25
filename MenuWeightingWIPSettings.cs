using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MenuWeightingWIP;

public class MenuWeightingWIPSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new(false);


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
        ("Mod_20", "Mod_human_text_here 20"),
        ("Mod_21", "Mod_human_text_here 21"),
        ("Mod_22", "Mod_human_text_here 22"),
        ("Mod_23", "Mod_human_text_here 23"),
        ("Mod_24", "Mod_human_text_here 24"),
        ("Mod_25", "Mod_human_text_here 25"),
        ("Mod_26", "Mod_human_text_here 26"),
        ("Mod_27", "Mod_human_text_here 27")
    };

    private string selectedModId;

    // Load last saved for both on initialization as its less confusing
    public string ModMobWeightingLastSaved { get; set; } = "";
    public string ModMobWeightingLastSelected { get; set; } = "";
    private const string ClearSettingPopup = "Clear Confirmation";
    private const string OverwritePopup = "Overwrite Confirmation";

    public Swappable HotSwap { get; set; } = new();

    public class Swappable
    {
        public Dictionary<string, Dictionary<string, float>> ModMobWeightings { get; set; } = [];
    }


    [JsonIgnore]
    public CustomNode ModsConfig { get; }

    [JsonIgnore]
    public CustomNode ModWeights { get; }

    public MenuWeightingWIPSettings()
    {
        List<string> files;
        InitializeModMobWeightings();

        ModsConfig = new CustomNode
        {
            DrawDelegate = () =>
            {
                var _fileSaveName = ModMobWeightingLastSaved;
                var _selectedFileName = ModMobWeightingLastSelected;

                if (!ImGui.CollapsingHeader(
                        $"Load / Save Hot Swappable Configurations##{MenuWeightingWIP.Main.Name}Load / Save",
                        ImGuiTreeNodeFlags.DefaultOpen
                    ))
                {
                    return;
                }

                ImGui.Indent();
                ImGui.InputTextWithHint("##SaveAs", "File Path...", ref _fileSaveName, 100);
                ImGui.SameLine();

                if (ImGui.Button("Save To File"))
                {
                    files = GetFiles();

                    // Sanitize the file name by replacing invalid characters
                    _fileSaveName = Path.GetInvalidFileNameChars().Aggregate(
                        _fileSaveName,
                        (current, c) => current.Replace(c, '_')
                    );

                    if (_fileSaveName == string.Empty)
                    {
                        // Log error when the file name is empty
                    }
                    else if (files.Contains(_fileSaveName))
                    {
                        ImGui.OpenPopup(OverwritePopup);
                    }
                    else
                    {
                        SaveFile(HotSwap, $"{_fileSaveName}.json");
                    }
                }

                ImGui.Separator();

                if (ImGui.BeginCombo("Load File##LoadFile", _selectedFileName))
                {
                    files = GetFiles();

                    foreach (var fileName in files)
                    {
                        var isSelected = _selectedFileName == fileName;

                        if (ImGui.Selectable(fileName, isSelected))
                        {
                            _selectedFileName = fileName;
                            _fileSaveName = fileName;
                            LoadFile(fileName);
                        }

                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.Separator();

                if (ImGui.Button("Open Crafting Template Folder"))
                {
                    var configDir = MenuWeightingWIP.Main.ConfigDirectory;
                    var directoryToOpen = Directory.Exists(configDir);

                    if (!directoryToOpen)
                    {
                        // Log error when the config directory doesn't exist
                    }

                    if (configDir != null)
                    {
                        Process.Start("explorer.exe", configDir);
                    }
                }

                if (ShowButtonPopup(OverwritePopup, ["Are you sure?", "STOP"], out var saveSelectedIndex))
                {
                    if (saveSelectedIndex == 0)
                    {
                        SaveFile(HotSwap, $"{_fileSaveName}.json");
                    }
                }

                ModMobWeightingLastSaved = _fileSaveName;
                ModMobWeightingLastSelected = _selectedFileName;
                ImGui.Unindent();
            }
        };

        string modFilter = "", mobFilter = "";

        ModWeights = new CustomNode
        {
            DrawDelegate = () =>
            {
                if (!ImGui.CollapsingHeader("Hot Swappable Configurations"))
                {
                    return;
                }

                ImGui.Indent();
                if (!ImGui.TreeNode("Modifier & Monster Weighting"))
                {
                    return;
                }

                if (ImGui.Button("[x] Clear All"))
                {
                    ImGui.OpenPopup(ClearSettingPopup);
                }

                if (ShowButtonPopup(ClearSettingPopup, ["Are you sure?", "STOP"], out var clearSelectedIndex))
                {
                    if (clearSelectedIndex == 0)
                    {
                        ResetModMobWeightings();
                        return;
                    }
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

                var filteredMods = ModDict
                                   .Where(t => t.Name.Contains(modFilter, StringComparison.InvariantCultureIgnoreCase))
                                   .ToList();

                for (var i = 0; i < filteredMods.Count; i++)
                {
                    var (modId, modName) = filteredMods[i];
                    HighlightSelected(modId, modName, selectedModId == modId, () => selectedModId = modId);

                    // Add a separator for all but the last item
                    if (i < filteredMods.Count - 1)
                    {
                        ImGui.Separator();
                    }
                }

                // Monsters column
                ImGui.TableNextColumn();
                DisplayMobWeightings(mobFilter);
                ImGui.EndTable();
                ImGui.TreePop();
                ImGui.Unindent();
            }
        };
    }

    private static void HighlightSelected(string modId, string modName, bool isSelected, Action onClick)
    {
        if (ImGui.Selectable($"{modName}##{modId}", isSelected))
        {
            onClick.Invoke();
        }
    }

    private void DisplayMobWeightings(string mobFilter)
    {
        if (string.IsNullOrEmpty(selectedModId) || !HotSwap.ModMobWeightings.TryGetValue(selectedModId, out var mobWeightings))
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

    private static bool HighlightWeightChange(float weight)
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
            HotSwap.ModMobWeightings[mod.Id] = MobDict.ToDictionary(mob => mob.Id, mob => 0f);

        selectedModId = ModDict.FirstOrDefault().Id;
    }

    private void ResetModMobWeightings()
    {
        HotSwap.ModMobWeightings = [];
        InitializeModMobWeightings();
    }

    #region Save / Load Section

    public static bool ShowButtonPopup(string popupId, List<string> items, out int selectedIndex)
    {
        selectedIndex = -1;
        var isItemClicked = false;
        var showPopup = true;

        if (!ImGui.BeginPopupModal(
                popupId,
                ref showPopup,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize
            ))
        {
            return false;
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (ImGui.Button(items[i]))
            {
                selectedIndex = i;
                isItemClicked = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
        }

        ImGui.EndPopup();
        return isItemClicked;
    }

    public void SaveFile(Swappable input, string filePath)
    {
        try
        {
            var fullPath = Path.Combine(MenuWeightingWIP.Main.ConfigDirectory, filePath);
            var jsonString = JsonConvert.SerializeObject(input, Formatting.Indented);
            File.WriteAllText(fullPath, jsonString);
        }
        catch (Exception e) { }
    }

    public void LoadFile(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(MenuWeightingWIP.Main.ConfigDirectory, $"{fileName}.json");
            var fileContent = File.ReadAllText(fullPath);

            HotSwap
                = JsonConvert.DeserializeObject<Swappable>(fileContent);
        }
        catch (Exception e) { }
    }

    public List<string> GetFiles()
    {
        var fileList = new List<string>();

        try
        {
            var dir = new DirectoryInfo(MenuWeightingWIP.Main.ConfigDirectory);
            fileList = dir.GetFiles().Select(file => Path.GetFileNameWithoutExtension(file.Name)).ToList();
        }
        catch (Exception e) { }

        return fileList;
    }

    #endregion
}