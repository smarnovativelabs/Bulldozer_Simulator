#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Adapter for the bundled Reporter, preserving its gestures with Input System-only projects.
internal static class ReporterInputCompat
{
    internal struct TouchData { public Vector2 position; public UnityEngine.TouchPhase phase; }
    internal struct TouchCollection
    {
        public int Length {
            get {
                var screen=Touchscreen.current; if(screen==null)return 0;
                int count=0; foreach(var touch in screen.touches) if(Active(touch))count++;
                return count;
            }
        }
        public TouchData this[int index] {
            get {
                var screen=Touchscreen.current;
                if(screen!=null)foreach(var touch in screen.touches)if(Active(touch)&&index--==0)
                    return new TouchData { position=touch.position.ReadValue(),phase=ConvertPhase(touch.phase.ReadValue()) };
                return default;
            }
        }
    }
    static bool Active(TouchControl touch)=>touch.press.isPressed||touch.press.wasReleasedThisFrame;
    static UnityEngine.TouchPhase ConvertPhase(UnityEngine.InputSystem.TouchPhase phase)
    {
        switch(phase){
            case UnityEngine.InputSystem.TouchPhase.Began:return UnityEngine.TouchPhase.Began;
            case UnityEngine.InputSystem.TouchPhase.Moved:return UnityEngine.TouchPhase.Moved;
            case UnityEngine.InputSystem.TouchPhase.Ended:return UnityEngine.TouchPhase.Ended;
            case UnityEngine.InputSystem.TouchPhase.Canceled:return UnityEngine.TouchPhase.Canceled;
            default:return UnityEngine.TouchPhase.Stationary;
        }
    }
    public static TouchCollection touches=>default;
    public static Vector3 mousePosition=>Mouse.current==null?Vector3.zero:(Vector3)Mouse.current.position.ReadValue();
    public static bool GetMouseButton(int button)=>button==0&&Mouse.current!=null&&Mouse.current.leftButton.isPressed;
    public static bool GetMouseButtonDown(int button)=>button==0&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame;
    public static bool GetMouseButtonUp(int button)=>button==0&&Mouse.current!=null&&Mouse.current.leftButton.wasReleasedThisFrame;
}
#endif
