using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading;
using Nethereum.Unity;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.JsonRpc.UnityClient;
using System;
using System.Numerics;
using Nethereum.RPC.Eth.Services;
using Nethereum.JsonRpc.Client;
using LitJson;
using System.IO;
using System.Text;
public class Main : MonoBehaviour {


	public GameObject welcomView;
	public GameObject selectView;
	public GameObject createView;
	public GameObject pwdConfirmView;
	public GameObject wordsView;
	public GameObject wordsConfirmView;
	public GameObject infoView;
	public GameObject importView;
	public GameObject keystoreView;
	public GameObject privateKeyView;
	public GameObject wordsImportView;
	public GameObject watingView;
	public GameObject messageView;
	public GameObject transferView;

	public GameObject TransactionsHistoryView;
	public GameObject settingView;
	public GameObject confirmPwdView;
	public GameObject backupView;
	public GameObject resetPwdView;
	
	public GameObject item;

    private List<GameObject> itemList = new List<GameObject>();
	private string mKeystore;
	private string mAddress;
	private string mNickName;
	private string mPrivateKeyString;
	private byte[] mPrivateKeyBytes;
	private string[] mWords;

	private Wallet mWallet;
	private string mPassword;

	private string[] urlNames = {"kovan", "ropsten", "mainnet", "rinkeby"};
	private int mCurrentNet = 0;
	private Dictionary<string, string> lineUrls = new Dictionary<string, string>{{"kovan", "https://kovan.infura.io"}, {"ropsten", "https://ropsten.infura.io"},{"mainnet", "https://mainnet.infura.io"},{"rinkeby", "https://rinkeby.infura.io"}};
 	// Use this for initialization
	 private string transaction;
    // private List<Transaction> historyTransactionList = new List<Transaction>();
 	
	private AccountJsonInfo mAccountJsonInfo;
	private int mCurrentAccountIdx = 0;

	private decimal gasPrice;
	 // Use this for initialization

	void Awake () {
		// 把日志打印出来
		string logPath = Application.persistentDataPath + "/outLog.txt";
		File.WriteAllText(logPath, Environment.TickCount / 10 % 10000 + "|" + System.DateTime.Now + "\r\n");
		Application.logMessageReceived += (condition, stackTrace, type) => {
			File.AppendAllText(logPath, condition + "\r\n", Encoding.UTF8);
		};
	 }
	void Start () {

		//初始化一下
		welcomView.SetActive (false);
		selectView.SetActive (false);
		createView.SetActive (false);
		pwdConfirmView.SetActive (false);
		wordsView.SetActive (false);
		wordsConfirmView.SetActive (false);
		infoView.SetActive (false);
		importView.SetActive (false);
		keystoreView.SetActive (false);
		privateKeyView.SetActive (false);
		wordsImportView.SetActive (false);
		watingView.SetActive (false);
		messageView.SetActive (false);
		transferView.SetActive (false);
		TransactionsHistoryView.SetActive(false);

		// 检查是否有本地缓存,有就显示个人信息界面, 没有则显示导入流程
		if (CheckSavedAccount()) {
			ShowInfoView();
            //StartCoroutine(ShowTransactionHistoryView());

           
			
		} else {
			ShowWelcomView();
		}

		StartCoroutine(RequestGasPrice());
	}
	
