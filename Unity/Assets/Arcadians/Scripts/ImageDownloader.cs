//#if !UNITY_WEBGL || UNITY_EDITOR
	#define USE_CACHE
//#endif

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace OPGames.Arcadians
{

// USAGE
// var info = await imgDownloader.Download(url);
// image.sprite = info.sprite;

public class ImageDownloader : MonoBehaviour
{
	static public bool IsVerbose = false;
	static public ImageDownloader Instance = null;

	public class Info
	{
		public string Url;
		public Texture2D Tex;
		public Sprite Spr;
	}

	private List<Info> list = new List<Info>();

	private List<string> inProgress = new List<string>();

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
	}

	private string CheckUrl(string url)
	{
		if (string.IsNullOrEmpty(url))
			return url;

		// if url doesn't have the first part of the URL, append the domain
		if (url.IndexOf("http") == -1)
		{
			string domain = "https://alpha.outplay.games/"; // Default
			url = domain + url;
		}

		int startIndex = url.IndexOf("://") + 3;
		string protocol = url.Substring(0, startIndex);
		string address = url.Substring(startIndex);

		address = address.Replace("//", "/");
		url = protocol + address;

		return url;
	}

	public async UniTask DownloadWithCB(string url, System.Action<Info> callback)
	{
		var info = await Download(url);
		if (callback != null)
			callback(info);
	}

	public async UniTask<Info> Download(string url, bool force = false)
	{
		url = CheckUrl(url);
		if (string.IsNullOrEmpty(url))
			return null;

		// Try to wait for in progress download
		int remaining = 20;
		while (inProgress.IndexOf(url) >= 0 && remaining >= 0)
		{
			//Debug.Log($"Waiting for {url} {remaining}");
			await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);
			remaining--;
		}

		// Try if we have it loaded already
		Info found = Get(url);
		if (found != null && !force)
			return found;

#if USE_CACHE
		// Try if it's on local cache
		found = LoadFile(url);
		if (found != null && !force)
			return found;
#endif

		// Just download the damn thing
		if (IsVerbose) Debug.LogFormat("ImageDownloader - Download {0}", url);

		inProgress.Add(url);

		UnityWebRequest www = UnityWebRequest.Get(url);
		await www.SendWebRequest();

		inProgress.Remove(url);

		if (www.error != null)
		{
			Debug.LogErrorFormat("ImageDownloader - Download error {0}\n{1}", url, www.error);
			return null;
		}

		var data = www.downloadHandler.data;
		WriteFile(url, data);
		Info info = LoadImageFromBytes(data);
		info.Url = url;

		list.Add(info);
		return info;
	}

	public Info Get(string url)
	{
		return list.Find((i) => i.Url == url);
	}

	private void WriteFile(string url, byte[] data)
	{
#if USE_CACHE
		string filename = GetFileNameFromUrl(url);
		string filepath = Path.Combine(Application.persistentDataPath, filename);
		File.WriteAllBytes(filepath, data);
		if (IsVerbose) Debug.Log($"ImageDownloader - WriteFile {filepath}");
#endif
	}

	private Info LoadFile(string url)
	{
		if (string.IsNullOrEmpty(url))
			return null;

#if USE_CACHE
		string filename = GetFileNameFromUrl(url);
		string filepath = Path.Combine(Application.persistentDataPath, filename);
		if (File.Exists(filepath) == false)
			return null;

		// force redownload if greater than a week
		DateTime modified = File.GetLastWriteTime(filepath);
		TimeSpan diff = DateTime.Now - modified;
		if (diff.Days >= 7)
		{
			if (IsVerbose) Debug.Log($"ImageDownloader - Force redownload {url}");
			return null;
		}

		var data = File.ReadAllBytes(filepath);
		Info info = LoadImageFromBytes(data);
		info.Url = url;
		list.Add(info);

		if (IsVerbose) Debug.Log($"ImageDownloader - LoadFile {url}");
		return info;
#else
		return null;
#endif
	}

	private Info LoadImageFromBytes(byte[] data)
	{
		Info info = new Info();

		Texture2D tex = new Texture2D(2,2);
		tex.LoadImage(data);
		info.Tex = tex;

		if (IsVerbose) Debug.Log($"ImageDownloader - Image Size {tex.width} x {tex.height}");

		Sprite spr = Sprite.Create(
				tex, 
				new Rect(0.0f, 0.0f, tex.width, tex.height), 
				new UnityEngine.Vector2(0.5f, 0.5f));

        if (spr == null)
            Debug.LogError("ImageDownloader - Cannot create sprite");

		info.Spr = spr;
		return info;
	}

	static private string GetFileNameFromUrl(string url)
	{
		Uri uri;
		if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			return "";

		return Path.GetFileName(uri.LocalPath);
	}
}

}
