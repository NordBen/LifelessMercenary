using System.Collections.Generic;
using JetBrains.Annotations;
using LM.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace LM
{
    [RequireComponent(typeof(UIDocument))]
    public class UiManager : MonoBehaviour
    {
        private UIDocument m_MainMenuDocument;

        private UIView m_currentView;
        private UIView m_previousView;
        
        List<UIView> m_AllViews = new List<UIView>();

        // Modal views
        private UIView m_HomeView;
        
        // Overlay views
        private UIView m_InventoryView;
        private UIView m_SettingsView;
        
        // Toolbars
        private UIView m_OptionsBarView;
        private UIView m_MenuBarView;

        [CanBeNull] private const string k_HomeViewName = "HomeScreen";
        [CanBeNull] private const string k_InventoryViewName = "InventoryScreen";
        [CanBeNull] private const string k_SettingsViewName = "SettingsScreen";
        [CanBeNull] private const string k_MenuBarViewName = "MenuBar";
        [CanBeNull] private const string k_OptionsBarViewName = "OptionsBar";

        private void Start()
        {
            Time.timeScale = 1f;
        }
        
        private void OnEnable()
        {
            m_MainMenuDocument = GetComponent<UIDocument>();
            SetupViews();
            SubsribeToEvents();
            ShowModalView(m_HomeView);
        }

        private void OnDisable()
        {
            UnsubsribeToEvents();

            foreach (var view in m_AllViews)
            {
                view.Dispose();
            }
        }
        
        private void SubsribeToEvents()
        {
            UIEvents.MainMenuShown += OnHomeScreenShown;
            
            UIEvents.InventoryScreenShown += OnInventoryScreenShown;
            UIEvents.InventoryScreenHidden += OnInventoryScreenHidden;
            UIEvents.SettingsScreenShown += OnSettingsScreenShown;
            UIEvents.SettingsScreenHidden += OnSettingsScreenHidden;
        }

        private void UnsubsribeToEvents()
        {
            UIEvents.MainMenuShown -= OnHomeScreenShown;
            
            UIEvents.InventoryScreenShown -= OnInventoryScreenShown;
            UIEvents.InventoryScreenHidden -= OnInventoryScreenHidden;
            UIEvents.SettingsScreenShown -= OnSettingsScreenShown;
            UIEvents.SettingsScreenHidden -= OnSettingsScreenHidden;
        }
        
        private void SetupViews()
        {
            VisualElement root = m_MainMenuDocument.rootVisualElement;

            // full screen modals
            m_HomeView = new HomeView(root.Q<VisualElement>(k_HomeViewName));
            
            // overlay views
            m_InventoryView = new InventoryView(root.Q<VisualElement>(k_InventoryViewName));
            m_SettingsView = new SettingsView(root.Q<VisualElement>(k_SettingsViewName));
            
            // toolbars
            m_OptionsBarView = new OptionsBarView(root.Q<VisualElement>(k_OptionsBarViewName));
            m_MenuBarView = new MenuBarView(root.Q<VisualElement>(k_MenuBarViewName));
            
            m_AllViews.Add(m_HomeView);
            m_AllViews.Add(m_InventoryView);
            m_AllViews.Add(m_SettingsView);
            m_AllViews.Add(m_OptionsBarView);
            m_AllViews.Add(m_MenuBarView);

            m_HomeView.Show();
            m_OptionsBarView.Show();
            m_MenuBarView.Show();
        }
        
        private void ShowModalView(UIView view)
        {
            if (m_currentView != null)
            {
                m_currentView.Hide();
            }
            
            m_previousView = m_currentView;
            m_currentView = view;
            
            if (m_currentView != null)
            {
                m_currentView.Show();
                UIEvents.CurrentViewChanged?.Invoke(m_currentView.GetType().Name);
            }
        }

        private void OnHomeScreenShown()
        {
            ShowModalView(m_HomeView);
        }
        
        private void OnSettingsScreenShown()
        {
            m_previousView = m_currentView;
            m_SettingsView.Show();
        }
        
        private void OnInventoryScreenShown()
        {
            m_previousView = m_currentView;
            m_InventoryView.Show();
        }
        
        private void OnSettingsScreenHidden()
        {
            m_SettingsView.Hide();

            if (m_previousView != null)
            {
                m_previousView.Show();
                m_currentView = m_previousView;
                UIEvents.CurrentViewChanged?.Invoke(m_currentView.GetType().Name);
            }
        }
        
        private void OnInventoryScreenHidden()
        {
            m_InventoryView.Hide();

            if (m_previousView != null)
            {
                m_previousView.Show();
                m_currentView = m_previousView;
                UIEvents.CurrentViewChanged?.Invoke(m_currentView.GetType().Name);
            }
        }
    }
}