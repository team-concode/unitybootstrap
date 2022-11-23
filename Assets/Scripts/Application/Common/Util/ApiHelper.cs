using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

public delegate void OnApiEvent(HTTPRequest www);
public delegate bool OnApiStartEvent();

public class ApiHelper {
    public int timeout { get; }
    public event OnApiEvent onSuccess;

    private Dictionary<string, string> headers;
    private StringBundleService sbService => UnityBean.BeanContainer.GetBean<StringBundleService>();
    private TimeService timeService => UnityBean.BeanContainer.GetBean<TimeService>();

    public ApiHelper() {
        HTTPManager.IsCachingDisabled = true;
        HTTPManager.KeepAliveDefaultValue = false;
        
        headers = new Dictionary<string, string>();
        AddHeader("Content-Type", "application/json");
        if (App.version != null) {
            AddHeader("version", App.version.ToString());
        }
        timeout = 60;
    }

    public void AddHeader(string key, string value) {
        if (value == null) {
            Debug.LogError("Can not set null header:" + key);
            return;
        }

        headers.Add(key, value);
    }

    public string GetHeader(string key) {
        headers.TryGetValue(key, out var res);
        return res;
    }

    public void SetHeader(string key, string value) {
        if (value == null) {
            Debug.LogError("Can not set null header:" + key);
            return;
        }

        headers[key] = value;
    }

    public async Task<HTTPRequest> Post(string url, string body, bool needWaitingPanel = true, Action<string> onError = null) {
        while (true) {
            var www = new HTTPRequest(new Uri(url), HTTPMethods.Post);
            www.ConnectTimeout = TimeSpan.FromSeconds(timeout);
            www.Timeout = TimeSpan.FromSeconds(timeout);
            www.DisableCache = true;
            www.IsKeepAlive = false;

            SetHeader(www);
            if (!string.IsNullOrEmpty(body)) {
                www.RawData = Encoding.UTF8.GetBytes(body);
            }

            await Send(www, needWaitingPanel);
            if (!www.IsSuccess()) {
                if (onError != null) {
                    onError.Invoke(www.Response?.DataAsText);
                    return www;
                }

                var result = await ShowErrorMessage(www.Response?.StatusCode??0, www.Response?.DataAsText);
                if (result == AlertBoxResult.Ok) {
                    continue;
                } 
            } else {
                onSuccess?.Invoke(www);
            }

            return www;
        }
    }

    private async Task ToIntroScene(){
        await new WaitForEndOfFrame();
        await App.mainUI.SwitchSceneWithHistoryClear(SceneType.Intro);
    }

    private async Task<AlertBoxResult> ShowErrorMessage(int responseCode, string result) {
        if (responseCode == 401) {
            var message = sbService.Get("error.code." + responseCode);
            await App.mainUI.ShowAlert(message, AlertBoxType.Ok);
            ToIntroScene().RunAsync();

            //Service.account.RemoveAccount();
            return AlertBoxResult.Cancel;
        }

        if (responseCode == 489) {
            var message = sbService.Get("intro.maintenance.break");
            await App.mainUI.ShowAlert(message, AlertBoxType.Ok);
            return AlertBoxResult.Cancel;
        }

        if (responseCode == 490) {
            var message = sbService.Get("intro.need.update");
            await App.mainUI.ShowAlert(message, AlertBoxType.Update);
            return AlertBoxResult.Cancel;
        }

        var errorMessage = sbService.Get("error.code.500");
        if (!string.IsNullOrEmpty(result)) {
            try {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(result);
                errorMessage = error.message;
                if (error.timestamp > 0) {
                    timeService.SetServerTimeMilli(error.timestamp);
                }
            } catch (Exception e) {
                errorMessage = result;
                Debug.LogError(result + "\n" + e.Message);
            }
            Debug.LogError(result);
        }
        
        return await App.mainUI.ShowAlert(errorMessage, AlertBoxType.Retry);
    }

    public async Task<HTTPRequest> Get(string url, bool needWaitingPanel = true, Action<string> onError = null) {
        while (true) {
            var www = new HTTPRequest(new Uri(url), HTTPMethods.Get);
            www.ConnectTimeout = TimeSpan.FromSeconds(timeout);
            www.Timeout = TimeSpan.FromSeconds(timeout);
            www.DisableCache = true;
            www.IsKeepAlive = false;

            SetHeader(www);

            await Send(www, needWaitingPanel);
            if (!www.IsSuccess()) {
                if (onError != null) {
                    onError.Invoke(www.Response?.DataAsText);
                    return www;
                }

                var result = await ShowErrorMessage(www.Response?.StatusCode??0, www.Response?.DataAsText);
                if (result == AlertBoxResult.Ok) {
                    continue;
                }
            } else {
                onSuccess?.Invoke(www);
            }

            return www;
        }
    }

    private void SetHeader(HTTPRequest www) {
        foreach (var item in headers) {
            www.SetHeader(item.Key, item.Value);
        }
    }

    private async Task Send(HTTPRequest request, bool needWaitingPanel) {
        if (needWaitingPanel) {
            App.mainUI.ShowWaitingPanel();
        }

        Debug.Log("START: " + request.MethodType + " " + request.Uri);
        await request.Send();
        if (needWaitingPanel) {
            App.mainUI.HideWaitingPanel();
        }

        var sb = new StringBuilder();
        sb.Append("DONE: " + request.MethodType + " " + request.Uri);
        sb.AppendLine();
        sb.Append(request.Response?.StatusCode??0);
        sb.AppendLine();
        sb.Append("[REQ]");
        sb.AppendLine();
        if (request.RawData != null) {
            sb.Append(Encoding.UTF8.GetString(request.RawData));
        }
        sb.AppendLine();
        sb.AppendLine();

        //if (App.phase != Phase.Production) {
            sb.Append("[REQ-HEADER]");
            sb.AppendLine();
            foreach (var item in headers) {
                sb.Append(item.Key);
                sb.Append(":");
                sb.Append(item.Value);
                sb.AppendLine();
            }
        //}

        sb.AppendLine();
        sb.Append("[RES]");
        sb.Append(request.Response?.DataAsText??"");

        if (request.IsSuccess()) {
            Debug.Log(sb.ToString());
        } else {
            Debug.LogError(sb.ToString());
        }
    }
}
