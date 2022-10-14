using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OPGames.Arcadians
{

public class UINFTListItem : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private TextMeshProUGUI textLabel;

	private NFTItemData nft;

	public void Fill(NFTItemData nft)
	{
		if (nft == null) return;

		this.nft = nft;
		this.nft.OnUpdate += UpdateNFT;

		UpdateNFT();
	}

	private void UpdateNFT()
	{
		if (image != null)
			image.sprite = nft.Spr;

		if (textLabel != null)
			textLabel.text = nft.Name;
	}
}

}
