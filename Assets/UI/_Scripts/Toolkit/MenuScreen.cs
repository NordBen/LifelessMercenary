using UnityEngine;
using UnityEngine.UIElements;

namespace LM
{
    public abstract class MenuScreen : MonoBehaviour
    {
        [Tooltip("String ID from the UXML for this menu panel/screen.")]
        [SerializeField] protected string m_ScreenName;

        [Header("UI Management")]
        //[Tooltip("Set the Main Menu here explicitly (or get automatically from current GameObject).")]
        //[SerializeField] protected MainMenuUIManager m_MainMenuUIManager;
        [Tooltip("Set the UI Document here explicitly (or get automatically from current GameObject).")]
        [SerializeField] protected UIDocument m_Document;
        
        protected VisualElement m_Screen;
        protected VisualElement m_Root;
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(m_ScreenName)) m_ScreenName = this.GetType().Name;
        }

        protected virtual void Awake()
        {
            if (m_Document == null) m_Document = GetComponent<UIDocument>();

            if (m_Document == null)
            {
                Debug.LogWarning("MenuScreen " + m_ScreenName + ": missing UIDocument. Check Script Execution Order.");
                return;
            }
            else
            {
                SetVisualElements();
                RegisterButtonCallbacks();
            }
        }
        
        protected virtual void SetVisualElements()
        {
            if (m_Document != null) m_Root = m_Document.rootVisualElement;
            m_Screen = GetVisualElement(m_ScreenName);
        }
        
        protected virtual void RegisterButtonCallbacks() { }
        
        public bool IsVisible()
        {
            if (m_Screen == null) return false;
            return (m_Screen.style.display == DisplayStyle.Flex);
        }
        
        public static void ShowVisualElement(VisualElement visualElement, bool state)
        {
            if (visualElement == null) return;
            visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public VisualElement GetVisualElement(string elementName)
        {
            if (string.IsNullOrEmpty(elementName) || m_Root == null) return null;
            return m_Root.Q(elementName);
        }

        public virtual void ShowScreen()
        {
            ShowVisualElement(m_Screen, true);
        }

        public virtual void HideScreen()
        {
            if (IsVisible())
            {
                ShowVisualElement(m_Screen, false);
            }
        }
    }
}