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
using Nethereum.Hex.HexTypes;
public class Utils : MonoBehaviour {
    public GameObject mainView;

	void Awake () {
        
    }
	void Start () {
        
	}
	public static void AddButtonClickEvent(Button btn, UnityAction action) {
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(action);
	}
	public static string ReadFile()
	{
		TextAsset ta = Resources.Load<TextAsset>("ABI");
		return ta.text;
	}

	public static void DestroyAllChildren(Transform go, bool flag = false) {
		if (go == null) return;
		for (int i = go.childCount - 1; i >= 0; i--) {
			if (flag) {
				GameObject.DestroyImmediate(go.GetChild(i).gameObject);
			} else {
				GameObject.Destroy(go.GetChild(i).gameObject);
			}
		}
	}

    public static string getTopicByName(string name){
        EventABI eventABI = new EventABI(name);
        var topic = eventABI.Sha33Signature.EnsureHexPrefix();
        return topic;
    }

//两个dice选择结果
	public static List<int> Decode2DiceBetMask( UInt64 mask )
	{		
		List<int> enums = new List<int> ();
		List<int> result = new List<int> ();
		for (var v = 1; v <= 6; v++)
		{
			for (var i = 1; i <= 6; i++)
			{
				enums.Add (v + i);
			}
		}
		UInt64 constBase = 1;
		for (int i = 0; i < enums.Count; i++) {
			if ( (mask & (constBase << i)) != 0 ) {
				if( !result.Contains(enums[i])){
					result.Add (enums[i]);
				}
			}
		}

//		foreach(var a in result )
//			Debug.LogError(a.ToString() );

		return result;
	}

	//猜硬币，猜数字，一个dice结果
	public static  UInt64 DecodeResultMask( string reveal_block_hash, string mask, UInt32 modulo  )
	{		
		
		UInt64 bet_mask = Convert.ToUInt64 (mask);
		// var number = HexBigInteger.Parse (reveal_block_hash);
		var number = new HexBigInteger(reveal_block_hash);
		UInt32 dice = (UInt32)(number.Value % modulo);
		
		if (modulo < 40) {
			return (UInt64)(Math.Pow (2, dice));
		}
		else {
			return dice;
		}
	}

	//两个dice结果
	public static  List<int> Decode2DiceResultMask( string reveal_block_hash, string mask, UInt32 modulo  )
	{
		UInt64 resultMask = DecodeResultMask (reveal_block_hash, mask, modulo);
		return Decode2DiceBetMask (resultMask);
	}

	//jackpot
	public static  UInt64 DecodeJackpot( string reveal_block_hash, UInt32 modulo  )
	{	
		// var number = BigInteger.Parse (reveal_block_hash);
		var number = new HexBigInteger(reveal_block_hash);
		return (UInt64)((number.Value / modulo)%1000);
	}

}
