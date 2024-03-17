using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public static class EditorExtensionMethods
    {
        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="key"></param>
        /// <param name="layerMask"></param>
        /// <param name="editorUtils"></param>
        /// <param name="helpSwitch"></param>
        /// <returns></returns>
        public static LayerMask LayerMaskField(this EditorUtils editorUtils, string key, LayerMask layerMask, bool helpSwitch)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = editorUtils.MaskField(key, maskWithoutEmpty, layers.ToArray(), helpSwitch);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }
        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="editorUtils"></param>
        /// <param name="key"></param>
        /// <param name="property"></param>
        /// <param name="helpSwitch"></param>
        /// <returns></returns>
        public static void LayerMaskField(this EditorUtils editorUtils, string key, SerializedProperty property, bool helpSwitch)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, editorUtils.GetContent(key), property);
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & property.intValue) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUI.MaskField(rect, label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            property.intValue = mask;
            EditorGUI.EndProperty();
            editorUtils.InlineHelp(key, helpSwitch);
        }
    }
}