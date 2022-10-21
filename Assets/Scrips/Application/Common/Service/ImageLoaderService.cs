using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine.Networking;

[UnityBean.Service]
public class ImageLoaderService {
	public int maxMemoryCacheCount { get; set; }
	public int maxFileCacheCount { get; set; }
	public int cacheLife { get; set; }

	public class ImageFileCacheMeta {
		public string url = "";
		public string file = "";
		public long time;
	}

	public class CachedImage {
		public string url = "";
		public Texture2D texture;
	}

	public enum LoaderType {
		None,
		Web,
		Resource
	}

	private string dbFilePath = Application.persistentDataPath + "/imageCache.db";
	private string cacheRoot = Application.persistentDataPath + "/ImageCache/";
	private Dictionary<string, ImageFileCacheMeta> fileCacheMeta = new();
	private Queue<CachedImage> cache = new Queue<CachedImage>();
	private bool isReady;

	public async Task<bool> Initialize() {
		if (!isReady) {
			maxMemoryCacheCount = 50;
			maxFileCacheCount = 128;
			cacheLife = 24 * 60 * 60;

			LoadMeta();
			PersistenceUtil.CreateFolder(cacheRoot);
			isReady = true;
		}
		return true;
	}

	private void LoadMeta() {
		var text = PersistenceUtil.LoadTextFile(dbFilePath);
		if (text.Length == 0) {
			PersistenceUtil.DeleteFolder(cacheRoot);
			return;
		}

		try {
			var result = JsonConvert.DeserializeObject<ImageFileCacheMeta[]>(text);
			foreach (var meta in result) {
				if (!fileCacheMeta.ContainsKey(meta.url)) {
					fileCacheMeta.Add(meta.url, meta);
				}
			}
		} catch (Exception e) {
			PersistenceUtil.DeleteFolder(cacheRoot);
			Debug.LogError(e.ToString());
		}
	}

	private void SaveMeta() {
		var items = new List<ImageFileCacheMeta>();
		items.AddRange(fileCacheMeta.Values);
		var text = JsonConvert.SerializeObject(items);
		PersistenceUtil.SaveTextFile(dbFilePath, text);
	}

	public async Task<Texture2D> LoadTexture(string url) {
		var result = new OutResult<Texture2D>();
		await LoadTexture(url, result);
		return result.value;
	}

	public IEnumerator LoadTexture(string url, OutResult<Texture2D> result) {
		var culture = StringComparison.Ordinal;
		var loaderType = LoaderType.None;
		
		if (url.IndexOf("http://", culture) >= 0 || 
			url.IndexOf("https://", culture) >= 0) {
			loaderType = LoaderType.Web;
		}

		if (url.IndexOf("res://", culture) >= 0) {
			loaderType = LoaderType.Resource;
		}

		if (loaderType == LoaderType.None) {
			Debug.LogWarning("not a vaild url");
			yield break;
		}

		// find in memory cache
		Texture2D texture;
		texture = FindInCache(url);
		if (texture != null) {
			result.value = texture;
			yield break;
		}

		// find in file cache
		texture = FindInFileCache(url);
		if (texture != null) {
			result.value = texture;
			CacheInMemory(url, texture);
			yield break;
		}

		byte[] bytes = null;

		if (loaderType == LoaderType.Web) {
			var data = new OutResult<byte[]>();
			yield return LoadImageFromWeb(url, data);
			bytes = data.value;
		} else if (loaderType == LoaderType.Resource) {
			bytes = LoadImageFromResource(url);
		}

		if (bytes != null) {
			texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			if (texture.LoadImage(bytes)) {
				texture.wrapMode = TextureWrapMode.Clamp;
				CacheInMemory(url, texture);
				if (loaderType != LoaderType.Resource) {
					CacheInFile(url, bytes);
				}

				result.value = texture;
			} else {
				Debug.LogWarning("can not read image: " + url);
			}
		} else {
			Debug.LogWarning("request failed: " + url);
		}
	}

