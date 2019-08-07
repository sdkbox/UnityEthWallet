using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MessageBox : MonoBehaviour {

	public Text mMessage;
	public Button mBtnCancle;
	public Button mBtnConfirm;

	private UnityAction mCancleAction;
	private UnityAction mConfirmAction;

	public void Setup(string msg, UnityAction confirmAction = null, UnityAction cancleAction = null) {
		mCancleAction = cancleAction;
		mConfirmAction = confirmAction;
		SetMessage(msg);
		CheckOneBtn();
	}

	private void SetMessage(string msg) {
		mMessage.text = msg;
	}

	private void CheckOneBtn() {
		if (mConfirmAction == null || mCancleAction == null) {
			mBtnCancle.gameObject.SetActive(false);
		} else {
			mBtnCancle.gameObject.SetActive(true);
		}
		
	}

	public void OnCancleBtnClick() {
		if (mCancleAction != null) {
			mCancleAction();
		}
		OnClose();
	}

	public void OnConfirmBtnClick() {
		if (mConfirmAction != null) {
			mConfirmAction();
		}
		OnClose();
	}

	private void OnClose() {
		gameObject.SetActive(false);
	}
}
