#if UNITY_EDITOR
using MegaPint.Editor.Scripts.GUI.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class used to display the right pane in the BaseWindow </summary>
internal static partial class DisplayContent
{
    #region Private Methods

    // Called by reflection
    // ReSharper disable once UnusedMember.Local
    private static void Screenshot(DisplayContentReferences refs)
    {
        InitializeDisplayContent(
            refs,
            new TabSettings {info = true, settings = true, guides = true, help = true},
            new TabActions
            {
                info = ScreenshotActivateLinks,
                settings = root => { },
                guides = ScreenshotActivateLinks,
                help = ScreenshotActivateLinks
            });
    }

    /// <summary> Activate all links in the text </summary>
    /// <param name="root"> RootVisualElement </param>
    private static void ScreenshotActivateLinks(VisualElement root)
    {
        root.ActivateLinks(
            link =>
            {
                switch (link.linkID)
                {
                    case "windowCapture":
                        EditorApplication.ExecuteMenuItem(Constants.Screenshot.Links.WindowCapture);

                        break;

                    case "shortcutCapture":
                        EditorApplication.ExecuteMenuItem(Constants.Screenshot.Links.ShortcutCapture);

                        break;

                    case "shortcutManager":
                        EditorApplication.ExecuteMenuItem(Constants.BasePackage.Links.Shortcuts);

                        break;

                    case "captureNow":
                        EditorApplication.ExecuteMenuItem(Constants.Screenshot.Links.CaptureNow);

                        break;
                }
            });
    }

    #endregion
}

}
#endif
