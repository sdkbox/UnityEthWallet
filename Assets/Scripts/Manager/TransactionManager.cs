using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nethereum.Contracts;
using LitJson;
using System;
using Nethereum.Util;
using Nethereum.ABI;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.JsonRpc.UnityClient;
using UnityEngine.Networking;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Transaction = Nethereum.Signer.Transaction;


public class TransactionManager : MonoBehaviour {
	private const string contractAddress = "0x773126205E055e770058638E4d7f82462B980D79"; //
    //private const string contractAddress = "0x489C3c76Ae83c7d6185116d72aa6bB691c371eb5";//test
    private const string randomUrl = "https://dice.skrskr.online/sign?addr={0}";
    private const string historyUrl = "https://dice.skrskr.online/history?addr=";
    private const float delayTime = 1.0f;
    private const int gasLimit = 1000000;

	private string ABI;
	private Contract mContract;

	private static TransactionManager _instance;  
  
    public static TransactionManager Instance  {  
        get {  
            if (_instance == null) {  
                GameObject g = new GameObject ("TransactionManager");				//若示例为空则创建一个物体并把该实例附加
				DontDestroyOnLoad(g);
				_instance = g.AddComponent <TransactionManager> (); 
				_instance.Init();

            }  
            return _instance;  
        }  
    }

	private void Init() {
		ABI = Utils.ReadFile();
		mContract = new Contract(null, ABI, contractAddress);
	}

	public void OnBetCoin(int coinSelectIdx, float bet) {
		//coinSelectIdx 1 正面, 2 反面
		// int [] nums = {coinSelectIdx};
		// byte[] bytes = BetCase2Byte32(nums);
		// var input = Byte32ToBigInteger(bytes);
		StartTrx(BigInteger.Parse(coinSelectIdx.ToString()), bet,2);
	}

	public void OnBetNumber(int sliderValue, float bet) {
		StartTrx(BigInteger.Parse(sliderValue.ToString()),bet,100);
	}
	public void OnBetDice1(int[] selectDicesOne, float bet) {
		List<int> list = new List<int>();
		for (int i = 0; i < selectDicesOne.Length; i++) {
			if (selectDicesOne[i] == 1) {
				list.Add(i);
			}

		}
		if (list.Count == 0) {
			Debug.LogError("OnBetDice1 未选择下注项");
            ViewManager.ShowMessageBox("请选择至少一个下注项");
			return;
		}
		byte[] bytes = BetCase2Byte32(list.ToArray());
		var input = Byte32ToBigInteger(bytes);
		StartTrx(input, bet,6);
	}
	public void OnBetDice2(int[] selectDicesTwo, float bet) {
		List<int> list = new List<int>();
		for (int i = 0; i < selectDicesTwo.Length; i++) {
			if (selectDicesTwo[i] == 1) {
				list.Add(i + 2);
			}
		}
		if (list.Count == 0) {
            ViewManager.ShowMessageBox("请选择至少一个下注项");
			Debug.LogError("OnBetDice2 未选择下注项");
			return;
		}

		string mask = Calc2DiceMask(list).ToString();
		StartTrx(BigInteger.Parse(mask), bet,36);
	}


	//magic formula
	public UInt64 Calc2DiceMask(List<int> indexs)
	{
		UInt64 c = 0;
		for( var index =0; index< indexs.Count; index++)
		{
			for (var v = 1; v <= 6; v++)
			{
				for (var i = 1; i <= 6; i++)
				{
					if ((v + i) == indexs[index]) 
					{
						c += (UInt64)Math.Pow (2, 6 * (v - 1) + (i - 1));
					}
				}
			}
		}
			
		return c;
	}



	private void StartTrx(BigInteger input, float bet, int type)
	{
		ViewManager.ShowWaitTip("下注中...", 99999);
		
		Action<JsonData,BigInteger,float,int> callBack = new Action<JsonData,BigInteger,float,int>(GetCommitCallBack);
		StartCoroutine(GetCommitData(callBack,input,bet,type));
	}

	//IEnumerator GetCommitData(Action<JsonData> callBack)
    //{
    //    UnityEngine.Debug.LogError(">>>>>>>>GetCommitData>");
    //    byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
    //    string string1 = "address:"+AccountManager.Instance.GetAddress();
    //    string string2 = "network_id:1";
    //    byte[] data =System.Text.Encoding.UTF8.GetBytes(string1+string2);

