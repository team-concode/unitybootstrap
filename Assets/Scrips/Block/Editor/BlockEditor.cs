using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Assertions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class BlockEditorAttribute : Attribute {
    public readonly Type componentType;

    public BlockEditorAttribute(Type componentType) {
        this.componentType = componentType;
    }
}

public class BlockEditor<TBlock> where TBlock : Block {
    public TBlock target { get; private set; }
    public SerializedObject serializedObject { get; private set; }
    public SerializedProperty baseProperty { get; internal set; }
    
    protected Editor inspector;
    private List<(GUIContent displayName, int displayOrder, BlockDataParameter param)> parameters;
    private readonly Dictionary<Type, BlockParameterDrawer> parameterDrawers;

    public BlockEditor() {
        parameterDrawers = new Dictionary<Type, BlockParameterDrawer>();
        ReloadDecoratorTypes();
    }
    
    public void ReloadDecoratorTypes() {
        parameterDrawers.Clear();

        var types = BlockUtils.GetAllTypesDerivedFrom<BlockParameterDrawer>()
            .Where(t => t.IsDefined(typeof(BlockParameterDrawerAttribute), false) && !t.IsAbstract);

        foreach (var type in types) {
            var attr = (BlockParameterDrawerAttribute)type.GetCustomAttributes(typeof(BlockParameterDrawerAttribute), false)[0];
            var decorator = (BlockParameterDrawer)Activator.CreateInstance(type);
            parameterDrawers.Add(attr.parameterType, decorator);
        }
    }

    public void Repaint() {
        inspector.Repaint();
    }

    internal void Init(TBlock target, Editor inspector) {
        this.target = target;
        this.inspector = inspector;
        serializedObject = new SerializedObject(target);
        OnEnable();
    }

    class ParameterSorter : Comparer<(GUIContent displayName, int displayOrder, BlockDataParameter param)> {
        public override int Compare((GUIContent displayName, int displayOrder, BlockDataParameter param) x, (GUIContent displayName, int displayOrder, BlockDataParameter param) y) {
            if (x.displayOrder < y.displayOrder) {
                return -1;
            }
            if (x.displayOrder == y.displayOrder) {
                return 0;
            }
            return 1;
        }
    }

    public virtual void OnEnable() {
        parameters = new List<(GUIContent, int, BlockDataParameter)>();
        var fields = target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => t.FieldType.IsSubclassOf(typeof(BlockParameter)))
            .Where(t =>
                (t.IsPublic && t.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0) ||
                (t.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
                )
            .Where(t => t.GetCustomAttributes(typeof(HideInInspector), false).Length == 0)
            .ToList();

        foreach (var field in fields) {
            var property = serializedObject.FindProperty(field.Name);
            var name = "";
            var order = 0;
            var attr = (BlockDisplayInfoAttribute[])field.GetCustomAttributes(typeof(BlockDisplayInfoAttribute), true);
            if (attr.Length != 0) {
                name = attr[0].name;
                order = attr[0].order;
            }

            var parameter = new BlockDataParameter(property);
            parameters.Add((new GUIContent(name), order, parameter));
        }
        parameters.Sort(new ParameterSorter());
    }

    public virtual void OnDisable() {
    }

    internal void OnInternalInspectorGUI() {
        serializedObject.Update();
        OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }

    public virtual void OnInspectorGUI() {
        foreach (var parameter in parameters) {
            if (parameter.displayName.text != "") {
                PropertyField(parameter.param, parameter.displayName);
            } else {
                PropertyField(parameter.param);
            }
        }
    }

    public virtual string GetDisplayTitle() {
        return ObjectNames.NicifyVariableName(target.GetType().Name);
    }

    protected BlockDataParameter Unpack(SerializedProperty property) {
        Assert.IsNotNull(property);
        return new BlockDataParameter(property);
    }

    protected void PropertyField(BlockDataParameter property) {
        var title = EditorGUIUtility.TrTextContent(property.displayName, property.GetAttribute<TooltipAttribute>()?.tooltip);
        PropertyField(property, title);
    }

    protected void PropertyField(BlockDataParameter property, GUIContent title) {
        foreach (var attr in property.attributes) {
            if (attr is PropertyAttribute) {
                if (attr is SpaceAttribute) {
                    EditorGUILayout.GetControlRect(false, (attr as SpaceAttribute).height);
                } else if (attr is HeaderAttribute) {
                    var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                    rect.y += 0f;
                    rect = EditorGUI.IndentedRect(rect);
                    EditorGUI.LabelField(rect, (attr as HeaderAttribute).header, EditorStyles.miniLabel);
                } else if (attr is TooltipAttribute) {
                    if (string.IsNullOrEmpty(title.tooltip)) {
                        title.tooltip = (attr as TooltipAttribute).tooltip;
                    }
                }
            }
        }

        // Custom parameter drawer
        BlockParameterDrawer drawer;
        parameterDrawers.TryGetValue(property.referenceType, out drawer);

        bool invalidProp = false;
        if (drawer != null && !drawer.IsAutoProperty()) {
            if (drawer.OnGUI(property, title)) {
                return;
            }

            invalidProp = true;
        }

        if (BlockParameter.IsObjectParameter(property.referenceType)) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property.value, title, true);
            EditorGUI.indentLevel--;
            return;
        }
        
        using (new EditorGUILayout.HorizontalScope()) {
            if (drawer != null && !invalidProp) {
                drawer.OnGUI(property, title);
            } else {
                if (property.value != null) {
                    EditorGUILayout.PropertyField(property.value, title, true);
                }
            }
        }
    }

    protected void DrawOverrideCheckbox(BlockDataParameter property) {
        var overrideRect = GUILayoutUtility.GetRect(17f, 17f, GUILayout.ExpandWidth(false));
        overrideRect.yMin += 4f;
        //property.overrideState.boolValue = GUI.Toggle(overrideRect, property.overrideState.boolValue, EditorGUIUtility.TrTextContent("", "Override this setting for this volume."), CoreEditorStyles.smallTickbox);
    }
}