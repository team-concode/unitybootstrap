using UnityEngine;
using System.Threading.Tasks;
using UnityBean;

[Controller]
public class App {
    public static Phase phase { get; set; } = Phase.Production;
    public static Config config { get; set; }
    public static Version version { get; set; }
    public static bool ready { get; set; }
    public static UIPresenter mainUI { get; set; }

    public async Task<bool> Initialize() {
        if (Application.isPlaying) {
            Application.logMessageReceived += HandleException;
        }

        return true;
    } 

    private void HandleException(string condition, string stackTrace, LogType type) {
        if (type == LogType.Exception) {
            var text = condition + "\n" + stackTrace;
            if (text.Length > 512) {
                text = text.Substring(0, 512);
            }
            
            Debug.LogError(text);
            mainUI.ShowAlert(text, AlertBoxType.Ok);
        }
    }
}