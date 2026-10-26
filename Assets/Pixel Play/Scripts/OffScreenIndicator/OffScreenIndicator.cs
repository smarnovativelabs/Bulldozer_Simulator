using PixelPlay.OffScreenIndicator;
using System;
using System.Collections.Generic;
using UnityEngine;
using PixelPlay;
/// <summary>
/// Attach the script to the off screen indicator panel.
/// </summary>
[DefaultExecutionOrder(-1)]
public class OffScreenIndicator : MonoBehaviour
{
    [Range(0.5f, 0.9f)]
    [Tooltip("Distance offset of the indicators from the centre of the screen")]
    [SerializeField] private float screenBoundOffset = 0.9f;

    private Camera mainCamera;
    private Vector3 screenCentre;
    private Vector3 screenBounds;
    private int cachedScreenWidth;
    private int cachedScreenHeight;
    private RectTransform panelRect;
    private Canvas parentCanvas;

    private List<Target> targets = new List<Target>();
    private static readonly HashSet<Target> ActiveTargets = new HashSet<Target>();

    public static Action<Target, bool> TargetStateChanged;

    public static void SetTargetState(Target target, bool active)
    {
        if (target == null)
            return;

        if (active)
            ActiveTargets.Add(target);
        else
            ActiveTargets.Remove(target);

        TargetStateChanged?.Invoke(target, active);
    }

    void Awake()
    {
        mainCamera = Camera.main;
        panelRect = transform as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
        transform.SetAsLastSibling();
        RefreshScreenBounds();
        TargetStateChanged += HandleTargetStateChanged;

        foreach (Target target in ActiveTargets)
            HandleTargetStateChanged(target, true);
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        if (cachedScreenWidth != Screen.width || cachedScreenHeight != Screen.height)
            RefreshScreenBounds();

        DrawIndicators();
    }

    void RefreshScreenBounds()
    {
        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
        screenCentre = new Vector3(cachedScreenWidth, cachedScreenHeight, 0f) * 0.5f;
        screenBounds = screenCentre * screenBoundOffset;
    }

    /// <summary>
    /// Draw the indicators on the screen and set thier position and rotation and other properties.
    /// </summary>
    void DrawIndicators()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            Target target = targets[i];
            if (target == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            Vector3 screenPosition = OffScreenIndicatorCore.GetScreenPosition(mainCamera, target.transform.position);
            bool isTargetVisible = OffScreenIndicatorCore.IsTargetVisible(screenPosition);
            float distanceFromCamera = target.NeedDistanceText ? target.GetDistanceFromCamera(mainCamera.transform.position) : float.MinValue;// Gets the target distance from the camera.
            OffScreenBaseIndicator indicator = null;

            if(target.NeedBoxIndicator && isTargetVisible)
            {
                screenPosition.z = 0;
                indicator = GetIndicator(ref target.indicator, IndicatorType.BOX); // Gets the box indicator from the pool.
            }
            else if(target.NeedArrowIndicator && !isTargetVisible)
            {
                float angle = float.MinValue;
                OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPosition, ref angle, screenCentre, screenBounds);
                indicator = GetIndicator(ref target.indicator, IndicatorType.ARROW); // Gets the arrow indicator from the pool.
                if (indicator != null)
                    indicator.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Sets the rotation for the arrow indicator.
            }
            if(indicator)
            {
                indicator.SetImageColor(target.TargetColor);// Sets the image color of the indicator.
                indicator.SetDistanceText(distanceFromCamera); //Set the distance text for the indicator.
                SetIndicatorPosition(indicator, screenPosition);
                indicator.SetTextRotation(Quaternion.identity); // Sets the rotation of the distance text of the indicator.
            }
            else if (target.indicator != null)
            {
                target.indicator.Activate(false);
                target.indicator = null;
            }
        }
    }

    void SetIndicatorPosition(OffScreenBaseIndicator indicator, Vector2 screenPosition)
    {
        if (indicator == null)
            return;

        RectTransform indicatorRect = indicator.transform as RectTransform;
        Camera eventCamera = parentCanvas != null &&
                             parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;

        if (panelRect != null && indicatorRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                panelRect,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            indicatorRect.anchoredPosition = localPoint;
            return;
        }

        indicator.transform.position = screenPosition;
    }

    /// <summary>
    /// 1. Add the target to targets list if <paramref name="active"/> is true.
    /// 2. If <paramref name="active"/> is false deactivate the targets indicator, 
    ///     set its reference null and remove it from the targets list.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="active"></param>
    private void HandleTargetStateChanged(Target target, bool active)
    {
        if (target == null)
            return;

        if(active)
        {
            transform.SetAsLastSibling();
            if (!targets.Contains(target))
                targets.Add(target);
        }
        else
        {
            target.indicator?.Activate(false);
            target.indicator = null;
            targets.Remove(target);
        }
    }

    /// <summary>
    /// Get the indicator for the target.
    /// 1. If its not null and of the same required <paramref name="type"/> 
    ///     then return the same indicator;
    /// 2. If its not null but is of different type from <paramref name="type"/> 
    ///     then deactivate the old reference so that it returns to the pool 
    ///     and request one of another type from pool.
    /// 3. If its null then request one from the pool of <paramref name="type"/>.
    /// </summary>
    /// <param name="indicator"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private OffScreenBaseIndicator GetIndicator(ref OffScreenBaseIndicator indicator, IndicatorType type)
    {
        if(indicator != null)
        {
            if(indicator.Type != type)
            {
                indicator.Activate(false);
                indicator = RequestIndicator(type);
                indicator?.Activate(true); // Sets the indicator as active.
            }
        }
        else
        {
            indicator = RequestIndicator(type);
            indicator?.Activate(true); // Sets the indicator as active.
        }
        return indicator;
    }

    static OffScreenBaseIndicator RequestIndicator(IndicatorType type)
    {
        if (type == IndicatorType.BOX)
            return BoxObjectPool.current != null ? BoxObjectPool.current.GetPooledObject() : null;

        return ArrowObjectPool.current != null ? ArrowObjectPool.current.GetPooledObject() : null;
    }

    private void OnDestroy()
    {
        TargetStateChanged -= HandleTargetStateChanged;

        for (int i = 0; i < targets.Count; i++)
            if (targets[i] != null)
                targets[i].indicator?.Activate(false);
        targets.Clear();
    }
}
