using System.Collections;
using System.Collections.Generic;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using System;
using System.Numerics;
using Nethereum.JsonRpc.Client;



[System.Serializable]
public class AccountJsonInfo {    
	public List<AccountInfo> accountInfos;
}
[System.Serializable]
public class AccountInfo{
	
	public string name;    
	public string address;   
	public string keystore;
	public string password;  
	public string privateKey;  
	public string words;

	public AccountInfo(string _name, string _address, string _keystore, string _password, string _privateKey, string _words) {
		name = _name;
		address = _address;
		keystore = _keystore;
		password = _password;
		privateKey = _privateKey;
		words = _words;
	}
}

public class Wallet {
	private const string DEFAULT_PATH = "m/44'/60'/0'/0/0";
	
	public static string _url = "https://kovan.infura.io";
    private static List<Nethereum.RPC.Eth.DTOs.Transaction[]> TransactionsList = new List<Nethereum.RPC.Eth.DTOs.Transaction[]>();
	public string Seed {get; private set;}
	// 助记词(根据私钥和keystore导入的没有助记词)
	public string[] Words {get; private set;}
	public bool IsMneumonicValidChecksum {get; private set;}
	public string PrivateKeyString {get; private set;}
	public byte[] PrivateKeyBytes {get; private set;}
	public string PublicAddress {get; private set;}
    private static int blockIndex = 0;
    private static int batchCount = 100;
    private static int blockCount = 5760 * 7;
    private static BigInteger blockNumber = 0;
    private static BigInteger startBlockNumber = 0;

	// 创建钱包
	public Wallet() {
		InitialiseSeed();

	}
	// 根据助记词生成钱包
	public Wallet(string words) {
		InitialiseSeed(words);
	}
	// 通过字符串私钥导入钱包
	public Wallet(string privateKey, bool isPrivateKey) {
		LoadWalletFromPrivateKey(privateKey);
	}
	// 通过二进制私钥导入钱包
	public Wallet(byte[] privateKey, bool isPrivateKey) {
		LoadWalletFromPrivateKey(privateKey);
	}
	// 通过keystore和密码导入钱包
	public Wallet(string keystoreJson, string password) {
		LoadWalletFromKeystore(keystoreJson, password);
	}

	private void InitialiseSeed()
	{
		var mneumonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
		Seed = mneumonic.DeriveSeed(null).ToHex();
		Words = mneumonic.Words;
		IsMneumonicValidChecksum = mneumonic.IsValidChecksum;
		GetPrivateKeyAndAddress();
	}

	private void InitialiseSeed(string words)
	{
		var mneumonic = new Mnemonic(words);
		Seed = mneumonic.DeriveSeed(null).ToHex();
		Words = mneumonic.Words;
		IsMneumonicValidChecksum = mneumonic.IsValidChecksum;
		GetPrivateKeyAndAddress();
	}

	private void LoadWalletFromKeystore(string keystoreJson, string password) {
		if(string.IsNullOrEmpty(keystoreJson) || string.IsNullOrEmpty(password)) {
			throw new System.InvalidOperationException ("keystoreJson or password is null or empty");
		}
		var keystoreservice =  new Nethereum.KeyStore.KeyStoreService(); 
		var privateKey = keystoreservice.DecryptKeyStoreFromJson (password, keystoreJson);
		var ecKey = new EthECKey(privateKey, true);
		var address = ecKey.GetPublicAddress();

		PrivateKeyBytes = privateKey;
		PrivateKeyString = privateKey.ToHex();
		PublicAddress = address;
	}

	private void LoadWalletFromPrivateKey(string privateKey) {
		var ecKey = new EthECKey(privateKey);
		var address = ecKey.GetPublicAddress();

		PrivateKeyBytes = privateKey.HexToByteArray();
		PrivateKeyString = privateKey;
		PublicAddress = address;
	}

