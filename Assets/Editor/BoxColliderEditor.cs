using UnityEditor;
using UnityEngine;

namespace MO_IMPROVED_COLLIDERS
{
    [CustomEditor(typeof(BoxCollider))]
    public class BoxColliderEditor : ImprovedColliderEditor
    {
        /// <summary>
        /// This toggles the whole shebang.
        /// </summary>
        public static bool showImprovedHandles = false;
        /// <summary>
        /// Collapses unselected handles into buttons.
        /// </summary>
        public static bool collapseUnselectedHandles = true;
        /// <summary>
        /// Keeps track of which handle is currently being manipulated.
        /// </summary>
        // public int selectedHandle = 0;
        /// <summary>
        /// Stores instance's targetCollider separately so we can set it in Awake()...
        /// Unity doesn't share an instance between OnInspectorGUI and OnSceneGUI for some reason?
        /// So this necessary to keep in sync between the inspector and scene.
        /// </summary>
        public BoxCollider targetCollider;
        /// <summary>
        /// Stores only *actually* active targetCollider instance.
        /// Static to keep in sync over multiple instances.
        /// </summary>
        public static BoxCollider onlyActiveTargetCollider = null;

        /// <summary>
        /// Just for readability - the center handle's index will always be zero.
        /// </summary>
        const int centerIndex = 0;

        /// <summary>
        /// Assigns target collider and selects center handle.
        /// </summary>
        public void Awake()
        {
            targetCollider = (BoxCollider)target;
            selectedHandle = centerIndex;
        }

        /// <summary>
        /// Draws editor GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Clicked the collapse button too many times
            EditorGUILayout.Space();

            // Toggle Collider Handles Button
            // Ugly refs but just into base class so whatever...
            EditColliderButtonGUI(ref showImprovedHandles, ref onlyActiveTargetCollider, ref targetCollider);

            // Reset Center GUI
            // We can't get at targetCollider.center from inside since the function works for all collider types.
            targetCollider.center = CenterAxisGUI(ref targetCollider, targetCollider.center);

            base.OnInspectorGUI();

            /*
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Collapse unselected handles");
            collapseUnselectedHandles = EditorGUILayout.Toggle(collapseUnselectedHandles);
            EditorGUILayout.EndHorizontal();
            */
        }

        /// <summary>
        /// Calculates where each handle goes by applying parent rotation and position.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private Vector3 CalculateHandlePosition(Vector3 v)
        {
            Quaternion rotation = targetCollider.transform.rotation;
            v = targetCollider.transform.rotation * v;
            v += ColliderCenter();
            return v;
        }

        /// <summary>
        /// Draws Improved Collider Controls if active.
        /// </summary>
        public void OnSceneGUI()
        {

            if (showImprovedHandles)
            {
                Tools.hidden = true;
                UpdatePivotRotation(targetCollider);

                if (targetCollider == onlyActiveTargetCollider)
                {
                    Vector3[] points = BoxColliderPoints();

                    Vector3 scaleVector = Vector3.zero;
                    Vector3 moveSideVector = Vector3.zero;
                    Vector3 moveCenterVector = Vector3.zero;

                    EditorGUI.BeginChangeCheck();

                    int index = 0;

                    foreach (Vector3 point in points)
                    {
                        Vector3 handlePosition = CalculateHandlePosition(point);
                        UpdateHandleSizes(handlePosition);


                        // If this handle is selected...
                        if (!collapseUnselectedHandles)
                            DrawHandleAlwaysActive(index, handlePosition, ref moveCenterVector, ref moveSideVector, ref scaleVector, point);
                        else
                            DrawHandleSelectedActive(index, handlePosition, ref moveCenterVector, ref moveSideVector, ref scaleVector, point);

                        index++;
                    }

                    ReactToInput(ref moveSideVector, ref moveCenterVector, ref scaleVector, points);
                }
            }
        }

