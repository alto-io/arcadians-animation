using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OPGames.Arcadians
{

public class UIArcadiansList : MonoBehaviour
{
	[SerializeField] private GameObject listItemPrefab;
	[SerializeField] private Transform listParent;
	[SerializeField] private Input inputField;

	private List<GameObject> listFree = new List<GameObject>();
	private List<GameObject> listUsed = new List<GameObject>();

	public void OnSearch()
	{
		if (inputField == null) return;


	}

	public void OnBack()
	{
	}
}

}
