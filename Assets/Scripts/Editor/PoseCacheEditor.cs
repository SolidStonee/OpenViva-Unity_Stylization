using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


[CustomEditor(typeof(viva.PoseCache))]
[CanEditMultipleObjects]
public class PoseCacheEditor : Editor
{

    [FormerlySerializedAs("loli")] public viva.Companion companion;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        companion = EditorGUILayout.ObjectField("Companion", companion, typeof(viva.Companion), true) as viva.Companion;

        if (companion)
        {
            if (GUILayout.Button("Copy"))
            {
                SerializedObject sObj = new SerializedObject(target);
                sObj.ApplyModifiedProperties();

                var positionsObj = sObj.FindProperty("positions");
                var quaternionsObj = sObj.FindProperty("quaternions");
                positionsObj.arraySize = companion.bodySMRs[0].bones.Length;
                quaternionsObj.arraySize = companion.bodySMRs[0].bones.Length;
                for (int i = 0; i < positionsObj.arraySize; i++)
                {
                    positionsObj.GetArrayElementAtIndex(i).vector3Value = companion.bodySMRs[0].bones[i].localPosition;
                    quaternionsObj.GetArrayElementAtIndex(i).quaternionValue = companion.bodySMRs[0].bones[i].localRotation;
                    Debug.Log("[Pose Cache] " + i + " = " + companion.bodySMRs[0].bones[i].name);
                }
                sObj.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Apply"))
            {
                SerializedObject sObj = new SerializedObject(target);
                var positionsObj = sObj.FindProperty("positions");
                var quaternionsObj = sObj.FindProperty("quaternions");

                for (int i = 0; i < companion.bodySMRs[0].bones.Length; i++)
                {
                    Transform t = companion.bodySMRs[0].bones[i];
                    t.localPosition = positionsObj.GetArrayElementAtIndex(i).vector3Value;
                    t.localRotation = quaternionsObj.GetArrayElementAtIndex(i).quaternionValue;
                }
            }
        }
    }

}