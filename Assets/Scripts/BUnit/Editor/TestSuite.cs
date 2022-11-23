using System;

namespace BUnit {
    [AttributeUsage(AttributeTargets.Class)]  
    public class TestSuite : Attribute {
        public string testCaseName { get; set; }

        public TestSuite(string testCaseName) {
            this.testCaseName = testCaseName;
        }
        
        public TestSuite() {
            this.testCaseName = this.TypeId.ToString();
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]  
    public class Test : Attribute {
        public Test() {
        }
    }
}

