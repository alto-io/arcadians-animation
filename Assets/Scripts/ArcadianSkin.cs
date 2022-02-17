using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ArcadianSkin : MonoBehaviour
{
	public Renderer rendererRef;

	public int ArcadianId = 160;

	public ArcadianInfo arcadianInfo;

	private Dictionary<string, Material> matDict = 
		new Dictionary<string, Material>();

	private void Start()
	{
		var mats = rendererRef.materials;

		for (int i=0; i<mats.Length; i++)
		{
			AddMaterial(mats[i]);
		}

		Refresh();
	}

	// TODO: 
	// https://docs.unity3d.com/ScriptReference/Renderer-material.html
	//
	// This function automatically instantiates the materials and makes 
	// them unique to this renderer. It is your responsibility to destroy 
	// the materials when the game object is being destroyed. 
	// Resources.UnloadUnusedAssets also destroys the materials but it is 
	// usually only called when loading a new level.
	private void AddMaterial(Material mat)
	{
		if (mat.name.IndexOf("skin") != -1)
		{
			matDict.Add("Skin", mat);
			return;
		}
		if (mat.name.IndexOf("eyes") != -1)
		{
			matDict.Add("Eyes", mat);
			return;
		}
		if (mat.name.IndexOf("mouth") != -1)
		{
			matDict.Add("Mouth", mat);
			return;
		}
		if (mat.name.IndexOf("head") != -1)
		{
			matDict.Add("Head", mat);
			return;
		}
		if (mat.name.IndexOf("top") != -1)
		{
			matDict.Add("Top", mat);
			return;
		}
		if (mat.name.IndexOf("bottom") != -1)
		{
			matDict.Add("Bottom", mat);
			return;
		}
		if (mat.name.IndexOf("right") != -1)
		{
			matDict.Add("Right Hand", mat);
			return;
		}
		if (mat.name.IndexOf("left") != -1)
		{
			matDict.Add("Left Hand", mat);
			return;
		}
	}

	public void Refresh()
	{
		StartCoroutine(RefreshCR());
	}

	private IEnumerator RefreshCR()
	{
		string url = string.Format("https://api.arcadians.io/{0}", ArcadianId);
		using (var www = UnityWebRequest.Get(url))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;

			try
			{
				arcadianInfo = JsonUtility.FromJson<ArcadianInfo>(json);
				SetMaterialsFromInfo(arcadianInfo);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				yield break;
			}
		}
	}

	public void SetMaterialsFromInfo(ArcadianInfo info)
	{
		if (info == null)
			return;

		string className = "Female Assassin";

		var classAttr = info.attributes.Find((a) => a.trait_type == "Class");
		if (classAttr != null)
			className = classAttr.value;

		foreach (var attr in info.attributes)
		{
			string type = attr.trait_type;
			if (matDict.ContainsKey(type) == false)
				continue;

			var mat = matDict[type];
			SetMaterial(mat, className, type, attr.value);
		}
	}

	private void SetMaterial(Material mat, string className, string partType, string partId)
	{
		if (mat == null)
			Debug.LogWarningFormat("Material is NULL {0}, {1}", className, partType);

		string path = string.Format("Parts/{0}/{1}/{2}", className, partType, partId.Replace(" ", "-"));
		Debug.LogFormat("SetMaterial {0}", path);

		Texture2D tex = Resources.Load<Texture2D>(path);
		if (tex != null)
		{
			mat.mainTexture = tex;
		}
		else
		{
			Debug.LogWarningFormat("Cannot load {0}", path);
		}
	}
}
