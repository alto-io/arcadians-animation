using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace OPGames.Arcadians
{

public class UIMainMenu : MonoBehaviour
{
	public void OnBtnAnimation()
	{
		SceneManager.LoadScene("AnimationAndRarity");
	}

	public void OnBtnWebLogin()
	{
		SceneManager.LoadScene("WebLogin");
	}

	public void OnBtnArcadiansList()
	{
		SceneManager.LoadScene("ArcadiansList");
	}
}

}
