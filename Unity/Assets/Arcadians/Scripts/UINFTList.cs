using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Runtime.InteropServices;

namespace OPGames.Arcadians
{

public class UINFTList : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void JSPasteWallet(string gettext);

	[SerializeField] private GameObject loading;
	[SerializeField] private GameObject listItemPrefab;
	[SerializeField] private Transform listParent;
	[SerializeField] private TMP_InputField inputField;

	[SerializeField] private string nftContract = "0xc3C8A1E1cE5386258176400541922c414e1B35Fd";
	[SerializeField] private string nftChain = "eth";

	private List<GameObject> listClones = new List<GameObject>();

	public void OnBtnMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	public void OnSearch()
	{
		if (inputField == null || string.IsNullOrEmpty(inputField.text)) return;
		Search();
	}

	private async UniTaskVoid Search()
	{
		loading.SetActive(true);
		DisableAllItems();

		var list = await MoralisNFTQuery.QueryWallet(inputField.text, nftContract, nftChain);
		if (list == null)
		{
			loading.SetActive(false);
			return;
		}

		foreach (var item in list)
		{
			if (item == null) continue;

			GameObject go = GetListItem();
			if (go == null) continue;

			var listItem = go.GetComponent<UINFTListItem>();
			if (listItem == null) continue;

			listItem.Fill(item);
		}
		loading.SetActive(false);
	}

	public void OnClear()
	{
		inputField.text = "";
	}

	public void OnPaste()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		JSPasteWallet("wallet");
#else
		Debug.Log("WebGL Only");
#endif
	}

	public void PasteWallet(string str)   
	{ 
		inputField.text = str; 
	}

	private void DisableAllItems()
	{
		foreach (var item in listClones)
			item.SetActive(false);
	}

	private GameObject GetListItem()
	{
		foreach (var item in listClones)
		{
			if (item.activeSelf == false)
			{
				item.SetActive(true);
				return item;
			}
		}

		// still here, create a new one
		GameObject clone = Instantiate(listItemPrefab, listParent);
		listClones.Add(clone);
		clone.SetActive(true);
		return clone;
	}
}

}
