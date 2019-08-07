using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleView : MonoBehaviour {
	public Toggle mToggleGame;
	public Toggle mToggleAccount;
	public Toggle mToggleHelp;

	private Color selectedColor = new Color(32/255f, 187/255f, 201/255f, 1f);
	private Color diselectedColor = new Color(1f, 1f, 1f, 0.3f);

	


	void Start() {
		mToggleGame.onValueChanged.AddListener(delegate {
			OnToggleClick(mToggleGame);
		});
		mToggleAccount.onValueChanged.AddListener(delegate {
			OnToggleClick(mToggleAccount);
		});
		mToggleHelp.onValueChanged.AddListener(delegate {
			OnToggleClick(mToggleHelp);
		});
	}

	private void OnToggleClick(Toggle toggle) {
		if (!toggle.isOn) {
			toggle.transform.Find("Label").GetComponent<Text>().color = diselectedColor;
			return;
		}
		toggle.transform.Find("Label").GetComponent<Text>().color = selectedColor;
		if (toggle.gameObject.name == "ToggleGame") {
			Game.Instance.ShowGameView();
		} else if (toggle.gameObject.name == "ToggleAccount") {
			Game.Instance.ShowAccountView();
		} else if (toggle.gameObject.name == "ToggleHelp") {
			Game.Instance.ShowHelpView();
		}
	}
}
