using UnityEngine;
using UnityEngine.UIElements;

namespace LM
{
    public class SettingsView : UIView
    {
        private const string k_ScreenActiveClass = "settings__screen";
        private const string k_ScreenInactiveClass = "settings__screen--inactive";
        
        private Button m_BackButton;
        private TextField m_NameField;
        private Slider m_VolumeSlider;
        private Slider m_MusicSlider;
        private Slider m_SFXSlider;
        private SlideToggle m_SlideToggle;
        private RadioButtonGroup m_FrameRateRadioButtonsGroup;
        private VisualElement m_ScreenContainer;
        
        //private GameData m_LocalUISettings = new GameData();
        
        public static readonly string[] LanguageKeys = { "English", "Spanish", "French", "Danish" };
        public static readonly string[] ThemeOptionKeys = { "Default", "Halloween", "Christmas" };
        
        public SettingsView(VisualElement topElement) : base(topElement)
        {
            //SettingsEvents.GameDataLoaded += OnGameDataLoaded;
            
            base.SetVisualElements();
            
            m_ScreenContainer.AddToClassList(k_ScreenInactiveClass);
            m_ScreenContainer.RemoveFromClassList(k_ScreenActiveClass);
        }

        public override void Dispose()
        {
            base.Dispose();

            //SettingsEvents.GameDataLoaded -= OnGameDataLoaded;
            UnregisterButtonCallbacks();
        }

        public override void Show()
        {
            base.Show();
            
            m_ScreenContainer.RemoveFromClassList(k_ScreenInactiveClass);
            m_ScreenContainer.AddToClassList(k_ScreenActiveClass);

            SettingsEvents.SettingsShown?.Invoke();
        }

        protected override void SetVisualElements()
        {
            base.SetVisualElements();
            m_BackButton = m_TopElement.Q<Button>("settings__panel-back-button");
            m_NameField = m_TopElement.Q<TextField>("settings__name-field");
            m_VolumeSlider = m_TopElement.Q<Slider>("settings__slider1");
            m_MusicSlider = m_TopElement.Q<Slider>("settings__slider2");
            m_SFXSlider = m_TopElement.Q<Slider>("settings__slider3");
            m_SlideToggle = m_TopElement.Q<SlideToggle>("settings__slide-toggle");
            
            m_ScreenContainer = m_TopElement.Q<VisualElement>("settings__screen");
        }

        protected override void RegisterButtonCallbacks()
        {
            m_BackButton.RegisterCallback<ClickEvent>(CloseWindow);
            
            m_NameField.RegisterCallback<KeyDownEvent>(SetPlayerTextField);
            
            m_VolumeSlider.RegisterValueChangedCallback(ChangeVolume);
           // m_VolumeSlider.RegisterCallback<PointerCaptureOutEvent>(evt => SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings));
            m_VolumeSlider.RegisterCallback<PointerDownEvent>(evt => AudioManager.PlayDefaultButtonSound());
            
            m_MusicSlider.RegisterValueChangedCallback(ChangeMusicVolume);
            //m_MusicSlider.RegisterCallback<PointerCaptureOutEvent>(evt => SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings));
            m_MusicSlider.RegisterCallback<PointerDownEvent>(evt => AudioManager.PlayDefaultButtonSound());
            
            m_SFXSlider.RegisterValueChangedCallback(ChangeSFXVolume);
            //m_SFXSlider.RegisterCallback<PointerCaptureOutEvent>(evt => SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings));
            m_SFXSlider.RegisterCallback<PointerDownEvent>(evt => AudioManager.PlayDefaultButtonSound());

            m_SlideToggle.RegisterValueChangedCallback(ChangeSlideToggle);
            m_SlideToggle.RegisterCallback<ClickEvent>(evt => AudioManager.PlayDefaultButtonSound());

            m_FrameRateRadioButtonsGroup.RegisterCallback<ChangeEvent<int>>(ChangeRadioButton);
        }

        private void UnregisterButtonCallbacks()
        {
            m_BackButton?.UnregisterCallback<ClickEvent>(CloseWindow);
            m_NameField?.UnregisterCallback<KeyDownEvent>(SetPlayerTextField);
            m_MusicSlider?.UnregisterValueChangedCallback(ChangeVolume);
            m_MusicSlider?.UnregisterValueChangedCallback(ChangeMusicVolume);
            m_SFXSlider?.UnregisterValueChangedCallback(ChangeSFXVolume);
            m_SlideToggle?.UnregisterValueChangedCallback(ChangeSlideToggle);
            m_FrameRateRadioButtonsGroup?.UnregisterCallback<ChangeEvent<int>>(ChangeRadioButton);
        }
        /*
        private void OnGameDataLoaded(GameData gameData)
        {
            if (gameData == null) return;

            m_LocalUISettings = gameData;
            //m_NameField.value = gameData.Username;
            //m_VolumeSlider.value = gameData.Volume;
            //m_MusicSlider.value = gameData.MusicVolume;
            //m_SFXSlider.value = gameData.SfxVolume;
            //m_SlideToggle.value = gameData.IsFPSCounterEnabled;
            
            SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
        }*/
        
        private void CloseWindow(ClickEvent evt)
        {
            m_ScreenContainer.RemoveFromClassList(k_ScreenActiveClass);
            m_ScreenContainer.AddToClassList(k_ScreenInactiveClass);
            AudioManager.PlayDefaultButtonSound();
            //SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
            Hide();
        }

        private void SetPlayerTextField(KeyDownEvent evt)
        {/*
            if (evt.keyCode == KeyCode.Return && m_LocalUISettings != null)
            {
               //m_LocalUISettings.Username = m_NameField.text;
                //SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
            }*/
        }

        private void ChangeSlideToggle(ChangeEvent<bool> evt)
        {
            //m_LocalUISettings.IsFPSCounterEnabled = evt.newValue;
            //SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
        }

        private void ChangeToggle(ChangeEvent<bool> evt)
        {
            //m_LocalUISettings.IsToggled = evt.newValue;
            //SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
        }

        private void ChangeVolume(ChangeEvent<float> evt)
        {
            evt.StopPropagation();
            //m_LocalUISettings.Volume = evt.newValue;
        }

        private void ChangeSFXVolume(ChangeEvent<float> evt)
        {
            evt.StopPropagation();
            //m_LocalUISettings.SfxVolume = evt.newValue;
        }

        private void ChangeMusicVolume(ChangeEvent<float> evt)
        {
            evt.StopPropagation();
            //m_LocalUISettings.MusicVolume = evt.newValue;
        }
        
        void ChangeRadioButton(ChangeEvent<int> evt)
        {
            AudioManager.PlayDefaultButtonSound();
            //m_LocalUISettings.TargetFrameRateSelection = evt.newValue;
            //SettingsEvents.UIGameDataUpdated?.Invoke(m_LocalUISettings);
        }
    }
}