    //    UnityWebRequest www = UnityWebRequest.Put("https://dice2.win/api/v1/games/dev/random", data);
    //    yield return www.SendWebRequest();
    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        UnityEngine.Debug.LogError(www.error);
           
    //    }
    //    else
    //    {
    //        UnityEngine.Debug.LogError("---------------responsecode==" + www.responseCode);
    //        UnityEngine.Debug.LogError("---------------responsecode==" );
    //        UnityEngine.Debug.LogError("---------------responsecode==" + www.responseCode);
    //        UnityEngine.Debug.LogError("response:"+www.downloadHandler.text);
    //        UnityEngine.Debug.LogError("response:" + www.downloadHandler.data);
    //        JsonData jsonData = JsonMapper.ToObject(www.downloadHandler.text);

    //        if (www.error != null)
    //        {//判断下载的资源是否有错误
    //            Debug.Log("Error: " + www.error);
    //            yield break;
    //        }
    //        callBack(jsonData);
          
    //    }
    //}

    IEnumerator GetCommitData(Action<JsonData,BigInteger,float,int> callBack,BigInteger _input , float _bet,int _type)
    {
		string _url = string.Format(randomUrl,AccountManager.Instance.GetAddress());
        UnityEngine.Debug.LogError(_url);
        WWW www = new WWW(_url);//定义一个www类型的对象
        yield return www;//返回下载的值

        if (www.error != null)
        {//判断下载的资源是否有错误
            Debug.Log("Error: " + www.error);
            ViewManager.ShowMessageBox("http连接失败："+www.error);
            ViewManager.CloseWaitTip();
            yield break;
        }else{
            JsonData jsonData = JsonMapper.ToObject(www.text);
            UnityEngine.Debug.LogError(www.text);
            callBack(jsonData, _input, _bet, _type);

        }
    }


    IEnumerator GetHistoryData(){
        string _url = string.Format(historyUrl);
        UnityEngine.Debug.LogError(_url);
        WWW www = new WWW(_url);//定义一个www类型的对象
        yield return www;//返回下载的值
        if (www.error != null)
        {//判断下载的资源是否有错误
            Debug.Log("Error: " + www.error);
            yield break;
        }else{
            JsonData jsonData = JsonMapper.ToObject(www.text);
            UnityEngine.Debug.LogError(www.text);
        
        }
      
    }

    private void GetCommitCallBack(JsonData jsonData,BigInteger _input,float _bet,int type)
    {
        UnityEngine.Debug.LogError("GetCommitCallBack--sss----------"+jsonData["sign"]);
        StartCoroutine(PlaceBetRequest(jsonData,_input,_bet,type));
    }




    private Function getAddr()
    {
        return mContract.GetFunction("getOwner");
    }

    private CallInput CreatGetAddrInput(JsonData jsonData)
    {
        var function = getAddr();
        return function.CreateCallInput();
    }


    public TransactionInput CreateBetInput(
    string addressFrom,
    HexBigInteger gas = null,
    HexBigInteger gasPrice = null,
    HexBigInteger valueAmount = null)
    {
        var function = placeBetTest();
        function.CreateCallInput();
        return function.CreateTransactionInput(AccountManager.Instance.GetAddress(), gas, gasPrice, valueAmount);
    }




    private IEnumerator GetAddrRequest(JsonData jsonData)
    {
        var craeteInput = CreatGetAddrInput(jsonData);
        var request = new EthCallUnityRequest(Wallet._url);
        yield return request.SendRequest(craeteInput, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());


        if (request.Exception == null)
        {

            UnityEngine.Debug.LogError(">>>>>GetAddrRequest>>>>>>>success===" + request.Result);
        }
        else
        {
            UnityEngine.Debug.LogError(">>>>>>>>faild===" + request.Exception);
        }

    }

    private Function placeBet(){
        return mContract.GetFunction("placeBet");
    }

