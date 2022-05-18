using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using OPGames.Arcadians;

[System.Serializable]
public class RarityResult
{
	public string rarity;
	public float score;
	public List<Attribute> attributes;
}

public class UIDebug : MonoBehaviour
{
	[SerializeField] private Arcadian arcadian;
	[SerializeField] private Text textName;
	[SerializeField] private Text textClass;
	[SerializeField] private Text textParts;
	[SerializeField] private Text textRarity;
	[SerializeField] private Text textScore;
	[SerializeField] private Image image;
	[SerializeField] private InputField input;
	[SerializeField] private Text textLog;
	[SerializeField] private int logLines = 7;

	private Queue<string> logs = new Queue<string>();
	private const int MIN_ID = 1;
	private const int MAX_ID = 3732;
	private int currId = MIN_ID;

	private RarityResult rarityResult;

	private void Start()
	{
		OnBtnRandom();
	}

	public void OnBtnNext()
	{
		currId++;
		if (currId > MAX_ID) currId = MIN_ID;
		Load(currId);
	}

	public void OnBtnPrev()
	{
		currId--;
		if (currId < MIN_ID) currId = MAX_ID;
		Load(currId);
	}

	public void OnBtnLoad()
	{
		string t = input.text;
		int id = 0;
		if (System.Int32.TryParse(t, out id))
		{
			Load(id);
		}
	}

	// When the Load Random button is clicked
	public void OnBtnRandom()
	{
		int id = Random.Range(MIN_ID,MAX_ID+1);
		Load(id);
	}

	public void Load(int id)
	{
		if (arcadian == null) return;
		currId = id;
		input.text = id.ToString();

		arcadian.Load(id, (info) =>
		{
			if (info == null) return;

			textName.text = info.name;
			textClass.text = info.className;
			StartCoroutine(UpdateImage(image, info.image));
			StartCoroutine(GetRarityRequest(info));
		});
	}

	// Trigger an animation
	public void TriggerAnim(string trigger)
	{
		arcadian.AnimatorSetTrigger(trigger);
	}

	// Download the image from a URL and set it to img
	private IEnumerator UpdateImage(Image img, string url)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();

		Texture2D tex = (Texture2D)DownloadHandlerTexture.GetContent(www);
		SetImageTexture(img, tex);
	}

	// Set Image.sprite from a Texture2D param
	static public void SetImageTexture(Image img, Texture2D tex)
	{
		if (img == null || tex == null)
			return;

		Sprite spr = Sprite.Create(
				tex, 
				new Rect(0.0f, 0.0f, tex.width, tex.height), 
				new UnityEngine.Vector2(0.5f, 0.5f));

        if (spr == null)
        {
            Debug.LogError("Cannot create sprite");
			return;
        }

        img.sprite = spr;
		img.preserveAspect = true;
	}

	private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
		if (type != LogType.Warning && type != LogType.Error)
			return;

		if (logs.Count >= logLines)
			logs.Dequeue();

		logs.Enqueue(logString);

		string val = "";
		foreach (string s in logs)
			val += s + "\n";

		textLog.text = val;
    }

	// Download the image from a URL and set it to img
	private IEnumerator GetRarityRequest(ArcadianInfo info)
	{
		if (info == null || info.attributes == null)
			yield break;

		//string url = "http://localhost:3000/rarity?";
		string url = "https://arcadians-rarity.herokuapp.com/rarity?";

		int len = info.attributes.Count;
		for (int i=0; i<len; i++)
		{
			var attr = info.attributes[i];

			if (i > 0) url += "&";

			string temp = attr.value.Replace("_", " ");

			url += $"{attr.trait_type}={temp}";
		}

		var www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();

		rarityResult = JsonUtility.FromJson<RarityResult>(www.downloadHandler.text);
		FillRarityResult(rarityResult);
	}

	private void FillRarityResult(RarityResult r)
	{
		if (r == null) return;

		if (textRarity != null) textRarity.text = r.rarity;
		if (textScore != null) textScore.text = r.score.ToString();
		
		FillTextParts(r.attributes, textParts);
	}

	private void FillTextParts(List<Attribute> attributes, Text text)
	{
		if (text == null) return;
		if (attributes == null) return;

		string str = "";
		int len = attributes.Count;
		for (int i=0; i<len; i++)
		{
			var attr = attributes[i];
			if (i > 0) str += "\n";
			str += $"{attr.trait_type}: {attr.value} ({attr.rarity})";
		}

		text.text = str;
	}
}
