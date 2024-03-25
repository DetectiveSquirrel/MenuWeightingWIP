using ExileCore;

namespace MenuWeightingWIP;

public class MenuWeightingWIP : BaseSettingsPlugin<MenuWeightingWIPSettings>
{
    public static MenuWeightingWIP Main { get; set; }
    public override bool Initialise()
    {
        Main = this;
        return true;
    }
}