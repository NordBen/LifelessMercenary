using UnityEngine;

public class NPCUIAttributeBar : UIAttributeValueProgressBar
{
    [SerializeField] protected GameObject _followTarget;
    [SerializeField] private bool followTarget;
    [SerializeField] private Vector3 targetOffset;
    
    protected void Update()
    {
        if (this.followTarget)
        {
            transform.rotation = Camera.main.transform.rotation;
            transform.position = _followTarget.transform.position + targetOffset;
        }
    }
}