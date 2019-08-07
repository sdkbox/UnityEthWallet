using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour {
	public GameObject mGameListView;
	public GameObject mGamePlayView;
	public GameObject mGameHistoryView;
	public GameObject mGameDetailView;

	private static GameView instance;
	public static GameView Instance {
		get {
			if (instance == null) {
				GameObject g = GameObject.Find("Canvas/GameView");
				instance = g.GetComponent <GameView> ();
				
			}
			return instance;
		}
	}
	void Awake() {
		mGameListView.SetActive(false);
		mGamePlayView.SetActive(false);
		mGameHistoryView.SetActive(false);
		mGameDetailView.SetActive(false);
	}
	void Start() {
		ViewManager.OpenView(mGameListView, gameObject.name);
	}
	// idx : 游戏idx
	public void OnGameClick(int idx) {
		GamePlayView gp = mGamePlayView.GetComponent<GamePlayView>();
		gp.SetGameIdx(idx);
		ViewManager.ReplaceView(mGamePlayView, gameObject.name);
	}

	public void OnBackToGameList() {
		ViewManager.ReplaceView(mGameListView, gameObject.name);
	}

	public void OnBackToGamePlay() {
		ViewManager.ReplaceView(mGamePlayView, gameObject.name);
	}
	public void OnBackToGameHistory() {
		ViewManager.ReplaceView(mGameHistoryView, gameObject.name);
	}

	public void ToHistoryView(int modulo) {
		GameHistoryView ghv = mGameHistoryView.GetComponent<GameHistoryView>();
		ghv.SetModulo(modulo);
		ViewManager.ReplaceView(mGameHistoryView, gameObject.name);
	}

	public void ToDetailView() {
		ViewManager.ReplaceView(mGameDetailView, gameObject.name);
	}
}
