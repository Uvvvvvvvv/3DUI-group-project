using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] TMP_Text textField;

    static DialogueManager _instance;
    static bool waitingForInput = false;

    void Awake() => _instance = this;

    void Update()
    {
        if (waitingForInput && Time.timeSinceLevelLoad >= minDisplayTime && Input.GetKeyDown(KeyCode.E))
        {
            HideNow();
        }
    }

    static float minDisplayTime = 0f;

    public static void Show(string msg, bool waitForKey = true, float forceShowSeconds = 3f)
    {
        if (_instance == null) return;

        _instance.StopAllCoroutines();
        _instance.canvas.gameObject.SetActive(true);
        _instance.textField.text = msg;

        if (waitForKey)
        {
            waitingForInput = true;
            minDisplayTime = Time.timeSinceLevelLoad + forceShowSeconds;
        }
        else
        {
            _instance.StartCoroutine(_instance.HideAfter(forceShowSeconds));
        }
    }

    void HideNow()
    {
        waitingForInput = false;
        canvas.gameObject.SetActive(false);
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        canvas.gameObject.SetActive(false);
    }
}
