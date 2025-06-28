using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Pickup : MonoBehaviour
{
    bool isHolding = false;

    [SerializeField]
    float throwForce = 600f;
    [SerializeField]
    float maxDistance = 3f;
    float distance;

    TempParent tempParent;
    Rigidbody rb;

    Vector3 objectPos;

    // Input Actions
    public InputActionAsset inputActions;
    private InputAction pickUpAction;
    private InputAction throwAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tempParent = TempParent.Instance;

        // Get actions from asset
        var actionMap = inputActions.FindActionMap("Player");
        pickUpAction = actionMap.FindAction("PickUp");
        throwAction = actionMap.FindAction("Throw");

        pickUpAction.Enable();
        throwAction.Enable();
    }

    void Update()
    {
        // Distance check to tempParent
        if (isHolding)
        {
            Hold();
        }
        else
        {
            distance = Vector3.Distance(transform.position, tempParent.transform.position);
            if (distance <= maxDistance && pickUpAction.triggered)
            {
                PickupObject();
            }
        }
    }

    private void PickupObject()
    {
        isHolding = true;
        rb.useGravity = false;
        rb.detectCollisions = true;
        transform.SetParent(tempParent.transform);
    }

    private void Hold()
    {
        distance = Vector3.Distance(transform.position, tempParent.transform.position);

        if (distance >= maxDistance)
        {
            Drop();
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (throwAction.triggered)
        {
            rb.AddForce(tempParent.transform.forward * throwForce);
            Drop();
        }
    }

    private void Drop()
    {
        if (isHolding)
        {
            isHolding = false;
            transform.SetParent(null);
            rb.useGravity = true;
        }
    }

    private void OnDestroy()
    {
        pickUpAction.Disable();
        throwAction.Disable();
    }
}