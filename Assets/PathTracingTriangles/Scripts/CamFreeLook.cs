using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamFreeLook : MonoBehaviour
{
    [SerializeField]
    float lookSensitivity;
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    Vector2 pitchLim;

    Vector3 velocity;
    float yaw, pitch;
    Vector2 mousePos;
    Vector2 mouseDelta;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 400;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        // Look
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouseDelta *= lookSensitivity;
            pitch += mouseDelta.y;
            yaw += mouseDelta.x;
            mousePos = Input.mousePosition;
            pitch = Mathf.Clamp(pitch, pitchLim.x, pitchLim.y);
            transform.rotation = Quaternion.Euler(-pitch, yaw, 0);

            // Move
            velocity = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
            velocity *= moveSpeed;
            //transform.Translate(velocity * Time.deltaTime);
            transform.position += transform.forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical") +
                                   transform.right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        }
    }
}
