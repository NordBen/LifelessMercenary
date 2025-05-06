using UnityEngine;

public class Trap : InteractableActor
{
    [SerializeField] private Mesh trapMesh;
    [SerializeField] private bool useCutscene = true;
    [SerializeField] private bool useRagdoll = false;
    [SerializeField] private bool win = false;

    void Start()
    {
        this.SetMesh(trapMesh);
    }

    protected override void HandleInteract()
    {
        Debug.LogWarning($"interact was called from this object: {this.gameObject}");
        if (win)
        {
            GameManager.instance.SurviveDay(1);
            GameObject playerObj = GameManager.instance.player.gameObject;
            playerObj.SetActive(false);
            playerObj.transform.position = GameObject.Find("DayTwo").transform.position;
            playerObj.SetActive(true);
            GameManager.instance.player.Heal();
            return;
        }

        if (useCutscene)
            GameManager.instance.KillPlayer();
        else if (useRagdoll && !useCutscene)
            GameManager.instance.KillPlayerRagdoll();
    }
}