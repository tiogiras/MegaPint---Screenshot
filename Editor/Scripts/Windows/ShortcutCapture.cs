#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtility = Editor.Scripts.GUI.GUIUtility;

namespace Editor.Scripts.Windows
{

/// <summary>
///     Window based on the <see cref="MegaPintEditorWindowBase" /> to display and handle the shortcut capture
///     feature
/// </summary>
internal class ShortcutCapture : MegaPintEditorWindowBase
{
    private const string FolderBasePath = "Screenshot/User Interface";

    private VisualTreeAsset _baseWindow;

    private Button _btnAll;
    private Button _btnNone;
    private Button _btnRefresh;

    private ListView _cameras;

    private List <CameraCapture> _cams = new();
    private VisualTreeAsset _listItem;
    private Label _placeholder;
    #region Public Methods

    /// <summary> Show the window </summary>
    /// <returns> Window instance </returns>
    public override MegaPintEditorWindowBase ShowWindow()
    {
        titleContent.text = "Shortcut Capture";

        return this;
    }

    #endregion
    #region Protected Methods

    protected override string BasePath()
    {
        return Path.Combine(FolderBasePath, "Shortcut Capture");
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = GUIUtility.Instantiate(_baseWindow, root);
        
        content.style.flexGrow = 1;
        content.style.flexShrink = 1;

        _cameras = content.Q <ListView>("Cameras");
        _placeholder = content.Q <Label>("Placeholder");

        _btnAll = content.Q <Button>("BTN_All");
        _btnNone = content.Q <Button>("BTN_None");
        _btnRefresh = content.Q <Button>("BTN_Refresh");

        RegisterCallbacks();

        _cameras.makeItem += () => GUIUtility.Instantiate(_listItem);

        _cameras.bindItem += (element, i) =>
        {
            CameraCapture camera = _cams[i];
            element.Q <Label>("Name").text = camera.gameObject.name;

            var on = camera.listenToShortcut;
            var onButton = element.Q <Button>("BTN_On");
            var offButton = element.Q <Button>("BTN_Off");

            UpdateListItem(onButton, offButton, on);

            onButton.clickable = new Clickable(
                () =>
                {
                    camera.listenToShortcut = true;
                    UpdateListItem(onButton, offButton, true);
                });

            offButton.clickable = new Clickable(
                () =>
                {
                    camera.listenToShortcut = false;
                    UpdateListItem(onButton, offButton, false);
                });
        };

        RefreshCameras();
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());
        _listItem = Resources.Load <VisualTreeAsset>(Path.Combine(FolderBasePath, "Shortcut Capture/Item"));

        return _baseWindow != null && _listItem != null;
    }

    protected override void RegisterCallbacks()
    {
        _btnRefresh.clicked += RefreshCameras;

        _btnAll.clicked += SelectAll;
        _btnNone.clicked += SelectNone;
    }

    protected override void UnRegisterCallbacks()
    {
        _btnRefresh.clicked -= RefreshCameras;

        _btnAll.clicked -= SelectAll;
        _btnNone.clicked -= SelectNone;
    }

    #endregion
    #region Private Methods

    private static void UpdateListItem(VisualElement onButton, VisualElement offButton, bool on)
    {
        GUIUtility.ToggleActiveButtonInGroup(on ? 0 : 1, onButton, offButton);
    }

    private void RefreshCameras()
    {
        List <CameraCapture> cams = FindObjectsOfType <CameraCapture>().ToList();

        var hasCams = cams is {Count: > 0};

        if (hasCams)
        {
            _cams = cams;
            _cameras.itemsSource = _cams;
        }

        _cameras.style.display = hasCams ? DisplayStyle.Flex : DisplayStyle.None;
        _btnAll.style.display = hasCams ? DisplayStyle.Flex : DisplayStyle.None;
        _btnNone.style.display = hasCams ? DisplayStyle.Flex : DisplayStyle.None;

        _placeholder.style.display = hasCams ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void SelectAll()
    {
        SetAllCameraListeners(true);
    }

    private void SelectNone()
    {
        SetAllCameraListeners(false);
    }

    private void SetAllCameraListeners(bool listening)
    {
        if (_cams is not {Count: > 0})
            return;

        foreach (CameraCapture cam in _cams)
            cam.listenToShortcut = listening;

        _cameras.RefreshItems();
    }

    #endregion
}

}
#endif
