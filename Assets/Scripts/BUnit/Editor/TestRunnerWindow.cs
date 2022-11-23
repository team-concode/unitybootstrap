using System.Collections.Generic;
using BUnit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TestRunnerWindow : EditorWindow {
    private static TestRunnerWindow window;
    private static string basePath = "Assets/Scripts/BUnit/Editor/UIElements/";

    public class UITestItem {
        public Label result { get; }
        public Label method { get; }

        public UITestItem(Label result, Label method) {
            this.result = result;
            this.method = method;
        }
    }

    private Label summaryText;
    private Dictionary<string, UITestItem> uiItems = new Dictionary<string, UITestItem>();

    [MenuItem ("BUnit/Test Runner")]
    public static void Init() {
        window = (TestRunnerWindow)GetWindow(typeof(TestRunnerWindow));
        window.titleContent = new GUIContent("BUnit Test Runner");
    }

    private VisualElement LoadUxmlToRoot(string path, string styleSheetPath = null)  {
        VisualElement root = this.rootVisualElement; 
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);  
        var loadedVisualElement = uxml.CloneTree(); 
        root.Add(loadedVisualElement);  

        if(styleSheetPath != null) { 
            LoadUss(styleSheetPath, loadedVisualElement);
        }
        
        return loadedVisualElement;
    }

    private void LoadUss(string path, VisualElement target)  { 
        var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(path); 
        target.styleSheets.Add(uss);   
    }

    public void OnEnable()  {
        VisualElement root = this.rootVisualElement; 
        var rootWindow = LoadUxmlToRoot(basePath + "TestRunner.uxml", basePath + "TestRunner.uss");
        summaryText = rootWindow.Q<Label>(className : "Summary");
        rootWindow.Q<Button>(className : "Run").RegisterCallback(new EventCallback<MouseUpEvent>(target => Run()));
        

        var bodyElement = rootWindow.Q<VisualElement>(className : "TestItemBox");
        var res = BUnit.TestRunner.GetAllTests();
        foreach (var item in res) {
            foreach (var test in item.Value) {
                var testItem = LoadUxmlToRoot(basePath + "TestItem.uxml", basePath + "TestItem.uss");
                bodyElement.Add(testItem);

                var result = testItem.Q<Label>(className: "Result");
                var method = testItem.Q<Label>(className: "Method");
                var name = item.Key.name + "::" + test.method.Name;
                method.text = name;
                result.text = "Ready";
                
                uiItems.Add(name, new UITestItem(result, method));
            }
        }
    }

    private void Run() {
        Assert.doneCount = 0;
        foreach (var item in uiItems.Values) {
            item.result.text = "Ready";
            item.result.style.backgroundColor = Color.gray;
        }

        var res = BUnit.TestRunner.Run();
        foreach (var item in res.Result) {
            if (uiItems.TryGetValue(item.name, out UITestItem uiItem)) {
                uiItem.result.text = item.runRes.ToString();

                if (item.runRes == TestRunResult.Success) {
                    uiItem.result.style.backgroundColor = Color.green;
                } else {
                    uiItem.result.style.backgroundColor = Color.red;
                }
            }
        }

        summaryText.text = Assert.doneCount + " cases tested!";
    }
}