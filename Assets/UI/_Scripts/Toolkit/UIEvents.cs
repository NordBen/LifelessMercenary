using System;

namespace LM
{
    public static class UIEvents
    {
        public static Action MainMenuShown;
        public static Action OptionsBarShopScreenShowm;
        public static Action SettingsScreenShown;
        public static Action InventoryScreenShown;
        public static Action SettingsScreenHidden;
        public static Action InventoryScreenHidden;
        public static Action GameScreenShown;
        public static Action<MenuScreen> CurrentScreenChanged;
        public static Action<string> CurrentViewChanged;
        public static Action<string> TabbedUIReset;
    }
    
    public static class PlayerEvents
    {
        public static Action<int> LevelUp;
        public static Action<float> ExperienceChanged;
        public static Action<int> InventoryOpened;
        public static Action<bool> LevelUpButtonEnabled;
        public static Action ScreenStarted;
        public static Action ScreenEnded;
    }
    
    public static class GameplayEvents
    {
        //public static Action<GameData> SettingsUpdated;
        public static Action SettingsLoaded;
        public static Action<float> GamePaused;
        public static Action GameResumed;
        public static Action GameQuit;
        public static Action GameRestarted;
        public static Action<float> MusicVolumeChanged;
        public static Action<float> SfxVolumeChanged;
    }
    
    public static class SettingsEvents
    {
        public static Action SettingsShown;
        //public static Action<GameData> GameDataLoaded;
        //public static Action<GameData> UIGameDataUpdated;
        //public static Action<GameData> SettingsUpdated;
        public static Action<bool> FpsCounterToggled;
        public static Action<int> TargetFrameRateSet;
    }
}