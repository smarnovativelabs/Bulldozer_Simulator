using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
	[RequireComponent(typeof(Image))]
	public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		// Options for which axes to use
		public enum AxisOption
		{
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}


		public enum ControlStyle
		{
			Absolute, // operates from teh center of the image
			Relative, // operates from the center of the initial touch
			Swipe, // swipe to touch touch no maintained center
		}


		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public ControlStyle controlStyle = ControlStyle.Absolute; // control style to use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input
		public string swipeMagnitudeName = "SwipeMagnitude";
		public float Xsensitivity = 1f;
		public float Ysensitivity = 1f;
		public float touchSpeed = 10f;
		public float verticalDragThreshold = 0.05f;
		public float horizontalDragThreshold = 0.1f;
		public bool touchPadinEditor;


		Vector3 m_StartPos;
		Vector2 m_PreviousDelta;
		Vector3 m_JoytickOutput;
		bool m_UseX; // Toggle for using the x axis
		bool m_UseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_MagnitudeAxis;
		bool m_Dragging;
		int m_Id = -1;
		Vector2 m_PreviousTouchPos; // swipe style control touch
		Vector2 prevDeltaPoint;
		Vector3 m_PreviousMouse;

#if !UNITY_EDITOR
    private Vector3 m_Center;
    private Image m_Image;
#else
#endif

		void Awake()
		{
			CreateVirtualAxes();
		}

        void Start()
        {
#if !UNITY_EDITOR
            m_Image = GetComponent<Image>();
            m_Center = m_Image.transform.position;
#endif
        }

		void CreateVirtualAxes()
		{
			// set axes to use
			m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (m_UseX)
			{
				m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
			}
			m_MagnitudeAxis = new CrossPlatformInputManager.VirtualAxis(swipeMagnitudeName);
			CrossPlatformInputManager.RegisterVirtualAxis(m_MagnitudeAxis);

		}

		void UpdateVirtualAxes(Vector3 value)
		{
			value = value.normalized;
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Update(value.x);
			}

			if (m_UseY)
			{
				m_VerticalVirtualAxis.Update(value.y);
			}
		}
		void UpdateVirtualAxes(Vector3 value,float _magnitude)
		{
			value = value.normalized;
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Update(value.x);
			}

			if (m_UseY)
			{
				m_VerticalVirtualAxis.Update(value.y);
			}
			m_MagnitudeAxis.Update(_magnitude);
		}

		public void OnPointerDown(PointerEventData data)
		{
            if (!m_Dragging)
            {
				m_PreviousTouchPos = Input.mousePosition;
				m_StartPos = data.position;

			}
			m_Dragging = true;
			m_Id = data.pointerId;

			m_PreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
			prevDeltaPoint = m_PreviousMouse.normalized;
#if !UNITY_EDITOR
        if (controlStyle != ControlStyle.Absolute )
            m_Center = data.position;
#endif
		}

		void Update()
		{
			if (!m_Dragging)
			{
				return;
			}
#if UNITY_EDITOR
			m_PreviousTouchPos = Input.mousePosition;
			Vector2 pointerDelta1;
			pointerDelta1 = new Vector2(m_PreviousTouchPos.x - m_StartPos.x, m_PreviousTouchPos.y - m_StartPos.y);
			float _magnitude1 = pointerDelta1.magnitude;
			pointerDelta1 = pointerDelta1.normalized;
			pointerDelta1.x *= Xsensitivity;
			pointerDelta1.y *= Ysensitivity;
			UpdateVirtualAxes(new Vector3(pointerDelta1.x, pointerDelta1.y, 0), _magnitude1 / 2f);///2f
			m_StartPos = m_PreviousTouchPos;
			m_Id = Input.touchCount - 1;
#endif
			


			if (Input.touchCount >= m_Id + 1 && m_Id > -1)/////////////////////////////////////////////////for dual touch
			{
#if !UNITY_EDITOR

				//            if (controlStyle == ControlStyle.Swipe)
				//            {
				//                m_Center = m_PreviousTouchPos;
				                m_PreviousTouchPos = Input.touches[m_Id].position;
				//            }
				//            Vector2 pointerDelta = new Vector2(Input.touches[m_Id].position.x - m_Center.x , Input.touches[m_Id].position.y - m_Center.y);
				//			float _magnitude = pointerDelta.magnitude;
				//			pointerDelta = pointerDelta.normalized;

				//            pointerDelta.x *= Xsensitivity;
				//            pointerDelta.y *= Ysensitivity;
#else
				//				Vector2 pointerDelta;
				//				pointerDelta.x = Input.mousePosition.x - m_PreviousMouse.x;
				//				pointerDelta.y = Input.mousePosition.y - m_PreviousMouse.y;
				//				float _magnitude = pointerDelta.magnitude;
				//				pointerDelta = pointerDelta.normalized;
				//				pointerDelta.x *= Xsensitivity;
				//				pointerDelta.y *= Ysensitivity;
				m_PreviousTouchPos = Input.mousePosition;

				//				m_PreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);

#endif

				//				_magnitude = (_magnitude * _magnitude);
				//				UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0), _magnitude/4);

				//				prevDeltaPoint = pointerDelta;

				Vector2 pointerDelta;
				pointerDelta = new Vector2(m_PreviousTouchPos.x - m_StartPos.x, m_PreviousTouchPos.y - m_StartPos.y);
				float _magnitude = pointerDelta.magnitude;
				pointerDelta = pointerDelta.normalized;
				pointerDelta.x *= Xsensitivity;
				pointerDelta.y *= Ysensitivity;
				UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0), _magnitude / 2f);
				m_StartPos = m_PreviousTouchPos;
			}
			else if (Input.touchCount <= m_Id + 1 && Input.touchCount > 0)
			{
				m_Id = Input.touchCount - 1;
			}
		}


		public void OnPointerUp(PointerEventData data)
		{
			m_Dragging = false;
			m_Id = -1;
			//	prevDeltaPoint = Vector2.zero;
			UpdateVirtualAxes(Vector3.zero, 0);
		}

		public void DisbaleTouch()
        {
			m_Dragging = false;
			m_Id = -1;
			//	prevDeltaPoint = Vector2.zero;
			UpdateVirtualAxes(Vector3.zero, 0);
		}


		void OnDestroy()
        {
            if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
                CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
			if (CrossPlatformInputManager.AxisExists(swipeMagnitudeName))
				CrossPlatformInputManager.UnRegisterVirtualAxis(swipeMagnitudeName);
        }
    }
}
