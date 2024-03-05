using Editor.Scripts.Settings;

namespace Editor.Scripts
{

internal static class ScreenshotData
{
    private static MegaPintSettingsBase s_settings;

    #region Public Methods
    
    public static string Shortcut()
    {
        s_settings ??= MegaPintSettings.instance.GetSetting("Shortcut");

        return s_settings.GetValue("shortcut", "");
    }

    #endregion
}

}
