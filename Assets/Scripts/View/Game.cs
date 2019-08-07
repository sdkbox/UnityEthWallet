using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

public class Game : MonoBehaviour {

	public GameObject mAccountView;
	public GameObject mGameView;
	public GameObject mHelpView;
	public ToggleView mToggleView;
	private static Game instance;
	public static Game Instance {
		get {
			if (instance == null) {
				GameObject g = GameObject.Find("Canvas");
				instance = g.GetComponent <Game> ();
				
			}
			return instance;
		}
	}
	void Awake() {
		SaveLog();
		mAccountView.SetActive(false);
		mGameView.SetActive(false);
		mHelpView.SetActive(false);
	}
	void Start () {
		if (AccountManager.Instance.CheckSavedAccount()){
			ShowGameView();
		} else {
			ShowAccountView();
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			ViewManager.ShowMessageBox("退出游戏?", delegate {
				Application.Quit();
			}, delegate {});
		}
	}

	private void SaveLog() {
		// 把日志打印出来
		string logPath = Application.persistentDataPath + "/outLog.txt";
		File.WriteAllText(logPath, Environment.TickCount / 10 % 10000 + "|" + System.DateTime.Now + "\r\n");
		Application.logMessageReceived += (condition, stackTrace, type) => {
			File.AppendAllText(logPath, condition + "\r\n", Encoding.UTF8);
		};
	}

#region 切换界面显示
	public void ShowGameView() {
		ViewManager.ReplaceView(mGameView, gameObject.name);
		mToggleView.mToggleGame.isOn = true;
	}

	public void ShowAccountView() {
		ViewManager.ReplaceView(mAccountView, gameObject.name);
		mToggleView.mToggleAccount.isOn = true;
	}
	public void ShowHelpView() {
		ViewManager.ReplaceView(mHelpView, gameObject.name);
		mToggleView.mToggleHelp.isOn = true;
	}
#endregion

}
