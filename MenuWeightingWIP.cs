using ExileCore;

namespace MenuWeightingWIP;

public class MenuWeightingWIP : BaseSettingsPlugin<MenuWeightingWIPSettings>
{
    public static MenuWeightingWIP Main { get; set; }
    public override bool Initialise()
    {
        Main = this;
        Settings._fileSaveName = Settings.NonUserData.ModMobWeightingLastSaved ?? "NoSave";
        Settings._selectedFileName = Settings.NonUserData.ModMobWeightingLastSaved ?? "NoSave";
        return true;
    }
}