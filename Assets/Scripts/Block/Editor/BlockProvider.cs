using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlockProvider<TBlock, TMenu> : BlockFilterWindow.IProvider where TBlock : Block where TMenu : BlockMenu {
    private class Element : BlockFilterWindow.Element {
        public Type type;

        public Element(int level, string label, Type type) {
            this.level = level;
            this.type = type;
            content = new GUIContent(label);
        }
    }

    private class PathNode : IComparable<PathNode> {
        public List<PathNode> nodes =  new List<PathNode>();
        public string name;
        public Type type;

        public int CompareTo(PathNode other) {
            return name.CompareTo(other.name);
        }
    }

    public Vector2 position { get; set; }
    private BlockProfile<TBlock> target;
    private BlockListEditor<TBlock, TMenu> targetEditor;

    public BlockProvider(BlockProfile<TBlock> target, BlockListEditor<TBlock, TMenu> targetEditor) {
        this.target = target;
        this.targetEditor = targetEditor;
    }

    public void CreateComponentTree(List<BlockFilterWindow.Element> tree) {
        tree.Add(new BlockFilterWindow.GroupElement(0, "Area Paint Component"));

        var types = BlockUtils.GetAllTypesDerivedFrom<TBlock>()
            .Where(t => !t.IsAbstract);
        var rootNode = new PathNode();

        foreach (var t in types) {
            string path = string.Empty;
            var attrs = t.GetCustomAttributes(false);
            foreach (var attr in attrs) {
                var attrMenu = attr as TMenu;
                if (attrMenu != null)
                    path = attrMenu.menu;
            }

            if (string.IsNullOrEmpty(path)) {
                path = ObjectNames.NicifyVariableName(t.Name);
            }

            AddNode(rootNode, path, t);
        }

        Traverse(rootNode, 1, tree);
    }

    public bool GoToChild(BlockFilterWindow.Element element, bool addIfComponent) {
        if (element is Element) {
            var e = (Element)element;
            targetEditor.AddComponent(e.type);
            return true;
        }

        return false;
    }

    private void AddNode(PathNode root, string path, Type type) {
        var current = root;
        var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts) {
            var child = current.nodes.Find(x => x.name == part);

            if (child == null) {
                child = new PathNode { name = part, type = type };
                current.nodes.Add(child);
            }

            current = child;
        }
    }

    private void Traverse(PathNode node, int depth, List<BlockFilterWindow.Element> tree) {
        node.nodes.Sort();

        foreach (var n in node.nodes) {
            if (n.nodes.Count > 0) {
                tree.Add(new BlockFilterWindow.GroupElement(depth, n.name));
                Traverse(n, depth + 1, tree);
            } else  {
                tree.Add(new Element(depth, n.name, n.type));
            }
        }
    }
}
