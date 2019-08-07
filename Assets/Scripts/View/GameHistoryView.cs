using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using LitJson;
using System;

public class GameHistoryView : MonoBehaviour {

	public GameObject mRecordItem;
	public Transform mContent;
	private List<ItemData> dataList = new List<ItemData>();
    private  string historyUrl = "https://dice.skrskr.online/history?type={0}&addr={1}";
    private int mModulo;

    public Toggle showMeToggle;
    public void SetModulo(int modulo) {
        mModulo = modulo;
    }
    private void OnEnable() {
        // ShowAllHistory();
        OnShowMeClick();
    }

    public void OnShowMeClick() {
        if (showMeToggle.isOn) {
            ShowMyHistory();
        } else {
            ShowAllHistory();
        }
    }

    private void ShowAllHistory() {
        ViewManager.ShowWaitTip("正在请求记录......");
        StartCoroutine(GetHistory(GetHistoryCallBack));
    }

    private void ShowMyHistory() {
        ViewManager.ShowWaitTip("正在请求记录......");
        StartCoroutine(GetHistory(GetHistoryCallBack, AccountManager.Instance.GetAddress()));
    }

	private void ShowData() {
		Utils.DestroyAllChildren(mContent, true);
		for (int i = 0; i < dataList.Count; i++) {
			ItemData data = dataList[i];
			GameObject recordItem = Instantiate(mRecordItem);
			recordItem.SetActive(true);
			recordItem.transform.SetParent(mContent);
			recordItem.transform.localPosition = Vector3.zero;
			recordItem.transform.localScale = Vector3.one;
			recordItem.GetComponent<RecordItem>().InitItem(data);
            recordItem.GetComponent<RecordItem>().SetGray(i%2 == 0);
			// Utils.AddButtonClickEvent(recordItem.GetComponent<Button>(), delegate() {
			// 	OnItemClick(data);			
			// });
		}
	}
	
	// 点击单个item跳转到DetailView
	private void OnItemClick(ItemData data) {
		Debug.LogError("OnItemClick " + data.mAddress);
		// TODO 还有数据需要传过去
		GameView.Instance.ToDetailView();
	}

    private IEnumerator GetHistory(Action<JsonData> callBack ,string address){
        //string _url = string.Format(historyUrl);
        string _url = string.Format(historyUrl, mModulo, address);
        // string _url = historyUrl + address;
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
            callBack(jsonData);
        }    
    }



    private IEnumerator GetHistory(Action<JsonData> callBack)
    {
        //string _url = string.Format(historyUrl);
        string _url = string.Format(historyUrl, mModulo, "");
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
            callBack(jsonData);
        }
    }

    private void GetHistoryCallBack(JsonData jsonData){
        ViewManager.CloseWaitTip();
        dataList.Clear();
        if(jsonData != null && jsonData["games"] != null){
            var list = jsonData["games"];
            for (var i = 0; i < list.Count;i++){
                ItemData data = new ItemData();
                data.mAddress = list[i]["address_from"].ToString();
                // data.mBetCase = list[i]["modulo"].ToString();
                data.mBetMoney = list[i]["amount"].ToString();
                data.mWin = list[i]["dice_payment"].ToString();
                // data.mResult = list[i]["jackpot_payment"].ToString();
                data.mBigWin = list[i]["jackpot_payment"].ToString();
                data.mModulo = UInt32.Parse(list[i]["modulo"].ToString());
                data.mBetMask = list[i]["bet_mask"].ToString();
                data.mRevealBlockHash = list[i]["reveal_block_hash"].ToString();
                dataList.Add(data);
            }
        }
        ShowData();
    }

    public void OnBtnClose() {
        Utils.DestroyAllChildren(mContent, true);
        GameView.Instance.OnBackToGamePlay();
    }
}