    private TransactionInput CreatPlaceBetInput (JsonData jsonData, BigInteger _input, float _bet, int type)
	{
		var function = placeBet ();


		var sign = jsonData ["sign"];
		var commit_hash = jsonData ["sign"] ["commit_hash"];
		var commit_int_str = jsonData ["sign"] ["commit_int_str"];
		var commitLastBlock = jsonData ["sign"] ["blocknumber"];
		var rr = jsonData ["sign"] ["r"];
		var ss = jsonData ["sign"] ["s"];
       
		var r_str = HexString2Byte32 (rr.ToString ().Substring (2));
		var s_str = HexString2Byte32 (ss.ToString ().Substring (2));
		//var c_str = HexString2Byte32(commit_hash.ToString().Substring(2));
		//var c_str = String2BigInt (commit_int_str.ToString ().Substring (2));

		var commit = BigInteger.Parse (commit_int_str.ToString ());
		var lastb = BigInteger.Parse (commitLastBlock.ToString ());
	
        var bet = UnitConversion.Convert.ToWei(_bet);
        var gasprice = UnitConversion.Convert.ToWei(AccountManager.Instance.GetGasPrice(), UnitConversion.EthUnit.Gwei);

        TransactionInput transactionInput = function.CreateTransactionInput(AccountManager.Instance.GetAddress(), new HexBigInteger(gasLimit),
                                                                            new HexBigInteger(gasprice),
                                                                            new HexBigInteger(bet));
        return function.CreateTransactionInput(transactionInput, _input, type, lastb, commit, r_str, s_str);


    }

    private IEnumerator PlaceBetRequest(JsonData jsonData,BigInteger _input,float _bet,int type)
    {
        var transactionInput = CreatPlaceBetInput(jsonData,_input,_bet,type);
        var request = new EthCallUnityRequest(Wallet._url);
        string privatekey = AccountManager.Instance.GetPrivateKey();
        string address_ = AccountManager.Instance.GetAddress();
        var transactionSignedRequest = new TransactionSignedUnityRequest(Wallet._url, privatekey, address_);
       
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
       // yield return request.SendRequest(craeteInput, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());


        if (request.Exception == null)
        {
            
           
            //Commit2RobotRequestCallBack(transactionSignedRequest.Result);
           // StartCoroutine(Timer(transactionSignedRequest.Result,_bet));
            Commit2RobotRequestCallBack(transactionSignedRequest.Result, _bet);


        }
        else
        {
            
            ViewManager.ShowMessageBox(request.Exception.ToString());
            ViewManager.CloseWaitTip();
            UnityEngine.Debug.LogError(">>>>>>>>faild===" + request.Exception);
        }

    }



	private Function placeBetTest()
    {
        return mContract.GetFunction("placeBetTest");
    }

    private IEnumerator TestPlaceBetRequest(JsonData jsonData)
    {
        var craeteInput =  TestCreatPlaceBetInput(jsonData);
        var request = new EthCallUnityRequest(Wallet._url);
        yield return request.SendRequest(craeteInput, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());


        if (request.Exception == null)
        {
            var unitConversion = new UnitConversion();
            var result = request.Result;
            //BigInteger num = System.Convert.ToInt64(result, 16);
            ////var _num = unitConversion.FromWei(num, 18);
            //var arrayType = ArrayType.CreateABIType("uint[20]");
            //var list = arrayType.Decode<List<BigInteger>>(result);
            //for (var i = 0; i < list.Count; i++){
            UnityEngine.Debug.LogError(">>>>>>>>success=======" + result);
            //}
            UnityEngine.Debug.LogError(">>>>>>>>>>>>success");
        }
        else
        {
            UnityEngine.Debug.LogError(">>>>>>>>faild==="+request.Exception);
        }

    }

	    private CallInput TestCreatPlaceBetInput(JsonData jsonData)
    {
        var function = placeBet();
        var sign = jsonData["sign"];
      
        var commit_hash =jsonData["sign"]["commit_hash"];
        var commit = commit_hash.ToJson();
        var commitLastBlock = jsonData["sign"]["blocknumber"];
        var cc = "0x9964ff53498b2704552f1df7b29311897372c73fc99ceecbc066b5249ad148d1";
        var rr = jsonData["sign"]["r"];
        var ss= jsonData["sign"]["s"];
     
        var ccc = cc.Substring(2);
        var c_str = HexString2Byte32 (ccc);
        var ssssss = rr.ToString();
      
        var r_str = HexString2Byte32 (rr.ToString().Substring(2));
        var s_str = HexString2Byte32 (ss.ToString().Substring(2));

        return function.CreateCallInput(1, 2, commitLastBlock.ToJson(), commit, r_str, s_str );
      

    }

