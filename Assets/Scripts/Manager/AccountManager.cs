using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class AccountManager: MonoBehaviour {

	private static AccountManager instance;
	public static AccountManager Instance {
		get {
			if (instance == null) {
				GameObject g = 	GameObject.Find("AccountManager");
				DontDestroyOnLoad(g);
				instance = g.GetComponent <AccountManager> ();
			}
			return instance;
		}
	}

	public string[] netNames = {"kovan", "ropsten", "mainnet", "rinkeby"};
	private int mCurrentNet = 0;
	private Dictionary<string, string> netUrls = new Dictionary<string, string>{{"kovan", "https://kovan.infura.io"}, {"ropsten", "https://ropsten.infura.io"},{"mainnet", "https://mainnet.infura.io"},{"rinkeby", "https://rinkeby.infura.io"}};


	private string mKeystore;
	private string mAddress;
	private string mNickName;
	private string mPrivateKeyString;
	private string[] mWords;
	private string mPassword;
	private AccountJsonInfo mAccountJsonInfo;

	private decimal mGasPrice;
	private decimal mBalance;
	void Awake() {
		Register();
	}

	void Start() {
		StartCoroutine(RequestGasPrice());
	}

	private void Register() {
		Dictionary<string, Action<Notification>> observerDic = new Dictionary<string, Action<Notification>> ();
		observerDic.Add ("KeystoreGenerated", SaveAccount);
		observerDic.Add ("KeystoreLoaded", SaveAccount);
		foreach (KeyValuePair <string, Action <Notification>> p in observerDic) {
			NotificationCenter.DefaultCenter().AddObserver (p.Key, p.Value);
		}
	}

	public bool CheckSavedAccount() {
		mAccountJsonInfo = Wallet.GetSavedAccountJsonInfo();
		if(mAccountJsonInfo == null 
		|| mAccountJsonInfo.accountInfos == null 
		|| mAccountJsonInfo.accountInfos.Count == 0) {
			return false;
		}
		// 取第一个
		AccountInfo accountInfo = mAccountJsonInfo.accountInfos[0];
		string keystore = accountInfo.keystore;
		string address = accountInfo.address;
		string nickName = accountInfo.name;
		string words = accountInfo.words;
		string password = accountInfo.password;
		string privateKey = accountInfo.privateKey;
		if (string.IsNullOrEmpty(keystore) || string.IsNullOrEmpty(address)) {
			return false;
		} else {
			mKeystore = keystore;
			mAddress = address;
			mNickName = nickName;
			mPassword = password;
			mPrivateKeyString = privateKey;
			if(!string.IsNullOrEmpty(words)) {
				mWords = words.Split(' ');
			}
			return true;
		}
	}

	public bool HasAccount() {
		if(mAccountJsonInfo == null || mAccountJsonInfo.accountInfos == null || mAccountJsonInfo.accountInfos.Count == 0) {
			return false;
		}
		return true;
	}

	public void CreateWallet(string nickName, string password) {
		Wallet wallet = new Wallet();
		mWords = wallet.Words;
		mPrivateKeyString = wallet.PrivateKeyString;
		mAddress = wallet.PublicAddress;
		mNickName = nickName;
		mPassword = password;
		GenerateKeystoreInSubThread();
	}

	public void LoadWalletFromPrivateKey(string privateKey, string nickName, string password) {
		try {
			Wallet wallet = new Wallet(privateKey, true);
			mPrivateKeyString = wallet.PrivateKeyString;
			mAddress = wallet.PublicAddress;
			mNickName = nickName;
			mPassword = password;
			mWords = null;
			GenerateKeystoreInSubThread();
		} catch (Exception ex) {
			Debug.LogError("LoadWalletFromPrivateKey " + ex.Message);
			NotificationCenter.DefaultCenter().PostNotification(Constants.ImportAccountExeption);
		}

	}

	public void LoadWalletFromWords(string words, string nickName, string password) {
		try {
			Wallet wallet = new Wallet(words);
			mPrivateKeyString = wallet.PrivateKeyString;
			mAddress = wallet.PublicAddress;
			mNickName = nickName;
			mPassword = password;
			mWords = wallet.Words;;
			GenerateKeystoreInSubThread();
		} catch (Exception ex) {
			Debug.LogError("LoadWalletFromWords " + ex.Message);
			NotificationCenter.DefaultCenter().PostNotification(Constants.ImportAccountExeption);
		}

	}
	public void LoadWalletFromKeystore(string keystore, string nickName, string password) {
		LoadKeystoreInSubThread(keystore, password, nickName);
	}

	public void ResetPassword(string password) {
		mPassword = password;
		GenerateKeystoreInSubThread();
	}

#region 子线程生成keystore
	private void GenerateKeystoreInSubThread() {
		mKeystore = "";
		Thread thread = new Thread(new ThreadStart(KeystoreThread));
		thread.Start();
	}

	private void KeystoreThread() {
		mKeystore = Wallet.GenerateKeystore(mPassword, mPrivateKeyString, mAddress);
		KeystoreGenerated();
	}

	private void KeystoreGenerated() {
		NotificationCenter.DefaultCenter().PostNotification("KeystoreGenerated");
	}
#endregion

#region 子线程加载keystore
	private void LoadKeystoreInSubThread(string keystore, string nickName, string password) {
		mWords = null;
		mKeystore = keystore;
		mPassword = password;
		mNickName = nickName;
		Thread thread = new Thread(new ThreadStart(LoadKeystoreThread));
		thread.Start();
	}

	private void LoadKeystoreThread() {
		try {
			Wallet wallet = new Wallet(mKeystore, mPassword);
			mPrivateKeyString = wallet.PrivateKeyString;
			mAddress = wallet.PublicAddress;
			KeystoreLoaded();
		} catch (Exception ex) {
			Debug.LogError("LoadKeystoreThread " + ex.Message);
			NotificationCenter.DefaultCenter().PostNotification(Constants.ImportAccountExeption);
		}
	}

	private void KeystoreLoaded() {
		NotificationCenter.DefaultCenter().PostNotification("KeystoreLoaded");
	}
#endregion

	private void SaveAccount(Notification notification) {
		Debug.Log("SaveAccount " + notification.name);
		if(string.IsNullOrEmpty(mKeystore) 
		|| string.IsNullOrEmpty(mAddress) 
		|| string.IsNullOrEmpty(mNickName) 
		|| string.IsNullOrEmpty(mPassword) 
		|| string.IsNullOrEmpty(mPrivateKeyString)) {
			return;
		}
		mAccountJsonInfo = Wallet.SaveAccount(mKeystore, mAddress, mNickName, mPassword, mPrivateKeyString,  mWords);
	}

	public void SwitchAccount(int idx) {
		AccountInfo ai = mAccountJsonInfo.accountInfos[idx];
		mNickName = ai.name;
		mKeystore = ai.keystore;
		mAddress = ai.address;
		mPassword = ai.password;
		mPrivateKeyString = ai.privateKey;
		if(!string.IsNullOrEmpty(ai.words)) {
			mWords = ai.words.Split(' ');
		} else {
			mWords = null;
		}
	}

	public void DeleteWallet() {
		mAccountJsonInfo = Wallet.DeleteAccount(mAddress);
		if (mAccountJsonInfo.accountInfos.Count > 0) {
			AccountInfo ai = mAccountJsonInfo.accountInfos[0];
			mNickName = ai.name;
			mKeystore = ai.keystore;
			mAddress = ai.address;
			mPassword = ai.password;
			mPrivateKeyString = ai.privateKey;
			if(!string.IsNullOrEmpty(ai.words)) {
				mWords = ai.words.Split(' ');
			} else {
				mWords = null;
			}
		}
	}

	public void SetInitialNet() {
		if (Constants.isDebugMode) {
			SwitchNet(0);
		} else {
			SwitchNet(3);
		}
	}

	public void SwitchNet(int idx) {
		if (idx < 0 || idx >= netNames.Length) {
			Debug.LogError("SwitchNet idx error " + idx);
			return;
		}
		mCurrentNet = idx;
		Wallet.SetUrl(netUrls[netNames[mCurrentNet]]);
	}
	
#region 获取一些变量
	public string GetKeytore() {
		return mKeystore;
	}

	public string GetAddress() {
		return mAddress;
	}

	public string GetNickName() {
		return mNickName;
	}

	public string GetPrivateKey() {
		return mPrivateKeyString;
	}

	public string[] GetWords() {
		return mWords;
	}

	public string GetPassword() {
		return mPassword;
	}

	public AccountJsonInfo GetAccounts() {
		return mAccountJsonInfo;
	}

	public decimal GetGasPrice() {
		return mGasPrice;
	}

	public decimal GetBalance() {
		return mBalance;
	}

	public string GetCurrentNetName() {
		return netNames[mCurrentNet];
	}

#endregion


	public void LogOut() {
		mNickName = "";
		mAddress = "";
		mPrivateKeyString = "";
		mKeystore = "";
		mPassword = "";
	}
	private IEnumerator RequestGasPrice() {
		while (true) {
			yield return StartCoroutine(Wallet.GetGasPrice((_gasPrice) => {
				mGasPrice = _gasPrice;
				Debug.Log("mGasPrice " + mGasPrice.ToString());
			}));
			yield return new WaitForSeconds(10f);
		}
	}

	public void UpdateBalance(Action<decimal> callback) {
		// ViewManager.ShowWaitTip("正在请求更新余额");
		if (string.IsNullOrEmpty(mAddress)) {
			Debug.Log("UpdateBalance mAddress is null");
			return;
		}
		StartCoroutine(Wallet.GetBalance(mAddress, (balance) => {
			mBalance = balance;
			if (callback != null) {
				callback(balance);
			}
			NotificationCenter.DefaultCenter().PostNotification("RefreshBalance");
			// ViewManager.CloseWaitTip();
		}));
	}

	// 下注之后临时扣钱
	public void MinusBalanceTemporary(float bet) {
		mBalance -= (decimal)bet;
		NotificationCenter.DefaultCenter().PostNotification("RefreshBalance");
	}
}