	private void LoadWalletFromPrivateKey(byte[] privateKey) {
		var ecKey = new EthECKey(privateKey, true);
		var address = ecKey.GetPublicAddress();

		PrivateKeyBytes = privateKey;
		PrivateKeyString = privateKey.ToHex();
		PublicAddress = address;
	}

	public static string GetPrivateKeyByKeystoreAndPassword(string keystore, string password) {
		var keystoreservice =  new Nethereum.KeyStore.KeyStoreService(); 
		var privateKey = keystoreservice.DecryptKeyStoreFromJson (password, keystore);
		return privateKey.ToHex();
	}
	private void GetPrivateKeyAndAddress() {
		var key = GetKey();
		PrivateKeyBytes = key.PrivateKey.ToBytes();
		PrivateKeyString = PrivateKeyBytes.ToHex();

		var ecKey = new EthECKey(PrivateKeyBytes, true);
		PublicAddress = ecKey.GetPublicAddress();
	}

	private ExtKey GetKey() {
		var masterKey = new ExtKey(Seed);
		var keyPath = new KeyPath(DEFAULT_PATH);
		return masterKey.Derive(keyPath);
	}

	public static string GenerateKeystore(string password, string privateKey, string address) {
		var keystoreservice =  new Nethereum.KeyStore.KeyStoreService(); 
		var encryptedJson = keystoreservice.EncryptAndGenerateDefaultKeyStoreAsJson (password, privateKey.HexToByteArray(), address);
		return encryptedJson;
	}

	public static void SetUrl(string url) {
		UnityEngine.Debug.Log("SetUrl " + url);
		_url = url;
	}

	public static IEnumerator TransferEth(string privateKey, string accountAddress, string toAddress, decimal etherAmount,decimal? gasPriceGwei, BigInteger? gas, System.Action<string, bool,string> callback) {
		var _transactionSignedUnityRequest = new TransactionSignedUnityRequest(_url, privateKey, accountAddress);
		var transactionInput = new TransactionInput() {
			    To = toAddress,
                From = accountAddress,
                GasPrice = gasPriceGwei == null ? null : new HexBigInteger(UnitConversion.Convert.ToWei(gasPriceGwei.Value, UnitConversion.EthUnit.Gwei)),
                Value = new HexBigInteger(UnitConversion.Convert.ToWei(etherAmount)),
                Gas = gas == null ? null : new HexBigInteger(gas.Value)
		};
		yield return _transactionSignedUnityRequest.SignAndSendTransaction(transactionInput);
		if (_transactionSignedUnityRequest.Exception == null) {
			// If we don't have exceptions we just display the result, congrats!
			UnityEngine.Debug.Log ("transfer submitted: " + _transactionSignedUnityRequest.Result);
            callback(_transactionSignedUnityRequest.Result, true,toAddress);
		} else {
			// if we had an error in the UnityRequest we just display the Exception error
			UnityEngine.Debug.Log ("Error submitting transfer tx: " + _transactionSignedUnityRequest.Exception.Message);
            callback(_transactionSignedUnityRequest.Exception.Message, false,toAddress);
		}
	}

	public static IEnumerator GetBalance(string address, System.Action<decimal> callback) {
		var getBalanceRequest = new EthGetBalanceUnityRequest (_url);
		yield return getBalanceRequest.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest ());
		if (getBalanceRequest.Exception == null) {
			var balance = getBalanceRequest.Result.Value;
			callback (Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
		} else {
			UnityEngine.Debug.LogError("GetBalance exception " + getBalanceRequest.Exception.Message);
			throw new System.InvalidOperationException ("Get balance request failed");
		}
	}

