using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OPGames.Arcadians
{

public class UIArcadiansListItem : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private TextMeshProUGUI textLabel;

	public void Fill(Sprite spr, string label)
	{
		if (image != null && spr != null)
			image.sprite = spr;

		if (textLabel != null)
			textLabel.text = label;
	}
}

}
