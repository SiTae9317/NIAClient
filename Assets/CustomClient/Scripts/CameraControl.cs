using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public enum MouseEventFlags : uint
{
    LEFTDOWN = 0x00000002,
    LEFTUP = 0x00000004,
    MIDDLEDOWN = 0x00000020,
    MIDDLEUP = 0x00000040,
    MOVE = 0x00000001,
    ABSOLUTE = 0x00008000,
    RIGHTDOWN = 0x00000008,
    RIGHTUP = 0x00000010,
    WHEEL = 0x00000800,
    XDOWN = 0x00000080,
    XUP = 0x00000100
}

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, System.UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern void SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out CPoint point);

    [DllImport("user32.dll")]
    public static extern int ShowCursor(bool bShow);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public struct CPoint
    {
        public int x;
        public int y;
        public CPoint(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }

    public float rotSpeed = 4.0f;
    public float moveSpeed = 2.0f;

    private bool isCursorActive = true;

    private CPoint point;
    private CPoint beforePoint;

    private int width;
    private int height;

    private int offset = 10;

    private bool isWKeyDown = false;
    private bool isSKeyDown = false;
    private bool isAKeyDown = false;
    private bool isDKeyDown = false;

    private Vector3 velocity = Vector3.zero;

    private void OnDisable()
    {
        if (!isCursorActive)
        {
            ShowCursor(!isCursorActive);
        }
    }

    // Use this for initialization
    void Start()
    {
        CapsuleCollider cc = gameObject.GetComponent<CapsuleCollider>();
        if(cc == null)
        {
            cc = gameObject.AddComponent<CapsuleCollider>();
        }
        Vector3 keepVec = gameObject.transform.eulerAngles;
        keepVec.z = 0.0f;
        gameObject.transform.eulerAngles = keepVec;

        gameObject.transform.position += Vector3.up * 0.5f;

        cc.isTrigger = false;
        cc.material = null;
        cc.center = new Vector3(0.0f, -0.5f, 0.0f);
        cc.radius = 0.1f;
        cc.height = 2.0f;
        cc.direction = 1;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.mass = 1.0f;
        rb.drag = 0.0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;


        width = GetSystemMetrics(0);
        height = GetSystemMetrics(1);

        controlModeChange();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            controlModeChange();
        }

        if (!isCursorActive)
        {
            infinityMouseMove();
            cameraRotation();
            moveControl();
        }
    }

    void controlModeChange()
    {
        velocity = Vector2.zero;

        isCursorActive = !isCursorActive;
        ShowCursor(isCursorActive);

        if (!isCursorActive)
        {
            GetCursorPos(out beforePoint);
        }
    }

    void cameraRotation()
    {
        CPoint currentPos;
        GetCursorPos(out currentPos);

        Vector2 calcCursor = Vector2.zero;

        calcCursor.x = (float)-(beforePoint.x - currentPos.x);
        calcCursor.y = (float)-(beforePoint.y - currentPos.y);

        beforePoint = currentPos;

        if (Mathf.Abs(calcCursor.x) > width / 2.0f || Mathf.Abs(calcCursor.y) > height / 2.0f)
        {
            return;
        }

        Vector3 newRot = gameObject.transform.localEulerAngles;

        newRot.x += calcCursor.y / (0.75f / rotSpeed) * Time.deltaTime;
        newRot.y += calcCursor.x / (1.0f / rotSpeed) * Time.deltaTime;

        if (newRot.x > 180.0f)
        {
            newRot.x -= 360.0f;
        }

        if (newRot.x < -180.0f)
        {
            newRot.x += 360.0f;
        }

        if (newRot.x >= 80.0f)
        {
            newRot.x = 80.0f;
        }

        if (newRot.x <= -80.0f)
        {
            newRot.x = -80.0f;
        }

        gameObject.transform.localEulerAngles = newRot;
    }

    void infinityMouseMove()
    {
        GetCursorPos(out point);

        if (point.x < offset)
        {
            point.x = width - offset;
        }
        if (point.x > width - offset)
        {
            point.x = offset;
        }

        if (point.y < offset)
        {
            point.y = height - offset;
        }
        if (point.y > height - offset)
        {
            point.y = offset;
        }

        SetCursorPos(point.x, point.y);
    }

    void moveControl()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            velocity.x += moveSpeed;
            isWKeyDown = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            velocity.x -= moveSpeed;
            isWKeyDown = false;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            velocity.x -= moveSpeed;
            isSKeyDown = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            velocity.x += moveSpeed;
            isSKeyDown = false;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            velocity.y -= moveSpeed;
            isAKeyDown = true;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            velocity.y += moveSpeed;
            isAKeyDown = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            velocity.y += moveSpeed;
            isDKeyDown = true;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            velocity.y -= moveSpeed;
            isDKeyDown = false;
        }

        if (isWKeyDown || isSKeyDown || isAKeyDown || isDKeyDown)
        {
            Vector3 localPos = gameObject.transform.localPosition;

            localPos += gameObject.transform.forward * velocity.x * Time.deltaTime;
            localPos += gameObject.transform.right * velocity.y * Time.deltaTime;

            gameObject.transform.localPosition = localPos;
        }
    }
}
