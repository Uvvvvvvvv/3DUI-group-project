using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private BreadcrumbsPath breadcrumbs;
    private readonly HashSet<string> talkedTo = new();
    private int currentIndex = 0;

    public void RegisterNpc(string npcName, int waypointIndex)
    {
        if (talkedTo.Contains(npcName)) return;
        talkedTo.Add(npcName);

        Debug.Log($"Talked to: {npcName}");

        if (waypointIndex == currentIndex)          // correct order
        {
            currentIndex++;                         // next target
            breadcrumbs.SetTargetByIndex(currentIndex);
        }
    }
}
