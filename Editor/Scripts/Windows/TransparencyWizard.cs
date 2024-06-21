#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtility = MegaPint.Editor.Scripts.GUI.Utility.GUIUtility;
using Object = UnityEngine.Object;
#if USING_URP
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endif

namespace MegaPint.Editor.Scripts.Windows
{

/// <summary>
///     Window based on the <see cref="EditorWindowBase" /> to display a window that fixes an issue regarding
///     transparency in urp
/// </summary>
internal class TransparencyWizard : EditorWindowBase
{
    private VisualTreeAsset _baseWindow;

    private Button _btnFinish;
    private Button _btnNext;
    private ObjectField _pipelineAsset;

    private ObjectField _rendererData;

    private int _state;

    private GroupBox _step0;
    private GroupBox _step1;
    private GroupBox _step2;
    private GroupBox _step3;

    #region Public Methods

    public override EditorWindowBase ShowWindow()
    {
        titleContent.text = "Transparency Wizard";

        //minSize = new Vector2(700, 350); // TODO get correct values

        if (!SaveValues.Screenshot.ApplyPSTransparencyWizard)
            return this;

        //this.CenterOnMainWin(800, 450); // TODO get correct values
        SaveValues.Screenshot.ApplyPSTransparencyWizard = false;
        
        return this;
    }

    #endregion

    #region Protected Methods

    protected override string BasePath()
    {
        return Constants.Screenshot.UserInterface.TransparencyWizard;
    }

    protected override void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;
        VisualElement content = GUIUtility.Instantiate(_baseWindow, root);

        content.style.flexGrow = 1;
        content.style.flexShrink = 1;

        _btnNext = content.Q <Button>("BTN_Next");
        _btnFinish = content.Q <Button>("BTN_Finish");

        _rendererData = content.Q <ObjectField>("RendererData");
        _pipelineAsset = content.Q <ObjectField>("PipelineAsset");

        _step0 = content.Q <GroupBox>("Step0");
        _step1 = content.Q <GroupBox>("Step1");
        _step2 = content.Q <GroupBox>("Step2");
        _step3 = content.Q <GroupBox>("Step3");

#if USING_URP
        _pipelineAsset.value = SaveValues.Screenshot.RenderPipelineAsset();
#endif

        RegisterCallbacks();

        ChangeState(0);
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

        _pipelineAsset.RegisterValueChangedCallback(PipelineAssetChanged);
    }

    protected override void UnRegisterCallbacks()
    {
        _btnNext.clicked -= Next;
        _btnFinish.clicked -= Finish;

        _pipelineAsset.UnregisterValueChangedCallback(PipelineAssetChanged);
    }

    #endregion

    #region Private Methods

    /// <summary> Callback when pipeline asset was changed </summary>
    /// <param name="evt"> Callback event </param>
    private static void PipelineAssetChanged(ChangeEvent <Object> evt)
    {
        SaveValues.Screenshot.RenderPipelineAssetPath =
            evt.newValue == null ? "" : AssetDatabase.GetAssetPath(evt.newValue);
    }

    /// <summary> Can change to next step </summary>
    /// <returns> If the next step is available </returns>
    /// <exception cref="System.ArgumentOutOfRangeException"> Step not found </exception>
    private bool CanChange()
    {
        return _state switch
               {
                   0 => true,
                   1 => _pipelineAsset.value != null,
                   2 => _rendererData.value != null,
                   var _ => throw new ArgumentOutOfRangeException()
               };
    }

    /// <summary> Change step </summary>
    /// <param name="state"> Next step </param>
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

    /// <summary> Execute the transparency wizard </summary>
    /// <param name="path"> Path of the asset </param>
    private void ExecuteWizard(string path)
    {
#if USING_URP
        var rendererDataTemplate = (UniversalRendererData)_rendererData.value;

        SaveValues.Screenshot.RendererDataPath = Path.Combine(path, "Transparency Renderer Data.asset");
        UniversalRendererData rendererData =
            Utility.CopyAndLoadAsset(rendererDataTemplate, SaveValues.Screenshot.RendererDataPath);

        PostProcessData postProcessData = Utility.CopyAndLoadAsset(rendererData.postProcessData,
            Path.Combine(path, "Transparency PostProcess Data.asset"));

        var uberShader = Utility.CopyAndLoadAsset <Shader>(
            "Packages/com.tiogiras.megapint-screenshot/Editor/Scripts/Uber Post Alpha.txt",
            Path.Combine(path, "Uber Post Alpha.shader"));

        rendererData.postProcessData = postProcessData;
        EditorUtility.SetDirty(rendererData);

        postProcessData.shaders.uberPostPS = uberShader;
        EditorUtility.SetDirty(postProcessData);

        List <string> lines = File.ReadAllLines(SaveValues.Screenshot.RenderPipelineAssetPath).ToList();

        var foundRenderers = false;
        var index = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (foundRenderers)
            {
                if (line.StartsWith("  - {fileID:"))
                    continue;

                index = i;
                break;
            }

            if (line.Equals("  m_RendererDataList:"))
                foundRenderers = true;
        }

        GUID guid = AssetDatabase.GUIDFromAssetPath(SaveValues.Screenshot.RendererDataPath);
        lines.Insert(index, $"  - {{fileID: 11400000, guid: {guid}, type: 2}}");

        File.WriteAllLines(SaveValues.Screenshot.RenderPipelineAssetPath, lines);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        Close();
    }

    /// <summary> Finish the setup </summary>
    private void Finish()
    {
        var path = EditorUtility.SaveFolderPanel("Target folder for settings", "Assets", "");

        if (!path.IsPathInProject(out var pathInProject))
        {
            EditorUtility.DisplayDialog(
                "Not in project",
                "The selected folder is not in the project.",
                "Ok");

            return;
        }

        ExecuteWizard(pathInProject);
    }

    /// <summary> Next step </summary>
    private void Next()
    {
        if (!CanChange())
        {
            EditorUtility.DisplayDialog(
                "Missing Reference",
                "You must set all required references to continue the setup.",
                "Ok");

            return;
        }

        ChangeState(_state + 1);
    }

    /// <summary> Set window size </summary>
    private void SetSize()
    {
        maxSize = Size();
        minSize = Size();
    }

    /// <summary> Get the current window size based on the current step </summary>
    /// <returns> Window size </returns>
    /// <exception cref="System.ArgumentOutOfRangeException"> Step not found </exception>
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

    #endregion
}

}
#endif
