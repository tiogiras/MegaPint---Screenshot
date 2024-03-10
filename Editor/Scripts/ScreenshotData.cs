#if UNITY_EDITOR
using Editor.Scripts.Settings;

namespace Editor.Scripts
{

internal static class ScreenshotData
{
    private static MegaPintSettingsBase s_settings;

    public static string LastEditorWindowPath
    {
        get
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Shortcut");

            return s_settings.GetValue("lastEditorWindowPath", "Assets");
        }
        set
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Shortcut");

            s_settings.SetValue("lastEditorWindowPath", value);
        }
    }
}

}
#endif
