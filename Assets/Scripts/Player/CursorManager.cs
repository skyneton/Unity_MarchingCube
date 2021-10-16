using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static bool IsFocus { get; private set; } = true;
    public static bool IsCursorLock
    {
        get => Cursor.lockState == CursorLockMode.Locked && IsFocus;
    }

    private void Update()
    {
        CheckEscape();
    }

    public static void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void CursorUnLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void CheckEscape()
    {
        if (Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows))
        {
            IsFocus = false;
        }

        if (!IsFocus && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)))
        {
            IsFocus = true;
        }
    }
}