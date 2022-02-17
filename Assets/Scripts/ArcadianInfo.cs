using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Attribute
{
	public string trait_type;
	public string value;
}

[System.Serializable]
public class ArcadianInfo
{
	public List<Attribute> attributes;
	public string description;
	public string name;
	public string image;
}

