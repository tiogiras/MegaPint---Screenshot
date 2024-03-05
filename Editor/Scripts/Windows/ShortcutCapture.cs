#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{

/// <summary> Window based on the <see cref="MegaPintEditorWindowBase" /> to display and handle the shortcut capture feature </summary>
internal class ShortcutCapture : MegaPintEditorWindowBase
{
    private static readonly Color s_onColor = new(.8196078431372549f, 0f, .4470588235294118f);
    private static readonly Color s_offColor = new(.34f, .34f, .34f);
    
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

    private const string FolderBasePath = "Screenshot/User Interface/";

    protected override string BasePath()
    {
        return FolderBasePath + "ShortcutCapture";
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = _baseWindow.Instantiate();

        _cameras = content.Q <ListView>("Cameras");
        _placeholder = content.Q <Label>("Placeholder");
        
        _shortcut = content.Q <Label>("Shortcut");
        _shortcutHelp = content.Q <Label>("ShortcutHelp");

        _btnAll = content.Q <Button>("BTN_All");
        _btnNone = content.Q <Button>("BTN_None");
        _btnRefresh = content.Q <Button>("BTN_Refresh");
        _btnShortcut = content.Q <Button>("BTN_Shortcut");

        _shortcut.text = ScreenshotData.Shortcut();
        _shortcutHelp.style.display = DisplayStyle.None;
        
        RegisterCallbacks();

        _cameras.makeItem += () => _listItem.Instantiate();

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

        root.Add(content);
    }

    private static void UpdateListItem(VisualElement onButton, VisualElement offButton, bool on)
    {
        onButton.style.backgroundColor = on ? s_onColor : s_offColor;
        offButton.style.backgroundColor = !on ? s_onColor : s_offColor;
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

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());
        _listItem = Resources.Load <VisualTreeAsset>(FolderBasePath + "ShortcutCaptureItem");

        return _baseWindow != null && _listItem != null;
    }

    protected override void RegisterCallbacks()
    {
        _btnShortcut.clicked += ListenForShortcut;
        _btnRefresh.clicked += RefreshCameras;

        _btnAll.clicked += SelectAll;
        _btnNone.clicked += SelectNone;
        
        rootVisualElement.RegisterCallback <KeyDownEvent>(KeyDown);
    }

    private void ListenForShortcut()
    {
        _shortcutHelp.style.display = DisplayStyle.Flex;
        _listeningForInput = true;
    }

    private void SelectNone()
    {
        SetAllCameraListeners(false);
    }

    private void SelectAll()
    {
        SetAllCameraListeners(true);
    }

    private void SetAllCameraListeners(bool listening)
    {
        if (_cams is not {Count: > 0})
            return;

        foreach (CameraCapture cam in _cams)
        {
            cam.listenToShortcut = listening;
        }
        
        _cameras.RefreshItems();
    }

    protected override void UnRegisterCallbacks()
    {
        _btnShortcut.clicked -= ListenForShortcut;
        _btnRefresh.clicked -= RefreshCameras;

        _btnAll.clicked -= SelectAll;
        _btnNone.clicked -= SelectNone;

        rootVisualElement.UnregisterCallback<KeyDownEvent>(KeyDown);
    }

    private void KeyDown(KeyDownEvent evt)
    {
        Debug.Log("Input");
        
        if (!_listeningForInput)
            return;

        Debug.Log(evt.keyCode);
        
        switch (evt.keyCode)
        {
            case KeyCode.None:
                return;

            case KeyCode.Escape:
                _listeningForInput = false;
                _shortcutHelp.style.display = DisplayStyle.None;
                return;

            case KeyCode.Return:
                return;

            default:
                if (!_keys.Contains(evt.keyCode))
                    _keys.Add(evt.keyCode);

                _shortcut.text = KeysToString();
                return;
        }
    }

    #endregion

    // TODO make it work
    private string KeysToString()
    {
        var str = new StringBuilder("");

        if (_keys.Count == 0)
            return str.ToString();

        foreach (KeyCode key in _keys)
        {
            str.Append($"{key} + ");
        }

        return str.ToString()[..^3];
    }
    
    private List <KeyCode> _keys = new();
    private bool _listeningForInput;
    
    #region Private

    private VisualTreeAsset _baseWindow;
    private VisualTreeAsset _listItem;

    private ListView _cameras;
    private Label _placeholder;
    
    private Label _shortcut;
    private Label _shortcutHelp;

    private Button _btnAll;
    private Button _btnNone;
    private Button _btnRefresh;
    private Button _btnShortcut;

    #endregion

    private List <CameraCapture> _cams = new();
}

}
#endif
