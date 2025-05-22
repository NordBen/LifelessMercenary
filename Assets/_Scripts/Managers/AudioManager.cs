using UnityEngine;
using UnityEngine.Audio;

namespace LM
{
    public class AudioManager : MonoBehaviour, IDataPersistance
    {
        public static string MusicGroup = "Music";
        public static string SfxGroup = "SFX";
        
        const string k_Parameter = "Volume";
        private static float s_LastSFXPlayTime = -1f;
        private static float sfxCooldown = 0.1f; // Global cooldown for playing sound effects

        [SerializeField] AudioMixer m_MainAudioMixer;
        
        [Header("UI Sounds")]
        [Tooltip("General button click.")]
        [SerializeField] AudioClip m_DefaultButtonSound;
        [Tooltip("General button click.")]
        [SerializeField] AudioClip m_AltButtonSound;
        [Tooltip("General shop purchase clip.")]
        [SerializeField] AudioClip m_TransactionSound;
        [Tooltip("General error sound.")]
        [SerializeField] AudioClip m_DefaultWarningSound;

        [Header("Game Sounds")]
        [Tooltip("Level up or level win sound.")]
        [SerializeField] AudioClip m_VictorySound;
        [Tooltip("Level defeat sound.")]
        [SerializeField] AudioClip m_DefeatSound;
        [SerializeField] AudioClip m_PotionSound;

        private void OnEnable()
        {
            //SettingsEvents.SettingsUpdated += OnSettingsUpdated;
            //GameplayEvents.SettingsUpdated += OnSettingsUpdated;
        }

        private void OnDisable()
        {
            //SettingsEvents.SettingsUpdated -= OnSettingsUpdated;
            //GameplayEvents.SettingsUpdated -= OnSettingsUpdated;
        }
        
        public static void PlayOneSFX(AudioClip clip, Vector3 sfxPosition)
        {
            if (clip == null) return;
            
            if (Time.time - s_LastSFXPlayTime < sfxCooldown)
            {
                return; // Don't play the clip if within cooldown
            }
            s_LastSFXPlayTime = Time.time;
            
            GameObject sfxInstance = new GameObject(clip.name);
            sfxInstance.transform.position = sfxPosition;

            AudioSource source = sfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.Play();
            source.outputAudioMixerGroup = GetAudioMixerGroup(SfxGroup);
            
            Destroy(sfxInstance, clip.length);
        }
        
        public static AudioMixerGroup GetAudioMixerGroup(string groupName)
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();

            if (audioManager == null) return null;
            if (audioManager.m_MainAudioMixer == null) return null;

            AudioMixerGroup[] groups = audioManager.m_MainAudioMixer.FindMatchingGroups(groupName);

            foreach (AudioMixerGroup match in groups)
            {
                if (match.ToString() == groupName) return match;
            }
            return null;

        }
        
        public static float GetDecibelValue(float linearValue)
        {
            float conversionFactor = 20f;

            float decibelValue = (linearValue != 0) ? conversionFactor * Mathf.Log10(linearValue) : -144f;
            return decibelValue;
        }
        
        public static float GetLinearValue(float decibelValue)
        {
            float conversionFactor = 20f;
            return Mathf.Pow(10f, decibelValue / conversionFactor);
        }
        
        public static void SetVolume(string groupName, float linearValue)
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;

            float decibelValue = GetDecibelValue(linearValue);

            if (audioManager.m_MainAudioMixer != null)
            {
                audioManager.m_MainAudioMixer.SetFloat(groupName, decibelValue);
            }
        }
        
        public static float GetVolume(string groupName)
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return 0f;

            float decibelValue = 0f;
            if (audioManager.m_MainAudioMixer != null)
            {
                audioManager.m_MainAudioMixer.GetFloat(groupName, out decibelValue);
            }
            return GetLinearValue(decibelValue);
        }
        
        public static void PlayDefaultButtonSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_DefaultButtonSound, Vector3.zero);
        }

        public static void PlayAltButtonSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_AltButtonSound, Vector3.zero);
        }

        public static void PlayDefaultTransactionSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_TransactionSound, Vector3.zero);
        }

        public static void PlayDefaultWarningSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_DefaultWarningSound, Vector3.zero);
        }
        public static void PlayVictorySound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_VictorySound, Vector3.zero);
        }

        public static void PlayDefeatSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_DefeatSound, Vector3.zero);
        }

        public static void PlayPotionDropSound()
        {
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager == null) return;
            PlayOneSFX(audioManager.m_PotionSound, Vector3.zero);
        }
        /*
        private void OnSettingsUpdated(GameData gameData)
        {
            //SetVolume(MusicGroup + k_Parameter, gameData.MusicVolume / 100f);
           // SetVolume(SfxGroup + k_Parameter, gameData.SfxVolume / 100f);
        }*/
        public void SaveData(SaveGameData data)
        {
            
        }

        public void LoadData(SaveGameData data)
        {
            
        }
    }
}