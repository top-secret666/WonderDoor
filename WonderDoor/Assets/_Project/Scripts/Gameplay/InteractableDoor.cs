using UnityEngine;

// Дверь открывается на E: поворот вокруг вертикального края (петли), вычисляется по коллайдеру.
public class InteractableDoor : MonoBehaviour
{
    public float openAngle = 95f, speed = 3f;
    public bool hingeOnPositiveSide = false;

    Vector3 hingeLocal; Quaternion closedRot; Vector3 closedPos;
    float current, target; bool ready;

    void Start()
    {
        closedRot = transform.localRotation; closedPos = transform.localPosition;
        var col = GetComponent<Collider>();
        Bounds b = col is BoxCollider bc ? new Bounds(bc.center, bc.size) : new Bounds(Vector3.zero, Vector3.one);
        // петля по самой длинной горизонтальной оси, на её краю
        bool alongZ = Mathf.Abs(b.size.z * transform.localScale.z) >= Mathf.Abs(b.size.x * transform.localScale.x);
        float s = hingeOnPositiveSide ? 1f : -1f;
        hingeLocal = alongZ ? new Vector3(b.center.x, b.center.y, b.center.z + s * b.extents.z)
                            : new Vector3(b.center.x + s * b.extents.x, b.center.y, b.center.z);
        ready = true;
    }

    public void Toggle(Vector3 from)
    {
        if (!ready) return;
        if (Mathf.Abs(target) > 0.01f) { target = 0f; return; }
        // открываем от игрока
        Vector3 hingeW = transform.TransformPoint(hingeLocal);
        Vector3 toPlayer = from - hingeW; Vector3 centre = transform.position - hingeW;
        float side = Mathf.Sign(Vector3.Cross(centre, toPlayer).y);
        target = -side * openAngle;
    }

    void Update()
    {
        if (!ready || Mathf.Approximately(current, target)) return;
        current = Mathf.MoveTowards(current, target, openAngle * speed * Time.deltaTime);
        // восстановить закрытую позу и повернуть вокруг петли
        transform.localRotation = closedRot; transform.localPosition = closedPos;
        Vector3 hingeW = transform.TransformPoint(hingeLocal);
        transform.RotateAround(hingeW, Vector3.up, current);
    }
}
