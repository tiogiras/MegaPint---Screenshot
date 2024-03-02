#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Editor.Scripts.Internal
{

[Preserve]
public class AspectRatioPanel : VisualElement
{
    [Preserve]
    public new class UxmlFactory : UxmlFactory <AspectRatioPanel, UxmlTraits>
    {
    }

    [Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable <UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get
            {
                yield break;
            }
        }

        private readonly UxmlIntAttributeDescription _aspectRatioX = new()
        {
            name = "aspect-ratio-x", defaultValue = 16, restriction = new UxmlValueBounds {min = "1"}
        };

        private readonly UxmlIntAttributeDescription _aspectRatioY = new()
        {
            name = "aspect-ratio-y", defaultValue = 9, restriction = new UxmlValueBounds {min = "1"}
        };

        private readonly UxmlFloatAttributeDescription _scale = new() {name = "scale", defaultValue = 1f};

        #region Public Methods

        public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
        {
            base.Init(visualElement, attributes, creationContext);

            if (visualElement is not AspectRatioPanel element)
                return;

            element.aspectRatioX = Mathf.Max(1, _aspectRatioX.GetValueFromBag(attributes, creationContext));
            element.aspectRatioY = Mathf.Max(1, _aspectRatioY.GetValueFromBag(attributes, creationContext));
            element._scale = _scale.GetValueFromBag(attributes, creationContext);
            element.FitToParent();
        }

        #endregion
    }

    public int aspectRatioX = 16;

    public int aspectRatioY = 9;

    private float _scale = 1;

    public AspectRatioPanel()
    {
        style.position = Position.Relative;
        style.left = StyleKeyword.Auto;
        style.top = StyleKeyword.Auto;
        style.right = StyleKeyword.Auto;
        style.bottom = StyleKeyword.Auto;
        RegisterCallback <AttachToPanelEvent>(OnAttachToPanelEvent);
    }

    #region Private Methods

    public void FitToParent()
    {
        if (parent == null)
            return;

        var parentW = parent.resolvedStyle.width;
        var parentH = parent.resolvedStyle.height;

        if (float.IsNaN(parentW) || float.IsNaN(parentH))
            return;

        if (aspectRatioX <= 0.0f || aspectRatioY <= 0.0f)
        {
            style.width = parentW;
            style.height = parentH;

            return;
        }

        var ratio = Mathf.Min(parentW / aspectRatioX, parentH / aspectRatioY);
        var targetW = Mathf.Floor(aspectRatioX * ratio);
        var targetH = Mathf.Floor(aspectRatioY * ratio);
        style.width = targetW * _scale;
        style.height = targetH * _scale;
    }

    private void OnAttachToPanelEvent(AttachToPanelEvent e)
    {
        parent?.RegisterCallback <GeometryChangedEvent>(OnGeometryChangedEvent);
        FitToParent();
    }

    private void OnGeometryChangedEvent(GeometryChangedEvent e)
    {
        FitToParent();
    }

    #endregion
}

}
#endif