	private byte[] LoadImageFromResource(string url) {
		var path = url.Replace("res://", "");
		return PersistenceUtil.ReadBinaryResource(path);
	}

	private IEnumerator LoadImageFromWeb(string url, OutResult<byte[]> res) {
		var www = UnityWebRequest.Get(url);
		www.timeout = 10;
		yield return www.SendWebRequest();
		
		byte[] bytes = null;
		var statusCode = 0;
		if (www.downloadHandler.data != null) {
			bytes = www.downloadHandler.data;
			statusCode = (int)www.responseCode;
		}

		if (www.responseCode / 100 == 2 || www.responseCode == 304) {
			res.value = bytes;
		}
	}

	private bool IsSuccess(int responseCode) {
		if (responseCode == 304) {
			return true;
		} 
        
		return (responseCode / 100 == 2);
	}

	private void CacheInMemory(string url, Texture2D tex) {
		if (cache.Count > maxMemoryCacheCount) {
			cache.Dequeue();
		}

		if (tex == null) {
			return;
		}

		var data = new CachedImage {
			url = url,
			texture = tex
		};
		cache.Enqueue(data);
	}

	private Texture2D FindInCache(string url) {
		foreach (var image in cache) {
			if (url == image.url) {
				return image.texture;
			}
		}

		return null;
	}

	private Texture2D FindInFileCache(string url) {
#if !UNITY_WEBGL
		if (fileCacheMeta.ContainsKey(url) == false) {
			return null;
		}

		var meta = fileCacheMeta[url];
		var now = UnixTimeNow();
		if (now > meta.time + cacheLife) {
			return null;
		}

		var bytes = PersistenceUtil.ReadBinaryFile(cacheRoot + meta.file);
		if (bytes == null) {
			Debug.LogError("invalid file!");
			return null;
		}

		try {
			var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			if (tex.LoadImage(bytes, true) == false) {
				tex.wrapMode = TextureWrapMode.Clamp;
				fileCacheMeta.Remove(url);
				Debug.LogError("dirty file!");
				return null;
			}
			return tex;
		} catch (Exception e) {
			Debug.LogError(e.ToString());
		}
#endif

		return null;
	}

	private void CacheInFile(string url, byte[] bytes) {
#if !UNITY_WEBGL
		var meta = new ImageFileCacheMeta {
			url = url,
			time = UnixTimeNow(),
			file = Guid.NewGuid().ToString()
		};

		// reuse old file path
		if (fileCacheMeta.ContainsKey(meta.url)) {
			meta.file = fileCacheMeta[meta.url].file;
		}

		// cache
		fileCacheMeta[meta.url] = meta;
		
		// write file
		var res = PersistenceUtil.WriteBinaryFile(cacheRoot + meta.file, bytes);
		if (res) {
			PackFileCache();
			SaveMeta();
		} else {
			Debug.LogWarning("Can not cache image!");
		}
#endif
	}

	private void PackFileCache() {
		if (fileCacheMeta.Count < maxFileCacheCount) {
			return;
		}

		// sort with time
		var values = new List<ImageFileCacheMeta>();
		foreach (var meta in fileCacheMeta.Values) {
			values.Add(meta);
		}
		
		values.Sort((x, y) => {
			if (x.time < y.time) { return -1; }
			if (x.time > y.time) { return  1; }			
			return 0;
		});

		// remove cache
		var invalidCount = values.Count - maxFileCacheCount;
		for (var index = 0; index<invalidCount; index++) {
			ImageFileCacheMeta meta = values[index];
			if (string.IsNullOrEmpty(meta.file) == false) {
				PersistenceUtil.DeleteFile(cacheRoot + meta.file);
			}

			fileCacheMeta.Remove(meta.url);
		}
	}

	private long UnixTimeNow() {
		var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
		return (long)timeSpan.TotalSeconds;
	}
}