#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{

/// <summary>
///     Window based on the <see cref="MegaPintEditorWindowBase" /> to display a window that fixes an issue regarding transparency in urp
/// </summary>
internal class TransparencyWizard : MegaPintEditorWindowBase
{
    private const string FolderBasePath = "Screenshot/User Interface/";

    private VisualTreeAsset _baseWindow;

    private Button _btnFinish;
    private Button _btnNext;

    private ObjectField _rendererData;
    private ObjectField _postProcessData;

    private GroupBox _step0;
    private GroupBox _step1;
    private GroupBox _step2;
    private GroupBox _step3;

    private int _state;

    #region Public Methods

    /// <summary> Show the window </summary>
    /// <returns> Window instance </returns>
    public override MegaPintEditorWindowBase ShowWindow()
    {
        titleContent.text = "Transparency Wizard";

        return this;
    }

    #endregion

    #region Protected Methods

    protected override string BasePath()
    {
        return FolderBasePath + "TransparencyWizard";
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = _baseWindow.Instantiate();
        content.style.flexGrow = 1;
        content.style.flexShrink = 1;

        _btnNext = content.Q <Button>("BTN_Next");
        _btnFinish = content.Q <Button>("BTN_Finish");

        _rendererData = content.Q <ObjectField>("RendererData");
        _postProcessData = content.Q <ObjectField>("PostProcessData");

        _step0 = content.Q <GroupBox>("Step0");
        _step1 = content.Q <GroupBox>("Step1");
        _step2 = content.Q <GroupBox>("Step2");
        _step3 = content.Q <GroupBox>("Step3");

        RegisterCallbacks();

        ChangeState(0);
        
        root.Add(content);
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());

        return _baseWindow != null;
    }

    protected override void RegisterCallbacks()
    {
        _btnNext.clicked += Next;
        _btnFinish.clicked += Finish;
    }

    protected override void UnRegisterCallbacks()
    {
        _btnNext.clicked -= Next;
        _btnFinish.clicked -= Finish;
    }

    private void Finish()
    {
        var path = EditorUtility.SaveFolderPanel("Target folder for settings", "Assets", "");

        if (!path.IsPathInProject(out var pathInProject))
        {
            EditorUtility.DisplayDialog("Not in project", "The selected folder is not in the project.", "Ok");
            return;
        }
        
        ExecuteWizard(pathInProject);
    }

    private void ExecuteWizard(string path)
    {
        var rendererDataTemplate = _rendererData.value; // TODO cast to data

        //AssetDatabase.CopyAsset()
    }

    #endregion

    private void Next()
    {
        if (!CanChange())
        {
            EditorUtility.DisplayDialog("Missing Reference", "You must set the specified template to continue the setup.", "Ok");
            return;   
        }

        ChangeState(_state + 1);
    }
    
    private void ChangeState(int state)
    {
        _state = state;
        
        _step0.style.display = state == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        _step1.style.display = state == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        _step2.style.display = state == 2 ? DisplayStyle.Flex : DisplayStyle.None;
        _step3.style.display = state == 3 ? DisplayStyle.Flex : DisplayStyle.None;

        _btnNext.style.display = state != 3 ? DisplayStyle.Flex : DisplayStyle.None;
        _btnFinish.style.display = state == 3 ? DisplayStyle.Flex : DisplayStyle.None;
        
        SetSize();
    }

    private bool CanChange()
    {
        return _state switch
               {
                   0 => true,
                   1 => _rendererData.value != null,
                   2 => _postProcessData.value != null,
                   var _ => throw new ArgumentOutOfRangeException()
               };
    }

    private void SetSize()
    {
        maxSize = Size();
        minSize = Size();
    }
    
    private Vector2 Size()
    {
        return _state switch
               {
                   0 => new Vector2(300, 125),
                   1 => new Vector2(400, 200),
                   2 => new Vector2(400, 200),
                   3 => new Vector2(300, 125),
                   var _ => throw new ArgumentOutOfRangeException()
               };
    }
}

}
#endif
