using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WaitTip : MonoBehaviour {

	public Text mMessage;
	private int mSeconds;

	private UnityAction mTimeOutAction;

	public void Setup(string msg, int seconds, UnityAction timeOutAction) {
		mMessage.text = msg;
		mSeconds = seconds;
		mTimeOutAction = timeOutAction;
		StopAllCoroutines();
		StartCoroutine(DelayClose());
	}

	private IEnumerator DelayClose() {
		yield return new WaitForSeconds(mSeconds);
		if (mTimeOutAction != null) {
			mTimeOutAction();
		}
		gameObject.SetActive(false);
	}
}
