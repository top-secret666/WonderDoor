using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraPivot;
    public float walkSpeed = 8f, runSpeed = 13f, mouseSensitivity = 0.12f, gravity = -20f, jumpHeight = 1.6f;
    public float interactDistance = 6.5f;

    CharacterController cc;
    float pitch, vy;

    void Awake()
    {
        // На всякий случай: если два Player попали в Play — гасим чужие камеры
        var all = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (all.Length > 1)
        {
            Vector3 spawn = new Vector3(62.15f, 0.3f, -20.25f);
            PlayerController keep = this;
            float best = Vector3.Distance(transform.position, spawn);
            foreach (var p in all)
            {
                if (p == null) continue;
                float d = Vector3.Distance(p.transform.position, spawn);
                bool nearOrigin = p.transform.position.sqrMagnitude < 4f;
                float score = nearOrigin ? d + 1000f : d;
                if (score < best)
                {
                    best = score;
                    keep = p;
                }
            }

            if (keep != this)
            {
                enabled = false;
                foreach (var c in GetComponentsInChildren<Camera>(true)) c.enabled = false;
                foreach (var a in GetComponentsInChildren<AudioListener>(true)) a.enabled = false;
                return;
            }

            foreach (var p in all)
            {
                if (p == null || p == this) continue;
                p.enabled = false;
                foreach (var c in p.GetComponentsInChildren<Camera>(true)) c.enabled = false;
                foreach (var a in p.GetComponentsInChildren<AudioListener>(true)) a.enabled = false;
            }
        }

        cc = GetComponent<CharacterController>();
        if (!cameraPivot)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam) cameraPivot = cam.transform;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus && enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!enabled) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (mouse != null && Cursor.lockState == CursorLockMode.Locked && cameraPivot != null)
        {
            Vector2 d = mouse.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, d.x, 0f);
            pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        if (kb == null || cc == null) return;

        Vector2 m = Vector2.zero;
        if (kb.wKey.isPressed) m.y += 1;
        if (kb.sKey.isPressed) m.y -= 1;
        if (kb.dKey.isPressed) m.x += 1;
        if (kb.aKey.isPressed) m.x -= 1;

        float speed = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) ? runSpeed : walkSpeed;
        Vector3 move = Vector3.zero;
        if (m.sqrMagnitude > 0.001f)
            move = (transform.right * m.x + transform.forward * m.y).normalized * speed;

        if (cc.isGrounded)
        {
            vy = -2f;
            if (kb.spaceKey.wasPressedThisFrame)
                vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        vy += gravity * Time.deltaTime;
        move.y = vy;
        cc.Move(move * Time.deltaTime);

        if (kb.eKey.wasPressedThisFrame && cameraPivot != null &&
            Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            var door = hit.collider.GetComponentInParent<InteractableDoor>();
            if (door) door.Toggle(transform.position);
        }
    }

    void OnGUI()
    {
        if (!enabled || Cursor.lockState != CursorLockMode.Locked || cameraPivot == null) return;
        GUI.Label(new Rect(Screen.width / 2 - 4, Screen.height / 2 - 10, 20, 20), "+");
        if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactDistance, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<InteractableDoor>())
            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 2 + 14, 200, 22), "[E] открыть / закрыть");
    }
}
