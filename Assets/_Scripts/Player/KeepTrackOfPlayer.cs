using UnityEngine;

public class KeepTrackOfPlayer : MonoBehaviour
{
    #region Singelton

    public static KeepTrackOfPlayer instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    public GameObject player;
}
