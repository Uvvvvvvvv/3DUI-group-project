using System;
using UnityEngine;

public class Control : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float speed = 10f;
    private float mouseSensitivity = 150f;
    public Camera CharacterCam;
    private Rigidbody rb;
    private bool grounded = false;
    private float jumpstrength = 350f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check ground touching for jumps
        grounded = Physics.Raycast(transform.position, UnityEngine.Vector3.down,
        (float)(transform.localScale.y + 0.05f));

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(jumpstrength * Vector3.up, ForceMode.Impulse);
        }

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

            transform.Rotate(UnityEngine.Vector3.up, mouseX);

            float cam_pitch = CharacterCam.transform.rotation.eulerAngles.x;
            if (cam_pitch > 180) { cam_pitch -= 360; }
            if (cam_pitch < 45f && cam_pitch > -45f)
            {
                CharacterCam.transform.Rotate(UnityEngine.Vector3.left, mouseY);
            }
            else if (cam_pitch * mouseY > 0)
            {
                CharacterCam.transform.Rotate(UnityEngine.Vector3.left, mouseY);
            }
        }


        float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        UnityEngine.Vector3 move = new UnityEngine.Vector3(horizontal, 0, vertical);
        UnityEngine.Vector3 move_world = transform.TransformVector(move);
        rb.MovePosition(rb.position + move_world);
        float heading = transform.rotation.eulerAngles.y;
        //Debug.Log(heading);
    }
}
