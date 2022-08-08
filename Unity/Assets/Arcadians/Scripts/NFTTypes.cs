using System;
using System.Collections;
using UnityEngine;

namespace OPGames.Arcadians
{

[System.Serializable]
public class NFTItemData
{
	public string Chain;
	public string TokenId;
	public string Contract;
	public string URI;

	public string Name;
	public string Description;
	public string ImageURL;
	public string CharClass;
	public string Metadata;

	public Texture2D Texture;
	public Sprite Spr;
	public Action OnUpdate;

	public string UniqueId
	{
		get { return Contract + "-" + TokenId; }
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2}", Chain, Contract, TokenId);
	}
}

}
