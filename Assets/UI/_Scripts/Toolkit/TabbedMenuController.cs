using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace LM
{
    public class TabbedMenuController : MonoBehaviour
    {
        public static event Action TabSelected;
        private readonly VisualElement m_Root;
        private readonly TabbedMenuIDs m_IDs;
        
        public TabbedMenuController(VisualElement root, TabbedMenuIDs ids)
        {
            m_Root = root;
            m_IDs = ids;
        }
        
        public void RegisterTabCallbacks()
        {
            UQueryBuilder<VisualElement> tabs = GetAllTabs();
            
            tabs.ForEach((t) => { t.RegisterCallback<ClickEvent>(OnTabClick); });
        }
        
        private void OnTabClick(ClickEvent evt)
        {
            VisualElement clickedTab = evt.currentTarget as VisualElement;
            
            if (!IsTabSelected(clickedTab))
            {
                GetAllTabs().Where(
                    (tab) => tab != clickedTab && IsTabSelected(tab)
                    ).ForEach(UnselectTab);
                
                SelectTab(clickedTab);
                AudioManager.PlayDefaultButtonSound();
            }
        }
        
        private VisualElement FindContent(VisualElement tab)
        {
            return m_Root.Q(GetContentName(tab));
        }
        
        private string GetContentName(VisualElement tab)
        {
            return tab.name.Replace(m_IDs.tabNameSuffix, m_IDs.contentNameSuffix);
        }
        
        private UQueryBuilder<VisualElement> GetAllTabs()
        {
            return m_Root.Query<VisualElement>(className: m_IDs.tabClassName);
        }
        
        public VisualElement GetFirstTab(VisualElement visualElement)
        {
            return visualElement.Query<VisualElement>(className: m_IDs.tabClassName).First();
        }

        public bool IsTabSelected(string tabName)
        {
            VisualElement tabElement = m_Root.Query<VisualElement>(className: m_IDs.tabClassName, name: tabName);
            return IsTabSelected(tabElement);
        }

        public bool IsTabSelected(VisualElement tab)
        {
            return tab.ClassListContains(m_IDs.selectedTabClassName);
        }

        private void UnselectOtherTabs(VisualElement tab)
        {
            GetAllTabs().Where(
                (t) => t != tab && IsTabSelected(t)).
                ForEach(UnselectTab);
        }
        
        public void SelectTab(string tabName)
        {
            if (string.IsNullOrEmpty(tabName))
            {
                return;
            }
            VisualElement namedTab = m_Root.Query<VisualElement>(tabName, className: m_IDs.tabClassName);

            if (namedTab == null)
            {
                Debug.Log("TabbedMenuController.SelectTab: invalid tab specified");
                return;
            }
            UnselectOtherTabs(namedTab);
            SelectTab(namedTab);
        }
        
        private void SelectTab(VisualElement tab)
        {
            tab.AddToClassList(m_IDs.selectedTabClassName);
            VisualElement content = FindContent(tab);
            content.RemoveFromClassList(m_IDs.unselectedContentClassName);
            TabSelected?.Invoke();
        }
        
        public void SelectFirstTab(VisualElement visualElement)
        {
            VisualElement firstTab = GetFirstTab(visualElement);

            if (firstTab != null)
            {
                GetAllTabs().Where((tab) => tab != firstTab && IsTabSelected(tab)).ForEach(UnselectTab);
                SelectTab(firstTab);
            }
        }
        
        private void UnselectTab(VisualElement tab)
        {
            tab.RemoveFromClassList(m_IDs.selectedTabClassName);
            VisualElement content = FindContent(tab);
            content.AddToClassList(m_IDs.unselectedContentClassName);
        }
    }
}