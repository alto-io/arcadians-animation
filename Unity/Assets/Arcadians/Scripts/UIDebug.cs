using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using OPGames.Arcadians;

public class UIDebug : MonoBehaviour
{
	[SerializeField] private Arcadian arcadian;
	[SerializeField] private Text textName;
	[SerializeField] private Text textClass;
	[SerializeField] private Image image;
	[SerializeField] private InputField input;

	private void Start()
	{
		OnBtnRandom();
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
		int id = Random.Range(1,3733);
		input.text = id.ToString();
		Load(id);
	}

	public void Load(int id)
	{
		if (arcadian == null) return;

		arcadian.Load(id, (info) =>
		{
			if (info == null) return;

			textName.text = info.name;
			textClass.text = info.className;
			StartCoroutine(UpdateImage(image, info.image));
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
}
