using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BoolResult {
    public bool success { get; private set; }
    
    public bool IsSuccess() { return success; }
    public bool IsFailed() { return !success; }
    public void SetSuccess(bool success) { this.success = success; }
}

public class OutResult<T> {
    public T value;
}

public class NetResult<T> : BoolResult {
    public T value;
    public int responseCode;
}

public class RestResult<T> : NetResult<T> {
    public void SetResult(UnityWebRequest result) {
        responseCode = (int)result.responseCode;
        SetSuccess((result.responseCode / 100) == 2);
    }
}