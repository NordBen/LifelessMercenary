using System.Collections.Generic;
using UnityEngine;

public class SaveSlotsMenu : MonoBehaviour
{
    private SaveSlot[] saveSlots;

    private void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }

    private void Start()
    {
        ActivateMenu();
    }

    public void ActivateMenu()
    {
        // Load all of the profiles that exist
        Dictionary<string, SaveGameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        // Loop through all the save slots in the UI and set the content aproptualtly
        foreach (SaveSlot saveSlot in saveSlots)
        {
            SaveGameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);
        }
    
    }
}
