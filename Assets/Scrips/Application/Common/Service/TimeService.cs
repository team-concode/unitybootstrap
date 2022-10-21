using System;

[UnityBean.Service]
public class TimeService {
    [UnityBean.AutoWired] 
    private StringBundleService sb;
    
    public long serverTimeOffset { get; set; }
    
    public long GetLocalUnixTime() {
        return UnixTimestampFromDateTime(DateTime.Now) / 1000;
    }

    public long GetLocalUnixTimeMilli() {
        return UnixTimestampFromDateTime(DateTime.Now);
    }

    public long GetServerUnixTime() {
        return GetLocalUnixTime() + serverTimeOffset / 1000;
    }

    public long GetServerUnixTimeMilli() {
        return GetLocalUnixTimeMilli() + serverTimeOffset;
    }

    public void SetServerTimeMilli(long milliTime) {
        var offset = milliTime - GetLocalUnixTimeMilli();
        var diff = serverTimeOffset - offset;
        if (diff < 0) {
            diff = -diff;
        }

        if (serverTimeOffset == 0) {
            serverTimeOffset = offset;
        } else {
            if (diff < 10 * 1000) {
                serverTimeOffset = offset;
            }
        }
    }

    long UnixTimestampFromDateTime(DateTime date) {
        date = date.ToUniversalTime();
        long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
        unixTimestamp /= TimeSpan.TicksPerMillisecond;
        return unixTimestamp;
    }

    public string GetRemainTimeTextHMS(int remain) {
        var hour = remain / 3600;
        var min = (remain / 60) % 60;
        var sec = remain % 60;

        return hour.ToString("D2") + ":" + 
               min.ToString("D2") + ":" + 
               sec.ToString("D2");
    }

    public string GetRemainTimeTextDorHMS(int remain) {
        var day = remain / (3600 * 24);
        if (day > 0) {
            return day + sb.Get("common.days");
        }

        return GetRemainTimeTextHMS(remain);
    }

    public string GetRemainTimeTextMS(int remain) {
        var min = remain / 60;
        var sec = remain % 60;

        return min.ToString("D2") + ":" + 
               sec.ToString("D2");
    }
}