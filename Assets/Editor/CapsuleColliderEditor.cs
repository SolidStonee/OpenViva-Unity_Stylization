using UnityEditor;
using UnityEngine;

namespace MO_IMPROVED_COLLIDERS
{
    [CustomEditor(typeof(CapsuleCollider))]
    public class CapsuleColliderEditor : ImprovedColliderEditor
    {
        /// <summary>
        /// This toggles the whole shebang.
        /// </summary>
        public static bool showImprovedHandles = false;
        /// <summary>
        /// Collapses unselected handles into buttons.
        /// </summary>
        // NOT IMPLEMENTED (YET?)... I don't ever use the always active handles mode.
        // Try it on the cube by setting this to true there.
        // public static bool collapseUnselectedHandles = true;
        /// <summary>
        /// Keeps track of which handle is currently being manipulated.
        /// </summary>
        // public int selectedHandle = 0;
        /// <summary>
        /// Stores instance's targetCollider separately so we can set it in Awake()...
        /// Unity doesn't share an instance between OnInspectorGUI and OnSceneGUI for some reason?
        /// So this necessary to keep in sync between the inspector and scene.
        /// </summary>
        public CapsuleCollider targetCollider;
        /// <summary>
        /// Stores only *actually* active targetCollider instance.
        /// Static to keep in sync over multiple instances.
        /// </summary>
        public static CapsuleCollider onlyActiveTargetCollider = null;
        /// <summary>
        /// Just for readability - the center handle's index will always be zero.
        /// </summary>
        const int centerIndex = 0;

        /// <summary>
        /// Assigns target collider and selects center handle.
        /// </summary>
        public void Awake()
        {
            targetCollider = (CapsuleCollider)target;
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
                    Vector3[] points = CapsuleColliderPoints();

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
                        /*
                        if (!collapseUnselectedHandles)
                            DrawHandleAlwaysActive(index, handlePosition, ref moveCenterVector, ref moveSideVector, ref scaleVector, point);
                        else
                        */
                        DrawHandleSelectedActive(index, handlePosition, ref moveCenterVector, ref moveSideVector, ref scaleVector, point);

                        index++;
                    }

