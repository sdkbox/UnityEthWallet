using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpView : MonoBehaviour {

	public void OnLanguageChange(bool toEnglish) {
		Debug.Log("OnLanguageChange " + toEnglish);
		LocalizationManager.Instance.ChangeLanguage(toEnglish);
	}

	public void OnBtnContractClick() {
		Application.OpenURL("https://github.com/okami2018/UDice");
	}
}
