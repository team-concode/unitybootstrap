using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace BUnit {
    public enum TestRunResult {
        Success,
        Failure,
        Exception
    }

    public class TestResult {
        public string name { get; private set; }
        public TestRunResult runRes { get; private set; }

        public static TestResult Build(string name, TestRunResult runResult) {
            var res = new TestResult();
            res.runRes = runResult;
            res.name = name;
            return res;
        }
    }
    
    public class TestClass {
        public string name { get; }
        public object target { get; }
        public MethodInfo setUp { get; set; }
        public MethodInfo setUpClass { get; set; }
        public MethodInfo tearDown { get; set; }
        public MethodInfo tearDownClass { get; set; }

        public TestClass(string name, object target) {
            this.name = name;
            this.target = target;
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != typeof(TestClass)) {
                return false;
            }

            return ((TestClass) obj).name == name;
        }

        public override int GetHashCode() {
            return name.GetHashCode();
        }
    }
    
    
    public static class TestRunner {
        public class TestItem {
            public MethodInfo method { get; }

            public TestItem(MethodInfo method) {
                this.method = method;
            }
        }

        public static Dictionary<TestClass, List<TestItem>> GetAllTests() {
            var res = new Dictionary<TestClass, List<TestItem>>();

            // get all test suites
            var allTestSuites =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(TestSuite), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<TestSuite>() };

            foreach (var testSuite in allTestSuites) {
                var obj = GetNewObject(testSuite.Type);
                List<TestItem> tests = new List<TestItem>();

                TestClass key = new TestClass(testSuite.Type.FullName, obj);
                res.Add(key, tests);
                
                var allTests = 
                    from a in testSuite.Type.GetMethods()
                    let attributes = a.GetCustomAttributes(typeof(Test), true)
                    where attributes != null && attributes.Length > 0
                    select new { Type = a, Method = a, Attributes = attributes.Cast<Test>() };

                foreach (var test in allTests) {
                    tests.Add(new TestItem(test.Method));
                }

                foreach (var method in testSuite.Type.GetMethods()) {
                    if (method.Name == "SetUp") {
                        key.setUp = method;
                    } else if (method.Name == "SetUpClass") {
                        key.setUpClass = method;
                    } else if (method.Name == "TearDown") {
                        key.tearDown = method;
                    } else if (method.Name == "TearDownClass") {
                        key.tearDownClass = method;
                    }
                } 
            }

            return res;
        }

        public async static Task<List<TestResult>> Run() {
            var res = new List<TestResult>();
            var tests = GetAllTests();
            var currentMethod = "";
            
                foreach (var item in tests) {
                    var target = item.Key.target;
                    if (item.Key.setUpClass != null) {
                        await (Task)item.Key.setUpClass.Invoke(target, null);
                    }
                    
                    foreach (var test in item.Value) {
                        item.Key.setUp?.Invoke(target, null);
                        
                        try {
                            currentMethod = item.Key.name + "::" + test.method.Name;
                            test.method.Invoke(target, null);
                            res.Add(TestResult.Build(currentMethod, TestRunResult.Success));
                        } catch (TargetInvocationException e) {
                            if (e.InnerException is TestException) {
                                res.Add(TestResult.Build(currentMethod, TestRunResult.Failure));
                            } else {
                                res.Add(TestResult.Build(currentMethod, TestRunResult.Exception));
                            }
                            
                            Debug.LogError(e.InnerException?.Message + "\n" + e.InnerException?.StackTrace);
                        } catch (Exception e) {
                            res.Add(TestResult.Build(currentMethod, TestRunResult.Exception));
                            Debug.LogError(e.Message + "\n" + e.StackTrace);
                        }
                        
                        item.Key.tearDown?.Invoke(target, null);
                    }
                    
                    item.Key.tearDownClass?.Invoke(target, null);
                }

            return res;
        }

        private static object GetNewObject(Type t) {
            try  {
                return t.GetConstructor(new Type[] { })?.Invoke(new object[] { });
            } catch {
                return null;
            }
        }        
    }    
}
