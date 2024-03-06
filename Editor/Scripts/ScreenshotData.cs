using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Scripts.Settings;
using UnityEngine;

namespace Editor.Scripts
{

internal static class ScreenshotData
{
    private static MegaPintSettingsBase s_settings;

    #region Public Methods
    
    public static List <KeyCode> Shortcut
    {
        get
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Shortcut");
            
            var keyCodeString = s_settings.GetValue("shortcut", "");
            List <KeyCode> output = new();

            if (string.IsNullOrEmpty(keyCodeString))
                return output;

            var keys = keyCodeString.Split(",");
            output.AddRange(keys.Select(Enum.Parse <KeyCode>));

            return output;
        }
        set
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Shortcut");

            var input = new StringBuilder("");

            if (value is not {Count: > 0})
            {
                s_settings.SetValue("shortcut", input.ToString());
                return;
            }

            foreach (KeyCode keyCode in value)
            {
                input.Append($"{keyCode},");
            }

            s_settings.SetValue("shortcut", input.ToString()[..^1]);
        }
    }

    #endregion
}

}
