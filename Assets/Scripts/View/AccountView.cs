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
using Nethereum.ABI.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
public class AccountView : MonoBehaviour {


	public GameObject welcomView;
	public GameObject selectView;
	public GameObject createView;
	public GameObject wordsView;
	public GameObject wordsConfirmView;
	public GameObject infoView;
	public GameObject importView;
	public GameObject keystoreView;
	public GameObject privateKeyView;
	public GameObject wordsImportView;
	public GameObject transferView;

	public GameObject transactionsHistoryView;
	public GameObject settingView;
	public GameObject confirmPwdView;
	public GameObject backupView;
	public GameObject resetPwdView;
	
	public GameObject item;

    private List<GameObject> itemList = new List<GameObject>();

	private string transaction;
    // private List<Transaction> historyTransactionList = new List<Transaction>();
	private int mCurrentAccountIdx = 0;

	void Start () {
		RegisterNotifications();
		//初始化一下
		welcomView.SetActive (false);
		selectView.SetActive (false);
		createView.SetActive (false);
		wordsView.SetActive (false);
		wordsConfirmView.SetActive (false);
		infoView.SetActive (false);
		importView.SetActive (false);
		keystoreView.SetActive (false);
		privateKeyView.SetActive (false);
		wordsImportView.SetActive (false);
		transferView.SetActive (false);
		transactionsHistoryView.SetActive(false);
        TestForEventABI();
		// 设置初始网络
		AccountManager.Instance.SetInitialNet();
		// 检查是否有本地缓存,有就显示个人信息界面, 没有则显示导入流程
		if (AccountManager.Instance.CheckSavedAccount()){
			ShowInfoView();
		} else {
			ShowWelcomView();
		}
	}

	void OnEnable() {
		// 从其他Tab界面跳转过来时, 如果当前是账户界面, 则刷新一下
		if (infoView.activeSelf) {
			Debug.Log("accoutview onenable");
			ShowInfoView();
		}
	}

	private void RegisterNotifications() {
		NotificationCenter.DefaultCenter().AddObserver(Constants.ImportAccountExeption, ImportAccountExeption);
	}

	private void ShowWelcomView() {
		ViewManager.ReplaceView(welcomView, gameObject.name);
		Button btn = welcomView.transform.Find("Button").GetComponent<Button>();
		Utils.AddButtonClickEvent(btn, delegate() {
			UnityEngine.Debug.Log("ShowWelcomView");
			ShowSelectView();
		});
	}

