using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GamePlayView : MonoBehaviour {


	public LocalizationText mTitle;
	public Text mChance;
	public Text mWin;
	public Text mWinFee;
	public Text mBigWin;
	public Text mBigWinHint;
	public Text mBalance;
	public InputField mBetInput;

	public List<GameObject> objList;
	public List<string> titleList;


	private float betStep = 0.01f; //下注加减的步长
	private float mCurrentBet = 0.1f; //当前下注金额

	private float mWinChanceValue = 0f;
	private float mWinValue = 0f;
	private float mCurrentBigWinValue = 0f;

	private float mMinBet = 0.01f;
	private float mMaxBet = 5.00f;

	private Color mSelectedColor = new Color(32/255f, 187/255f, 201/255f, 1f);
	private Color mDiselectedColor = Color.white;

	void Start() {
		RegisterNotifications();
		UpdateInfo();
		InitDice1Click();
		InitDice2Click();
	}

	private void RegisterNotifications() {
		NotificationCenter.DefaultCenter().AddObserver("RefreshBalance", RefreshBalance);
	}
	
	// 重置打开界面, 每次gameidx不一致时调用
	private void ResetData() {
		mWinChanceValue = 0f;
		mWinValue = 0f;
		mCurrentBigWinValue = 0f;
		SetCoinToggleState();
		SetNumberSliderState();
		SetDice1ButtonState();
		SetDice2ButtonState();
		UpdateInfo();
		UpdateBalance();
	}

	#region 游戏类型
	public enum GameIndex {
		Empty = -1,
		Coin = 0,
		Number,
		OneDice,
		TwoDice
	}
	private GameIndex mGameIdx = GameIndex.Empty;
	public void SetGameIdx(int idx) {
		if (idx < 0 || idx > 3) {
			Debug.LogError("idx范围超过 SetGameIdx " + idx);
			idx = 0;
		}
		if ((int)mGameIdx != idx) {
			mGameIdx = (GameIndex)idx;
			ResetData();
		} else {
			mGameIdx = (GameIndex)idx;
		}
		// mTitle.text = LocalizationManager.Instance.GetValue(titleList[idx]);
		mTitle.SetKey(titleList[idx]);
		ViewManager.ReplaceView(objList[idx], gameObject.name);
	}
	#endregion

	#region 猜硬币相关
	private int mCoinSelectIdx = 1; // 0 未选择, 1 正面, 2 反面
	public void OnCoinClick(Toggle toogle) {
		int idx = int.Parse(toogle.name);
		if (idx != 1 && idx != 2) {
			Debug.LogError("OnCoinClick idx outof range " + idx.ToString());
			return;
		}
		mCoinSelectIdx = idx;
	}

	private void SetCoinToggleState() {
		if (mCoinSelectIdx != 1 && mCoinSelectIdx != 2) {
			return;
		}
		Toggle toggle = transform.Find("Coin/" + mCoinSelectIdx).GetComponent<Toggle>();
		toggle.isOn = true;
	}

	private void OnBetCoin() {
		// TODO 下注猜硬币
		// 下注量: mCurrentBet, 正反面 mCoinSelectIdx
		TransactionManager.Instance.OnBetCoin(mCoinSelectIdx, mCurrentBet);
	}
	#endregion

	#region 猜数字相关
	public Slider mRateSlider;
	private int mSliderValue = 50;

	private void SetNumberSliderState() {
		mRateSlider.value = mSliderValue;
	}
	public void OnRateSliderValueChange() {
		mSliderValue = (int)mRateSlider.value;
		mSliderValue = mSliderValue < 1 ? 1 : mSliderValue;
		mSliderValue = mSliderValue > 97 ? 97 : mSliderValue;
		UpdateInfo();
	}
	private void OnBetNumber() {
		// TODO 下注猜数字
		TransactionManager.Instance.OnBetNumber(mSliderValue, mCurrentBet);
	}

	#endregion


	#region 一个骰子相关
	private int[] mSelectDicesOne = {1,0,0,0,0,0};

	private void SetDice1ButtonState() {
		for (int i = 1; i <= 6; i++) {
			GameObject selectedImg = transform.Find("Dice1/Grid/" + i + "/SelectedImg").gameObject;
			Text text = transform.Find("Dice1/Grid/" + i + "/Text").GetComponent<Text>();
			if (mSelectDicesOne[i-1] == 1) {
				selectedImg.SetActive(true);
				text.color = mSelectedColor;
			} else {
				selectedImg.SetActive(false);
				text.color = mDiselectedColor;
			}
		}
	}

	private void InitDice1Click() {
		for (int i = 1; i <= 6; i++) {
			Button btn = transform.Find("Dice1/Grid/" + i).GetComponent<Button>();
			Utils.AddButtonClickEvent(btn, delegate() {
				OnDice1Click(btn);
			});
		}
	}
	public void OnDice1Click(Button btn) {
		int idx = int.Parse(btn.name);
		if (idx < 1 || idx > 6) {
			Debug.LogError("OnDice1Click idx out of range " +idx.ToString() );
			return;
		}
		int selectedCount = GetDice1SelectedCount();
		--idx;

		GameObject selectedImg = btn.transform.Find("SelectedImg").gameObject;
		Text text = btn.transform.Find("Text").GetComponent<Text>();
		if (mSelectDicesOne[idx] == 0) {
			if (selectedCount >= 5) {
				ViewManager.ShowMessageBox("最多只可选择5个");
				Debug.LogError("selectedCount " + selectedCount);
				return;
			}
			selectedImg.SetActive(true);
			text.color = mSelectedColor;
			mSelectDicesOne[idx] = 1;
		} else {
			selectedImg.SetActive(false);
			text.color = mDiselectedColor;
			mSelectDicesOne[idx] = 0;
		}

		UpdateInfo();
	}

	private int GetDice1SelectedCount() {
		int selectedCount = 0;
		for (int i = 0; i < mSelectDicesOne.Length; i++) {
			selectedCount += mSelectDicesOne[i];
		}
		return selectedCount;
	}
	private void OnBetDice1() {
		//TODO
		TransactionManager.Instance.OnBetDice1(mSelectDicesOne, mCurrentBet);
	}
	#endregion

	#region 两个骰子相关
	private int[] mSelectDicesTwo = {1,0,0,0,0,0,0,0,0,0,0};
	private void SetDice2ButtonState() {
		for (int i = 2; i <= 12; i++) {
			GameObject selectedImg = transform.Find("Dice2/Grid/" + i + "/SelectedImg").gameObject;
			Text text = transform.Find("Dice2/Grid/" + i + "/Text").GetComponent<Text>();
			if (mSelectDicesTwo[i-2] == 1) {
				selectedImg.SetActive(true);
				text.color = mSelectedColor;
			} else {
				selectedImg.SetActive(false);
				text.color = mDiselectedColor;
			}
		}
	}
	private void InitDice2Click() {
		for (int i = 2; i <= 12; i++) {
			Button btn = transform.Find("Dice2/Grid/" + i).GetComponent<Button>();
			Utils.AddButtonClickEvent(btn, delegate() {
				OnDice2Click(btn);
			});
		}
	}
	public void OnDice2Click(Button btn) {
		int idx = int.Parse(btn.name);
		if (idx < 2 || idx > 12) {
			Debug.LogError("OnDice1Click idx out of range " +idx.ToString() );
			return;
		}
		int selectedCount = GetDice2SelectedCount();
		idx = idx - 2;

		GameObject selectedImg = btn.transform.Find("SelectedImg").gameObject;
		Text text = btn.transform.Find("Text").GetComponent<Text>();
		if (mSelectDicesTwo[idx] == 0) {
			// 最多选10个
			if (selectedCount >= 10) {
				ViewManager.ShowMessageBox("最多只可选择10个");
				Debug.LogError("selectedCount " + selectedCount);
				return;
			}
			selectedImg.SetActive(true);
			text.color = mSelectedColor;
			mSelectDicesTwo[idx] = 1;
		} else {
			selectedImg.SetActive(false);
			text.color = mDiselectedColor;
			mSelectDicesTwo[idx] = 0;
		}
		UpdateInfo();
    }
	private int GetDice2SelectedCount() {
		int selectedCount = 0;
		for (int i = 0; i < mSelectDicesTwo.Length; i++) {
			selectedCount += mSelectDicesTwo[i];
		}
		return selectedCount;
	}
	private void OnBetDice2() {
		//TODO
		TransactionManager.Instance.OnBetDice2(mSelectDicesTwo, mCurrentBet);
	}
	#endregion

	#region 更新界面信息显示
	private void UpdateInfo() {
		UpdateBetInput();
		UpdateWinChance();
		UpdateWinEth();
		UpdateHintText();
	}

	private void UpdateWinChance() {
		if (mGameIdx == GameIndex.Coin) {
			mWinChanceValue = 0.5f;
		} else if (mGameIdx == GameIndex.Number) {
			mWinChanceValue = mSliderValue / 100f;
		} else if (mGameIdx == GameIndex.OneDice) {
			CalcOneDiceWinChance();
		} else if (mGameIdx == GameIndex.TwoDice) {
			CalcTwoDiceWinChance();
		}
		mWinChanceValue = (float)System.Math.Round(mWinChanceValue, 4);
		mChance.text = mWinChanceValue.ToString("0.00%");
	}

	// 计算一个骰子的获胜机会
	private void CalcOneDiceWinChance() {
		int selectedCount = GetDice1SelectedCount();
		mWinChanceValue = selectedCount / 6.0f;
	}

	private int[] twoDiceOdds = {1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1};
	private void CalcTwoDiceWinChance() {
		
		int count = 0;
		for (int i = 0; i < mSelectDicesTwo.Length; i++) {
			if (mSelectDicesTwo[i] == 1) {
				count += twoDiceOdds[i];
			}
		}
		mWinChanceValue = count / 36.0f;

	}

	private void UpdateWinEth() {
		if (mWinChanceValue == 0f) {
			mWinValue = 0f;
			mWin.text = mWinValue.ToString("0.0000") + "ETH";
			return;
		}
		float bet = mCurrentBet;
		if (mCurrentBet >= 0.1f) {
			// 大于0.1 扣1%费用 扣0.001以太幣累积大奖
			bet *= 0.99f;
			bet -= 0.001f;
		} else if (mCurrentBet >= 0.03 && mCurrentBet < 0.1f) {
			bet *= 0.99f;
		} else if (mCurrentBet < 0.03) {
			bet -= 0.0003f;
		}
		mWinValue = bet / mWinChanceValue;
		mWinValue = (float)System.Math.Round(mWinValue, 4);
		mWin.text = mWinValue.ToString("0.0000") + "ETH";
	}

	// 更新提示信息
	private void UpdateHintText() {
		if (mCurrentBet >= 0.1f) {
			mWinFee.text = "(1%费用，0.001以太幣累积大奖)";
		} else if (mCurrentBet >= 0.03f && mCurrentBet < 0.1f) {
			mWinFee.text = "(1%费用)";
		} else if (mCurrentBet == 0.02f) {
			mWinFee.text = "(1.5%费用)";
		} else if (mCurrentBet == 0.01f) {
			mWinFee.text = "(3%费用)";
		}

		// Debug.LogError("mCurrentBet " + mCurrentBet);
		if (mCurrentBet >= 0.1f) {
			mBigWinHint.text = "<color=#1ADF1CFF>" + "(0.1％的赢大奖机会!)" + "</color>";
		} else {
			mBigWinHint.text = "<color=#FF9999FF>" + "(投注0.10以太幣以获得资格)" + "</color>";
		}
	}

	private void UpdateBalance() {
		AccountManager.Instance.UpdateBalance((balance)=> {
			mBalance.text = balance.ToString("0.0000") + "ETH";
		});
	}

	private void RefreshBalance(Notification notification) {
		mBalance.text = AccountManager.Instance.GetBalance().ToString("0.0000") + "ETH";
	}
	#endregion

	#region 下注相关
	public void OnBetInputEndEdit() {
		float result;
		bool success = float.TryParse(mBetInput.text, out result);
		if (success) {
			mCurrentBet = result;
		}
		UpdateInfo();
	}

	public void UpdateBetInput() {
		float balance = (float)AccountManager.Instance.GetBalance();
		if (mCurrentBet > balance) {
			mCurrentBet = (float)System.Math.Round(balance - 0.005f, 2);
		}
		mCurrentBet = mCurrentBet < mMinBet ? mMinBet : mCurrentBet;
		mCurrentBet = mCurrentBet > mMaxBet ? mMaxBet : mCurrentBet;
		mCurrentBet = (float)System.Math.Round(mCurrentBet, 2);
		mBetInput.text = mCurrentBet.ToString("0.00");
	}

	public void OnBtnMinus() {
		mCurrentBet -= betStep;
		UpdateInfo();
	}

	public void OnBtnAdd() {
		mCurrentBet += betStep;
		UpdateInfo();
	}

	public void OnBtnBet() {
		if (!AccountManager.Instance.HasAccount() || string.IsNullOrEmpty(AccountManager.Instance.GetAddress())) {
			ViewManager.ShowMessageBox("未检测到账户, 请先创建账户", delegate {
				Game.Instance.ShowAccountView();
			});
			return;
		}
		if (mCurrentBet > (float)AccountManager.Instance.GetBalance()) {
			ViewManager.ShowMessageBox("余额不足");
			return;
		}
		if (mGameIdx == GameIndex.Coin) {
			OnBetCoin();
		} else if (mGameIdx == GameIndex.Number) {
			OnBetNumber();
		} else if (mGameIdx == GameIndex.OneDice) {
			OnBetDice1();
		} else if (mGameIdx == GameIndex.TwoDice) {
			OnBetDice2();
		} 
	}
	#endregion

	private int[] moduloArray = {2, 100, 6, 36};
	public void OnBtnHistory() {
		GameView.Instance.ToHistoryView(moduloArray[(int)mGameIdx]);
	}
	public void OnBtnClose() {
		GameView.Instance.OnBackToGameList();
	}
}
