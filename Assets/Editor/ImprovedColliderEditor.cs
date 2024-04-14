using UnityEditor;
using UnityEngine;

namespace MO_IMPROVED_COLLIDERS
{
    public class ImprovedColliderEditor : Editor
    {
        protected readonly Color COLOR_DEFAULT_UI = Color.white;
        protected readonly Color COLOR_SELECTED_HANDLE = Color.cyan;
        protected readonly Color COLOR_UNSELECTED_HANDLE = Color.white;
        protected readonly Color COLOR_UNINTERACTABLE = Color.gray;
        protected readonly Color COLOR_STOP_BUTTON = Color.gray;
        protected readonly Color COLOR_UNSELECTED_CENTER_HANDLE = Color.green;
        protected readonly Color COLOR_UNSELECTED_aCENTER_HANDLE = new Color(0.1f, 0.1f, 0.1f);

        /// <summary>
        /// All strings used in the GUI.
        /// </summary>
        // TODO: Make this true.
        private static string EditColliderString = "Edit Collider";
        private static string StopEditingColliderString = "Stop Editing Collider";
        private static string RecenterByAxisString = "Center to 0";

        /// <summary>
        /// All handle sizes used in the Scene view.
        /// </summary>
        private float squareHandleSize;
        private float circleHandleSize;
        private float cubeHandleSize;
        private float sphereHandleSize;
        private float dotHandleSize;
        private float centerHandleSize;
        private float sizeHandleSize;

        public int selectedHandle = 0;

        /// <summary>
        /// Global options, not exposed in GUI currently.
        /// </summary>
        static bool defaultPositionHandleForCenter = false;

        /// <summary>
        /// Most of the handle functions take a 'pickSize',
        /// but here I apply a multiplier to whatever handle size that handle uses.
        /// Controls the 'clickable margin' of handles in scene view.
        /// </summary>
        protected float pickMultiplier = 1.2f;

        /// <summary>
        /// We use this to store either World or Local rotation.
        /// </summary>
        protected Quaternion pivotRotation = Quaternion.identity;

        /// <summary>
        /// Sets center handle pivot rotation (Global/Local).
        /// </summary>
        /// <param name="collider"></param>

        protected void UpdatePivotRotation(Collider collider)
        {
            // respect pivot rotation mode
            pivotRotation = Tools.pivotRotation == PivotRotation.Local ?
                collider.transform.rotation : Quaternion.identity;
        }
        /// <summary>
        /// On disable, show default tools again.
        /// </summary>
        protected void OnDisable()
        {
            Tools.hidden = false;
        }

        /// <summary>
        /// Sets all handle sizes, per frame.
        /// </summary>
        /// <param name="pos"></param>
        protected void UpdateHandleSizes(Vector3 pos)
        {
            float handleSize = HandleUtility.GetHandleSize(pos);
            sizeHandleSize = handleSize * 0.8f;
            centerHandleSize = handleSize * 0.8f;
            squareHandleSize = handleSize / 8;
            cubeHandleSize = handleSize / 8;
            circleHandleSize = handleSize / 6;
            sphereHandleSize = handleSize / 6;
            dotHandleSize = handleSize / 32;
        }