        /// <summary>
        /// Applies all handle manipulations to the collider.
        /// </summary>
        /// <param name="moveSideVector"></param>
        /// <param name="moveCenterVector"></param>
        /// <param name="scaleVector"></param>
        /// <param name="points"></param>
        void ReactToInput(ref Vector3 moveSideVector, ref Vector3 moveCenterVector, ref Vector3 scaleVector, Vector3[] points)
        {
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 scale = GetScaleVector();
                // moving center
                if (selectedHandle == centerIndex)
                {
                    Undo.RecordObject(targetCollider, "Move BoxCollider Center Position (Improved Collider Editor)");

                    moveCenterVector = DivideVectorPerAxis(moveCenterVector, scale);
                    targetCollider.center += Quaternion.Inverse(targetCollider.transform.rotation) * moveCenterVector;
                }
                // moving side
                else
                {
                    if (moveSideVector != Vector3.zero)
                    {
                        Undo.RecordObject(targetCollider, "Change BoxCollider Size - Pull (Improved Collider Editor)");

                        Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * moveSideVector;

                        // Log center size adjustment before we do any more scaling
                        Vector3 centerSizeAdjustmentVector = sizeAdjustmentVector / 2;

                        // Correct for negatively scaled gameObjects
                        sizeAdjustmentVector = Vector3.Scale(sizeAdjustmentVector, Vector3Sign(targetCollider.transform.localScale));

                        // Handle negative axii properly
                        if (!Vector3AxisIsPositive(points[selectedHandle]))
                            sizeAdjustmentVector *= -1;

                        sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, scale);
                        centerSizeAdjustmentVector = DivideVectorPerAxis(centerSizeAdjustmentVector, scale);

                        // Do placement
                        targetCollider.size += sizeAdjustmentVector;
                        targetCollider.center += centerSizeAdjustmentVector;
                    }

                    // scale
                    if (scaleVector != Vector3.zero)
                    {
                        Undo.RecordObject(targetCollider, "Change BoxCollider Size - Scale (Improved Collider Editor)");

                        Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * scaleVector;

                        // Correct for negatively scaled gameObjects
                        sizeAdjustmentVector = Vector3.Scale(sizeAdjustmentVector, Vector3Sign(targetCollider.transform.localScale));

                        // Handle negative axii properly
                        if (!Vector3AxisIsPositive(points[selectedHandle]))
                            sizeAdjustmentVector *= -1;

                        sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, scale);

                        // Do placement
                        targetCollider.size += sizeAdjustmentVector;
                    }

