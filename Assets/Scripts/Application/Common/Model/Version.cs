using System;
using System.Text;

public class Version {
    public int major { get; private set; }
    public int minor { get; private set; }
    public int patch { get; private set; }

    public Version(string version) {
        From(version);
    }

    public Version(int major, int minor, int patch) {
        this.major = major;
        this.minor = minor;
        this.patch = patch;
    }

    public override string ToString() { 
        StringBuilder builder = new StringBuilder();
        builder.Append(major);
        builder.Append(".");
        builder.Append(minor);
        builder.Append(".");
        builder.Append(patch);
        return builder.ToString();
    }

    public void From(string ver) {
        string[] versions = ver.Split('.');
        if (versions.Length != 3) {
            return;
        }

        int[] numbers = new int[3];
        int index = 0;
        foreach (string text in versions) {
            numbers[index++] = Int32.Parse(text);
        }

        major = numbers[0];
        minor = numbers[1];
        patch = numbers[2];
    }

    public bool IsGreaterThan(Version rhs) {
        // major check
        if (major < rhs.major) {
            return false;
        }
        if (rhs.major < major) {
            return true;
        }

        // minor check
        if (minor < rhs.minor) {
            return false;
        }
        if (rhs.minor < minor) {
            return true;
        }

        // patch check
        if (patch < rhs.patch) {
            return false;
        }
        if (rhs.patch < patch) {
            return true;
        }

        return false;       
    }

    public bool IsLessThan(Version rhs) {
        // major check
        if (major > rhs.major) {
            return false;
        }
        if (rhs.major > major) {
            return true;
        }

        // minor check
        if (minor > rhs.minor) {
            return false;
        }
        if (rhs.minor > minor) {
            return true;
        }

        // patch check
        if (patch > rhs.patch) {
            return false;
        }
        if (rhs.patch > patch) {
            return true;
        }

        return false;       
    }
}
