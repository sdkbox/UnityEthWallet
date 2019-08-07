using UnityEngine;  
using System.Collections;  
using System.Collections.Generic;  
using UnityEngine.UI;
  
public class LocalizationManager  
{  
    //单例模式  
    private static LocalizationManager _instance;  
  
    public static LocalizationManager Instance  
    {  
        get  
        {  
            if (_instance == null)  
            {  
                _instance = new LocalizationManager();  
            }  
  
            return _instance;  
        }  
    }  
  
    private const string chinese = "Chinese";  
    private const string english = "English";  
  
    //选择自已需要的本地语言  
    private static string curLanguage = chinese;  
	
	private static string[] languages = {chinese, english};
    private Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();  
    /// <summary>  
    /// 读取配置文件，将文件信息保存到字典里  
    /// </summary>  
    public LocalizationManager()  
    {  
		foreach (string language in languages) {
			TextAsset ta = Resources.Load<TextAsset>(language);  
			string text = ta.text;  
			string[] lines = text.Split('\n');
			Dictionary<string, string> dic = new Dictionary<string, string>();
			foreach (string line in lines)  
			{  
				if (line == null || string.IsNullOrEmpty(line))  
				{  
					continue;  
				}  
				string[] keyAndValue = line.Split('=');  
				dic.Add(keyAndValue[0], keyAndValue[1]);  
			} 
			dict.Add(language, dic);
		}
        
    }  
  
    /// <summary>  
    /// 获取value  
    /// </summary>  
    /// <param name="key"></param>  
    /// <returns></returns>  
    public string GetValue(string key)  
    {  
		Dictionary<string, string> dic = dict[curLanguage];
        if (dic.ContainsKey(key) == false)  
        {  
            return null;  
        }  
        string value = null;  
        dic.TryGetValue(key, out value);  
        return value;  
    }  

	public void ChangeLanguage(bool toEnglish) {
		if (toEnglish) {
			curLanguage = english;
		} else {
			curLanguage = chinese;
		}
		ResetAllText();
	}

	private void ResetAllText() {
		GameObject canvas = GameObject.Find("Canvas");
		LocalizationText[] children = canvas.transform.GetComponentsInChildren<LocalizationText>(true);
		foreach(LocalizationText t in children) {
 			// Debug.LogError(t.name + " : " + t.key);
			// t.GetComponent<Text>().text = Instance.GetValue(t.key);
            t.Localize();
		}
	}
}  
