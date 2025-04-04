using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