		// 下注选项转为byte32(猜硬币,一个骰子, 2个骰子)
	private byte[] BetCase2Byte32(int[] nums) {
		byte[] bytes32= new byte[32];
		for (int i = 0; i< nums.Length; i++) {
			int idx = nums[i] / 8;
			int offset = nums[i] % 8;
			offset = 1 << offset;
			int b = bytes32[idx];
			b = b | offset;
			bytes32[idx] = (Byte)b;

			// // 以二进制输出
			// Debug.LogError("offset " + Convert.ToString(offset, 2));
			// Debug.LogError("b " + Convert.ToString(b, 2));
		}
		return bytes32;
	}

    private BigInteger Byte32ToBigInteger(byte[] byte32){

        BigInteger bigInteger = new BigInteger(byte32);
        return bigInteger;
    }

	// 下注选项转为byte32(猜数字)
	private byte[] GuessNumber2Byte32(int num) {
		byte[] bytes32= new byte[32];
		int idx = num / 8;
		int offset = num % 8;
		for (int i = 0; i < idx; i++) {
			if (i == 0) {
				bytes32[i] = 254;
			} else {
				bytes32[i] = 255;
			}
		}
		int b = (1 << (offset + 1)) - 1;
		bytes32[idx] = (Byte)b;
		return bytes32;
	}


	public byte[] HexString2Byte32( string src ){
		if( src.Length != 64 ){
			return null;
		}
		byte[] bytes32= new byte[32];
		for (int i = 1; i < src.Length; i+=2) {
			//string t(src[i]);
			int ret1 = Convert.ToInt32 (src[i-1].ToString(), 16);
			int ret2 = Convert.ToInt32 (src[i].ToString(), 16);

			int ret = (ret2 & 0x0000000f | ((ret1 <<4) & 0x000000f0));
			bytes32 [i / 2] = BitConverter.GetBytes (ret)[0];
		}

		return bytes32;
	}

    ///// <summary>
    ///// 获取事件
    ///// </summary>
    ///// <returns>The get event logs.</returns>
    ///// <param name="callback">Callback.</param>
    ///// <param name="topic">Topic.</param>
    ///// <param name="txHash">Tx hash.</param>
    //public IEnumerator wwwGetEventLogs(Action<JsonData,string> callback,string topic,string txHash){
    //    topic = "0xd4f43975feb89f48dd30cabbb32011045be187d1e11c8ea9faa43efc35282519";
    //    EventABI JackpotPaymentABI = new EventABI("Payment");
    //    var ssss=mContract.GetDefaultFilterInput();
    //    //var multiplyEvent = mContract.GetEvent("Payment");
    //    //var filterAll= multiplyEvent.CreateFilterAsync();
    //    UnityEngine.Debug.LogError(">>>>>>>filterAll===" +ssss);
    //    string _url = string.Format("https://api-{0}.etherscan.io/api?module=logs&action=getLogs&fromBlock=379224&toBlock=latest&address={1}&topic0={2}&apikey=", urlNames[0], contractAddress,topic);

    //    WWW www = new WWW(_url);//定义一个www类型的对象
    //    yield return www;//返回下载的值
    //    JsonData jsonData = JsonMapper.ToObject(www.text);
    //    UnityEngine.Debug.LogError(">>>>>>>www.text===" + www.text);
    //    if (www.error != null)
    //    {//判断下载的资源是否有错误
    //        Debug.Log("Error: " + www.error);
    //        yield break;
    //    }
    //    callback(jsonData,txHash);
    //}


  
    /// <summary>
    /// 下注成功返回
    /// </summary>
    /// <param name="txHash">Tx hash.</param>
    private void Commit2RobotRequestCallBack(string txHash,float _bet)
    {
        
        Action<JsonData,string,float> action = new Action<JsonData,string,float>(CheckTxSuccessCallBack);
        StartCoroutine(CheckTransactionSuccess(action, txHash,_bet));
    }

    /// <summary>
    /// 获取事件
    /// </summary>
    /// <returns>The get event logs.</returns>
    /// <param name="callback">Callback.</param>
    /// <param name="topic">Topic.</param>
    /// <param name="txHash">Tx hash.</param>
    public IEnumerator wwwGetEventLogs(Action<JsonData, string> callback, string topic, string txHash)
    {
        string _url = historyUrl+AccountManager.Instance.GetAddress();
        UnityEngine.Debug.LogError(_url);
        WWW www = new WWW(_url);//定义一个www类型的对象
        yield return new WaitForSeconds(delayTime);
        yield return www;//返回下载的值
        if (www.error != null) {//判断下载的资源是否有错误
            Debug.Log ("Error: " + www.error);
            ViewManager.ShowMessageBox("http连接失败"+www.error);

            yield break;
        } else {
            JsonData jsonData = JsonMapper.ToObject(www.text);
            UnityEngine.Debug.LogError(www.text);
            callback(jsonData, txHash);
        }
    }

   

