#pragma warning disable CS4014
using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace OPGames.Arcadians
{

[System.Serializable]
public class MoralisNFTResponse
{
	[System.Serializable]
	public class Item
	{
		public string token_id;
		public string token_address;
		public string metadata;
		public string name;

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", token_id, token_address, name);
		}
	}

	public int total;
	public int page;
	public int page_size;
	public string cursor;
	public Item[] result;
}

public class MoralisNFTQuery
{
	private const int NFT_LIMIT = 50;

	static private string GetApiKey()
	{
		var config = Resources.Load<MoralisConfig>("MoralisConfig");
		if (config == null) return "";
		else return config.ApiKey;
	}

	static public async UniTask<List<NFTItemData>> QueryWallet(string wallet, string nftContract, string chain = "eth")
	{
		Debug.Log("MoralisNFTQuery:QueryWallet");

		string apiKey = GetApiKey();
		if (string.IsNullOrEmpty(apiKey))
		{
			Debug.LogError("Please provide a valid Api Key for Moralis");
			return null;
		}

		List<NFTItemData> list = new List<NFTItemData>();
		MoralisNFTResponse response = null;

		do
		{
			// This supports paging
			const string api = "https://deep-index.moralis.io/api/v2/{0}/nft/{1}?chain={2}&format=decimal&limit={3}";
			string url = string.Format(api, wallet, nftContract, chain, NFT_LIMIT);

			if (response != null && string.IsNullOrEmpty(response.cursor) == false)
				url += "&cursor=" + response.cursor;

			//Debug.Log(url);

			using (var www = UnityWebRequest.Get(url))
			{
				www.SetRequestHeader("X-API-KEY", apiKey);
				www.SetRequestHeader("accept", "application/json");

				await www.SendWebRequest();
				var json = www.downloadHandler.text;
				if (string.IsNullOrEmpty(www.error) == false)
				{
					Debug.LogError(www.error);
					break;
				}

				response = JsonUtility.FromJson<MoralisNFTResponse>(json);
				if (response == null || response.result == null)
				{
					Debug.LogError($"Json response is invalid {json}");
					break;
				}

				foreach (var a in response.result)
				{
					NFTItemData item = ToItemData(a, chain);

					Debug.LogFormat("Found {0}, {1}", a.ToString(), item.CharClass);
					list.Add(item);
				}
			}
		}
		while (response.result.Length == NFT_LIMIT);
		return list;
	}

	static public async UniTask<List<NFTItemData>> QueryNFT(string contract, string[] tokenIds, string chain="eth")
	{
		string apiKey = GetApiKey();
		if (string.IsNullOrEmpty(apiKey))
		{
			Debug.LogError("Please provide a valid Api Key for Moralis");
			return null;
		}

		List<NFTItemData> list = new List<NFTItemData>();

		if (contract.IndexOf("0x") != 0)
			return list;

		const string api = "https://deep-index.moralis.io/api/v2/nft/{0}/{1}?chain={2}&format=decimal";

		foreach (string tokenId in tokenIds)
		{
			string url = string.Format(api, contract, tokenId, chain);

			using (var www = UnityWebRequest.Get(url))
			{
				www.SetRequestHeader("X-API-KEY", apiKey);
				www.SetRequestHeader("accept", "application/json");

				await www.SendWebRequest();
				var json = www.downloadHandler.text;

				if (string.IsNullOrEmpty(www.error) == false)
				{
					Debug.LogError(www.error);
					continue;
				}

				var a = JsonUtility.FromJson<MoralisNFTResponse.Item>(json);
				if (a == null)
				{
					Debug.LogError($"Json response is invalid {json}");
					continue;
				}

				NFTItemData item = ToItemData(a, chain);

				Debug.LogFormat("Found {0}, {1}", a.ToString(), item.CharClass);
				list.Add(item);
			}
		}

		return list;
	}

	static public NFTItemData ToItemData(MoralisNFTResponse.Item a, string chain)
	{
		var info = JsonUtility.FromJson<ArcadianInfo>(a.metadata);

		NFTItemData item = new NFTItemData();
		item.Chain       = chain;
		item.TokenId     = a.token_id;
		item.Contract    = a.token_address;
		item.Name        = $"{a.name} #{a.token_id}";
		item.Metadata    = a.metadata;
		item.ImageURL    = info.image;

	//	Context.imageDownloader.DownloadWithCB(info.image, (info) =>
	//			{
	//				item.Texture = info.Tex;
	//			});

		//Debug.Log(info.image);
		return item;
	}
}

}
