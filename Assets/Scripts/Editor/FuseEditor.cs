﻿using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Viva.Fuse), true)]
[CanEditMultipleObjects]
public class FuseEditor : Editor
{
    private SerializedObject sObj;
    private SerializedProperty fusePSys;

    private void OnEnable()
    {
        sObj = new SerializedObject(target);
        fusePSys = sObj.FindProperty("fuseFXContainer");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (fusePSys != null && fusePSys.objectReferenceValue != null && GUILayout.Button("Register Path Point at Particle System"))
        {

            var fuse = (Viva.Fuse)target;
            var pSys = (ParticleSystem)fusePSys.objectReferenceValue;

            SerializedProperty fusePath = sObj.FindProperty("localFusePath");
            fusePath.arraySize = fusePath.arraySize + 1;
            fusePath.GetArrayElementAtIndex(fusePath.arraySize - 1).vector3Value = fuse.transform.InverseTransformPoint(pSys.transform.position);
            sObj.ApplyModifiedProperties();
        }
    }
}