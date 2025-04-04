using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameManager instance;

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
