using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.Arcadians
{

public class ArcadianSkin : MonoBehaviour
{
	public Renderer rendererRef;

	public int ArcadianId = 160;

	public ArcadianInfo arcadianInfo;

	private Animator animator;

	private Dictionary<string, Material> matDict = 
		new Dictionary<string, Material>();

	private void Start()
	{
		animator = GetComponent<Animator>();

		var mats = rendererRef.materials;
		for (int i=0; i<mats.Length; i++)
			AddMaterial(mats[i]);
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
		if      (mat.name.IndexOf("skin")   != -1) { matDict.Add("Skin", mat);       }
		else if (mat.name.IndexOf("eyes")   != -1) { matDict.Add("Eyes", mat);       }
		else if (mat.name.IndexOf("mouth")  != -1) { matDict.Add("Mouth", mat);      }
		else if (mat.name.IndexOf("head")   != -1) { matDict.Add("Head", mat);       }
		else if (mat.name.IndexOf("top")    != -1) { matDict.Add("Top", mat);        }
		else if (mat.name.IndexOf("bottom") != -1) { matDict.Add("Bottom", mat);     }
		else if (mat.name.IndexOf("right")  != -1) { matDict.Add("Right Hand", mat); }
		else if (mat.name.IndexOf("left")   != -1) { matDict.Add("Left Hand", mat);  }
	}

	public void LoadFromInfo(ArcadianInfo info)
	{
		if (info == null)
			return;

		arcadianInfo = info;

		string classNameFull = "Female Assassin";

		var classAttr = info.attributes.Find((a) => a.trait_type == "Class");
		if (classAttr != null)
			classNameFull = classAttr.value;

		foreach (var attr in info.attributes)
		{
			string type = attr.trait_type;
			if (matDict.ContainsKey(type) == false)
				continue;

			var mat = matDict[type];
			SetMaterial(mat, classNameFull, type, attr.value);
		}

		animator.SetInteger("Class", ClassToInt(info.className));
	}

	private void SetMaterial(Material mat, string className, string partType, string partId)
	{
		if (mat == null)
			Debug.LogWarningFormat("Material is NULL {0}, {1}", className, partType);

		string path = string.Format("Parts/{0}/{1}/{2}", className, partType, partId.Replace(" ", "-"));
		//Debug.LogFormat("SetMaterial {0}", path);

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

	public void AnimatorSetTrigger(string trigger)
	{
		if (animator != null)
			animator.SetTrigger(trigger);
	}

	static public int ClassToInt(string className)
	{
		className = className.ToLower();
		if (className == "assassin") return 0;
		if (className == "gunner") return 1;
		if (className == "knight") return 2;
		if (className == "tech") return 3;
		if (className == "wizard") return 4;
		return 0;
	}
}

}