    /// <summary>
    /// 获取事件成功返回
    /// </summary>
    /// <param name="jsonData">Json data.</param>
    /// <param name="txHash">Tx hash.</param>
    private void GetEventLogsCallBack(JsonData jsonData, string txHash)
    {
		if (jsonData != null && jsonData["games"] != null)
        {
            var list = jsonData["games"];

            //itemList.Add(item);
            var isSucess = false;
            UnityEngine.Debug.LogError(">>>>>>>count==" + list.Count);
            var payment = "";
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i]["tx_hash"].Equals(txHash))
                {
                    payment = list[i]["dice_payment"].ToString();
                    if(payment == ""){
                        
                    }else if(payment == "0"){
                        isSucess = true;
                    }else{
                        isSucess = true;
                    }
                    break;
                }
            }
            if(isSucess){
                ViewManager.CloseWaitTip();
                if(payment == "0"){
                    ViewManager.ShowMessageBox("您输了");
                }else{
                    var unitConversion = new UnitConversion();
                    ViewManager.ShowMessageBox("赢："+ unitConversion.FromWei(BigInteger.Parse(payment), 18).ToString("0.0000"));
                }
                AccountManager.Instance.UpdateBalance(null);
            }else{
                StartGetEventLogs(txHash);
            }
         
            UnityEngine.Debug.LogError(">>>>>>>>>>是否成功---" + isSucess);

        }else{
            //重新请求
            StartGetEventLogs(txHash);

        }
    }

    private IEnumerator CheckTransactionSuccess(Action<JsonData,string,float> callBack, string txHash,float _bet){
        string [] _params = {txHash};
        JsonData json = new JsonData();
        json.Add(txHash);
        string _url = string.Format("https://api.infura.io/v1/jsonrpc/{0}/eth_getTransactionReceipt?params={1}",AccountManager.Instance.GetCurrentNetName(),json.ToJson());
        UnityEngine.Debug.LogError(_url);
        WWW www = new WWW(_url);//定义一个www类型的对象

        yield return www;//返回下载的值
        if (www.error != null)
        {//判断下载的资源是否有错误
            Debug.Log("Error: " + www.error);
            ViewManager.CloseWaitTip();
            ViewManager.ShowMessageBox("http连接失败" + www.error);
            yield break;
        }
        else
        {
            JsonData jsonData = JsonMapper.ToObject(www.text);
            UnityEngine.Debug.LogError(www.text);
            callBack(jsonData,txHash,_bet);
        }    
    }

    private void CheckTxSuccessCallBack(JsonData jsonData,string txHash,float _bet){
        if(jsonData != null){
            try{
                if (jsonData["result"] != null)
                {
                    if (jsonData["result"]["status"].ToString() == "0x1")
                    {
                        ViewManager.CloseWaitTip();
                        // AccountManager.Instance.MinusBalanceTemporary(_bet);
                        ViewManager.ShowMessageBox("下注成功", delegate {
                            ViewManager.ShowWaitTip("等待结果...", 999999);
                        });
                        AccountManager.Instance.UpdateBalance(null);
                        StartGetEventLogs(txHash);
                    }
                    else
                    {
                        ViewManager.CloseWaitTip();
                        ViewManager.ShowMessageBox("下注失败");
                    }


                } else{
                    //todo 重新发起请求
                    Commit2RobotRequestCallBack(txHash,_bet);
                }           
            } catch(Exception ex){
                ViewManager.CloseWaitTip();
                ViewManager.ShowMessageBox("下注失败");

            }      

        }else{
            //重新发起请求
            Commit2RobotRequestCallBack(txHash,_bet);
        }
    }

  


    private void testForRecipt(){
        string _url = "https://kovan.infura.io/v3/a490c3214d3c47f5aa8024ec5d887a6f";
        EthGetTransactionReceiptUnityRequest request = new EthGetTransactionReceiptUnityRequest(_url);
        
    }



    private void StartGetEventLogs(string txHash){
        Action<JsonData, string> action = new Action<JsonData, string>(GetEventLogsCallBack);
        StartCoroutine(wwwGetEventLogs(GetEventLogsCallBack, null, txHash));
        Debug.Log(string.Format("Timer2 is up !!! time=${0}", Time.time));
    }




   



}
