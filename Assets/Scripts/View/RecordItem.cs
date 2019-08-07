using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using System;


[System.Serializable]
public class ItemData {
	public string mAddress;
	public string mBetCase;
	public string mBetMoney;
	public string mWin;
	public string mResult;
	public string mBigWin;
	public UInt32 mModulo;
	public string mBetMask;
	public string mRevealBlockHash;
}
public class RecordItem : MonoBehaviour {

	public Text mAddressText;
	public Text mBetCaseText;
	public Text mBetMoneyText;
	public Text mWinText;
	public Text mResultText;
	public Text mBigWinText;

	public ItemData mItemData;

	public void InitItem(ItemData itemData) {
		mAddressText.text = itemData.mAddress;
		
		BigInteger betMoney = BigInteger.Parse(itemData.mBetMoney);
		decimal moneyDec = Nethereum.Util.UnitConversion.Convert.FromWei(betMoney, 18);
		string moneyStr = moneyDec.ToString("0.00");
		mBetMoneyText.text = moneyStr + " ETH";

		if (string.IsNullOrEmpty(itemData.mWin) || itemData.mWin == "0") {
			mWinText.text = "0";
		} else {
			BigInteger win = BigInteger.Parse(itemData.mWin);
			decimal winDec = Nethereum.Util.UnitConversion.Convert.FromWei(win, 18);
			string winStr = winDec.ToString("0.0000");
			mWinText.text = winStr + " ETH";
		}

		// mBetCaseText.text = itemData.mBetCase;
		// mResultText.text = itemData.mResult;
		mBetCaseText.text = Mask2String(Convert.ToUInt64(itemData.mBetMask), itemData.mModulo);
		
		UInt64 resultMask = Utils.DecodeResultMask(itemData.mRevealBlockHash, itemData.mBetMask, itemData.mModulo);
		mResultText.text = Mask2String(resultMask, itemData.mModulo, true);

		mBigWinText.text = Utils.DecodeJackpot(itemData.mRevealBlockHash, itemData.mModulo).ToString();
		if ((float)moneyDec < 0.1f) {
			mBigWinText.text = "-";
		}
	}

	public void SetGray(bool flag) {
		Image img = gameObject.GetComponent<Image>();
		if (flag) {
			img.color = new Color(1f, 1f, 1f, 0.1f);
		} else {
			img.color = new Color(1f, 1f, 1f, 0f);
		}
		
	}

	public string Mask2String(UInt64 betMask ,UInt32 modulo, bool isResult = false) {
		if (modulo == 2) {
			if (betMask == 1) {
				return "正面";
			} else if (betMask == 2) {
				return "反面";
			}
		} else if (modulo == 6) {
			string toBinary = Convert.ToString((int)betMask, 2);
			List<string> result = new List<string>();
			int num = 1;
			for(int i = toBinary.Length - 1; i >= 0; i--) {
				if (toBinary[i] == '1') {
					result.Add(num.ToString());
				}
				num++;
			}
			return string.Join(" ", result.ToArray());
		} else if (modulo == 36) {
			List<int> result = new List<int>();
			result = Utils.Decode2DiceBetMask(betMask);
			string s = "";
			foreach(var r in result) {
				s = s + r + " ";
			}
			return s;
		} else if (modulo == 100) {
			if (isResult) {
				return betMask.ToString();
			} else {
				return "0-" + betMask;
			}
		}
		return "";
	}
}
