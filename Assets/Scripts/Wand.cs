using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class Wand : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public XRBaseInteractor interactor;
    public InputActionReference input;
    public XRInteractionManager interactManager;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
            float val = input.action.ReadValue<float>();
            if (val > 0.5)
            {
            Shoot();
            }
       
    }

    void Shoot()
    {
        if ((interactor.interactablesSelected).Count != 0)
        {
           
            if (interactor.interactablesSelected[0] is XRBaseInteractable obj)
            {
                //interactor.interactionManager.SelectExit(interactor, interactor.interactablesSelected[0]);
                Rigidbody r = obj.GetComponent<Rigidbody>();
                obj.interactionManager.CancelInteractableSelection(interactor.interactablesSelected[0]);
                if (r != null)
                {    
                    r.isKinematic = false;
                    Debug.Log("Shooting");
                    r.AddForce(transform.up * 100f, ForceMode.Impulse);
                    r.constraints = RigidbodyConstraints.None;
                    Debug.Log(r.linearVelocity);
                }
            }
        }
    }
}
