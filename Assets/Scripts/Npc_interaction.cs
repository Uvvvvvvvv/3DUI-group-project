



using UnityEngine;
using UnityEngine.Events;

public class NpcInteraction : MonoBehaviour
{
    public string npcName = "Elder";
    public int waypointIndex = 0;           // ★ new

    [Header("Music")]
    public int musicIndex = -1;            // ★ -1 = do not change music
    public bool loopMusic = true;          // ★ should this track loop?


    [TextArea(3, 6)]
    public string greeting =
       "default greetigs hould not be seen ";


    public float interactionRange = 3f;

    [System.Serializable]
    public class TalkEvent : UnityEvent<string, int> { }   // ★ two parameters

    public TalkEvent OnTalk = new TalkEvent();

    private Transform player;

    private void Start()
    {

        DialogueManager.Show("Welcome hero to Chronicles of Drakenvale\n\n" +
    "Our valley lives in fear of an ancient dragon that broods in the western woods\n\n" +
    "• Move with the left joystick\n" +
    "• Look around with the right joystick\n" +
    "• Press X to talk to characters\n\n" +
    "Seek the village elder at the centre of the town", waitForKey: true, forceShowSeconds: 3f);
        
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
        if (distance <= interactionRange && XRDialogueInput.ConfirmPressed)
        {
            DialogueManager.Show(greeting, waitForKey: true, forceShowSeconds: 3f);

            OnTalk.Invoke(npcName, waypointIndex);       // ★ pass both
            if (musicIndex >= 0)
                MusicManager.Instance?.PlayTrack(musicIndex, loopMusic);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Just for debugging: shows the interaction radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
