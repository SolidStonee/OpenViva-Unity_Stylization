using UnityEditor;
using UnityEngine;

namespace MO_IMPROVED_COLLIDERS
{
    [CustomEditor(typeof(SphereCollider))]
    public class SphereColliderEditor : ImprovedColliderEditor
    {
        /// <summary>
        /// This toggles the whole shebang.
        /// </summary>
        public static bool showImprovedHandles = false;
        /// <summary>
        /// Stores instance's targetCollider separately so we can set it in Awake()...
        /// Unity doesn't share an instance between OnInspectorGUI and OnSceneGUI for some reason?
        /// So this necessary to keep in sync between the inspector and scene.
        /// </summary>
        public SphereCollider targetCollider;
        /// <summary>
        /// Stores only active targetCollider instance.
        /// Static to keep in sync over multiple instances.
        /// </summary>
        public static SphereCollider onlyActiveTargetCollider = null;
        /// <summary>
        /// Just for readability - the center handle's index will always be zero.
        /// </summary>
        const int centerIndex = 0;
        /// <summary>
        /// Whether to draw handles on all sides or just on one.
        /// </summary>
        private static bool handlesOnAllSides = true;

        /// <summary>
        /// Assigns target collider and selects center handle.
        /// </summary>
        public void Awake()
        {
            targetCollider = (SphereCollider)target;
            selectedHandle = centerIndex;
        }

        /// <summary>
        /// Draws editor GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
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
            GUILayout.Label("Handles on all sides");
            handlesOnAllSides = EditorGUILayout.Toggle(handlesOnAllSides);
            EditorGUILayout.EndHorizontal();
            */
        }

        /// <summary>
        /// Returns the largest lossyScale axis.
        /// </summary>
        /// <returns></returns>
        float GetMaximumScale()
        {
            Vector3 scale = targetCollider.transform.lossyScale;
            return Mathf.Max(scale.x, scale.y, scale.z);
        }

        /// <summary>
        /// Returns array of vector3s representing everywhere that we want a handle.
        /// </summary>
        /// <returns></returns>
        Vector3[] SphereColliderPoints()
        {
            // Vector3 ex = targetCollider.bounds.extents;
            // Vector3 ex = targetCollider.bounds.size / 2;

            float rad = targetCollider.radius;
            rad *= GetMaximumScale();
            if (handlesOnAllSides)
                return new Vector3[] {
                    Vector3.zero,
                    // SIDES
                    Vector3.right * rad,
                    Vector3.left * rad,
                    Vector3.up * rad,
                    Vector3.down * rad,
                    Vector3.forward * rad,
                    Vector3.back * rad,
                };
            else
                return new Vector3[]
                {
                    Vector3.zero,
                    Vector3.right * rad
                };
        }

        /// <summary>
        /// Calculates where each handle goes by applying parent transform.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private Vector3 CalculateHandlePosition(Vector3 v)
        {
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
                    Vector3[] points = SphereColliderPoints();

                    Vector3 scaleVector = Vector3.zero;
                    Vector3 moveSideVector = Vector3.zero;
                    Vector3 moveCenterVector = Vector3.zero;

                    EditorGUI.BeginChangeCheck();

                    int index = 0;

                    foreach (Vector3 point in points)
                    {
                        Vector3 handlePosition = CalculateHandlePosition(point);
                        UpdateHandleSizes(handlePosition);

                        DrawHandleSelectedActive(index, handlePosition, ref moveCenterVector, ref moveSideVector, ref scaleVector, point);

                        index++;
                    }

                    ReactToInput(ref moveSideVector, ref moveCenterVector, ref scaleVector, points);
                }
            }
        }

        /// <summary>
        /// Draws selected handles as full handles and unselected ones as dots.
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

                if (index == centerIndex)
                {
                    // Draw center manipulation handle
                    moveCenterVector = ActiveCenterHandle(handlePosition);
                }
                else
                {
                    // Draw radius manipulation handles
                    Vector3 radiusHandlePosition = handlePosition + ((handlePosition - center).normalized * HandleUtility.GetHandleSize(handlePosition) * 0.2f);
                    Quaternion circleHandleRotation = LookRotationOrIdentity(point, targetCollider.transform.rotation);
                    scaleVector = ActiveRadiusHandle(index, handlePosition, circleHandleRotation, radiusHandlePosition, center);
                }
            }
            else
            {
                // Dotted line from center of collider to center of all faces
                DrawInactiveDottedLine(center, handlePosition);

                if (index == centerIndex)
                {
                    // Draw center manipulation handle
                    if (InactiveCenterHandle(handlePosition))
                        selectedHandle = index;
                }
                else
                {
                    // Draw radius manipulation handles
                    Quaternion circleHandleRotation = LookRotationOrIdentity(point, targetCollider.transform.rotation);
                    if (InactiveRadiusHandle(handlePosition, circleHandleRotation))
                        selectedHandle = index;
                }
            }
        }

        /// <summary>
        /// Calculates collider's center point.
        /// </summary>
        /// <returns></returns>
        private Vector3 ColliderCenter()
        {
            return base.ColliderCenter(targetCollider);
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
                // moving center
                if (selectedHandle == centerIndex)
                {
                    Undo.RecordObject(targetCollider, "Move SphereCollider Center Position (Improved Collider Editor)");

                    Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * moveCenterVector;
                    sizeAdjustmentVector = DivideVectorPerAxis(sizeAdjustmentVector, targetCollider.transform.lossyScale);
                    targetCollider.center += sizeAdjustmentVector;
                }
                // moving radius
                else
                {
                    // scale
                    if (scaleVector != Vector3.zero)
                    {
                        Undo.RecordObject(targetCollider, "Change SphereCollider Scale (Improved Collider Editor)");
                        Vector3 sizeAdjustmentVector = Quaternion.Inverse(targetCollider.transform.rotation) * scaleVector;

                        // Handle negative axii properly
                        if (!Vector3AxisIsPositive(points[selectedHandle]))
                            sizeAdjustmentVector *= -1;
                        // Do placement
                        float radiusAdjustment = sizeAdjustmentVector.x + sizeAdjustmentVector.y + sizeAdjustmentVector.z;
                        targetCollider.radius += (radiusAdjustment / 2);
                    }

                    // keeping things positive, editor does funny things with negative sizes. bug? hmmmm
                    targetCollider.radius = Mathf.Abs(targetCollider.radius);
                }
            }

        }

    }
}