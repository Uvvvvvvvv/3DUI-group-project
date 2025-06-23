using UnityEngine;
using UnityEngine.InputSystem;

public class XRDialogueInput : MonoBehaviour
{
    public InputActionReference confirmDialogue;

    public static bool ConfirmPressed { get; private set; }

    void Update()
    {
        if (confirmDialogue != null)
        {
            ConfirmPressed = confirmDialogue.action.WasPressedThisFrame();
        }
    }
}
