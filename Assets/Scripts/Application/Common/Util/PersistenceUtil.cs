using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class PersistenceUtil {
    public static string LoadTextFile(string path) {
#if !UNITY_WEBGL        
        try {
            var sr = new StreamReader(path);
            var text = sr.ReadToEnd();
            sr.Close();
            log.debug("File load complete : " + path);
            return text;
        } catch (Exception e) {
            Debug.Log(e.Message);
            return "";
        }
#else    
        if (path.IndexOf(Application.persistentDataPath) == 0) {
            path = path.Substring(Application.persistentDataPath.Length + 1);
        }
        
        path = path.Replace("/", "_");
        return PlayerPrefs.GetString(path);
#endif
    }
    
    public static string LoadTextResource(string path) {
        var textAsset = (TextAsset) Resources.Load(path, typeof(TextAsset));
        if (textAsset == null) {
            log.debug("Can not load text : " + path);
            return "";
        }
        
        return textAsset.text;
    }

    public static bool SaveTextFile(string path, string text) {
#if !UNITY_WEBGL
        try {
            var sw = new StreamWriter(path);
            sw.Write(text);
            sw.Close();
            
            log.debug("File save complete: "  + path);
            return true;
        } catch (Exception e) {
            log.error(e.Message);
            return false;
        }
#else
        if (path.IndexOf(Application.persistentDataPath) == 0) {
            path = path.Substring(Application.persistentDataPath.Length + 1);
        }

        path = path.Replace("/", "_");
        PlayerPrefs.SetString(path, text);
        PlayerPrefs.Save();
        return true;
#endif
    }
    
    public static bool DeleteFile(string path) {
#if !UNITY_WEBGL
        try {
            if (File.Exists(path)) {
                File.Delete(path);
            }
            return true;
        } catch (Exception e) {
            Debug.LogError(e.Message);
            return false;
        }
#else
        path = path.Replace("/", "_");
        PlayerPrefs.DeleteKey(path);
        PlayerPrefs.Save();
        return true;
#endif
    }

    public static byte[] ReadBinaryResource(string path) {
        var textAsset = (TextAsset) Resources.Load(path, typeof(TextAsset));
        if (textAsset == null) {
            log.debug("Can not load binary building : " + path);
            return null;
        }

        return textAsset.bytes;
    }

    public static byte[] ReadBinaryFile(string path) {
        try {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (fs == null) {
                return null;
            }

            var reader = new BinaryReader(fs);
            var result = reader.ReadBytes((int)reader.BaseStream.Length);
            reader.Close();
            fs.Close();

            return result;
        } catch (Exception e) {
            Debug.Log(e.Message);
            return null;
        }
    }

    public static bool WriteBinaryFile(string path, byte[] data) {
        try {
            var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            if (fs == null) {
                return false;
            }

            var writer = new BinaryWriter(fs);
            writer.Write(data);
            writer.Close();
            fs.Close();
            return true;
        } catch (Exception e) {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public static void CreateFolder(string path) {
#if !UNITY_WEBGL
        try {
            var di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            Directory.CreateDirectory(path); // second try
        }
#endif
    }

    public static void DeleteFolder(string path) {
#if !UNITY_WEBGL
        DirectoryInfo di = new DirectoryInfo(path);
        try {
            if (di.Exists == true) {
                di.Delete(true);
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            Directory.Delete(path);  // second try
        }
#endif    
    }

    public static void MoveFile(string from, string to) {
        try {
            File.Move(from, to);
        } catch (Exception e) {
            log.error(e);
        }
    }
    
    public static List<string> GetFileList(string folderPath, string extName, bool includeExt) {
        List<string> fileList = new List<string>();
#if !UNITY_WEBGL
        DirectoryInfo di = new DirectoryInfo(folderPath);
        try {
            if (di.Exists == true) {
                var list = di.GetFiles();
                for (int i=0; i < list.Length; i++) {
                    var fileInfo = list[i];
                    var fileName = fileInfo.Name;
                    if (fileInfo.Extension.Equals(extName)) {
                        if (!includeExt) {
                            fileName = fileName.Replace(fileInfo.Extension, "");
                        }
                        
                        fileList.Add(fileName);
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
        }
#endif
        return fileList;
    }
}