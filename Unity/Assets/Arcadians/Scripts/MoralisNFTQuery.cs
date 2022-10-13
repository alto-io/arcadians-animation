#pragma warning disable CS4014
using System;
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
public class NFTAttribute
{
	public string trait_type;
	public string value;
	public string rarity;
}

[System.Serializable]
public class NFTMetadata
{
	public List<NFTAttribute> attributes;
	public string description;
	public string name;
	public string image;
}

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

			Debug.Log(url);

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

				Debug.Log(json);

				response = JsonUtility.FromJson<MoralisNFTResponse>(json);
				if (response == null || response.result == null)
				{
					Debug.LogError($"Json response is invalid {json}");
					break;
				}

				foreach (var a in response.result)
				{
					if (a == null) continue;

					NFTItemData item = await ToNFTItemData(a, chain);
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

				NFTItemData item = await ToNFTItemData(a, chain);

				Debug.LogFormat("Found {0}, {1}", a.ToString(), item.CharClass);
				list.Add(item);
			}
		}

		return list;
	}

	static public async UniTask<NFTItemData> ToNFTItemData(MoralisNFTResponse.Item a, string chain)
	{

		NFTItemData item = new NFTItemData();
		item.Chain       = chain;
		item.TokenId     = a.token_id;
		item.Contract    = a.token_address;
		item.Name        = $"{a.name} #{a.token_id}";
		item.Metadata    = a.metadata;

		if (string.IsNullOrEmpty(a.metadata))
		{
			RequestURI(item, chain);
			await UniTask.Delay(TimeSpan.FromSeconds(2)); // avoid rate limiting
		}
		else
		{
			ReadNFTMetadata(item);
		}

		return item;
	}

	static private async UniTaskVoid RequestURI(NFTItemData nft, string chain)
	{
		string chainForURI = "ethereum";
		if (chain != "eth")
			chainForURI = chain;

		// testnet needs the RPC parameter
		string uri = await ERC721.URI(chainForURI, "mainnet", nft.Contract, nft.TokenId /*, rpc*/ );
		uri = SanitizeURL(uri);

		Debug.Log($"Requesting URI for {nft.TokenId}\n{uri}");
		using (var www2 = UnityWebRequest.Get(uri))
		{
			await www2.SendWebRequest();
			nft.Metadata = www2.downloadHandler.text;

			ReadNFTMetadata(nft);
		}

		// also tell moralis to resync the NFT
		ResyncNFT(nft.Contract, nft.TokenId, chain);
	}

	static private void ReadNFTMetadata(NFTItemData nft)
	{
		NFTMetadata info = null;
		if (string.IsNullOrEmpty(nft.Metadata) == false)
			info = JsonUtility.FromJson<NFTMetadata>(nft.Metadata);

		if (info != null)
			nft.ImageURL = SanitizeURL(info.image);

		Debug.Log(nft.ImageURL);
		ImageDownloader.Instance.DownloadWithCB(nft.ImageURL, (info)=>
		{
			if (info != null)
			{
				nft.Texture = info.Tex; 
				nft.Spr = info.Spr; 
				if (nft.OnUpdate != null)
					nft.OnUpdate();
			}
		});
	}

	static private async UniTaskVoid ResyncNFT(string nftContract, string tokenId, string chain = "eth")
	{
		const string api = "https://deep-index.moralis.io/api/v2/nft/{0}/{1}/metadata/resync?chain={2}&flag=uri&mode=async";
		string apiKey = GetApiKey();
		string url = string.Format(api, nftContract, tokenId, chain);
		using (var www = UnityWebRequest.Get(url))
		{
			www.SetRequestHeader("X-API-KEY", apiKey);
			www.SetRequestHeader("accept", "application/json");

			Debug.Log($"ResyncNFT {url}");

			await www.SendWebRequest();
			var json = www.downloadHandler.text;

			if (string.IsNullOrEmpty(www.error) == false)
			{
				Debug.LogError(www.error);
				Debug.LogError($"Response: {json}");
			}
		}
	}

	static public string SanitizeURL(string url)
	{
		url = url.Trim();
		if (url.IndexOf("ipfs://") == 0)
		{
			string ipfsPath = url.Replace("ipfs://", "");
			url = "https://ipfs.io/ipfs/" + ipfsPath;
		}

		return url;
	}
}

}