                    ReactToInput(ref moveSideVector, ref moveCenterVector, ref scaleVector, points);
                }
            }
        }

        /// <summary>
        /// Draws and keeps track of deltas of all handles, drawing buttons in place of inactive handles.
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
                DrawActiveDottedLine(center, handlePosition);

                // If this point is the center point
                if (index == centerIndex)
                {
                    // Draw center manipulation handle
                    moveCenterVector = ActiveCenterHandle(handlePosition);
                }
                // If this point is a height editor
                else if (index == 1 || index == 2)
                {
                    // Draw height manipulation handles
                    Vector3 scaleHandlePosition = handlePosition + ((handlePosition - center).normalized * HandleUtility.GetHandleSize(handlePosition) * 1.5f);
                    Quaternion rectHandleRotation = LookRotationOrIdentity(RotationFromUpToDirection() * point, targetCollider.transform.rotation);

                    moveSideVector = ActiveSizeHandle(index, handlePosition, rectHandleRotation, center);
                    scaleVector = ActiveScaleHandle(scaleHandlePosition, handlePosition, center);
                }
                // The rest are radius handles
                else
                {
                    // Draw radius manipulation handles
                    Vector3 radiusHandlePosition = handlePosition + ((handlePosition - center).normalized * HandleUtility.GetHandleSize(handlePosition) * 0.2f);
                    Quaternion circleHandleRotation = LookRotationOrIdentity(RotationFromUpToDirection() * point, targetCollider.transform.rotation);
                    scaleVector = ActiveRadiusHandle(index, handlePosition, circleHandleRotation, radiusHandlePosition, center);

                }
            }
            // Unselected handles
            else
            {
                // Dotted line from center of collider to center of all faces
                DrawInactiveDottedLine(center, handlePosition);

                // If this point is the center point
                if (index == centerIndex)
                {
                    if (InactiveCenterHandle(handlePosition))
                        selectedHandle = index;
                }
                // If this point is a height editor
                else if (index == 1 || index == 2)
                {
                    // Draw height manipulation handles
                    Quaternion rectHandleRotation = LookRotationOrIdentity(RotationFromUpToDirection() * point, targetCollider.transform.rotation);
                    if (InactiveSizeHandle(handlePosition, rectHandleRotation))
                        selectedHandle = index;
                }
                // The rest are radius handles
                else
                {
                    // Draw radius manipulation handles
                    Quaternion circleHandleRotation = LookRotationOrIdentity(RotationFromUpToDirection() * point, targetCollider.transform.rotation);
                    if (InactiveRadiusHandle(handlePosition, circleHandleRotation))
                        selectedHandle = index;
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
            Vector3 scale = GetScale();

            if (EditorGUI.EndChangeCheck())
            {
                // Moving center
                if (selectedHandle == centerIndex)
                {
                    Undo.RecordObject(targetCollider, "Move CapsuleCollider Center Position (Improved Collider Editor)");

                    Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * moveCenterVector;
                    sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, scale);
                    targetCollider.center += sizeAdjustmentVector;
                }
                // Moving height
                else if (selectedHandle == 1 || selectedHandle == 2)
                {
                    bool doScale = scaleVector == Vector3.zero ? false : true;
                    if (doScale) moveSideVector = scaleVector;

                    if (moveSideVector != Vector3.zero)
                    {
                        Undo.RecordObject(targetCollider, "Change CapsuleCollider Height (Improved Collider Editor)");
                        Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * moveSideVector;

                        // Log center size adjustment before we do any more scaling
                        Vector3 centerSizeAdjustmentVector = sizeAdjustmentVector / 2;

                        // Correct for negatively scaled gameObjects
                        sizeAdjustmentVector = Vector3.Scale(sizeAdjustmentVector, Vector3Sign(targetCollider.transform.lossyScale));

                        // Handle negative axii properly
                        if (!Vector3AxisIsPositive(points[selectedHandle]))
                            sizeAdjustmentVector *= -1;

                        sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, scale);
                        centerSizeAdjustmentVector = DivideVectorPerAxis(centerSizeAdjustmentVector, scale);

                        // Do placement
                        targetCollider.height += sizeAdjustmentVector.x + sizeAdjustmentVector.y + sizeAdjustmentVector.z;
                        if (!doScale)
                            targetCollider.center += centerSizeAdjustmentVector;

                    }
                }
                // Moving side
                else
                {
                    // Scale
                    if (scaleVector != Vector3.zero)
                    {
                        Undo.RecordObject(targetCollider, "Change BoxCollider Radius (Improved Collider Editor)");
                        Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * scaleVector;
                        // Correct for negatively scaled gameObjects
                        sizeAdjustmentVector = Vector3.Scale(sizeAdjustmentVector, Vector3Sign(targetCollider.transform.lossyScale));
                        // Handle negative axii properly
                        if (!Vector3AxisIsPositive(points[selectedHandle]))
                            sizeAdjustmentVector *= -1;

                        sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, scale);
                        // TODO find out why y is reversed?
                        float radiusAdjustment = sizeAdjustmentVector.x + -sizeAdjustmentVector.y + sizeAdjustmentVector.z;
                        // Do placement
                        targetCollider.radius += (radiusAdjustment / 2);
                    }

                }
                // keeping things positive, editor does funny things with negative sizes. bug? hmmmm
                targetCollider.radius = Mathf.Abs(targetCollider.radius);
                targetCollider.height = Mathf.Abs(targetCollider.height);

                if (targetCollider.height < targetCollider.radius * 2)
                    targetCollider.height = targetCollider.radius * 2;
                if (targetCollider.radius > targetCollider.height / 2)
                    targetCollider.radius = targetCollider.height / 2;
            }

        }

        /// <summary>
        /// Gets a vector from the capsule collider's direction integer index.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetDirectionVector()
        {
            Vector3 directionVector;
            switch (targetCollider.direction)
            {
                case 0:
                    directionVector = Vector3.right;
                    break;
                case 1:
                    directionVector = Vector3.up;
                    break;
                case 2:
                    directionVector = Vector3.forward;
                    break;
                default:
                    directionVector = Vector3.zero;
                    break;
            }
            return directionVector;
        }

        /// <summary>
        /// Rotates a point from 'Vector3.up' space to collider's direction.
        /// </summary>
        /// <returns></returns>
        private Quaternion RotationFromUpToDirection()
        {
            Vector3 directionVector = GetDirectionVector();
            // if (directionVector != Vector3.up)
            return Quaternion.FromToRotation(Vector3.up, directionVector);
            /*
            else
                return Quaternion.FromToRotation(Vector3.right, directionVector);
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
            v = RotationFromUpToDirection() * v;
            v = targetCollider.transform.rotation * v;
            v += ColliderCenter();
            return v;
        }

        /// <summary>
        /// Returns center of collider.
        /// </summary>
        /// <returns></returns>
        private Vector3 ColliderCenter()
        {
            return base.ColliderCenter(targetCollider);
        }

        /// <summary>
        /// Returns lossyScale of parent transform
        /// </summary>
        /// <returns></returns>
        Vector3 GetScale()
        {
            return targetCollider.transform.lossyScale;
        }

        /// <summary>
        /// Returns array of all points on collider where we want to make a handle.
        /// </summary>
        /// <returns></returns>
        Vector3[] CapsuleColliderPoints()
        {
            float height = targetCollider.height / 2;
            float rad = targetCollider.radius;
            Vector3 scale = GetScale();
            Vector3 directionVector = GetDirectionVector();
            if (directionVector == Vector3.up)
            {
                height *= targetCollider.transform.lossyScale.y;
                rad *= Mathf.Max(scale.x, scale.z);
            }
            else if (directionVector == Vector3.right)
            {
                height *= targetCollider.transform.lossyScale.x;
                rad *= Mathf.Max(scale.y, scale.z);
            }
            else if (directionVector == Vector3.forward)
            {
                height *= targetCollider.transform.lossyScale.z;
                rad *= Mathf.Max(scale.y, scale.x);
            }
            Vector3 baseVector = Vector3.one;
            return new Vector3[] {
                Vector3.zero,
                // height
                new Vector3(
                    0,
                    height,
                    0
                    ),
                new Vector3(
                    0,
                    -height,
                    0
                    ),
                // Radius
                // Center
                new Vector3(
                    rad,
                    0,
                    0
                    ),
                new Vector3(
                    -rad,
                    0,
                    0
                    ),
                new Vector3(
                    0,
                    0,
                    rad
                    ),
                new Vector3(
                    0,
                    0,
                    -rad
                    ),
                /*
                // Top Radius
                new Vector3(
                    rad,
                    height - rad,
                    0
                    ),
                new Vector3(
                    -rad,
                    height - rad,
                    0
                    ),
                new Vector3(
                    0,
                    height - rad,
                    rad
                    ),
                new Vector3(
                    0,
                    height - rad,
                    -rad
                    ),
                // Bottom Radius
                new Vector3(
                    rad,
                    -(height - rad),
                    0
                    ),
                new Vector3(
                    -rad,
                    -(height - rad),
                    0
                    ),
                new Vector3(
                    0,
                    -(height - rad),
                    rad
                    ),
                new Vector3(
                    0,
                    -(height - rad),
                    -rad
                    ),
                    */
            };
        }

    }
}
