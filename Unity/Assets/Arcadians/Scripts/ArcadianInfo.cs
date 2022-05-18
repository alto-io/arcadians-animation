using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.Arcadians
{

[System.Serializable]
public class Attribute
{
	public string trait_type;
	public string value;
	public string rarity;
}

[System.Serializable]
public class ArcadianInfo
{
	public List<Attribute> attributes;
	public string description;
	public string name;
	public string image;

	// added for convenience, not part of json response
	public bool isFemale;
	public string className;
}


}
