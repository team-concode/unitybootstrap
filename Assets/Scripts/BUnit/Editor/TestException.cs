using System;

namespace BUnit {
    public class TestException : Exception {
        public TestException(string message) : base(message) {
        }
    }    
}
