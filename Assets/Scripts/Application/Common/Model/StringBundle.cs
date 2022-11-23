using System.Collections.Generic;
using Newtonsoft.Json;

public class StringBundle {
    public string key;
    public string ko;
    public string en;
    public string ja;
    public string ru;
    public string fr;
    [JsonProperty("zh-hant")] public string zhHant;
    public string es;
    public string de;
    public string th;
    public string id;
    public string vi;
    public string pt;
}

public class StringBundleSet {
    public int mlVersion;
    public List<StringBundle> bundles;
}