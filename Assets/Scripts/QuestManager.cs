using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private BreadcrumbsPath breadcrumbs;
    private readonly HashSet<string> talkedTo = new();
    private int currentIndex = 0;
    public GameObject player;
    private GameObject wand;
    private GameObject wand_visual;
    private GameObject sword;

    public AudioSource wizard, warrior, captain;

    private void Start()
    {
        wand = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Left Controller/Near-Far Interactor");
        wand_visual = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Left Controller/Left Controller Visual/wand03_green");
        sword = GameObject.Find("XR Origin (XR Rig)/Camera Offset/Right Controller/RightControllerAttach/Sword1_FBX");
        Debug.Log(wand);
        Debug.Log(wand_visual);
        Debug.Log(sword);

        wand.SetActive(false);
        sword.SetActive(false); 
        wand_visual.SetActive(false);
    }
    public void RegisterNpc(string npcName, int waypointIndex)
    {
        if (talkedTo.Contains(npcName)) return;
        talkedTo.Add(npcName);

        Debug.Log($"Talked to: {npcName}");

        if (npcName == "Magician")
        {
            wand.SetActive(true);
            wand_visual.SetActive(true);
            if (!wizard.isPlaying)
            {
                wizard.Play();
            }
        }

        else if (npcName == "Warrior")
        {
           sword.SetActive(true);
            if (!warrior.isPlaying)
            {
                warrior.Play();
            }
        }

        else if (npcName == "Captain")
        {
            if (!captain.isPlaying)
            {
                captain.Play();
            }
        }



        if (waypointIndex == currentIndex)          // correct order
        {
            currentIndex++;                         // next target
            breadcrumbs.SetTargetByIndex(currentIndex);
        }
    }
}
