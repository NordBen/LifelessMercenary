using System;
using UnityEngine.UIElements;

namespace LM
{
    public class UIView : IDisposable
    {
        protected bool m_HideOnAwake = true;

        protected bool m_IsOverlay;

        protected VisualElement m_TopElement;
        
        public VisualElement Root => m_TopElement;
        public bool IsTransparent => m_IsOverlay;
        public bool IsHidden => m_TopElement.style.display == DisplayStyle.None;

        public UIView(VisualElement topElement)
        {
            m_TopElement = topElement ?? throw new ArgumentNullException(nameof(topElement));
            Initialize();
        }

        public virtual void Initialize()
        {
            if (m_HideOnAwake)
            {
                Hide();
            }

            SetVisualElements();
            RegisterButtonCallbacks();
        }

        protected virtual void SetVisualElements() { }
        protected virtual void RegisterButtonCallbacks() { }
        
        public virtual void Show()
        {
            m_TopElement.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            m_TopElement.style.display = DisplayStyle.None;
        }
        
        public virtual void Dispose() { }
    }
}