using UnityEngine;

public class TempParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static TempParent Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}