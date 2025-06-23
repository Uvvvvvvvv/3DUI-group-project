



using UnityEngine;
using UnityEngine.Events;

public class NpcInteraction : MonoBehaviour
{
    public string npcName = "Merchant";
    public int waypointIndex = 0;           // ★ new

    [TextArea(3, 6)]
    public string greeting =
       "«Hello traveller, take a look at my wares. "
     + "By the way, the village elder is looking for someone like you.»";


    public float interactionRange = 3f;

    [System.Serializable]
    public class TalkEvent : UnityEvent<string, int> { }   // ★ two parameters

    public TalkEvent OnTalk = new TalkEvent();

    private Transform player;

    private void Start()
    {
        DialogueManager.Show("Welcome hero to our game, right now not vr, but will be. The merchant is your first point of contact. press e to continue", waitForKey: true, forceShowSeconds: 3f);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found. Make sure it has the tag 'Player'.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Show(greeting, waitForKey: true, forceShowSeconds: 3f);

            OnTalk.Invoke(npcName, waypointIndex);       // ★ pass both
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Just for debugging: shows the interaction radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