	private void ShowSelectView() {
		ViewManager.ReplaceView(selectView, gameObject.name);
		Button btnCreate = selectView.transform.Find("BtnCreate").GetComponent<Button>();
		Button btnImport = selectView.transform.Find("BtnImport").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnCreate,delegate() {
			ShowCreateView();
		});		
		Utils.AddButtonClickEvent(btnImport,delegate() {
			ShowImportView();
		});
		Button btnClose = selectView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			if(!AccountManager.Instance.HasAccount()) {
				ShowWelcomView();
			} else {
				ShowInfoView();
			}
		});
	}

	private void ShowCreateView() {
		ViewManager.ReplaceView(createView, gameObject.name);
		Button btnCreate = createView.transform.Find("BtnCreate").GetComponent<Button>();
		InputField nameInput = createView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdInput = createView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = createView.transform.Find("PwdConfirmInputField").GetComponent<InputField>();
		Utils.AddButtonClickEvent(btnCreate, delegate() {
			if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(pwdConfirmInput.text)) {
				ViewManager.ShowMessageBox("有信息未填入");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ViewManager.ShowMessageBox("两次密码输入不一致");
				return;
			}
			ViewManager.ShowMessageBox("我们无法提供找回钱包密码功能，请确认并妥善保管钱包密码？", delegate() {
				ShowWaitingView();
				AccountManager.Instance.CreateWallet(nameInput.text, pwdInput.text);
				HideWaitingView();
				ShowWordsView();
			}, delegate(){

			});
		});

		Button btnClose = createView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowSelectView();
		});
	}

	private void ShowWordsView() {
		ViewManager.ReplaceView(wordsView, gameObject.name);
		for(int i = 0; i < 12; i++) {
			Text text = wordsView.transform.Find(string.Format("Grid/{0}/Text", i.ToString())).GetComponent<Text>();
			text.text = AccountManager.Instance.GetWords()[i];
		}
		Button btnConfirm = wordsView.transform.Find("BtnConfirm").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnConfirm, delegate() {
			ShowWordsConfirmView();
		});

		Button btnClose = wordsView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowCreateView();
		});
	}

	private void ShowWordsConfirmView() {
		ViewManager.ReplaceView(wordsConfirmView, gameObject.name);
		string[] randomWords = new string[12];
		for(int i = 0; i < 12; i++) {
			randomWords[i] = AccountManager.Instance.GetWords()[i];
		}
		for(int i = 11; i > 0; i--) {
			int j = UnityEngine.Random.Range(0, i);
			string tmp = randomWords[i];
			randomWords[i] = randomWords[j];
			randomWords[j] = tmp;
		}

		Text selectedWordsText = wordsConfirmView.transform.Find("SelectedWords/Text").GetComponent<Text>();
		bool[] selectedFlags = new bool[12];
		List<string> selectedWords = new List<string>();
		Color selectedColor = new Color(32/255f, 187/255f, 201/255f);
		Color normalColor = new Color(1f, 1f, 1f, 0.5f);
		for(int i = 0; i < 12; i++) {
			Text text = wordsConfirmView.transform.Find(string.Format("GridInput/{0}/Text", i.ToString())).GetComponent<Text>();
			Button btn = wordsConfirmView.transform.Find(string.Format("GridInput/{0}", i.ToString())).GetComponent<Button>();
            GameObject image = wordsConfirmView.transform.Find(string.Format("GridInput/{0}/Image", i.ToString())).gameObject;
            image.SetActive(false);
			text.text = randomWords[i];
			Utils.AddButtonClickEvent(btn, delegate() {
				int idx = int.Parse(btn.name);
				bool flag = selectedFlags[idx];
				if (flag) {
					text.color = normalColor;
					image.SetActive(false);
					selectedFlags[idx] = false;
					UnityEngine.Debug.Log("diselect " + idx);
					selectedWords.Remove(randomWords[idx]);
				} else {
					text.color = selectedColor;
					image.SetActive(true);
					selectedFlags[idx] = true;
					UnityEngine.Debug.Log("select " + idx);
					selectedWords.Add(randomWords[idx]);
				}
				selectedWordsText.text = string.Join("    ", selectedWords.ToArray());
			});
		}

		Button btnConfirm = wordsConfirmView.transform.Find("BtnConfirm").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnConfirm, delegate(){
			bool isCorrect = true;
			for(int i = 0; i < 12; i++) {
				if (i >= selectedWords.Count || selectedWords[i] != AccountManager.Instance.GetWords()[i]) {
					isCorrect = false;
					ViewManager.ShowMessageBox("输入错误");
					break;
				}
			}
			if (isCorrect) {
				CheckKeystoreInited();
			}
			
		});

		Button btnClose = wordsConfirmView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowWordsView();
		});
	}

	private void ShowImportView() {
		ViewManager.ReplaceView(importView, gameObject.name);
		Button btnKeystore = importView.transform.Find("BtnKeystore").GetComponent<Button>();
		Button btnPrivateKey = importView.transform.Find("BtnPrivateKey").GetComponent<Button>();
		Button btnWords = importView.transform.Find("BtnWords").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnKeystore, delegate() {
			ShowKeystoreView();
		});		
		Utils.AddButtonClickEvent(btnPrivateKey, delegate() {
			ShowPrivateKeyView();
		});		
		Utils.AddButtonClickEvent(btnWords, delegate() {
			ShowWordsImportView();
		});

		Button btnClose = importView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowSelectView();
		});
	}

	private void ShowKeystoreView() {
		ViewManager.ReplaceView(keystoreView, gameObject.name);
		InputField keystoreInput = keystoreView.transform.Find("KeystoreInputField").GetComponent<InputField>();
		InputField pwdInput = keystoreView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = keystoreView.transform.Find("NameInputField").GetComponent<InputField>();
		Button btnPasteKeystore = keystoreView.transform.Find("BtnPasteKeystore").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnPasteKeystore, delegate(){
			keystoreInput.text = UniClipboard.GetText();
		});
		Button btnImport = keystoreView.transform.Find("BtnImport").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(keystoreInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ViewManager.ShowMessageBox("请输入全部信息");
				return;
			}
			ShowWaitingView(ImportAccountTimeOut);
			NotificationCenter.DefaultCenter().AddObserver("KeystoreLoaded", KeystoreLoaded);
			AccountManager.Instance.LoadWalletFromKeystore(keystoreInput.text, nameInput.text, pwdInput.text);
		});

		Button btnClose = keystoreView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowImportView();
		});
	}

	private void KeystoreLoaded(Notification notification) {
		NotificationCenter.DefaultCenter().RemoveObserver("KeystoreLoaded", KeystoreLoaded);
		HideWaitingView();
		ShowInfoView();
	}

	private void ShowPrivateKeyView() {
		ViewManager.ReplaceView(privateKeyView, gameObject.name);
		InputField privateInput = privateKeyView.transform.Find("PrivateKeyInputField").GetComponent<InputField>();
		InputField pwdInput = privateKeyView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = privateKeyView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = privateKeyView.transform.Find("PwdConfirmInputField").GetComponent<InputField>();
		Button btnPastePrivateKey = privateKeyView.transform.Find("BtnPastePrivateKey").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnPastePrivateKey, delegate(){
			privateInput.text = UniClipboard.GetText();
		});
		Button btnImport = privateKeyView.transform.Find("BtnImport").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(privateInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ViewManager.ShowMessageBox("请输入全部信息");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ViewManager.ShowMessageBox("两次输入密码不一致");
				return;
			}
			ShowWaitingView(ImportAccountTimeOut);
			NotificationCenter.DefaultCenter().AddObserver("KeystoreGenerated", KeystoreGenerated);
			AccountManager.Instance.LoadWalletFromPrivateKey(privateInput.text, nameInput.text, pwdInput.text);
		
		});

		Button btnClose = privateKeyView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowImportView();
		});
	}
	private void ShowWordsImportView() {
		ViewManager.ReplaceView(wordsImportView, gameObject.name);
		InputField wordsInput = wordsImportView.transform.Find("WordsInputField").GetComponent<InputField>();
		InputField pwdInput = wordsImportView.transform.Find("PwdInputField").GetComponent<InputField>();
		InputField nameInput = wordsImportView.transform.Find("NameInputField").GetComponent<InputField>();
		InputField pwdConfirmInput = wordsImportView.transform.Find("PwdConfirmInputField").GetComponent<InputField>();
		Button btnPasteWords = wordsImportView.transform.Find("BtnPasteWords").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnPasteWords, delegate(){
			wordsInput.text = UniClipboard.GetText();
		});
		Button btnImport = wordsImportView.transform.Find("BtnImport").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnImport, delegate() {
			if (string.IsNullOrEmpty(wordsInput.text) || string.IsNullOrEmpty(pwdInput.text) || string.IsNullOrEmpty(nameInput.text)) {
				ViewManager.ShowMessageBox("请输入全部信息");
				return;
			}
			if (pwdInput.text != pwdConfirmInput.text) {
				ViewManager.ShowMessageBox("两次输入密码不一致");
				return;
			}
			ShowWaitingView(ImportAccountTimeOut);
			NotificationCenter.DefaultCenter().AddObserver("KeystoreGenerated", KeystoreGenerated);
			AccountManager.Instance.LoadWalletFromWords(wordsInput.text, nameInput.text, pwdInput.text);
		});

		Button btnClose = wordsImportView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowImportView();
		});
	}
	private void KeystoreGenerated(Notification notification) {
		Debug.Log("KeystoreGenerated " + notification.name);
		NotificationCenter.DefaultCenter().RemoveObserver("KeystoreGenerated", KeystoreGenerated);
		HideWaitingView();
		ShowInfoView();
	}

	private void ImportAccountExeption(Notification notification) {
		ViewManager.CloseWaitTip();
		ViewManager.ShowMessageBox("导入失败, 请检查输入");
	}

	private void ImportAccountTimeOut() {
		ViewManager.ShowMessageBox("导入超时, 请检查输入");
	}

	private void ShowInfoView() {
		ViewManager.ReplaceView(infoView, gameObject.name);

		Text nickName = infoView.transform.Find("NickName").GetComponent<Text>();
		Text money = infoView.transform.Find("Money").GetComponent<Text>();
		Text address = infoView.transform.Find("Address").GetComponent<Text>();
		Text keystore = infoView.transform.Find("Panel/Keystore").GetComponent<Text>();

		nickName.text = AccountManager.Instance.GetNickName();
		address.text = AccountManager.Instance.GetAddress();
		keystore.text = AccountManager.Instance.GetKeytore();

		Button btnCopyAddress = infoView.transform.Find("BtnCopyAddress").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnCopyAddress, delegate(){
			UniClipboard.SetText(AccountManager.Instance.GetAddress());
			ViewManager.ShowTip("复制成功");
		});

		money.text = "正在请求刷新";
		AccountManager.Instance.UpdateBalance((balance) => {
			money.text = balance.ToString("0.0000");
		});

		AccountJsonInfo mAccountJsonInfo = AccountManager.Instance.GetAccounts();
		for (int i = 0; i < mAccountJsonInfo.accountInfos.Count ; i++) {
			AccountInfo ai = mAccountJsonInfo.accountInfos[i];
			if (ai.address == AccountManager.Instance.GetAddress()) {
				mCurrentAccountIdx = i;
				break;
			}
		}

	// netdropdown
		Dropdown urlDropDown = infoView.transform.Find("NetDropdown").GetComponent<Dropdown>();
		urlDropDown.options.Clear();
		Dropdown.OptionData tempData;
		for(int i = 0; i < AccountManager.Instance.netNames.Length; i++) {
			tempData = new Dropdown.OptionData(AccountManager.Instance.netNames[i]);
			urlDropDown.options.Add(tempData);
		}
		urlDropDown.captionText.text = AccountManager.Instance.GetCurrentNetName();
		urlDropDown.onValueChanged.RemoveAllListeners();
		urlDropDown.onValueChanged.AddListener(delegate(int value) {
			UnityEngine.Debug.Log("点击了value " + value);
			AccountManager.Instance.SwitchNet(value);
			// 更新资产
			money.text = "正在请求刷新";
			AccountManager.Instance.UpdateBalance((balance) => {
				money.text = balance.ToString("0.0000");
			});
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
			AccountManager.Instance.SwitchAccount(mCurrentAccountIdx);
			ShowInfoView();
		});


		// Button btnCopyPrivateKey = infoView.transform.Find("BtnCopyPrivateKey").GetComponent<Button>();
		// Utils.AddButtonClickEvent(btnCopyPrivateKey, delegate(){
		// 	UniClipboard.SetText(mPrivateKeyString);
		// });
		// Button btnCopyKeystore = infoView.transform.Find("BtnCopyKeystore").GetComponent<Button>();
		// Utils.AddButtonClickEvent(btnCopyKeystore, delegate(){
		// 	UniClipboard.SetText(mKeystore);
		// });
		Button btnLogout = infoView.transform.Find("BtnLogout").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnLogout, delegate(){
			Logout();
		});
		Button btnTransfer = infoView.transform.Find("BtnTransfer").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnTransfer, delegate(){
			ShowTransferView();
		});
		Button btnNewAccount = infoView.transform.Find("NewAccount").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnNewAccount, delegate(){
			ShowSelectView();
		});

		Button btnSetting = infoView.transform.Find("BtnSetting").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnSetting, delegate(){
			ShowSettingView();
		});
	}

	private void ShowSettingView() {
		ViewManager.ReplaceView(settingView, gameObject.name);

		Text nickName = settingView.transform.Find("NickName").GetComponent<Text>();
		Text address = settingView.transform.Find("Address").GetComponent<Text>();
		nickName.text = AccountManager.Instance.GetNickName();
		address.text = AccountManager.Instance.GetAddress();

		Button btnPwd = settingView.transform.Find("BtnPwd").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnPwd, delegate(){
			ShowResetPwdView();
		});

		Button btnBakKeystore = settingView.transform.Find("BtnBakKeystore").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnBakKeystore, delegate(){
			ShowConfirmPwdView(BackupType.Keystore);
		});

		Button btnBakPrivateKey = settingView.transform.Find("BtnBakPrivateKey").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnBakPrivateKey, delegate(){
			ShowConfirmPwdView(BackupType.PrivateKey);
		});

		Button btnBakWords = settingView.transform.Find("BtnBakWords").GetComponent<Button>();
		if (AccountManager.Instance.GetWords() != null && AccountManager.Instance.GetWords().Length > 0) {
			btnBakWords.gameObject.SetActive(true);
			Utils.AddButtonClickEvent(btnBakWords, delegate(){
				ShowConfirmPwdView(BackupType.Words);
			});
		} else {
			btnBakWords.gameObject.SetActive(false);
		}

		Button btnDeleteWallet = settingView.transform.Find("BtnDeleteWallet").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnDeleteWallet, delegate(){
			ShowConfirmPwdView(BackupType.DeleteWallet);
		});

		Button btnClose = settingView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate(){
			ShowInfoView();
		});
	}

	private void ShowResetPwdView() {
		ViewManager.ReplaceView(resetPwdView, gameObject.name);
		InputField oldPwdInput = resetPwdView.transform.Find("OldPwdInputField").GetComponent<InputField>();
		InputField newPwdInputField = resetPwdView.transform.Find("NewPwdInputField").GetComponent<InputField>();
		InputField confirmPwdInputField = resetPwdView.transform.Find("ConfirmPwdInputField").GetComponent<InputField>();
		Button btnConfirm = resetPwdView.transform.Find("BtnConfirm").GetComponent<Button>();
		Button btnClose = resetPwdView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnConfirm, delegate(){
			if (string.IsNullOrEmpty(oldPwdInput.text) || string.IsNullOrEmpty(newPwdInputField.text) || string.IsNullOrEmpty(confirmPwdInputField.text)) {
				ViewManager.ShowMessageBox("请填写所有信息");
				return;
			}
			if (newPwdInputField.text != confirmPwdInputField.text) {
				ViewManager.ShowMessageBox("两次输入密码不一致");
				return;
			}
			// TODO 检查旧密码
			if (AccountManager.Instance.GetPassword() != oldPwdInput.text) {
				ViewManager.ShowMessageBox("旧密码输入错误");
				return;
			}
			// TODO 根据新密码生成keystore
			AccountManager.Instance.ResetPassword(newPwdInputField.text);
			CheckKeystoreInited();
		});
		Utils.AddButtonClickEvent(btnClose, delegate(){
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
		// ViewManager.ReplaceView(confirmPwdView, gameObject.name);
		ViewManager.OpenView(confirmPwdView, gameObject.name);
		InputField pwdInputField = confirmPwdView.transform.Find("PwdInputField").GetComponent<InputField>();
		Button btnConfirm = confirmPwdView.transform.Find("BtnConfirm").GetComponent<Button>();
		Button btnCancle = confirmPwdView.transform.Find("BtnCancle").GetComponent<Button>();
		GameObject deleteWarn = confirmPwdView.transform.Find("DeleteWarn").gameObject;
		pwdInputField.text = "";
		if (backupType == BackupType.DeleteWallet) {
			deleteWarn.SetActive(true);
		} else {
			deleteWarn.SetActive(false);
		}
		Utils.AddButtonClickEvent(btnCancle, delegate(){
			// ShowSettingView();
			ViewManager.CloseCurrentView(gameObject.name);
		});
		Utils.AddButtonClickEvent(btnConfirm, delegate(){
			if(string.IsNullOrEmpty(pwdInputField.text)) {
				ViewManager.ShowMessageBox("请输入密码");
				return;
			}
			// TODO - 验证密码
			if (AccountManager.Instance.GetPassword() != pwdInputField.text) {
				ViewManager.ShowMessageBox("密码输入错误");
				return;
			}
			ViewManager.CloseCurrentView(gameObject.name);
			if (backupType == BackupType.DeleteWallet) {
				//删除钱包
				DeleteWallet();
			} else {
				ShowBackupView(backupType);
			}
		});
	}

	private void DeleteWallet() {
		AccountManager.Instance.DeleteWallet();
		if (AccountManager.Instance.HasAccount()) {
			ShowInfoView();
		} else {
			ShowWelcomView();
		}
	}

	private void ShowBackupView(BackupType backupType) {
		ViewManager.ReplaceView(backupView, gameObject.name);
		GameObject keystoreText = backupView.transform.Find("TextKeystore").gameObject;
		GameObject privateKeyText = backupView.transform.Find("TextPrivateKey").gameObject;
		GameObject wordsText = backupView.transform.Find("TextWords").gameObject;
		Text contentText = backupView.transform.Find("Text").GetComponent<Text>();
		Button btnClose = backupView.transform.Find("BtnClose").GetComponent<Button>();
		Button btnCopy = backupView.transform.Find("BtnCopy").GetComponent<Button>();
		keystoreText.SetActive(false);
		privateKeyText.SetActive(false);
		wordsText.SetActive(false);
		contentText.text = "";
		if (backupType == BackupType.Keystore) {
			keystoreText.SetActive(true);
			contentText.text = AccountManager.Instance.GetKeytore();
		} else if (backupType == BackupType.PrivateKey) {
			privateKeyText.SetActive(true);
			contentText.text = AccountManager.Instance.GetPrivateKey();
		} else if (backupType == BackupType.Words) {
			wordsText.SetActive(true);
			contentText.text = string.Join(" ", AccountManager.Instance.GetWords());
		}
		Utils.AddButtonClickEvent(btnClose, delegate() {
			ShowSettingView();
		});

		Utils.AddButtonClickEvent(btnCopy, delegate() {
			UniClipboard.SetText(contentText.text);
			ViewManager.ShowTip("复制成功");
		});
	}
	
	private void Logout() {
		AccountManager.Instance.LogOut();
		ShowWelcomView();
	}

	private void ShowWaitingView(UnityAction timeOutAction = null) {
		ViewManager.ShowWaitTip("请稍等......", 45, timeOutAction);
	}

	private void HideWaitingView() {
		// ViewManager.CloseCurrentView(gameObject.name);
		ViewManager.CloseWaitTip();
	}

    private void ShowTransactionHistoryView(){
		ViewManager.ReplaceView(transactionsHistoryView, gameObject.name);
        Button btnClose = transactionsHistoryView.transform.Find("BtnClose").GetComponent<Button>();
        Utils.AddButtonClickEvent(btnClose, delegate () {
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
        // WWW getRequest = new WWW("http://api-kovan.etherscan.io/api?module=account&action=txlist&address=0x06282A3A29aEff3A1fA9AEcd0F8aCd9525d13308&startblock=0&endblock=99999999&sort=asc&apikey=");
        startGetTransactionTask();
	}


    public  IEnumerator wwwGetTransactionsByAddress(string address, Action<JsonData> callback)
    {
        string _url=string.Format("http://api-{0}.etherscan.io/api?module=account&action=txlist&address={1}&startblock=0&endblock=99999999&sort=asc&apikey=", AccountManager.Instance.GetCurrentNetName(),address);
      
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
        Action<JsonData> callBack = new Action<JsonData>(GetTransactionsCallBack);
       
        StartCoroutine(wwwGetTransactionsByAddress(AccountManager.Instance.GetAddress(), callBack));
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
            GameObject Content = transactionsHistoryView.transform.Find("TransactionScroll/Viewport/Content").gameObject;
            item.SetActive(true);

            itemList.Clear();
            //itemList.Add(item);

            UnityEngine.Debug.LogError(">>>>>>>count==" + list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                GameObject itemClone = GameObject.Instantiate(item) as GameObject;
                var _itemData = list[i];
				Text fromText = itemClone.transform.Find("Text_from").GetComponent<Text>();
				Text toText = itemClone.transform.Find("Text_to").GetComponent<Text>();
				string from = _itemData["from"].ToString();
				string to = _itemData["to"].ToString();
                fromText.text = from;
                toText.text = to;
				fromText.color = from.ToLower() == AccountManager.Instance.GetAddress().ToLower() ? Color.green : Color.white;
				toText.color = to.ToLower() == AccountManager.Instance.GetAddress().ToLower() ? Color.green : Color.white;
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
		ViewManager.ReplaceView(transferView, gameObject.name);
		InputField addressInput = transferView.transform.Find("AddressInputField").GetComponent<InputField>();
		InputField ethInput = transferView.transform.Find("EthInputField").GetComponent<InputField>();
		InputField gasPriceInput = transferView.transform.Find("GasPriceInputField").GetComponent<InputField>();
		InputField gasInput = transferView.transform.Find("GasInputField").GetComponent<InputField>();
		// InputField pwdInput = transferView.transform.Find("PwdInputField").GetComponent<InputField>();

		Button btnReports = transferView.transform.Find("BtnHistory").GetComponent<Button>();
        Utils.AddButtonClickEvent(btnReports, delegate(){
			ShowTransactionHistoryView();
		});

		Button btnPasteAddress = transferView.transform.Find("BtnPasteAddress").GetComponent<Button>();

		gasInput.text = "21000";
		gasPriceInput.text = AccountManager.Instance.GetGasPrice().ToString();

		Utils.AddButtonClickEvent(btnPasteAddress, delegate(){
			addressInput.text = UniClipboard.GetText();
		});
		Button btnClose = transferView.transform.Find("BtnClose").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnClose, delegate(){
			ShowInfoView();
		});
		Button btnConfirm = transferView.transform.Find("BtnConfirm").GetComponent<Button>();
		Utils.AddButtonClickEvent(btnConfirm, delegate(){
			if(string.IsNullOrEmpty(addressInput.text) 
			|| string.IsNullOrEmpty(ethInput.text)
			|| string.IsNullOrEmpty(gasPriceInput.text)
			|| string.IsNullOrEmpty(gasInput.text)) {
				ViewManager.ShowMessageBox("请输入完整的转账信息");
				return;
			}
			// UnityEngine.Debug.Log(System.DateTime.Now);
			// string privateKey = Wallet.GetPrivateKeyByKeystoreAndPassword(mKeystore, pwdInput.text);
			// UnityEngine.Debug.Log(System.DateTime.Now);

			//gasPriceInput.text = Web3.Web3.Convert.FromWei(TransactionBase.DEFAULT_GAS_PRICE, UnitConversion.EthUnit.Gwei);

			ethAmount = Decimal.Parse(ethInput.text);
			decimal gasPrice = Decimal.Parse(gasPriceInput.text);
			gas = BigInteger.Parse(gasInput.text);
			toAddress = addressInput.text;
            UnityEngine.Debug.LogError("ethAmount " + ethAmount.ToString() + " gasPrice " + gasPrice.ToString() + " gas " + gas.ToString() + " toaddr " + toAddress);
			// LoadPrivateKeyInSubThread(mKeystore, pwdInput.text);
			// StartCoroutine(WaitForPrivateKey());

			ShowWaitingView();
			Action<string, bool ,string> callback = new Action<string, bool,string>(transferCallBack);
			StartCoroutine(Wallet.TransferEth(AccountManager.Instance.GetPrivateKey(),AccountManager.Instance.GetAddress(), toAddress, ethAmount, gasPrice, gas, callback));
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
		ShowWaitingView();
		StartCoroutine(WaitForKeystore());
	}

	// 因为keystore在子线程中生成,所以开个协程每隔1s去检查有没有生成, 如果生成了则保存
	private IEnumerator WaitForKeystore() {
		while(string.IsNullOrEmpty(AccountManager.Instance.GetKeytore())) {
			yield return new WaitForSeconds(1f);
		}
		HideWaitingView();
		ShowInfoView();
	}

    private void transferCallBack(string msg, bool success ,string toAddress) {
		HideWaitingView();
		UnityEngine.Debug.Log("success "+ success.ToString() + "transferCallBack msg " + msg);
		if (!success) {
			ViewManager.ShowMessageBox(msg);
			return;
		} else {
            Wallet.SavedToAddresses(toAddress);
           
			ViewManager.ShowMessageBox("转账成功");
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


    private void TestForEventABI(){
       
        EventABI eventABI = new EventABI("Commit");
        var sssssss = eventABI.Sha33Signature;
        // var ssss=eventABI.Sha33Signature.EnsureHexPrefix();
        var ssssss=eventABI.InputParameters;

        UnityEngine.Debug.LogError("topic====" + ssssss);
        EventABI JackpotPaymentABI = new EventABI("JackpotPayment");
        var JackpotPayment = JackpotPaymentABI.Sha33Signature.EnsureHexPrefix();
        UnityEngine.Debug.LogError("topic==JackpotPayment==" + JackpotPayment);

        EventTopicBuilder eventTopicBuilder = new EventTopicBuilder(eventABI);
        UnityEngine.Debug.LogError("topics==Commit==" + eventTopicBuilder.GetSignatureTopicAsTheOnlyTopic());

        var topics = eventTopicBuilder.GetSignatureTopicAsTheOnlyTopic();
        for (int i = 0; i < topics.Length;i++){
            UnityEngine.Debug.LogError("topics==i==" + topics[i]);
        }

        EventABI PaymentABI = new EventABI("Payment");
        var Payment = PaymentABI.Sha33Signature.EnsureHexPrefix();
        UnityEngine.Debug.LogError("topic==Payment==" + Payment);
    }



}
