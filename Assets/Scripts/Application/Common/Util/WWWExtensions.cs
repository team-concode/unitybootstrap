using BestHTTP;

public static class WWWExtensions {
    public static bool IsSuccess(this HTTPRequest www) {
        if (www.Response == null) {
            return false;
        }
    
        var code = www.Response.StatusCode;
        return (code / 100) == 2 || code == 304;
    }    
    
    public static string GetResponseText(this HTTPRequest www) {
        return www.Response?.DataAsText;
    }
}
