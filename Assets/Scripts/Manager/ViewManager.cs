using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ViewManager {
	private static Dictionary<string, Stack<GameObject>> viewDict = new Dictionary<string, Stack<GameObject>> ();
	private static GameObject currentView;
	private static Stack<GameObject> currentStack;

	private static WaitTip mWaitTip;
	private static MessageBox mMessageBox;
	private static Tip mTip;

	public static void OpenView(GameObject view, string viewGroup) {
		view.SetActive(true);
		currentStack = GetCurrentStack(viewGroup);
		currentStack.Push(view);
	}

	public static void ReplaceView(GameObject view, string viewGroup) {
		CloseCurrentView(viewGroup);
		OpenView(view, viewGroup);
	}

	public static void CloseCurrentView(string viewGroup) {
		currentStack = GetCurrentStack(viewGroup);
		if (currentStack.Count > 0) {
			currentView = currentStack.Pop();
			if (currentView) {
				currentView.SetActive(false);
			}
		}
	}
	private static Stack<GameObject> GetCurrentStack(string viewGroup) {
		if (!viewDict.ContainsKey(viewGroup)) {
			Stack<GameObject> s = new Stack<GameObject>();
			viewDict.Add(viewGroup, s);
		}
		return viewDict[viewGroup];
	}


	public static void ShowWaitTip(string msg = "", int seconds = 30, UnityAction timeOutAction = null) {
		if (mWaitTip == null) {
			GameObject canvas = GameObject.Find("Canvas");
			mWaitTip = canvas.transform.Find("WaitTip").GetComponent<WaitTip>();
		}
		if (mWaitTip == null) {
			Debug.LogError("mWaitTip is null");
			return;
		}
		mWaitTip.gameObject.SetActive(true);
		mWaitTip.Setup(msg, seconds, timeOutAction);
	}

	public static void CloseWaitTip() {
		if (mWaitTip == null) {
			Debug.LogError("mWaitTip is null");
			return;
		}
		mWaitTip.gameObject.SetActive(false);
	}

	public static void ShowMessageBox(string msg, UnityAction confirmAction = null, UnityAction cancleAction = null) {
		if (mMessageBox == null) {
			GameObject canvas = GameObject.Find("Canvas");
			mMessageBox = canvas.transform.Find("MessageBox").GetComponent<MessageBox>();
		}
		if (mMessageBox == null) {
			Debug.LogError("mMessageBox is null");
			return;
		}
		mMessageBox.gameObject.SetActive(true);
		mMessageBox.Setup(msg, confirmAction, cancleAction);
	}

	public static void CloseMessageBox() {
		if (mMessageBox == null) {
			Debug.LogError("mMessageBox is null");
			return;
		}
		mMessageBox.gameObject.SetActive(false);
	}

	public static void ShowTip(string msg = "", int seconds = 2) {
		if (mTip == null) {
			GameObject canvas = GameObject.Find("Canvas");
			mTip = canvas.transform.Find("Tip").GetComponent<Tip>();
		}
		if (mTip == null) {
			Debug.LogError("mTip is null");
			return;
		}
		mTip.gameObject.SetActive(true);
		mTip.Setup(msg, seconds);
	}

	public static void CloseTip() {
		if (mTip == null) {
			Debug.LogError("mTip is null");
			return;
		}
		mTip.gameObject.SetActive(false);
	}
}