	// 检查是否已经导入过,本地是否有缓存的keystore和address
	private bool CheckSavedAccount() {
		// PlayerPrefs.DeleteAll();
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

	private void ShowWelcomView() {
		welcomView.SetActive(true);
		Button btn = welcomView.transform.Find("Button").GetComponent<Button>();
		AddButtonClickEvent(btn, delegate() {
			UnityEngine.Debug.Log("ShowWelcomView");
			welcomView.SetActive(false);
			ShowSelectView();
		});
	}

	private void AddButtonClickEvent(Button btn, UnityAction action) {
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(action);
	}

	private void ShowSelectView() {
		selectView.SetActive(true);
		Button btnCreate = selectView.transform.Find("BtnCreate").GetComponent<Button>();
		Button btnImport = selectView.transform.Find("BtnImport").GetComponent<Button>();
		AddButtonClickEvent(btnCreate,delegate() {
			selectView.SetActive(false);
			ShowCreateView();
		});		
		AddButtonClickEvent(btnImport,delegate() {
			selectView.SetActive(false);
			ShowImportView();
		});

		

		Button btnClose = selectView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			selectView.SetActive(false);
			if(mAccountJsonInfo == null 
			|| mAccountJsonInfo.accountInfos == null 
			|| mAccountJsonInfo.accountInfos.Count == 0) {
				ShowWelcomView();
			} else {
				ShowInfoView();
			}
		});
	}

	private void ShowCreateView() {
		createView.SetActive(true);
		Button btnCreate = createView.transform.Find("BtnCreate").GetComponent<Button>();
		InputField nameInput = createView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdInput = createView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = createView.transform.Find("NameInputField").GetComponent<InputField>();
		AddButtonClickEvent(btnCreate, delegate() {
			if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(pwdConfirmInput.text)) {
				ShowMessageView("有信息未填入");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ShowMessageView("两次密码输入不一致");
				return;
			}
			watingView.SetActive(true);
			CreateWallet(nameInput.text, pwdInput.text);
		});

		Button btnClose = createView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			createView.SetActive(false);
			ShowSelectView();
		});
	}

	// private void ShowPwdConfirmView() {

	// }

	private void CreateWallet(string nickName, string password) {
		Wallet wallet = new Wallet();
		mWords = wallet.Words;
		mPrivateKeyString = wallet.PrivateKeyString;
		mPrivateKeyBytes = wallet.PrivateKeyBytes;
		mAddress = wallet.PublicAddress;
		mNickName = nickName;
		mPassword = password;
		createView.SetActive(false);
		watingView.SetActive(false);
		ShowWordsView();

		// keystore生成需要十几秒甚至更久, 所以开子线程去做
		// mKeystore = wallet.GenerateKeystore(password);
		GenerateKeystoreInSubThread();
	}


	private void ShowWordsView() {
		wordsView.SetActive(true);
		for(int i = 0; i < 12; i++) {
			Text text = wordsView.transform.Find(string.Format("Grid/{0}", i.ToString())).GetComponent<Text>();
			text.text = mWords[i];
		}
		Button btnConfirm = wordsView.transform.Find("BtnConfirm").GetComponent<Button>();
		AddButtonClickEvent(btnConfirm, delegate() {
			wordsView.SetActive(false);
			ShowWordsConfirmView();
		});

		Button btnClose = wordsView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			wordsView.SetActive(false);
			ShowCreateView();
		});
	}

	private void ShowWordsConfirmView() {
		wordsConfirmView.SetActive(true);
		string[] randomWords = new string[12];
		for(int i = 0; i < 12; i++) {
			randomWords[i] = mWords[i];
		}
		for(int i = 11; i > 0; i--) {
			int j = UnityEngine.Random.Range(0, i);
			string tmp = randomWords[i];
			randomWords[i] = randomWords[j];
			randomWords[j] = tmp;
		}

		Text[] toFillWords = new Text[12];
		bool[] selectedFlags = new bool[12];
		List<string> selectedWords = new List<string>();
		for(int i = 0; i < 12; i++) {
			toFillWords[i] = wordsConfirmView.transform.Find(string.Format("GridShow/{0}", i.ToString())).GetComponent<Text>();
		}
		for(int i = 0; i < 12; i++) {
			Text text = wordsConfirmView.transform.Find(string.Format("GridInput/{0}/Text", i.ToString())).GetComponent<Text>();
			Button btn = wordsConfirmView.transform.Find(string.Format("GridInput/{0}", i.ToString())).GetComponent<Button>();
            GameObject image = wordsConfirmView.transform.Find(string.Format("GridInput/{0}/image", i.ToString())).gameObject;
            //image.SetActive(false);
			text.text = randomWords[i];
			AddButtonClickEvent(btn, delegate() {
                image.SetActive(true);
                text.color = new Color(32, 187, 201);
				int idx = int.Parse(btn.name);
				bool flag = selectedFlags[idx];
				if (flag) {
					selectedFlags[idx] = false;
					UnityEngine.Debug.Log("diselect " + idx);
					selectedWords.Remove(randomWords[idx]);
				} else {
					selectedFlags[idx] = true;
					UnityEngine.Debug.Log("select " + idx);
					selectedWords.Add(randomWords[idx]);
				}
				for (int j = 0; j < 12; j++) {
					if (j >= selectedWords.Count) {
						toFillWords[j].text = "";
					} else {
						toFillWords[j].text = selectedWords[j];
					}
				}
			});
		}

		Button btnConfirm = wordsConfirmView.transform.Find("BtnConfirm").GetComponent<Button>();
		AddButtonClickEvent(btnConfirm, delegate(){
			bool isCorrect = true;
			for(int i = 0; i < 12; i++) {
				if (toFillWords[i].text != mWords[i]) {
					isCorrect = false;
					ShowMessageView("输入错误");
					break;
				}
			}
			if (isCorrect) {
				wordsConfirmView.SetActive(false);
				CheckKeystoreInited();
			}
			
		});

		Button btnClose = wordsConfirmView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			wordsConfirmView.SetActive(false);
			ShowWordsView();
		});
	}

	private void ShowImportView() {
		importView.SetActive(true);
		Button btnKeystore = importView.transform.Find("BtnKeystore").GetComponent<Button>();
		Button btnPrivateKey = importView.transform.Find("BtnPrivateKey").GetComponent<Button>();
		Button btnWords = importView.transform.Find("BtnWords").GetComponent<Button>();
		AddButtonClickEvent(btnKeystore, delegate() {
			importView.SetActive(false);
			ShowKeystoreView();
		});		
		AddButtonClickEvent(btnPrivateKey, delegate() {
			importView.SetActive(false);
			ShowPrivateKeyView();
		});		
		AddButtonClickEvent(btnWords, delegate() {
			importView.SetActive(false);
			ShowWordsImportView();
		});

		Button btnClose = importView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			importView.SetActive(false);
			ShowSelectView();
		});
	}

	private void ShowKeystoreView() {
		keystoreView.SetActive(true);
		InputField keystoreInput = keystoreView.transform.Find("KeystoreInputField").GetComponent<InputField>();
		InputField pwdInput = keystoreView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = keystoreView.transform.Find("NameInputField").GetComponent<InputField>();
		Button btnPasteKeystore = keystoreView.transform.Find("BtnPasteKeystore").GetComponent<Button>();
		AddButtonClickEvent(btnPasteKeystore, delegate(){
			keystoreInput.text = UniClipboard.GetText();
		});
		Button btnImport = keystoreView.transform.Find("BtnImport").GetComponent<Button>();
		AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(keystoreInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ShowMessageView("请输入全部信息");
				return;
			}
			UnityEngine.Debug.Log(keystoreInput.text + "\n" + pwdInput.text);
			// 在子线程中执行, 开协程检查执行结果
			LoadKeystoreInSubThread(keystoreInput.text, pwdInput.text, nameInput.text);
			StartCoroutine(WaitForLoadKeystore());
		});

		Button btnClose = keystoreView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			keystoreView.SetActive(false);
			ShowImportView();
		});
	}

	private void ShowPrivateKeyView() {
		privateKeyView.SetActive(true);
		InputField privateInput = privateKeyView.transform.Find("PrivateKeyInputField").GetComponent<InputField>();
		InputField pwdInput = privateKeyView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = privateKeyView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = privateKeyView.transform.Find("PwdConfirmInputField").GetComponent<InputField>();
		Button btnPastePrivateKey = privateKeyView.transform.Find("BtnPastePrivateKey").GetComponent<Button>();
		AddButtonClickEvent(btnPastePrivateKey, delegate(){
			privateInput.text = UniClipboard.GetText();
		});
		Button btnImport = privateKeyView.transform.Find("BtnImport").GetComponent<Button>();
		AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(privateInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ShowMessageView("请输入全部信息");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ShowMessageView("两次输入密码不一致");
				return;
			}
			Wallet wallet = new Wallet(privateInput.text, true);
			mPrivateKeyString = wallet.PrivateKeyString;
			mPrivateKeyBytes = wallet.PrivateKeyBytes;
			mAddress = wallet.PublicAddress;
			mNickName = nameInput.text;
			mPassword = pwdInput.text;
			mWords = null;
			privateKeyView.SetActive(false);

			// 子线程里生成keystore
			GenerateKeystoreInSubThread();
			CheckKeystoreInited();
		});

		Button btnClose = privateKeyView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			privateKeyView.SetActive(false);
			ShowImportView();
		});
	}
	private void ShowWordsImportView() {
		wordsImportView.SetActive(true);
		InputField wordsInput = wordsImportView.transform.Find("WordsInputField").GetComponent<InputField>();
		InputField pwdInput = wordsImportView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = wordsImportView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = wordsImportView.transform.Find("PwdConfirmInputField").GetComponent<InputField>();
		Button btnPasteWords = wordsImportView.transform.Find("BtnPasteWords").GetComponent<Button>();
		AddButtonClickEvent(btnPasteWords, delegate(){
			wordsInput.text = UniClipboard.GetText();
		});
		Button btnImport = wordsImportView.transform.Find("BtnImport").GetComponent<Button>();
		AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(wordsInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ShowMessageView("请输入全部信息");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ShowMessageView("两次输入密码不一致");
				return;
			}
			Wallet wallet = new Wallet(wordsInput.text);
			mPrivateKeyString = wallet.PrivateKeyString;
			mPrivateKeyBytes = wallet.PrivateKeyBytes;
			mAddress = wallet.PublicAddress;
			mNickName = nameInput.text;
			mWords = wallet.Words;
			mPassword = pwdInput.text;
			wordsImportView.SetActive(false);

			// 子线程里生成keystore
			GenerateKeystoreInSubThread();
			CheckKeystoreInited();

		});

		Button btnClose = wordsImportView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate() {
			wordsImportView.SetActive(false);
			ShowImportView();
		});
	}

	private void ShowInfoView() {
		infoView.SetActive(true);
		Text nickName = infoView.transform.Find("NickName").GetComponent<Text>();
		Text money = infoView.transform.Find("Money").GetComponent<Text>();
		Text address = infoView.transform.Find("Address").GetComponent<Text>();
		Text keystore = infoView.transform.Find("Panel/Keystore").GetComponent<Text>();

		nickName.text = mNickName;
		address.text = mAddress;
		keystore.text = mKeystore;

		Button btnCopyAddress = infoView.transform.Find("BtnCopyAddress").GetComponent<Button>();
		AddButtonClickEvent(btnCopyAddress, delegate(){
			UniClipboard.SetText(mAddress);
		});

		money.text = "正在请求刷新";
		StartCoroutine(Wallet.GetBalance(mAddress, (balance) => {
			money.text = balance.ToString();
		}));

		for (int i = 0; i < mAccountJsonInfo.accountInfos.Count ; i++) {
			AccountInfo ai = mAccountJsonInfo.accountInfos[i];
			if (ai.address == mAddress) {
				mCurrentAccountIdx = i;
				break;
			}
		}

	// netdropdown
		Dropdown urlDropDown = infoView.transform.Find("NetDropdown").GetComponent<Dropdown>();
		urlDropDown.options.Clear();
		Dropdown.OptionData tempData;
		for(int i = 0; i < urlNames.Length; i++) {
			tempData = new Dropdown.OptionData(urlNames[i]);
			urlDropDown.options.Add(tempData);
		}
		urlDropDown.captionText.text = urlNames[mCurrentNet];
		urlDropDown.onValueChanged.RemoveAllListeners();
		urlDropDown.onValueChanged.AddListener(delegate(int value) {
			UnityEngine.Debug.Log("点击了value " + value);
			Wallet.SetUrl(lineUrls[urlNames[value]]);
			mCurrentNet = value;
			// 更新资产
			money.text = "正在请求刷新";
			StartCoroutine(Wallet.GetBalance(mAddress, (balance) => {
				Debug.Log("Account balance: " + balance);
				money.text = balance.ToString();
			}));
		});
	// namedropdown
		Dropdown nameDropDown = infoView.transform.Find("NameDropdown").GetComponent<Dropdown>();
		nameDropDown.options.Clear();
		for(int i = 0; i < mAccountJsonInfo.accountInfos.Count; i++) {
			tempData = new Dropdown.OptionData(mAccountJsonInfo.accountInfos[i].name);
			nameDropDown.options.Add(tempData);
		}
		nameDropDown.value = mCurrentAccountIdx;
		nameDropDown.captionText.text = mAccountJsonInfo.accountInfos[mCurrentAccountIdx].name;
		nameDropDown.onValueChanged.RemoveAllListeners();
		nameDropDown.onValueChanged.AddListener(delegate(int value) {
			UnityEngine.Debug.Log("点击了nameDropDown value " + value);
			mCurrentAccountIdx = value;
			AccountInfo ai = mAccountJsonInfo.accountInfos[mCurrentAccountIdx];
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
			ShowInfoView();
		});


		// Button btnCopyPrivateKey = infoView.transform.Find("BtnCopyPrivateKey").GetComponent<Button>();
		// AddButtonClickEvent(btnCopyPrivateKey, delegate(){
		// 	UniClipboard.SetText(mPrivateKeyString);
		// });
		// Button btnCopyKeystore = infoView.transform.Find("BtnCopyKeystore").GetComponent<Button>();
		// AddButtonClickEvent(btnCopyKeystore, delegate(){
		// 	UniClipboard.SetText(mKeystore);
		// });
		Button btnLogout = infoView.transform.Find("BtnLogout").GetComponent<Button>();
		AddButtonClickEvent(btnLogout, delegate(){
			infoView.SetActive(false);
			Logout();
		});
		Button btnTransfer = infoView.transform.Find("BtnTransfer").GetComponent<Button>();
		AddButtonClickEvent(btnTransfer, delegate(){
			infoView.SetActive(false);
			ShowTransferView();
		});
		Button btnNewAccount = infoView.transform.Find("NewAccount").GetComponent<Button>();
		AddButtonClickEvent(btnNewAccount, delegate(){
			infoView.SetActive(false);
			ShowSelectView();
		});

		Button btnSetting = infoView.transform.Find("BtnSetting").GetComponent<Button>();
		AddButtonClickEvent(btnSetting, delegate(){
			infoView.SetActive(false);
			ShowSettingView();
		});
	}

	private void ShowSettingView() {
		settingView.SetActive(true);

		Text nickName = settingView.transform.Find("NickName").GetComponent<Text>();
		Text address = settingView.transform.Find("Address").GetComponent<Text>();
		nickName.text = mNickName;
		address.text = mAddress;

		Button btnPwd = settingView.transform.Find("BtnPwd").GetComponent<Button>();
		AddButtonClickEvent(btnPwd, delegate(){
			settingView.SetActive(false);
			ShowResetPwdView();
		});

		Button btnBakKeystore = settingView.transform.Find("BtnBakKeystore").GetComponent<Button>();
		AddButtonClickEvent(btnBakKeystore, delegate(){
			settingView.SetActive(false);
			ShowConfirmPwdView(BackupType.Keystore);
		});

		Button btnBakPrivateKey = settingView.transform.Find("BtnBakPrivateKey").GetComponent<Button>();
		AddButtonClickEvent(btnBakPrivateKey, delegate(){
			settingView.SetActive(false);
			ShowConfirmPwdView(BackupType.PrivateKey);
		});

		Button btnBakWords = settingView.transform.Find("BtnBakWords").GetComponent<Button>();
		if (mWords != null && mWords.Length > 0) {
			btnBakWords.gameObject.SetActive(true);
			AddButtonClickEvent(btnBakWords, delegate(){
				settingView.SetActive(false);
				ShowConfirmPwdView(BackupType.Words);
			});
		} else {
			btnBakWords.gameObject.SetActive(false);
		}

		Button btnDeleteWallet = settingView.transform.Find("BtnDeleteWallet").GetComponent<Button>();
		AddButtonClickEvent(btnDeleteWallet, delegate(){
			settingView.SetActive(false);
			ShowConfirmPwdView(BackupType.DeleteWallet);
		});

		Button btnClose = settingView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate(){
			settingView.SetActive(false);
			ShowInfoView();
		});
	}

	private void ShowResetPwdView() {
		resetPwdView.SetActive(true);
		InputField oldPwdInput = resetPwdView.transform.Find("OldPwdInputField").GetComponent<InputField>();
		InputField newPwdInputField = resetPwdView.transform.Find("NewPwdInputField").GetComponent<InputField>();
		InputField confirmPwdInputField = resetPwdView.transform.Find("ConfirmPwdInputField").GetComponent<InputField>();
		Button btnConfirm = resetPwdView.transform.Find("BtnConfirm").GetComponent<Button>();
		Button btnClose = resetPwdView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnConfirm, delegate(){
			if (string.IsNullOrEmpty(oldPwdInput.text) || string.IsNullOrEmpty(newPwdInputField.text) || string.IsNullOrEmpty(confirmPwdInputField.text)) {
				ShowMessageView("请填写所有信息");
				return;
			}
			if (newPwdInputField.text != confirmPwdInputField.text) {
				ShowMessageView("两次输入密码不一致");
				return;
			}
			// TODO 检查旧密码
			if (mPassword != oldPwdInput.text) {
				ShowMessageView("旧密码输入错误");
				return;
			}
			// TODO 根据新密码生成keystore
			mPassword = newPwdInputField.text;
			resetPwdView.SetActive(false);
			GenerateKeystoreInSubThread();
			CheckKeystoreInited();
		});
		AddButtonClickEvent(btnClose, delegate(){
			resetPwdView.SetActive(false);
			ShowSettingView();
		});
	}

	private enum BackupType
	{
		Keystore = 1,
		PrivateKey,
		Words,
		DeleteWallet
	}

	private void ShowConfirmPwdView(BackupType backupType) {
		Debug.Log("ShowConfirmPwdView " + backupType.ToString());
		confirmPwdView.SetActive(true);
		InputField pwdInputField = confirmPwdView.transform.Find("PwdInputField").GetComponent<InputField>();
		Button btnConfirm = confirmPwdView.transform.Find("BtnConfirm").GetComponent<Button>();
		Button btnCancle = confirmPwdView.transform.Find("BtnCancle").GetComponent<Button>();
		AddButtonClickEvent(btnCancle, delegate(){
			confirmPwdView.SetActive(false);
			ShowSettingView();
		});
		AddButtonClickEvent(btnConfirm, delegate(){
			if(string.IsNullOrEmpty(pwdInputField.text)) {
				ShowMessageView("请输入密码");
				return;
			}
			// TODO - 验证密码
			if (mPassword != pwdInputField.text) {
				ShowMessageView("密码输入错误");
				return;
			}
			confirmPwdView.SetActive(false);
			if (backupType == BackupType.DeleteWallet) {
				//删除钱包
				confirmPwdView.SetActive(false);
				DeleteWallet();
			} else {
				confirmPwdView.SetActive(false);
				ShowBackupView(backupType);
			}
		});
	}

	private void DeleteWallet() {
		mAccountJsonInfo = Wallet.DeleteAccount(mAddress);
		if (mAccountJsonInfo.accountInfos.Count > 0) {
			mCurrentAccountIdx = 0;
			AccountInfo ai = mAccountJsonInfo.accountInfos[mCurrentAccountIdx];
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
			ShowInfoView();
		} else {
			ShowWelcomView();
		}
	}

	private void ShowBackupView(BackupType backupType) {
		backupView.SetActive(true);
		GameObject keystoreText = backupView.transform.Find("TextKeystore").gameObject;
		GameObject privateKeyText = backupView.transform.Find("TextPrivateKey").gameObject;
		GameObject wordsText = backupView.transform.Find("TextWords").gameObject;
		Text contentText = backupView.transform.Find("Text").GetComponent<Text>();
		Button btnConfirm = backupView.transform.Find("BtnConfirm").GetComponent<Button>();
		Button btnCopy = backupView.transform.Find("BtnCopy").GetComponent<Button>();
		keystoreText.SetActive(false);
		privateKeyText.SetActive(false);
		wordsText.SetActive(false);
		contentText.text = "";
		if (backupType == BackupType.Keystore) {
			keystoreText.SetActive(true);
			contentText.text = mKeystore;
		} else if (backupType == BackupType.PrivateKey) {
			privateKeyText.SetActive(true);
			contentText.text = mPrivateKeyString;
		} else if (backupType == BackupType.Words) {
			wordsText.SetActive(true);
			contentText.text = string.Join(" ", mWords);
		}
		AddButtonClickEvent(btnConfirm, delegate() {
			backupView.SetActive(false);
			ShowSettingView();
		});

		AddButtonClickEvent(btnCopy, delegate() {
			UniClipboard.SetText(contentText.text);
		});
	}
	
	private void Logout() {
		mNickName = "";
		mAddress = "";
		mPrivateKeyBytes = null;
		mPrivateKeyString = "";
		mKeystore = "";
		mWallet = null;
		mPassword = "";
		ShowWelcomView();
	}

	private void ShowMessageView(string msg) {
		messageView.SetActive(true);
		Text message = messageView.transform.Find("Text").GetComponent<Text>();
		message.text = msg;
		Button btn = messageView.transform.Find("Button").GetComponent<Button>();
		AddButtonClickEvent(btn,delegate(){
			messageView.SetActive(false);
		}) ;
	}

    private void ShowTransactionHistoryView(){
		TransactionsHistoryView.SetActive(true);

        Button btnClose = TransactionsHistoryView.transform.Find("BtnClose").GetComponent<Button>();
        AddButtonClickEvent(btnClose, delegate () {
            TransactionsHistoryView.SetActive(false);
            if (itemList.Count > 0)
            {
                for (var k = 0; k < itemList.Count; k++)
                {
                    DestroyImmediate(itemList[k].gameObject);
                }
                itemList.Clear();

            }
            ShowTransferView();
        });
        //    	Thread getThread = new Thread(new ThreadStart(startGetTransactionTask));
        //getThread.IsBackground = true;
        //getThread.Start(); 
        // WWW getRequest = new WWW("http://api-kovan.etherscan.io/api?module=account&action=txlist&address=0x06282A3A29aEff3A1fA9AEcd0F8aCd9525d13308&startblock=0&endblock=99999999&sort=asc&apikey=");

        startGetTransactionTask();
        //StartCoroutine(wwwGetTransactionsByAddress());

	}


    public  IEnumerator wwwGetTransactionsByAddress(string address, Action<JsonData> callback)
    {
        string _url=string.Format("http://api-{0}.etherscan.io/api?module=account&action=txlist&address={1}&startblock=0&endblock=99999999&sort=asc&apikey=", urlNames[mCurrentNet],address);
      
        WWW www = new WWW(_url);//定义一个www类型的对象
        yield return www;//返回下载的值
        JsonData jsonData = JsonMapper.ToObject(www.text);

        if (www.error != null)
        {//判断下载的资源是否有错误
            Debug.Log("Error: " + www.error);
            yield break;
        }
        callback(jsonData);


    }

	private void startGetTransactionTask(){
        UnityEngine.Debug.LogWarning(mAddress);
 		
        Action<JsonData> callBack = new Action<JsonData>(GetTransactionsCallBack);
       
        StartCoroutine(wwwGetTransactionsByAddress(mAddress, callBack));
	}

    private void GetTransactionsCallBack(JsonData transactionsList){
        UnityEngine.Debug.LogError("GetTransactionsCallBack--sss----------" );
        CreateTransactionItem(transactionsList);
    }

    private IEnumerator GetBlockWithTransactionsHashesByNumberUnityRequest(BigInteger number){
        var hashesRequest = new EthGetBlockWithTransactionsHashesByNumberUnityRequest(Wallet._url);
        var _number = new Nethereum.Hex.HexTypes.HexBigInteger(number);
       
        yield return hashesRequest.SendRequest(_number);
        var Result = hashesRequest.Result;
        string[] Result_hashes = Result.TransactionHashes;
   
    }

    private IEnumerator GetBlockWithTransactionsByNumberUnityRequest(BigInteger number){
        var request = new EthGetBlockWithTransactionsByNumberUnityRequest(Wallet._url);
        var _number = new Nethereum.Hex.HexTypes.HexBigInteger(number);
        yield return request.SendRequest(_number);
        var transactions = request.Result.Transactions;
  

    }

    private void CreateTransactionItem(JsonData _list){
       
        if(_list != null){
            var list = _list["result"];
            GameObject Content = TransactionsHistoryView.transform.Find("TransactionScroll/Viewport/Content").gameObject;
            item.SetActive(true);

            itemList.Clear();
            //itemList.Add(item);

            UnityEngine.Debug.LogError(">>>>>>>count==" + list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                GameObject itemClone = GameObject.Instantiate(item) as GameObject;
                var _itemData = list[i];
                itemClone.transform.Find("Text_from").GetComponent<Text>().text = _itemData["from"].ToJson();
                itemClone.transform.Find("Text_to").GetComponent<Text>().text = _itemData["to"].ToJson();
               // itemClone.transform.parent = Content.transform;
                itemClone.transform.SetParent(Content.transform);
                itemList.Add(itemClone);
              
                RectTransform t = itemList[i].GetComponent<RectTransform>();
                itemClone.GetComponent<RectTransform>().localPosition =
                        new UnityEngine.Vector3(t.localPosition.x, t.localPosition.y - t.rect.height, t.localPosition.z);
                itemClone.GetComponent<RectTransform>().localScale = new UnityEngine.Vector3(1, 1, 1);
             

            }
            Content.GetComponent<RectTransform>().sizeDelta =
                   new UnityEngine.Vector2(Content.GetComponent<RectTransform>().sizeDelta.x,
                               itemList.Count * item.GetComponent<RectTransform>().rect.height);
            item.SetActive(false); 
        }

		
	}


	private decimal ethAmount;
	private BigInteger gas;
	private string toAddress;
	private void ShowTransferView() {
		transferView.SetActive(true);
		InputField addressInput = transferView.transform.Find("AddressInputField").GetComponent<InputField>();
		InputField ethInput = transferView.transform.Find("EthInputField").GetComponent<InputField>();
		InputField gasPriceInput = transferView.transform.Find("GasPriceInputField").GetComponent<InputField>();
		InputField gasInput = transferView.transform.Find("GasInputField").GetComponent<InputField>();
		// InputField pwdInput = transferView.transform.Find("PwdInputField").GetComponent<InputField>();

		Button btnReports = transferView.transform.Find("BtnHistory").GetComponent<Button>();
        AddButtonClickEvent(btnReports, delegate(){
			transferView.SetActive(false);
			ShowTransactionHistoryView();
		});

		Button btnPasteAddress = transferView.transform.Find("BtnPasteAddress").GetComponent<Button>();

		gasInput.text = "21000";
		gasPriceInput.text = gasPrice.ToString();

		AddButtonClickEvent(btnPasteAddress, delegate(){
			addressInput.text = UniClipboard.GetText();
		});
		Button btnClose = transferView.transform.Find("BtnClose").GetComponent<Button>();
		AddButtonClickEvent(btnClose, delegate(){
			transferView.SetActive(false);
			ShowInfoView();
		});
		Button btnConfirm = transferView.transform.Find("BtnConfirm").GetComponent<Button>();
		AddButtonClickEvent(btnConfirm, delegate(){
			if(string.IsNullOrEmpty(addressInput.text) 
			|| string.IsNullOrEmpty(ethInput.text)
			|| string.IsNullOrEmpty(gasPriceInput.text)
			|| string.IsNullOrEmpty(gasInput.text)) {
				ShowMessageView("请输入完整的转账信息");
				return;
			}
			// UnityEngine.Debug.Log(System.DateTime.Now);
			// string privateKey = Wallet.GetPrivateKeyByKeystoreAndPassword(mKeystore, pwdInput.text);
			// UnityEngine.Debug.Log(System.DateTime.Now);

			//gasPriceInput.text = Web3.Web3.Convert.FromWei(TransactionBase.DEFAULT_GAS_PRICE, UnitConversion.EthUnit.Gwei);

			ethAmount = Decimal.Parse(ethInput.text);
			gasPrice = Decimal.Parse(gasPriceInput.text);
			gas = BigInteger.Parse(gasInput.text);
			toAddress = addressInput.text;
            UnityEngine.Debug.LogError("ethAmount " + ethAmount.ToString() + " gasPrice " + gasPrice.ToString() + " gas " + gas.ToString() + " toaddr " + toAddress);
			// LoadPrivateKeyInSubThread(mKeystore, pwdInput.text);
			// StartCoroutine(WaitForPrivateKey());

			watingView.SetActive(true);
			Action<string, bool ,string> callback = new Action<string, bool,string>(transferCallBack);
			StartCoroutine(Wallet.TransferEth(mPrivateKeyString, mAddress, toAddress, ethAmount, gasPrice, gas, callback));
		});


        // addressdropdown
        Dropdown addressDropDown = transferView.transform.Find("AddressDropdown").GetComponent<Dropdown>();
        addressDropDown.options.Clear();
        var address_list = GetToAddresses();
        Dropdown.OptionData tempData;
        for (int i = 0; i < address_list.Length; i++)
        {
            tempData = new Dropdown.OptionData(address_list[i]);
            addressDropDown.options.Add(tempData);
        }
       
        addressDropDown.onValueChanged.RemoveAllListeners();
        addressDropDown.onValueChanged.AddListener(delegate (int value) {
            UnityEngine.Debug.Log("点击了value " + value);
            // 选中地址
           // addressDropDown.captionText.text = address_list[value];
            addressInput.text = address_list[value];
        });
	}
	
	private void CheckKeystoreInited() {
		watingView.SetActive(true);
		StartCoroutine(WaitForKeystore());
	}

	private void GenerateKeystoreInSubThread() {
		mKeystore = "";
		Thread thread = new Thread(new ThreadStart(KeystoreThread));
		thread.Start();
	}

	private void KeystoreThread() {
		mKeystore = Wallet.GenerateKeystore(mPassword, mPrivateKeyString, mAddress);
	}

	// 因为keystore在子线程中生成,所以开个协程每隔1s去检查有没有生成, 如果生成了则保存
	private IEnumerator WaitForKeystore() {
		while(string.IsNullOrEmpty(mKeystore)) {
			yield return new WaitForSeconds(1f);
		}
		watingView.SetActive(false);
		mAccountJsonInfo = Wallet.SaveAccount(mKeystore, mAddress, mNickName, mPassword, mPrivateKeyString, mWords);
		ShowInfoView();
	}

	private void LoadKeystoreInSubThread(string keystore, string password, string nickName) {
		mWords = null;
		mWallet = null;
		mKeystore = keystore;
		mPassword = password;
		mNickName = nickName;
		watingView.SetActive(true);
		Thread thread = new Thread(new ThreadStart(LoadKeystoreThread));
		thread.Start();
	}

	private void LoadKeystoreThread() {
		mWallet = new Wallet(mKeystore, mPassword);
	}

	private IEnumerator WaitForLoadKeystore() {
		while(mWallet == null) {
			yield return new WaitForSeconds(1f);
		}
		mPrivateKeyString = mWallet.PrivateKeyString;
		mPrivateKeyBytes = mWallet.PrivateKeyBytes;
		mAddress = mWallet.PublicAddress;
		// Wallet.SaveKeystoreAddressAndNickName(mKeystore, mAddress, mNickName);
		mAccountJsonInfo = Wallet.SaveAccount(mKeystore, mAddress, mNickName, mPassword, mPrivateKeyString,  mWords);
		watingView.SetActive(false);
		keystoreView.SetActive(false);
		ShowInfoView();
	}

	// private void LoadPrivateKeyInSubThread(string keystore, string password) {
	// 	UnityEngine.Debug.Log("LoadPrivateKeyInSubThread");
	// 	mPrivateKeyString = "";
	// 	mPassword = password;
	// 	watingView.SetActive(true);
	// 	Thread thread = new Thread(new ThreadStart(LoadPrivateKeyThread));
	// 	thread.Start();
	// }

	// private void LoadPrivateKeyThread() {
	// 	UnityEngine.Debug.Log("LoadPrivateKeyThread");

	// 	mPrivateKeyString = Wallet.GetPrivateKeyByKeystoreAndPassword(mKeystore, mPassword);
	// }

	// private IEnumerator WaitForPrivateKey() {
	// 	while (string.IsNullOrEmpty(mPrivateKeyString)) {
	// 		UnityEngine.Debug.Log("WaitForPrivateKey");

	// 		yield return new WaitForSeconds(1.0f);
	// 	}
	// 	Action<string, bool> callback = new Action<string, bool>(transferCallBack);
	// 	yield return StartCoroutine(Wallet.TransferEth(mPrivateKeyString, mAddress, toAddress, ethAmount, gasPrice, gas, callback));
	// 	StartCoroutine(Wallet.GetBalance(toAddress, (balance) => {
	// 		Debug.Log("Account balance: " + balance);
	// 	}));
	// 	watingView.SetActive(false);
	// }

    private void transferCallBack(string msg, bool success ,string toAddress) {
		watingView.SetActive(false);
		UnityEngine.Debug.Log("success "+ success.ToString() + "transferCallBack msg " + msg);
		if (!success) {
			ShowMessageView(msg);
			return;
		} else {
            Wallet.SavedToAddresses(toAddress);
           
			ShowMessageView("转账成功");
		}
	}

    private string[] GetToAddresses(){
        string[] result_list = null;
        string addressStr = Wallet.CheckSavedToAddresses();
        if(addressStr != null || addressStr != ""){
            result_list = addressStr.Split(',');
        }

        return result_list;
    }


	private IEnumerator RequestGasPrice() {
		while (true) {
			yield return StartCoroutine(Wallet.GetGasPrice((_gasPrice) => {
				gasPrice = _gasPrice;
				Debug.Log("gasprice " + gasPrice.ToString());
			}));
			yield return new WaitForSeconds(10f);
		}
	}
}
