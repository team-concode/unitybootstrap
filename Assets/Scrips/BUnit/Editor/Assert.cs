using UnityEngine;
using Object = System.Object;

namespace BUnit {
    public static class Assert {
        public static int doneCount { get; set; }
        
        public static void EQ(Object a, Object b) {
            var eq = false;
            if (a is float valueA) {
                eq = Mathf.Abs(valueA - (float)b) < 0.0001f;
            }
            
            if (!eq && !a.Equals(b)) {
                var message = "\"" + a + "\" is not equals to \"" + b + "\"";
                Debug.LogError(message);
                throw new TestException(message);
            }
            doneCount++;
        }
        
        public static void NE(Object a, Object b) {
            if (a.Equals(b)) {
                var message = "\"" + a + "\" is not equals to \"" + b + "\"";
                Debug.LogError(message);
                throw new TestException(message);
            }
            doneCount++;
        }        

        public static void NULL(Object a) {
            if (a != null) {
                var message = "\"" + a + "\" is not null";
                Debug.LogError(message);
                throw new TestException(message);
            }
            doneCount++;
        }

        public static void TRUE(bool target) {
            if (!target) {
                var message = "Test is not TRUE";
                Debug.LogError(message);
                throw new TestException(message);
            }
            doneCount++;
        }

        public static void FALSE(bool target) {
            if (target) {
                var message = "Test is not FALSE";
                Debug.LogError(message);
                throw new TestException(message);
            }
            doneCount++;
        }
    }    
}
