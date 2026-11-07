using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraPivot;
    public float walkSpeed = 6f, runSpeed = 13f, mouseSensitivity = 0.12f, gravity = -20f, jumpHeight = 1.1f;
    public float interactDistance = 5f;

    CharacterController cc; float pitch, vy;

    void Awake() { cc = GetComponent<CharacterController>(); if (!cameraPivot) cameraPivot = GetComponentInChildren<Camera>().transform; }
    void OnEnable() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

    void Update()
    {
        var kb = Keyboard.current; var mouse = Mouse.current; if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 d = mouse.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, d.x, 0f);
            pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        Vector2 m = Vector2.zero;
        if (kb.wKey.isPressed) m.y += 1; if (kb.sKey.isPressed) m.y -= 1;
        if (kb.dKey.isPressed) m.x += 1; if (kb.aKey.isPressed) m.x -= 1;
        float speed = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * m.x + transform.forward * m.y).normalized * speed;

        if (cc.isGrounded) { vy = -2f; if (kb.spaceKey.wasPressedThisFrame) vy = Mathf.Sqrt(jumpHeight * -2f * gravity); }
        vy += gravity * Time.deltaTime; move.y = vy;
        cc.Move(move * Time.deltaTime);

        if (kb.eKey.wasPressedThisFrame &&
            Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            var door = hit.collider.GetComponentInParent<InteractableDoor>();
            if (door) door.Toggle(transform.position);
        }
    }

    void OnGUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        GUI.Label(new Rect(Screen.width / 2 - 4, Screen.height / 2 - 10, 20, 20), "+");
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<InteractableDoor>())
            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 2 + 14, 200, 22), "[E] открыть / закрыть");
    }
}

