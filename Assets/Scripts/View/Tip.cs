using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tip : MonoBehaviour {

	public Text mMessage;
	private int mSeconds;

	public void Setup(string msg, int seconds) {
		mMessage.text = msg;
		mSeconds = seconds;
		StopAllCoroutines();
		StartCoroutine(DelayClose());
	}

	private IEnumerator DelayClose() {
		yield return new WaitForSeconds(mSeconds);
		gameObject.SetActive(false);
	}
}
