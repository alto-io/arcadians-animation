using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace OPGames.Arcadians
{

public class UIWebLogin : MonoBehaviour
{
	[SerializeField] private string welcomeMessage = "Welcome to Arcadians";
	[SerializeField] private TextMeshProUGUI textWallet;

	public void OnBtnMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	static private string GetNonce()
	{
		//https://answers.unity.com/questions/965798/generate-a-random-string-from-a-specified-length.html
		string result = "";
		const string glyphs= "0123456789abcdef";
		int charCount = 10;
		for(int i=0; i<charCount; i++)
			result += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];

		return result;
	}


#if UNITY_EDITOR
    public void OnLogin()
    {
		textWallet.text = "Does not work in the Editor";
    }

#elif UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void Web3Connect();

    [DllImport("__Internal")]
    private static extern string ConnectAccount();

    [DllImport("__Internal")]
    private static extern void SetConnectAccount(string value);

    private int expirationTime;
    private string account; 

    public void OnLogin()
    {
		Debug.Log("OnLogin");
        Web3Connect();
        OnConnected();
    }

    async private void OnConnected()
    {
        account = ConnectAccount();
        while (account == "") {
            await new WaitForSeconds(1f);
            account = ConnectAccount();
        };

		Debug.Log($"OnConnected {account}");

		// Create a message and let the user sign
		string message = $"{welcomeMessage}\nNonce: {GetNonce()}";
		string signature = await Web3GL.Sign(message);
		string verifyWallet = await EVM.Verify(message, signature);
		
		if (verifyWallet.ToLower() == account.ToLower())
		{
			Debug.Log("Signature is valid");
			PlayerPrefs.SetString("Account", account);
			SetConnectAccount("");
			if (textWallet != null)
				textWallet.text = account;
		}
		else
		{
			Debug.LogError("Signature is invalid");
			Debug.LogError(verifyWallet);
			Debug.LogError(signature);

			if (textWallet != null)
				textWallet.text = "Signature is invalid";
		}
    }
#endif

}

}
