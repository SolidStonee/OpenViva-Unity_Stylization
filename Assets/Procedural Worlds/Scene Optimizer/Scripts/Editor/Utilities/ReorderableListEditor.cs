using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class ReorderableListEditor<T>
    {
        public bool IsEmpty => m_reorderableList.count == 0;
        public ReorderableList m_reorderableList;
        public Action<int> OnSelectionChangedEvent;
        public Action OnChanged;
        public void Create(List<T> value, bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true, bool displayAddDropdown = false)
        {
            m_reorderableList = new ReorderableList(value, typeof(T), draggable, displayHeader, displayAddButton, displayRemoveButton);
            m_reorderableList.elementHeightCallback = OnElementHeightExtensionListEntry;
            m_reorderableList.drawElementCallback = DrawListElement;
            m_reorderableList.drawHeaderCallback = DrawListHeader;
            m_reorderableList.onAddCallback = AddListEntry;
            m_reorderableList.onRemoveCallback = OnRemoveListEntry;
            m_reorderableList.onReorderCallback = OnReorderList;
            m_reorderableList.onSelectCallback = OnSelectChanged;
            m_reorderableList.onChangedCallback = OnListChanged;
            if (displayAddDropdown)
            {
                m_reorderableList.onAddDropdownCallback = OnAddDropdownCallback;
            }
            OnCreate();
        }
        protected void OnListChanged(ReorderableList reorderableList)
        {
            OnChanged?.Invoke();
        }
        protected virtual void OnSelectChanged(ReorderableList reorderableList)
        {
            int index = reorderableList.index;
            OnSelectionChangedEvent?.Invoke(index);
            OnSelectionChanged(index);
        }
        protected virtual void OnSelectionChanged(int index)
        {
        }
        protected virtual void OnCreate()
        {
        }
        protected virtual void OnReorderList(ReorderableList reorderableList)
        {
            //Do nothing, changing the order does not immediately affect anything in the stamper
        }
        protected virtual void OnRemoveListEntry(ReorderableList reorderableList)
        {
            int indexToRemove = reorderableList.index;
            reorderableList.list.RemoveAt(indexToRemove);
            if (indexToRemove >= reorderableList.list.Count)
                indexToRemove = reorderableList.list.Count - 1;
            reorderableList.index = indexToRemove;
        }
        protected virtual void AddListEntry(ReorderableList reorderableList)
        {
            // TODO : Manny : Pop up a dialogue!
            T value = Activator.CreateInstance<T>();
            reorderableList.list.Add(value);
        }
        protected virtual void OnAddDropdownCallback(Rect rect, ReorderableList list)
        {
            
        }
        protected virtual void OnAddListEntry(T entry)
        {
        }
        protected virtual void DrawListHeader(Rect rect)
        {
        }
        protected virtual void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            T entry = (T)m_reorderableList.list[index];
            if (entry == null)
                return;
            DrawListElement(rect, entry, isFocused);
        }
        protected virtual void DrawListElement(Rect rect, T entry, bool isFocused)
        {
        }
        protected virtual float OnElementHeightExtensionListEntry(int index)
        {
            return OnElementHeight();
        }
        protected virtual float OnElementHeight()
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
        public virtual void DrawList()
        {
            if (m_reorderableList == null)
                return;
            Rect maskRect = EditorGUILayout.GetControlRect(true, m_reorderableList.GetHeight());
            m_reorderableList.DoList(maskRect);
        }
    }
}