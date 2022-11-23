using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BlockFilterWindow : EditorWindow {
    public interface IProvider {
        Vector2 position { get; set; }
        void CreateComponentTree(List<Element> tree);
        bool GoToChild(Element element, bool addIfComponent);
    }

    public static readonly float DefaultWidth = 250f;
    public static readonly float DefaultHeight = 300f;

    #region BaseElements
    public class Element : IComparable {
        public int level;
        public GUIContent content;
        public string name => content.text;
        public int CompareTo(object o) {
            return name.CompareTo((o as Element).name);
        }
    }

    [Serializable]
    public class GroupElement : Element {
        public Vector2 scroll;
        public int selectedIndex;
        public bool WantsFocus { get; protected set; }
        public virtual bool ShouldDisable => false;
        
        public GroupElement(int level, string name) {
            this.level = level;
            content = new GUIContent(name);
        }

        public virtual bool HandleKeyboard(Event evt, BlockFilterWindow window, Action goToParent) {
            return false;
        }

        public virtual bool OnGUI(BlockFilterWindow sFilterWindow) {
            return false;
        }
    }

    #endregion

    // Styles

    class Styles {
        public GUIStyle header = (GUIStyle)typeof(EditorStyles).GetProperty("inspectorBig", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
        public GUIStyle componentButton = new GUIStyle("PR Label");
        public GUIStyle groupButton;
        public GUIStyle background = "grey_border";
        public GUIStyle rightArrow = "AC RightArrow";
        public GUIStyle leftArrow = "AC LeftArrow";

        public Styles() {
            header.font = EditorStyles.boldLabel.font;
            componentButton.alignment = TextAnchor.MiddleLeft;
            componentButton.fixedHeight = 20;
            componentButton.imagePosition = ImagePosition.ImageAbove;
            groupButton = new GUIStyle(componentButton);
        }
    }

    const int kHeaderHeight = 30;
    const int kWindowHeight = 400 - 80;
    const int kHelpHeight = 80 * 0;
    const string kComponentSearch = "NodeSearchString";

    static Styles sStyles;
    static BlockFilterWindow sFilterWindow;
    static long sLastClosedTime;
    static bool sDirtyList;

    private IProvider provider;
    private Element[] tree;
    private Element[] searchResultTree;
    private List<GroupElement> stack = new List<GroupElement>();

    private float anim = 1;
    private int animTarget = 1;
    private long lastTime;
    private bool scrollToSelected;
    private string delayedSearch;
    private string search = "";

    private bool hasSearch => !string.IsNullOrEmpty(search);
    private GroupElement activeParent => stack[stack.Count - 2 + animTarget];
    private Element[] activeTree => hasSearch ? searchResultTree : tree;
    private Element activeElement {
        get {
            if (activeTree == null) {
                return null;
            }

            var children = GetChildren(activeTree, activeParent);
            return children.Count == 0
                ? null
                : children[activeParent.selectedIndex];
        }
    }
    private bool isAnimating { get { return !Mathf.Approximately(anim, animTarget); } }

    static BlockFilterWindow() {
        sDirtyList = true;
    }

    private void OnEnable() {
        sFilterWindow = this;
        search = "";
    }

    private void OnDisable() {
        sLastClosedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        sFilterWindow = null;
    }

    private void OnLostFocus() => Close();

    internal static bool Show(Vector2 position, IProvider provider) {
        // If the window is already open, close it instead.
        var wins = Resources.FindObjectsOfTypeAll(typeof(BlockFilterWindow));
        if (wins.Length > 0) {
            try {
                ((EditorWindow)wins[0]).Close();
                return false;
            } catch (Exception) {
                sFilterWindow = null;
            }
        }

        long nowMilliSeconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        bool justClosed = nowMilliSeconds < sLastClosedTime + 50;

        if (!justClosed) {
            Event.current.Use();

            if (sFilterWindow == null) {
                sFilterWindow = CreateInstance<BlockFilterWindow>();
                sFilterWindow.hideFlags = HideFlags.HideAndDontSave;
            }

            sFilterWindow.Init(position, provider);
            return true;
        }
        return false;
    }

    static object Invoke(Type t, object inst, string method, params object[] args) {
        var flags = (inst == null ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.NonPublic;
        var mi = t.GetMethod(method, flags);
        return mi.Invoke(inst, args);
    }

    private void Init(Vector2 position, IProvider provider) {
        this.provider = provider;
        // Has to be done before calling Show / ShowWithMode
        this.provider.position = position;
        position = GUIUtility.GUIToScreenPoint(position);
        var buttonRect = new Rect(position.x - DefaultWidth / 2, position.y - 16, DefaultWidth, 1);

        CreateComponentTree();

        ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, kWindowHeight));

        Focus();

        wantsMouseMove = true;
    }

    private void CreateComponentTree() {
        var tree = new List<Element>();
        provider.CreateComponentTree(tree);

        this.tree = tree.ToArray();

        if (stack.Count == 0) {
            stack.Add(this.tree[0] as GroupElement);
        } else {
            // The root is always the match for level 0
            var match = this.tree[0] as GroupElement;
            int level = 0;
            while (true)
            {
                // Assign the match for the current level
                var oldElement = stack[level];
                stack[level] = match;
                stack[level].selectedIndex = oldElement.selectedIndex;
                stack[level].scroll = oldElement.scroll;

                // See if we reached last element of stack
                level++;
                if (level == stack.Count) {
                    break;
                }

                // Try to find a child of the same name as we had before
                var children = GetChildren(activeTree, match);
                var childMatch = children.FirstOrDefault(c => c.name == stack[level].name);

                if (childMatch is GroupElement) {
                    match = childMatch as GroupElement;
                } else {
                    // If we couldn't find the child, remove all further elements from the stack
                    while (stack.Count > level) {
                        stack.RemoveAt(level);
                    }
                }
            }
        }

        sDirtyList = false;
        RebuildSearch();
    }

    internal void OnGUI() {
        // Avoids errors in the console if a domain reload is triggered while the filter window
        // is opened
        if (provider == null)
            return;

        if (sStyles == null)
            sStyles = new Styles();

        GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, sStyles.background);

        if (sDirtyList)
            CreateComponentTree();

        // Keyboard
        HandleKeyboard();

        GUILayout.Space(7);

        // Search
        if (!activeParent.WantsFocus)
        {
            EditorGUI.FocusTextInControl("ComponentSearch");
        }

        var searchRect = GUILayoutUtility.GetRect(10, 20);
        searchRect.x += 8;
        searchRect.width -= 16;

        GUI.SetNextControlName("ComponentSearch");

        using (new EditorGUI.DisabledScope(activeParent.ShouldDisable))
        {
            string newSearch = (string)Invoke(typeof(EditorGUI), null, "SearchField", searchRect, delayedSearch ?? search);

            if (newSearch != search || delayedSearch != null)
            {
                if (!isAnimating)
                {
                    search = delayedSearch ?? newSearch;
                    EditorPrefs.SetString(kComponentSearch, search);
                    RebuildSearch();
                    delayedSearch = null;
                }
                else
                {
                    delayedSearch = newSearch;
                }
            }
        }

        // Show lists
        ListGUI(activeTree, anim, GetElementRelative(0), GetElementRelative(-1));
        if (anim < 1)
            ListGUI(activeTree, anim + 1, GetElementRelative(-1), GetElementRelative(-2));

        // Animate
        if (isAnimating && Event.current.type == EventType.Repaint)
        {
            long now = DateTime.Now.Ticks;
            float deltaTime = (now - lastTime) / (float)TimeSpan.TicksPerSecond;
            lastTime = now;
            anim = Mathf.MoveTowards(anim, animTarget, deltaTime * 4);

            if (animTarget == 0 && Mathf.Approximately(anim, 0))
            {
                anim = 1;
                animTarget = 1;
                stack.RemoveAt(stack.Count - 1);
            }

            Repaint();
        }
    }

    void HandleKeyboard()
    {
        var evt = Event.current;

        if (evt.type == EventType.KeyDown)
        {
            // Special handling when in new script panel
            if (!activeParent.HandleKeyboard(evt, sFilterWindow, GoToParent))
            {
                // Always do these
                if (evt.keyCode == KeyCode.DownArrow)
                {
                    activeParent.selectedIndex++;
                    activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex, GetChildren(activeTree, activeParent).Count - 1);
                    scrollToSelected = true;
                    evt.Use();
                }

                if (evt.keyCode == KeyCode.UpArrow)
                {
                    activeParent.selectedIndex--;
                    activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
                    scrollToSelected = true;
                    evt.Use();
                }

                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    if (activeElement != null)
                    {
                        GoToChild(activeElement, true);
                        evt.Use();
                    }
                }

                // Do these if we're not in search mode
                if (!hasSearch)
                {
                    if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.Backspace)
                    {
                        GoToParent();
                        evt.Use();
                    }

                    if (evt.keyCode == KeyCode.RightArrow)
                    {
                        GoToChild(activeElement, false);
                        evt.Use();
                    }

                    if (evt.keyCode == KeyCode.Escape)
                    {
                        Close();
                        evt.Use();
                    }
                }
            }
        }
    }

    const string k_SearchHeader = "Search";

    void RebuildSearch()
    {
        if (!hasSearch)
        {
            searchResultTree = null;

            if (stack[stack.Count - 1].name == k_SearchHeader)
            {
                stack.Clear();
                stack.Add(this.tree[0] as GroupElement);
            }

            animTarget = 1;
            lastTime = DateTime.Now.Ticks;
            return;
        }

        // Support multiple search words separated by spaces.
        var searchWords = search.ToLower().Split(' ');

        // We keep two lists. Matches that matches the start of an item always get first priority.
        var matchesStart = new List<Element>();
        var matchesWithin = new List<Element>();

        foreach (var e in this.tree)
        {
            if (e is GroupElement)
                continue;

            string name = e.name.ToLower().Replace(" ", "");
            bool didMatchAll = true;
            bool didMatchStart = false;

            // See if we match ALL the seaarch words.
            for (int w = 0; w < searchWords.Length; w++)
            {
                string search = searchWords[w];

                if (name.Contains(search))
                {
                    // If the start of the item matches the first search word, make a note of that.
                    if (w == 0 && name.StartsWith(search))
                        didMatchStart = true;
                }
                else
                {
                    // As soon as any word is not matched, we disregard this item.
                    didMatchAll = false;
                    break;
                }
            }

            // We always need to match all search words.
            // If we ALSO matched the start, this item gets priority.
            if (didMatchAll)
            {
                if (didMatchStart)
                    matchesStart.Add(e);
                else
                    matchesWithin.Add(e);
            }
        }

        matchesStart.Sort();
        matchesWithin.Sort();

        // Create search tree
        var tree = new List<Element>();

        // Add parent
        tree.Add(new GroupElement(0, k_SearchHeader));

        // Add search results
        tree.AddRange(matchesStart);
        tree.AddRange(matchesWithin);

        // Create search result tree
        searchResultTree = tree.ToArray();
        stack.Clear();
        stack.Add(searchResultTree[0] as GroupElement);

        // Always select the first search result when search is changed (e.g. a character was typed in or deleted),
        // because it's usually the best match.
        if (GetChildren(activeTree, activeParent).Count >= 1)
            activeParent.selectedIndex = 0;
        else
            activeParent.selectedIndex = -1;
    }

    GroupElement GetElementRelative(int rel)
    {
        int i = stack.Count + rel - 1;

        if (i < 0)
            return null;

        return stack[i];
    }

    void GoToParent()
    {
        if (stack.Count > 1)
        {
            animTarget = 0;
            lastTime = DateTime.Now.Ticks;
        }
    }

    void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent)
    {
        // Smooth the fractional part of the anim value
        anim = Mathf.Floor(anim) + Mathf.SmoothStep(0, 1, Mathf.Repeat(anim, 1));

        // Calculate rect for animated area
        var animRect = position;
        animRect.x = position.width * (1 - anim) + 1;
        animRect.y = kHeaderHeight;
        animRect.height -= kHeaderHeight + kHelpHeight;
        animRect.width -= 2;

        // Start of animated area (the part that moves left and right)
        GUILayout.BeginArea(animRect);

        // Header
        var headerRect = GUILayoutUtility.GetRect(10, 25);
        string name = parent.name;
        GUI.Label(headerRect, name, sStyles.header);

        // Back button
        if (grandParent != null)
        {
            var arrowRect = new Rect(headerRect.x + 4, headerRect.y + 7, 13, 13);
            var e = Event.current;

            if (e.type == EventType.Repaint)
                sStyles.leftArrow.Draw(arrowRect, false, false, false, false);

            if (e.type == EventType.MouseDown && headerRect.Contains(e.mousePosition))
            {
                GoToParent();
                e.Use();
            }
        }

        if (!parent.OnGUI(sFilterWindow))
            ListGUI(tree, parent);

        GUILayout.EndArea();
    }

    void GoToChild(Element e, bool addIfComponent)
    {
        if (provider.GoToChild(e, addIfComponent))
        {
            Close();
        }
        else if (!hasSearch)
        {
            lastTime = DateTime.Now.Ticks;

            if (animTarget == 0)
            {
                animTarget = 1;
            }
            else if (Mathf.Approximately(anim, 1f))
            {
                anim = 0;
                stack.Add(e as GroupElement);
            }
        }
    }

    void ListGUI(Element[] tree, GroupElement parent)
    {
        // Start of scroll view list
        parent.scroll = GUILayout.BeginScrollView(parent.scroll);

        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        var children = GetChildren(tree, parent);
        var selectedRect = new Rect();
        var evt = Event.current;

        // Iterate through the children
        for (int i = 0; i < children.Count; i++)
        {
            var e = children[i];
            var r = GUILayoutUtility.GetRect(16, 20, GUILayout.ExpandWidth(true));

            // Select the element the mouse cursor is over.
            // Only do it on mouse move - keyboard controls are allowed to overwrite this until the next time the mouse moves.
            if (evt.type == EventType.MouseMove || evt.type == EventType.MouseDown)
            {
                if (parent.selectedIndex != i && r.Contains(evt.mousePosition))
                {
                    parent.selectedIndex = i;
                    Repaint();
                }
            }

            bool selected = false;

            // Handle selected item
            if (i == parent.selectedIndex)
            {
                selected = true;
                selectedRect = r;
            }

            // Draw element
            if (evt.type == EventType.Repaint)
            {
                var labelStyle = (e is GroupElement) ? sStyles.groupButton : sStyles.componentButton;
                labelStyle.Draw(r, e.content, false, false, selected, selected);

                if (e is GroupElement)
                {
                    var arrowRect = new Rect(r.x + r.width - 13, r.y + 4, 13, 13);
                    sStyles.rightArrow.Draw(arrowRect, false, false, false, false);
                }
            }

            if (evt.type == EventType.MouseDown && r.Contains(evt.mousePosition))
            {
                evt.Use();
                parent.selectedIndex = i;
                GoToChild(e, true);
            }
        }

        EditorGUIUtility.SetIconSize(Vector2.zero);

        GUILayout.EndScrollView();

        // Scroll to show selected
        if (scrollToSelected && evt.type == EventType.Repaint)
        {
            scrollToSelected = false;
            var scrollRect = GUILayoutUtility.GetLastRect();

            if (selectedRect.yMax - scrollRect.height > parent.scroll.y)
            {
                parent.scroll.y = selectedRect.yMax - scrollRect.height;
                Repaint();
            }

            if (selectedRect.y < parent.scroll.y)
            {
                parent.scroll.y = selectedRect.y;
                Repaint();
            }
        }
    }

    List<Element> GetChildren(Element[] tree, Element parent)
    {
        var children = new List<Element>();
        int level = -1;
        int i;

        for (i = 0; i < tree.Length; i++)
        {
            if (tree[i] == parent)
            {
                level = parent.level + 1;
                i++;
                break;
            }
        }

        if (level == -1)
            return children;

        for (; i < tree.Length; i++)
        {
            var e = tree[i];

            if (e.level < level)
                break;

            if (e.level > level && !hasSearch)
                continue;

            children.Add(e);
        }

        return children;
    }
}
