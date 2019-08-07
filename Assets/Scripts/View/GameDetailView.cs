using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailData {
	public string mAddress;
	public string mBetMoney;
	public string mBetCase;
	public string mBetTrx;
	public string mSha3Secret;
	public string mSecret;
	public string mSha3BetBlock;
	public string mSha3Mod;
	public string mWin;
}

public class GameDetailView : MonoBehaviour {

	private DetailData mDetailData;

	public Text mAddress;
	public Text mBetMoney;
	public Text mBetCase;
	public Text mBetTrx;
	public Text mSha3Secret;
	public Text mSecret;
	public Text mSha3BetBlock;
	public Text mSha3Mod;
	public Text mWin;

	public void SetDetailData(DetailData data) {
		mDetailData = data;
		ShowDetailData();
	}

	public void ShowDetailData() {
		mAddress.text = mDetailData.mAddress;
		mBetMoney.text = mDetailData.mBetMoney;
		mBetCase.text = mDetailData.mBetCase;
		mBetTrx.text = mDetailData.mBetTrx;
		mSha3Secret.text = mDetailData.mSha3Secret;
		mSecret.text = mDetailData.mSecret;
		mSha3BetBlock.text = mDetailData.mSha3BetBlock;
		mSha3Mod.text = mDetailData.mSha3Mod;
		mWin.text = mDetailData.mWin;
	}
}
