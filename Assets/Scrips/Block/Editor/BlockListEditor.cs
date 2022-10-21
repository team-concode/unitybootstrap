using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Assertions;

public class BlockListEditor<TBlock, TMenu> where TBlock : Block where TMenu : BlockMenu {
    public BlockProfile<TBlock> asset { get; private set; }

    private Editor baseEditor;
    private SerializedObject serializedObject;
    private SerializedProperty componentsProperty;
    private Dictionary<Type, Type> editorTypes;
    private BlockEditor<Block> rootEditor;
    private List<BlockEditor<TBlock>> editors;
    private int currentHashCode;

    public BlockListEditor(Editor editor) {
        Assert.IsNotNull(editor);
        baseEditor = editor;
    }

    public void Init(BlockProfile<TBlock> asset, SerializedObject serializedObject) {
        Assert.IsNotNull(asset);
        Assert.IsNotNull(serializedObject);

        this.asset = asset;
        this.serializedObject = serializedObject;
        //componentsProperty = serializedObject.Find((WdAreaProfile x) => x.blocks);
        Assert.IsNotNull(componentsProperty);

        this.editorTypes = new Dictionary<Type, Type>();
        rootEditor = new BlockEditor<Block>();
        editors = new List<BlockEditor<TBlock>>();

        var types = BlockUtils.GetAllTypesDerivedFrom<BlockEditor<TBlock>>()
            .Where(t => t.IsDefined(typeof(BlockEditorAttribute), false) && !t.IsAbstract);

        foreach (var editorType in types) {
            var attribute = (BlockEditorAttribute)editorType.GetCustomAttributes(typeof(BlockEditorAttribute), false)[0];
            this.editorTypes.Add(attribute.componentType, editorType);
        }

        var components = asset.blocks;
        for (int i = 0; i < components.Count; i++) {
            CreateEditor(components[i], componentsProperty.GetArrayElementAtIndex(i));
        }

        rootEditor.Init(asset, baseEditor);
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed() {
        asset.isDirty = true;

        if (serializedObject != null
             && !serializedObject.Equals(null)
             && serializedObject.targetObject != null
             && !serializedObject.targetObject.Equals(null))
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        baseEditor.Repaint();
    }

    private void CreateEditor(TBlock component, SerializedProperty property, int index = -1, bool forceOpen = false) {
        var componentType = component.GetType();
        if (!editorTypes.TryGetValue(componentType, out var editorType)) {
            editorType = typeof(BlockEditor<TBlock>);
        }

        var editor = (BlockEditor<TBlock>)Activator.CreateInstance(editorType);
        editor.Init(component, baseEditor);
        editor.baseProperty = property.Copy();

        if (forceOpen) {
            editor.baseProperty.isExpanded = true;
        }

        if (index < 0) {
            editors.Add(editor);
        } else {
            editors[index] = editor;
        }
    }

    private void RefreshEditors() {
        foreach (var editor in editors) {
            editor.OnDisable();
        }
        
        editors.Clear();

        serializedObject.Update();
        //componentsProperty = serializedObject.Find((WdAreaProfile x) => x.blocks);
        Assert.IsNotNull(componentsProperty);

        var components = asset.blocks;
        for (int i = 0; i < components.Count; i++) {
            CreateEditor(components[i], componentsProperty.GetArrayElementAtIndex(i));
        }
    }

    public void Clear() {
        if (editors == null) {
            return;
        }

        rootEditor.OnDisable();
        foreach (var editor in editors) {
            editor.OnDisable();
        }

        editors.Clear();
        editorTypes.Clear();

        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    public void OnGUI() {
        if (asset == null) {
            return;
        }
        
        if (asset.isDirty || asset.GetComponentListHashCode() != currentHashCode) {
            RefreshEditors();
            currentHashCode = asset.GetComponentListHashCode();
            asset.isDirty = false;
        }
        
        rootEditor.OnInternalInspectorGUI();

        bool isEditable = AssetDatabase.IsOpenForEdit(asset, StatusQueryOptions.UseCachedIfPossible);
        using (new EditorGUI.DisabledScope(!isEditable)) {
            for (int i = 0; i < editors.Count; i++) {
                var editor = editors[i];
                string title = editor.GetDisplayTitle();
                int id = i; // Needed for closure capture below

                CoreEditorUtils.DrawSplitter();
                bool displayContent = BlockEditorUtils.DrawHeader(
                        EditorGUIUtility.TrTextContent(title),
                        editor.baseProperty,
                        pos => OnContextClick(pos, editor.target, id));

                if (displayContent) {
                    editor.OnInternalInspectorGUI();
                }
            }

            if (editors.Count > 0) {
                CoreEditorUtils.DrawSplitter();
            } else {
                EditorGUILayout.HelpBox("This Area Profile contains no component.", MessageType.Info);
            }

            EditorGUILayout.Space();
            using (var hscope = new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button(EditorGUIUtility.TrTextContent("Add Component"), EditorStyles.miniButton)) {
                    var r = hscope.rect;
                    var pos = new Vector2(r.x + r.width / 2f, r.yMax + 18f);
                    var provider = new BlockProvider<TBlock, TMenu>(asset, this);
                    BlockFilterWindow.Show(pos, provider);
                }
            }
        }
    }

    private void OnContextClick(Vector2 position, TBlock targetComponent, int id) {
        var menu = new GenericMenu();

        if (id == 0) {
            menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Move Up"));
            menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Move to Top"));
        } else {
            menu.AddItem(EditorGUIUtility.TrTextContent("Move to Top"), false, () => MoveComponent(id, -id));
            menu.AddItem(EditorGUIUtility.TrTextContent("Move Up"), false, () => MoveComponent(id, -1));
        }

        if (id == editors.Count - 1) {
            menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Move to Bottom"));
            menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Move Down"));
        } else {
            menu.AddItem(EditorGUIUtility.TrTextContent("Move to Bottom"), false, () => MoveComponent(id, (editors.Count -1) - id));
            menu.AddItem(EditorGUIUtility.TrTextContent("Move Down"), false, () => MoveComponent(id, 1));
        }

        menu.AddSeparator(string.Empty);
        menu.AddItem(EditorGUIUtility.TrTextContent("Collapse All"), false, () => CollapseComponents());
        menu.AddItem(EditorGUIUtility.TrTextContent("Expand All"), false, () => ExpandComponents());
        menu.AddSeparator(string.Empty);
        menu.AddItem(EditorGUIUtility.TrTextContent("Reset"), false, () => ResetComponent(targetComponent.GetType(), id));
        menu.AddItem(EditorGUIUtility.TrTextContent("Remove"), false, () => RemoveComponent(id));
        menu.AddSeparator(string.Empty);
        menu.AddItem(EditorGUIUtility.TrTextContent("Copy Settings"), false, () => CopySettings(targetComponent));

        if (CanPaste(targetComponent)) {
            menu.AddItem(EditorGUIUtility.TrTextContent("Paste Settings"), false, () => PasteSettings(targetComponent));
        } else {
            menu.AddDisabledItem(EditorGUIUtility.TrTextContent("Paste Settings"));
        }

        //menu.AddSeparator(string.Empty);
        //menu.AddItem(EditorGUIUtility.TrTextContent("Toggle All"), false, () => editors[id].SetAllOverridesTo(true));
        //menu.AddItem(EditorGUIUtility.TrTextContent("Toggle None"), false, () => editors[id].SetAllOverridesTo(false));
        menu.DropDown(new Rect(position, Vector2.zero));
    }

    private TBlock CreateNewComponent(Type type) {
        var effect = (TBlock)ScriptableObject.CreateInstance(type);
        effect.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        effect.name = type.Name;
        return effect;
    }

    internal void AddComponent(Type type) {
        serializedObject.Update();

        var component = CreateNewComponent(type);
        Undo.RegisterCreatedObjectUndo(component, "Add Ap Component");

        if (EditorUtility.IsPersistent(asset)) {
            AssetDatabase.AddObjectToAsset(component, asset);
        }

        componentsProperty.arraySize++;
        var componentProp = componentsProperty.GetArrayElementAtIndex(componentsProperty.arraySize - 1);
        componentProp.objectReferenceValue = component;

        if (EditorUtility.IsPersistent(asset)) {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        CreateEditor(component, componentProp, forceOpen: true);
        serializedObject.ApplyModifiedProperties();
    }

    internal void RemoveComponent(int id) {
        bool nextFoldoutState = false;
        if (id < editors.Count - 1) {
            nextFoldoutState = editors[id + 1].baseProperty.isExpanded;
        }

        editors[id].OnDisable();
        editors.RemoveAt(id);
        serializedObject.Update();

        var property = componentsProperty.GetArrayElementAtIndex(id);
        var component = property.objectReferenceValue;

        property.objectReferenceValue = null;
        componentsProperty.DeleteArrayElementAtIndex(id);

        for (var i = 0; i < editors.Count; i++) {
            editors[i].baseProperty = componentsProperty.GetArrayElementAtIndex(i).Copy();
        }

        if (id < editors.Count) {
            editors[id].baseProperty.isExpanded = nextFoldoutState;
        }

        serializedObject.ApplyModifiedProperties();

        Undo.DestroyObjectImmediate(component);

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    internal void ResetComponent(Type type, int id) {
        editors[id].OnDisable();
        editors[id] = null;
        serializedObject.Update();

        var property = componentsProperty.GetArrayElementAtIndex(id);
        var prevComponent = property.objectReferenceValue;
        property.objectReferenceValue = null;

        var newComponent = CreateNewComponent(type);
        Undo.RegisterCreatedObjectUndo(newComponent, "Reset Ap Components");

        AssetDatabase.AddObjectToAsset(newComponent, asset);
        property.objectReferenceValue = newComponent;
        CreateEditor(newComponent, property, id);
        serializedObject.ApplyModifiedProperties();
        Undo.DestroyObjectImmediate(prevComponent);

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    internal void MoveComponent(int id, int offset) {
        serializedObject.Update();
        componentsProperty.MoveArrayElement(id, id + offset);
        serializedObject.ApplyModifiedProperties();

        bool targetExpanded = editors[id + offset].baseProperty.isExpanded;
        bool sourceExpanded = editors[id].baseProperty.isExpanded;

        var prev = editors[id + offset];
        editors[id + offset] = editors[id];
        editors[id] = prev;

        editors[id + offset].baseProperty.isExpanded = targetExpanded;
        editors[id].baseProperty.isExpanded = sourceExpanded;
    }

    internal void CollapseComponents() {
        serializedObject.Update();
        int numEditors = editors.Count;
        for (var i = 0; i < numEditors; ++i) {
            editors[i].baseProperty.isExpanded = false;
        }
        serializedObject.ApplyModifiedProperties();
    }

    internal void ExpandComponents() {
        serializedObject.Update();
        int numEditors = editors.Count;
        for (var i = 0; i < numEditors; ++i) {
            editors[i].baseProperty.isExpanded = true;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private bool CanPaste(TBlock targetComponent) {
        if (string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer)) {
            return false;
        }
        
        var clipboard = EditorGUIUtility.systemCopyBuffer;
        var separator = clipboard.IndexOf('|');
        if (separator < 0) {
            return false;
        }

        return targetComponent.GetType().AssemblyQualifiedName == clipboard.Substring(0, separator);
    }

    private void CopySettings(TBlock targetComponent) {
        string typeName = targetComponent.GetType().AssemblyQualifiedName;
        string typeData = JsonUtility.ToJson(targetComponent);
        EditorGUIUtility.systemCopyBuffer = $"{typeName}|{typeData}";
    }

    private void PasteSettings(TBlock targetComponent) {
        var clipboard = EditorGUIUtility.systemCopyBuffer;
        var typeData = clipboard.Substring(clipboard.IndexOf('|') + 1);
        Undo.RecordObject(targetComponent, "Paste Settings");
        JsonUtility.FromJsonOverwrite(typeData, targetComponent);
    }

    public void Refresh() {
        foreach (var item in editors) {
            item.ReloadDecoratorTypes();
        }
    }
}