using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using OPGames.Arcadians;

[System.Serializable]
public class RarityResult
{
	[System.Serializable]
	public class Stats
	{
		public float hpVal;
		public int attackRange;
		public float attackSpeed;
		public float moveSpeed;
		public float damageVal;
		public float defenseVal;
		public float critChance;
	}

	public string className;
	public string rarity;
	public float score;
	public List<Attribute> attributes;
	public Stats statsBase;
	public Stats statsFinal;
}

public class UIDebug : MonoBehaviour
{
	[SerializeField] private Arcadian arcadian;
	[SerializeField] private Text textName;
	[SerializeField] private Text textClass;
	[SerializeField] private Text textParts;
	[SerializeField] private Text textRarity;
	[SerializeField] private Text textScore;
	[SerializeField] private Text textStats;
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

		textParts.text = "-";
		textStats.text = "-";
		textRarity.text = "-";
		textScore.text = "-";

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
		FillTextStats(r, textStats);
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

	private void FillTextStats(RarityResult result, Text text)
	{
		if (text == null) return;
		if (result == null) return;
		if (result.statsBase == null) return;
		if (result.statsFinal == null) return;

		var statsBase = result.statsBase;
		var statsFinal = result.statsFinal;
		var statsBonus = new RarityResult.Stats();

		statsBonus.hpVal = statsFinal.hpVal - statsBase.hpVal;
		statsBonus.attackRange = statsFinal.attackRange - statsBase.attackRange;
		statsBonus.attackSpeed = statsFinal.attackSpeed - statsBase.attackSpeed;
		statsBonus.moveSpeed = statsFinal.moveSpeed - statsBase.moveSpeed;
		statsBonus.damageVal = statsFinal.damageVal - statsBase.damageVal;
		statsBonus.defenseVal = statsFinal.defenseVal - statsBase.defenseVal;
		statsBonus.critChance = statsFinal.critChance - statsBase.critChance;

		string str = FormatStat("HP", statsBase.hpVal, statsBonus.hpVal);
		str += FormatStat("Attack Range", statsBase.attackRange, statsBonus.attackRange);
		str += FormatStat("Attack Speed", statsBase.attackSpeed, statsBonus.attackSpeed);
		str += FormatStat("Move Speed", statsBase.moveSpeed, statsBonus.moveSpeed);
		str += FormatStat("Damage", statsBase.damageVal, statsBonus.damageVal);
		str += FormatStat("Defense", statsBase.defenseVal, statsBonus.defenseVal);
		str += FormatStat("CritChance", statsBase.critChance, statsBonus.critChance, true);

		text.text = str;
	}

	private string FormatStat(string label, float val, float bonus, bool isPercent = false)
	{
		string bonusColor = "orange";
		string format = "{0}: {1:G4}\n";
		if (bonus > 0) format = "{0}: {1:G4} <color={3}>(+{2:G4})</color>\n";

		if (isPercent)
		{
			val *= 100.0f;
			bonus *= 100.0f;

			if (bonus > 0) format = "{0}: {1:G4}% <color={3}>(+{2:G4}%)</color>\n";
			else           format = "{0}: {1:G4}%\n";
		}

		return string.Format(format, label, val, bonus, bonusColor);
	}
	
	private string FormatStat(string label, int val, int bonus)
	{
		string bonusColor = "orange";
		string format = "{0}: {1}\n";
		if (bonus > 0) format = "{0}: {1} <color={3}>(+{2})</color>\n";
		return string.Format(format, label, val, bonus, bonusColor);
	}
}
