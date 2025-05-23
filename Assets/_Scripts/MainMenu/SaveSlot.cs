using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileID = " ";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;
    [SerializeField] private TextMeshProUGUI percentageCompleteText;
    [SerializeField] private TextMeshProUGUI deathCountText;

    public void SetData(SaveGameData data){
        // There is no data for this profile ID
        if (data == null){
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
        }
        // There is data for this profile ID
        else{
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);

            percentageCompleteText.text = data.GetPercentageComplete() + "% Complete";
            deathCountText.text = "Death count: " + data.deathCount;
        }
    }

    public string GetProfileId(){
        return this.profileID;
    }
}