	public static IEnumerator GetGasPrice(System.Action<decimal> callback) {
		var getGasPriceRequest = new EthGasPriceUnityRequest (_url);
		yield return getGasPriceRequest.SendRequest();
		if (getGasPriceRequest.Exception == null) {
			var gasPrice = getGasPriceRequest.Result.Value;
			UnityEngine.Debug.Log("xxx gasprice" + gasPrice.ToString());
			callback (Nethereum.Util.UnitConversion.Convert.FromWei(gasPrice, 9));
		} else {
			UnityEngine.Debug.LogError("GetGasPrice exception " + getGasPriceRequest.Exception.Message);
			throw new System.InvalidOperationException ("Get gasprice request failed");
		}
	}

	public static string CheckSavedToAddresses() {
        string ToAddresses = UnityEngine.PlayerPrefs.GetString("WALLET_TOADDRESSES");
	
        return ToAddresses;
	}

    public static void SavedToAddresses(string toAddress){
        string addressStr = CheckSavedAddress();
        UnityEngine.Debug.LogError(">>>>>>>>>>>toaddress-----1-----" + addressStr);
        if(addressStr == null || addressStr == ""){
            addressStr = toAddress;
        }else{
            addressStr = addressStr+"," + toAddress;
        }
        UnityEngine.Debug.LogError(">>>>>>>>>>>toaddress----2------" + addressStr);
        UnityEngine.PlayerPrefs.SetString("WALLET_TOADDRESSES",addressStr);
    }
	public static string CheckSavedKeystore() {

		string keystore = UnityEngine.PlayerPrefs.GetString("WALLET_KEYSTORE");
		return keystore;
	}

	public static string CheckSavedAddress() {
		string address = UnityEngine.PlayerPrefs.GetString("WALLET_ADDRESS");
		return address;
	}

	public static string CheckSavedNickName() {
		string name = UnityEngine.PlayerPrefs.GetString("WALLET_NICKNAME");
		return name;
	}

	public static AccountJsonInfo GetSavedAccountJsonInfo() {
		AccountJsonInfo aji = null;
		string json = UnityEngine.PlayerPrefs.GetString("WALLET_ACCOUNTS");
		UnityEngine.Debug.Log("GetSavedAccountJsonInfo " + json);
		if (string.IsNullOrEmpty(json)) {
			UnityEngine.Debug.Log("没有账户保存记录");
		} else {
			aji = UnityEngine.JsonUtility.FromJson<AccountJsonInfo>(json);
		}
		return aji;
	}

	public static AccountJsonInfo SaveAccount(string keystore, string address, string nickName, string password, string privateKey, string[] words = null) {
		string wordsStr = "";
		if (words != null) {
			wordsStr = string.Join(" ", words);
			UnityEngine.Debug.Log("wordstr " + wordsStr);
		}
		// 保存时先读取之前的数据, 然后添加进去
		AccountJsonInfo aji = GetSavedAccountJsonInfo();
		if (aji == null) {
			aji = new AccountJsonInfo();
		}
		if (aji.accountInfos == null) {
			aji.accountInfos = new List<AccountInfo>();
		}
		// 检查是否存在了,如果是则更新
		bool exist = false;
		foreach(AccountInfo ai in aji.accountInfos) {
			if (ai.address == address) {
				ai.keystore = keystore;
				ai.name = nickName;
				ai.password = password;
				ai.privateKey = privateKey;
				ai.words = wordsStr;
				exist = true;
				break;
			}
		}
		if (!exist) {
			AccountInfo ai = new AccountInfo(nickName, address, keystore, password, privateKey, wordsStr);
			aji.accountInfos.Add(ai);
		}
		string json = UnityEngine.JsonUtility.ToJson(aji);
		UnityEngine.Debug.Log(" SaveAccount json " + json);
		UnityEngine.PlayerPrefs.SetString("WALLET_ACCOUNTS", json);
		return aji;
	}

