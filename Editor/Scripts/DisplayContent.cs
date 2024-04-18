#if UNITY_EDITOR
using Editor.Scripts.GUI;
using Editor.Scripts.PackageManager.Cache;
using Editor.Scripts.PackageManager.Packages;
using Editor.Scripts.Windows;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.Scripts
{

internal static partial class DisplayContent
{
    // Called by reflection
    // ReSharper disable once UnusedMember.Local
    private static void Screenshot(DisplayContentReferences refs)
    {
        InitializeDisplayContent(
            refs,
            new TabSettings
            {
                info = true,
                settings = true,
                guides = true,
                help = true
            },
            new TabActions
            {
                info = ActivateLinks,
                settings = root =>
                {
                    
                },
                guides = ActivateLinks,
                help = root =>
                {
                    
                }
            });
    }

    private static void ActivateLinks(VisualElement root)
    {
        GUIUtility.ActivateLinks(root,
                                 link =>
                                 {
                                     switch (link.linkID)
                                     {
                                         case "windowCapture":
                                             EditorApplication.ExecuteMenuItem(
                                                 "MegaPint/Packages/Screenshot/Window Capture");
                                             break;
                                                     
                                         case "shortcutCapture":
                                             EditorApplication.ExecuteMenuItem(
                                                 "MegaPint/Packages/Screenshot/Shortcut Capture");
                                             break;
                                         
                                         case "shortcutManager":
                                             EditorApplication.ExecuteMenuItem(
                                                 "Edit/Shortcuts...");
                                             break;
                                         
                                         case "captureNow":
                                             EditorApplication.ExecuteMenuItem(
                                                 "MegaPint/Packages/Screenshot/Capture Now");
                                             break;
                                     }
                                 });
    }

    /*#region Private Methods

    private static void OnTabChangedScreenshot(int tab, VisualElement root)
    {
        switch (tab)
        {
            case 0:

                root.Q <Label>("PackageText").text =
                    PackageCache.Get(PackageKey.Screenshot).Description;

                root.Q <Button>("BTN_WindowCapture").clickable = new Clickable(
                    () => {ContextMenu.TryOpen <WindowCapture>(false);});
                
                root.Q <Button>("BTN_ShortcutCapture").clickable = new Clickable(
                    () => {ContextMenu.TryOpen <ShortcutCapture>(false);});
                
                break;
            case 1:
                
                var toggle = root.Q <Toggle>("ExternalExport");
                
                toggle.value = ScreenshotData.ExternalExport;

                toggle.RegisterValueChangedCallback(
                    evt => ScreenshotData.ExternalExport = evt.newValue);
                
                break;
        }
    }


    private static void Screenshot(VisualElement root)
    {
        var tabs = root.Q <GroupBox>("Tabs");
        var tabContentParent = root.Q <GroupBox>("TabContent");

        const int TabCount = 4;
        
        RegisterTabCallbacks(tabs, tabContentParent, TabCount);

        SetTabContentLocations(BasePathScreenshot, TabCount);

        s_onSelectedTabChanged += OnTabChangedScreenshot;
        s_onSelectedPackageChanged += UnsubscribeScreenshot;

        SwitchTab(tabContentParent, 0);
    }

    private static void UnsubscribeScreenshot()
    {
        s_onSelectedTabChanged -= OnTabChangedScreenshot;
        s_onSelectedPackageChanged -= UnsubscribeScreenshot;
    }

    #endregion*/
}

}
#endif
