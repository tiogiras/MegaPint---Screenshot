#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Editor.Scripts
{

internal static partial class DisplayContent
{
    private const string BasePathScreenshot = "Screenshot/User Interface/Display Content Tabs/";
    #region Private Methods

    private static void OnTabChangedScreenshot(int tab, VisualElement root)
    {

    }

    // Called by reflection
    // ReSharper disable once UnusedMember.Local
    private static void Screenshot(VisualElement root)
    {
        var tabs = root.Q <GroupBox>("Tabs");
        var tabContentParent = root.Q <GroupBox>("TabContent");

        RegisterTabCallbacks(tabs, tabContentParent, 3);

        SetTabContentLocations(BasePathScreenshot + "Tab0", BasePathScreenshot + "Tab1",
            BasePathScreenshot + "Tab2");

        s_onSelectedTabChanged += OnTabChangedScreenshot;
        s_onSelectedPackageChanged += UnsubscribeScreenshot;

        SwitchTab(tabContentParent, 0);
    }

    private static void UnsubscribeScreenshot()
    {
        s_onSelectedTabChanged -= OnTabChangedScreenshot;
        s_onSelectedPackageChanged -= UnsubscribeScreenshot;
    }

    #endregion
}

}
#endif