	public static AccountJsonInfo DeleteAccount(string address) {
		AccountJsonInfo aji = GetSavedAccountJsonInfo();
		for (int i = 0; i < aji.accountInfos.Count ; i++) {
			AccountInfo ai = aji.accountInfos[i];
			if (ai.address == address) {
				aji.accountInfos.Remove(ai);
				break;
			}
		}
		string json = UnityEngine.JsonUtility.ToJson(aji);
		UnityEngine.Debug.Log(" DeleteAccount json " + json);
		UnityEngine.PlayerPrefs.SetString("WALLET_ACCOUNTS", json);
		return aji;
	}

	public static void SaveKeystoreAddressAndNickName(string keystore, string address, string nickName, string words = "") {
		UnityEngine.PlayerPrefs.SetString("WALLET_KEYSTORE", keystore);
		UnityEngine.PlayerPrefs.SetString("WALLET_ADDRESS", address);
		UnityEngine.PlayerPrefs.SetString("WALLET_NICKNAME", nickName);
	}

    public static IEnumerator GetTransactionsByAddress(string address,Action<List<Nethereum.RPC.Eth.DTOs.Transaction>> callback)
    {
       
        Action<bool> _callBack = new Action<bool> ((bool isSuccess)=>{
           
            List<Nethereum.RPC.Eth.DTOs.Transaction> resultList = new List<Nethereum.RPC.Eth.DTOs.Transaction>();
            foreach (Nethereum.RPC.Eth.DTOs.Transaction[] itemList in TransactionsList)
            {
                foreach (Nethereum.RPC.Eth.DTOs.Transaction item in itemList)
                {
                    if(item != null && item.From !=null && item.To != null){
                      
                        if ( item.From.ToLower() == address.ToLower()  ||  item.To.ToLower() == address.ToLower())
                        {
                          
                            resultList.Add(item);
                        } 
                    }
                }
            }
            if(isSuccess){
                callback(resultList); 
            }

        });
		
        yield return  CoroutineManager.StartCoroutineTask( getTransactions( _callBack ));;
    }

    private static  IEnumerator getTransactions(Action<bool> callBack)
    {

        var blockNumberRequest = new EthBlockNumberUnityRequest(Wallet._url);
        yield return blockNumberRequest.SendRequest();
		
        blockNumber = blockNumberRequest.Result.Value;
       
        startBlockNumber=blockNumber - blockCount;
        Action _callBack = new Action(() => {
            //callBack(true);
            if (blockIndex > batchCount)
            {
                callBack(true);
            }
           
        });
    
        for (var _i = 0; _i <= batchCount; _i++){

            CoroutineManager.StartCoroutineTask(GetBlockWithTransactionsByNumberUnityRequest(blockNumber - _i, _callBack));
        }
     
    }

    private static void BatchRequestTransactions(Action callback)
	{
        for (var _i = 0; _i <= blockCount; _i++)
        {
            CoroutineManager.StartCoroutineTask(GetBlockWithTransactionsByNumberUnityRequest(startBlockNumber + _i, callback));
        }
    }
    private IEnumerator GetBlockWithTransactionsHashesByNumberUnityRequest(BigInteger number)
    {
        var hashesRequest = new EthGetBlockWithTransactionsHashesByNumberUnityRequest(Wallet._url);
        var _number = new Nethereum.Hex.HexTypes.HexBigInteger(number);

        yield return hashesRequest.SendRequest(_number);
        //var Result = hashesRequest.Result;
        //string[] Result_hashes = Result.TransactionHashes;
    }

    private static IEnumerator GetBlockWithTransactionsByNumberUnityRequest(BigInteger number,Action callBack)
    {
        var request = new EthGetBlockWithTransactionsByNumberUnityRequest(Wallet._url);
        var _number = new Nethereum.Hex.HexTypes.HexBigInteger(number);
  
        yield return request.SendRequest(_number);

        if(request.Result != null){
            Nethereum.RPC.Eth.DTOs.Transaction[] transactions = request.Result.Transactions;
            lock(TransactionsList){
                TransactionsList.Add(transactions);
            }
        }

        blockIndex += 1;
        //startBlockNumber += 1;
        callBack();
    }
}
