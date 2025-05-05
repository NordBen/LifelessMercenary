using UnityEngine;
using UnityEngine.Playables;

public class TempCutscenePosition : MonoBehaviour
{
    [SerializeField] private PlayableDirector timeLine;
    public Transform dynamicTarget;

    public void PlayDirector()
    {
        if (dynamicTarget != null)
        {
            timeLine.transform.position = dynamicTarget.position;
            //timeLine.SetGenericBinding(dynamicTarget, dynamicTarget);

            if (timeLine != null)
            {
                timeLine.Play();
            }
        }
    }
}
