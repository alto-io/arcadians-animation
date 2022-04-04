using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

namespace OPGames.Arcadians
{

// TODO: add caching
public class Arcadian : MonoBehaviour
{
	[SerializeField] private ArcadianSkin male;
	[SerializeField] private ArcadianSkin female;

	private ArcadianSkin active;
	private ArcadianInfo info;

	public ArcadianInfo Info { get { return info; } }

	private void Start()
	{
		active = male;
	}

	public void Load(int id, Action<ArcadianInfo> onDone)
	{
		if (id == 0) return;
		StartCoroutine(LoadCR(id, onDone));
	}

	// Check Arcadians API to get attributes of a specifc arcadian
	private IEnumerator LoadCR(int id, Action<ArcadianInfo> onDone)
	{
		female.gameObject.SetActive(false);
		male.gameObject.SetActive(false);

		string url = string.Format("https://api.arcadians.io/{0}", id);
		using (var www = UnityWebRequest.Get(url))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;
			LoadFromJson(json, onDone);

		}
	}

	public void LoadFromJson(string json, Action<ArcadianInfo> onDone)
	{
		try
		{
			var temp = JsonUtility.FromJson<ArcadianInfo>(json);
			ProcessInfo(temp);

			if (onDone != null)
				onDone(temp);
		}
		catch (System.Exception e)
		{
			Debug.LogError(e);
			return;
		}
	}

	public void ProcessInfo(ArcadianInfo _info)
	{
		if (_info == null) return;

		this.info = _info;

		bool isFemale = true;
		string className = "";

		var classAttr = _info.attributes.Find((a) => a.trait_type == "Class");
		if (classAttr != null)
		{
			isFemale = classAttr.value.IndexOf("Female") == 0;

			if (isFemale) className = classAttr.value.Replace("Female ", "");
			else          className = classAttr.value.Replace("Male ", "");

			_info.isFemale = isFemale;
			_info.className = className;
		}

		female.gameObject.SetActive(isFemale);
		male.gameObject.SetActive(!isFemale);

		active = isFemale ? female : male;
		active.LoadFromInfo(_info);
		active.AnimatorSetTrigger("Idle");
	}

	public void AnimatorSetTrigger(string trigger)
	{
		if (info == null) return;

		if (info.isFemale) female.AnimatorSetTrigger(trigger);
		else               male.AnimatorSetTrigger(trigger);
	}
}

}
