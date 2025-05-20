using UnityEngine;
using UnityEngine.UIElements;

namespace LM
{
    [System.Serializable]
    public struct TabbedMenuIDs
    {
        public string tabClassName;// = "tab";
        public string selectedTabClassName; //= "selected-tab";
        public string unselectedContentClassName; // = "unselected-content";
        public string tabNameSuffix;// = "-tab";
        public string contentNameSuffix;// = "-content";
    }
    
    public class TabbedMenu : MonoBehaviour
    {
        [Tooltip("Defaults to current component unless specified")]
        [SerializeField] UIDocument m_Document;
        [Tooltip("VisualElement for TabbedMenu, defaults to document rootVisualElement if unspecified")]
        [SerializeField] string m_MenuElementName;

        private TabbedMenuController m_Controller;
        private VisualElement m_MenuElement;
        
        public TabbedMenuIDs m_TabbedMenuStrings;
        
        void OnEnable()
        {
            VisualElement root = m_Document.rootVisualElement;
            m_MenuElement = root.Q(m_MenuElementName);
            
            m_Controller = (string.IsNullOrEmpty(m_MenuElementName) || m_MenuElement == null) ?
                new TabbedMenuController(root, m_TabbedMenuStrings) : new TabbedMenuController(m_MenuElement, m_TabbedMenuStrings);
            
            m_Controller.RegisterTabCallbacks();

            UIEvents.TabbedUIReset += OnTabbedUIReset;
        }

        void OnDisable()
        {
            UIEvents.TabbedUIReset -= OnTabbedUIReset;
        }
        
        void OnValidate()
        {
            if (string.IsNullOrEmpty(m_TabbedMenuStrings.tabClassName))
            {
                m_TabbedMenuStrings.tabClassName = "tab";
            }

            if (string.IsNullOrEmpty(m_TabbedMenuStrings.selectedTabClassName))
            {
                m_TabbedMenuStrings.selectedTabClassName = "selected-tab";
            }

            if (string.IsNullOrEmpty(m_TabbedMenuStrings.unselectedContentClassName))
            {
                m_TabbedMenuStrings.unselectedContentClassName = "unselected-content";
            }

            if (string.IsNullOrEmpty(m_TabbedMenuStrings.tabNameSuffix))
            {
                m_TabbedMenuStrings.tabNameSuffix = "-tab";
            }

            if (string.IsNullOrEmpty(m_TabbedMenuStrings.contentNameSuffix))
            {
                m_TabbedMenuStrings.contentNameSuffix = "-content";
            }
        }

        public void SelectFirstTab()
        {
            SelectFirstTab(m_MenuElement);
        }
        
        public void SelectFirstTab(VisualElement elementToQuery)
        {
            m_Controller.SelectFirstTab(elementToQuery);
        }
        
        public void SelectTab(string tabName)
        {
            
            m_Controller.SelectTab(tabName);
        }

        public bool IsTabSelected(VisualElement visualElement)
        {
            if (m_Controller == null || visualElement == null)
            {
                return false;
            }

            return m_Controller.IsTabSelected(visualElement);
        }
        
        void OnTabbedUIReset(string newView)
        {
            if (newView == m_MenuElementName)
            {
                SelectFirstTab();
            }
        }
    }
}