        #region Vector3Methods
        /// <summary>
        /// Returns true if ANY axis of a Vector3 is positive.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected bool Vector3AxisIsPositive(Vector3 v)
        {
            if (v.x > 0 ||
                v.y > 0 ||
                v.z > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Returns just the sign of each vector.
        /// For example, (32, -2, 0) would return (1, -1 1).
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected Vector3 Vector3Sign(Vector3 v)
        {
            return new Vector3(
                Mathf.Sign(v.x),
                Mathf.Sign(v.y),
                Mathf.Sign(v.z)
                );
        }

        /// <summary>
        /// Returns each axis of a vector divided individually by each axis of another vector.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        protected Vector3 DivideVectorPerAxis(Vector3 v1, Vector3 v2)
        {
            return new Vector3(
                v1.x / v2.x,
                v1.y / v2.y,
                v1.z / v2.z
                );
        }
        #endregion

        /// <summary>
        /// Draws a dotted line between two points.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        protected void DrawDottedLine(Vector3 startPoint, Vector3 endPoint)
        {
            Handles.DrawDottedLine(startPoint, endPoint, 0.2f);
        }

        /// <summary>
        /// Draws 'active' dotted line.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        protected void DrawActiveDottedLine(Vector3 startPoint, Vector3 endPoint)
        {
            Handles.color = COLOR_SELECTED_HANDLE;
            Handles.DrawDottedLine(startPoint, endPoint, 0.2f);
        }

        /// <summary>
        /// Draws 'inactive' dotted line.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        protected void DrawInactiveDottedLine(Vector3 startPoint, Vector3 endPoint)
        {
            Handles.color = COLOR_UNSELECTED_HANDLE;
            Handles.DrawDottedLine(startPoint, endPoint, 0.2f);
        }
        /// <summary>
        /// Draws the 'Edit Collider' button.
        /// </summary>
        /// <returns></returns>
        protected bool EditColliderButton()
        {
            return GUILayout.Button(EditColliderString, GUILayout.Height(30));
        }

        /// <summary>
        /// Draws the 'Stop Editing Collider' button.
        /// </summary>
        /// <returns></returns>
        protected bool StopEditingColliderButton()
        {
            GUI.backgroundColor = COLOR_STOP_BUTTON;
            bool pressedButton = GUILayout.Button(StopEditingColliderString, GUILayout.Height(30));
            GUI.backgroundColor = COLOR_DEFAULT_UI;
            return pressedButton;

        }

        /// <summary>
        /// Draws either the edit or stop editing button and handles their 'onClick'
        /// which handles the mode switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isEditing"></param>
        /// <param name="onlyActiveCollider"></param>
        /// <param name="targetCollider"></param>
        protected void EditColliderButtonGUI<T>(ref bool isEditing, ref T onlyActiveCollider, ref T targetCollider) where T : Collider
        {
            if (!isEditing || onlyActiveCollider != targetCollider)
            {
                if (EditColliderButton())
                {
                    isEditing = true;
                    onlyActiveCollider = targetCollider;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (StopEditingColliderButton())
                {
                    isEditing = false;
                    Tools.hidden = false;
                    SceneView.RepaintAll();
                }
            }

        }

        /// <summary>
        /// Draws the buttons that reset the collider's center axis.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected Vector3 ResetCenterAxisGUI(Collider col, Vector3 center)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(RecenterByAxisString);
            Vector3 c = center;
            if (GUILayout.Button("X"))
                c.x = 0;
            if (GUILayout.Button("Y"))
                c.y = 0;
            if (GUILayout.Button("Z"))
                c.z = 0;
            GUILayout.EndHorizontal();

            return c;
        }

        /// <summary>
        /// Draws the active radius handle and returns its delta movement.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="handlePosition"></param>
        /// <param name="circleHandleRotation"></param>
        /// <param name="radiusHandlePosition"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected Vector3 ActiveRadiusHandle(int index, Vector3 handlePosition, Quaternion circleHandleRotation, Vector3 radiusHandlePosition, Vector3 center)
        {
            Handles.CircleHandleCap(index, handlePosition, circleHandleRotation, circleHandleSize, EventType.Repaint);
            return Handles.Slider
                (
                    radiusHandlePosition,
                    radiusHandlePosition - center,
                    sphereHandleSize,
                    Handles.SphereHandleCap,
                    0.1f
                ) - radiusHandlePosition;
        }

        /// <summary>
        /// Draws the active sizing handle and returns its delta movement.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="handlePosition"></param>
        /// <param name="rectHandleRotation"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected Vector3 ActiveSizeHandle(int index, Vector3 handlePosition, Quaternion rectHandleRotation, Vector3 center)
        {
            Handles.RectangleHandleCap(index, handlePosition, rectHandleRotation, squareHandleSize, EventType.Repaint);
            return Handles.Slider
                (
                    handlePosition,
                    handlePosition - center,
                    sizeHandleSize,
                    Handles.ArrowHandleCap,
                    0.1f
                ) - handlePosition;
        }

        /// <summary>
        /// Draws the inactive sizing handle and returns if it was clicked.
        /// </summary>
        /// <param name="handlePosition"></param>
        /// <param name="rectHandleRotation"></param>
        /// <returns></returns>
        protected bool InactiveSizeHandle(Vector3 handlePosition, Quaternion rectHandleRotation)
        {
            Handles.color = COLOR_UNINTERACTABLE;
            Handles.Button(handlePosition, rectHandleRotation, squareHandleSize, squareHandleSize * pickMultiplier, Handles.RectangleHandleCap);
            Handles.color = COLOR_UNSELECTED_HANDLE;
            return Handles.Button(handlePosition, rectHandleRotation, dotHandleSize, squareHandleSize * pickMultiplier, Handles.DotHandleCap);
        }

        /// <summary>
        /// Draws the inactive radius handle and returns if it was clicked.
        /// </summary>
        /// <param name="handlePosition"></param>
        /// <param name="circleHandleRotation"></param>
        /// <returns></returns>
        protected bool InactiveRadiusHandle(Vector3 handlePosition, Quaternion circleHandleRotation)
        {
            Handles.color = COLOR_UNINTERACTABLE;
            Handles.Button(handlePosition, circleHandleRotation, circleHandleSize, circleHandleSize * pickMultiplier, Handles.CircleHandleCap);
            Handles.color = COLOR_UNSELECTED_HANDLE;
            return Handles.Button(handlePosition, circleHandleRotation, dotHandleSize, circleHandleSize * pickMultiplier, Handles.DotHandleCap);
        }

        /// <summary>
        /// Draws the inactive center handle and returns if it was clicked.
        /// </summary>
        /// <param name="handlePosition"></param>
        /// <returns></returns>
        protected bool InactiveCenterHandle(Vector3 handlePosition)
        {
            Handles.color = COLOR_UNSELECTED_CENTER_HANDLE;
            return Handles.Button(handlePosition, pivotRotation, cubeHandleSize, cubeHandleSize * pickMultiplier, Handles.CubeHandleCap);
        }

        /// <summary>
        /// Draws the active center handle and returns its delta movement.
        /// </summary>
        /// <param name="handlePosition"></param>
        /// <returns></returns>
        protected Vector3 ActiveCenterHandle(Vector3 handlePosition)
        {
            if (defaultPositionHandleForCenter)
            {
                return Handles.PositionHandle(
                    handlePosition,
                    pivotRotation
                    ) - handlePosition;
            }
            else
            {
                Vector3 xMoveVec = Handles.Slider(handlePosition, pivotRotation * Vector3.right, centerHandleSize, Handles.ArrowHandleCap, 0.1f) - handlePosition;
                Vector3 yMoveVec = Handles.Slider(handlePosition, pivotRotation * Vector3.up, centerHandleSize, Handles.ArrowHandleCap, 0.1f) - handlePosition;
                Vector3 zMoveVec = Handles.Slider(handlePosition, pivotRotation * Vector3.forward, centerHandleSize, Handles.ArrowHandleCap, 0.1f) - handlePosition;
                Vector3 centerMoveVec = xMoveVec + yMoveVec + zMoveVec;
                return centerMoveVec;
            }
        }

        /// <summary>
        /// Draws the active scale handle and returns its delta movement.
        /// </summary>
        /// <param name="scaleHandlePosition"></param>
        /// <param name="handlePosition"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected Vector3 ActiveScaleHandle(Vector3 scaleHandlePosition, Vector3 handlePosition, Vector3 center)
        {
            return Handles.Slider
                (
                    scaleHandlePosition,
                    handlePosition - center,
                    cubeHandleSize,
                    Handles.CubeHandleCap,
                    0.1f
                ) - scaleHandlePosition;
        }

        /// <summary>
        /// Draws the center axis buttons and returns a vector of where to move the center to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="col"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        protected Vector3 CenterAxisGUI<T>(ref T col, Vector3 center) where T : Collider
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Center to 0 on Axis");
            Vector3 c = center;
            if (GUILayout.Button("X"))
                c.x = 0;
            if (GUILayout.Button("Y"))
                c.y = 0;
            if (GUILayout.Button("Z"))
                c.z = 0;
            GUILayout.EndHorizontal();
            if (c != center)
            {
                Undo.RecordObject(col, "Centered Collider (Improved Collider Editor)");
                // col.bounds.center = c;
            }
            return c;
        }

        /// <summary>
        /// Returns the collider's center position.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        protected Vector3 ColliderCenter(Collider col)
        {
            // return LocalVectorScaled(targetCollider.center);
            return col.bounds.center;
        }

        /// <summary>
        /// Returns either a Look Rotation or a Quaternion.identity.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        protected Quaternion LookRotationOrIdentity(Vector3 v, Quaternion rotation)
        {
            return v == Vector3.zero ?
                Quaternion.identity :
                rotation * Quaternion.LookRotation(v, Vector3.up);
        }

    }
}