                    // keeping things positive, editor does funny things with negative sizes. bug? hmmmm
                    // TODO make box collider editor work with negatively sized colliders in a satisfying way?
                    targetCollider.size = new Vector3(
                        Mathf.Abs(targetCollider.size.x),
                        Mathf.Abs(targetCollider.size.y),
                        Mathf.Abs(targetCollider.size.z)
                        );
                }
            }

        }

        /// <summary>
        /// Returns the objects world scale
        /// </summary>
        /// <returns></returns>
        private Vector3 GetScaleVector()
        {
            return targetCollider.transform.lossyScale;
        }

        /// <summary>
        /// Draws and keeps track of deltas of all handles,
        /// drawing buttons in place of inactive handles.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="handlePosition"></param>
        /// <param name="moveCenterVector"></param>
        /// <param name="moveSideVector"></param>
        /// <param name="scaleVector"></param>
        /// <param name="point"></param>
        void DrawHandleSelectedActive(int index, Vector3 handlePosition, ref Vector3 moveCenterVector, ref Vector3 moveSideVector, ref Vector3 scaleVector, Vector3 point)
        {
            Vector3 center = ColliderCenter();

            if (selectedHandle == index)
            {
                // Dotted line from center of collider to center of all faces
                Handles.color = COLOR_SELECTED_HANDLE;
                DrawDottedLine(center, handlePosition);

                // if this point is the center point
                if (index == centerIndex)
                {
                    // Draw center manipulation handle
                    moveCenterVector = ActiveCenterHandle(handlePosition);
                }
                // if this point is a side editor
                else
                {
                    // Draw side manipulation handles
                    Vector3 scaleHandlePosition = handlePosition + ((handlePosition - center).normalized * HandleUtility.GetHandleSize(handlePosition) * 1.5f);
                    Quaternion rectHandleRotation = LookRotationOrIdentity(point, targetCollider.transform.rotation);

                    moveSideVector = ActiveSizeHandle(index, handlePosition, rectHandleRotation, center);
                    scaleVector = ActiveScaleHandle(scaleHandlePosition, handlePosition, center);
                }
            }
            // unselected handles
            else
            {
                // Dotted line from center of collider to center of all faces
                Handles.color = COLOR_UNINTERACTABLE;
                DrawDottedLine(center, handlePosition);

                // if this point is the center point
                if (index == centerIndex)
                {
                    if (InactiveCenterHandle(handlePosition))
                        selectedHandle = index;
                }
                // if this point is a side editor
                else
                {
                    // Draw side manipulation handles
                    Quaternion rectHandleRotation = LookRotationOrIdentity(point, targetCollider.transform.rotation);
                    if (InactiveSizeHandle(handlePosition, rectHandleRotation))
                        selectedHandle = index;
                }
            }
        }

        /// <summary>
        /// Draws and keeps track of deltas of all handles,
        /// draws inactive handles in a different color.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="handlePosition"></param>
        /// <param name="moveCenterVector"></param>
        /// <param name="moveSideVector"></param>
        /// <param name="scaleVector"></param>
        /// <param name="point"></param>
        void DrawHandleAlwaysActive(int index, Vector3 handlePosition, ref Vector3 moveCenterVector, ref Vector3 moveSideVector, ref Vector3 scaleVector, Vector3 point)
        {
            Vector3 center = ColliderCenter();

            if (index == centerIndex)
            {
                // Draw center manipulation handle
                moveCenterVector = ActiveCenterHandle(handlePosition);
            }
            else
            {
                // if this handle is selected
                if (index == selectedHandle)
                {
                    // Dotted line from center of collider to center of all faces
                    DrawActiveDottedLine(center, handlePosition);
                }
                else
                {
                    // Dotted line from center of collider to center of all faces
                    DrawInactiveDottedLine(center, handlePosition);
                }

                // Draw side manipulation handles
                Vector3 scaleHandlePosition = handlePosition + ((handlePosition - center).normalized * HandleUtility.GetHandleSize(handlePosition) * 1.5f);
                Quaternion rectHandleRotation = LookRotationOrIdentity(point, targetCollider.transform.rotation);

                // Apply size handle manipulation to temporary vector
                Vector3 sideMoveVec = ActiveSizeHandle(index, handlePosition, rectHandleRotation, center);

                // If vector is non-zero
                // Select it and apply its rotation
                if (sideMoveVec != Vector3.zero)
                {
                    selectedHandle = index;
                    moveSideVector = sideMoveVec;
                }

                // Draw scale manipulation handles
                Vector3 sideScaleVec = ActiveScaleHandle(scaleHandlePosition, handlePosition, center);

                // If vector is non-zero
                // Select it and apply its rotation
                if (sideScaleVec != Vector3.zero)
                {
                    selectedHandle = index;
                    scaleVector = sideScaleVec;
                }
            }
        }

        /// <summary>
        /// Returns the collider's center point.
        /// </summary>
        /// <returns></returns>
        private Vector3 ColliderCenter()
        {
            return base.ColliderCenter(targetCollider);
        }

        /// <summary>
        /// Returns all points on the BoxCollider where we want to make a handle.
        /// </summary>
        /// <returns></returns>
        Vector3[] BoxColliderPoints()
        {
            // Vector3 ex = targetCollider.bounds.extents;
            // Vector3 ex = targetCollider.bounds.size / 2;
            Vector3 ex = targetCollider.size / 2;
            ex = Vector3.Scale(ex, targetCollider.transform.lossyScale);
            return new Vector3[] {
                Vector3.zero,
                // SIDES
                new Vector3(
                    ex.x,
                    0,
                    0
                    ),
                new Vector3(
                    -ex.x,
                    0,
                    0
                    ),
                new Vector3(
                    0,
                    ex.y,
                    0
                    ),
                new Vector3(
                    0,
                    -ex.y,
                    0
                    ),
                new Vector3(
                    0,
                    0,
                    ex.z
                    ),
                new Vector3(
                    0,
                    0,
                    -ex.z
                    ),

            };
        }
    }
}
