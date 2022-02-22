using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class UIDebug : MonoBehaviour
{
	public ArcadianSkin male;
	public ArcadianSkin female;

	public Text maleName;
	public Text femaleName;

	public Text maleClass;
	public Text femaleClass;

	public Image maleImage;
	public Image femaleImage;

	public ArcadianInfo arcadianInfo;

	private Animator maleAnimator;
	private Animator femaleAnimator;

	public void OnBtnRandom()
	{
		int id = Random.Range(1,3733);
		StartCoroutine(LoadArcadian(id));
	}

	private IEnumerator Start()
	{
		maleAnimator = male.GetComponent<Animator>();
		femaleAnimator = female.GetComponent<Animator>();

		yield return StartCoroutine(LoadArcadian(160));
		yield return StartCoroutine(LoadArcadian(661));
	}

	public void TriggerAnim(string trigger)
	{
		if (maleAnimator != null)
			maleAnimator.SetTrigger(trigger);

		if (femaleAnimator != null)
			femaleAnimator.SetTrigger(trigger);
	}

	private IEnumerator LoadArcadian(int id)
	{
		string url = string.Format("https://api.arcadians.io/{0}", id);
		using (var www = UnityWebRequest.Get(url))
		{
			yield return www.SendWebRequest();
			var json = www.downloadHandler.text;
			try
			{
				arcadianInfo = JsonUtility.FromJson<ArcadianInfo>(json);
				if (arcadianInfo != null)
				{
					bool isFemale = true;
					string className = "";

					var classAttr = arcadianInfo.attributes.Find((a) => a.trait_type == "Class");
					if (classAttr != null)
					{
						isFemale = classAttr.value.IndexOf("Female") == 0;

						if (isFemale)
							className = classAttr.value.Replace("Female ", "");
						else
							className = classAttr.value.Replace("Male ", "");
					}

					Debug.Log($"ClassName {className}");

					if (isFemale)
					{
						female.SetMaterialsFromInfo(arcadianInfo);
						femaleName.text = arcadianInfo.name;
						femaleClass.text = className;
						femaleAnimator.SetInteger("Class", ClassToInt(className));

						StartCoroutine(UpdateImage(femaleImage, arcadianInfo.image));
					}
					else
					{
						male.SetMaterialsFromInfo(arcadianInfo);
						maleName.text = arcadianInfo.name;
						maleClass.text = className;
						maleAnimator.SetInteger("Class", ClassToInt(className));

						StartCoroutine(UpdateImage(maleImage, arcadianInfo.image));
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				yield break;
			}
		}
	}

	private IEnumerator UpdateImage(Image img, string url)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();

		Texture2D tex = (Texture2D)DownloadHandlerTexture.GetContent(www);
		SetImageTexture(img, tex);
	}

	static public void SetImageTexture(Image img, Texture2D tex)
	{
		if (img == null || tex == null)
			return;

		Sprite spr = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
        if (spr == null)
        {
            Debug.LogError("Cannot create sprite");
			return;
        }

        img.sprite = spr;
		img.preserveAspect = true;
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
