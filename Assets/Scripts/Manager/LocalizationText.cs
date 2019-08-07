using UnityEngine;  
using System.Collections;  
using UnityEngine.UI;  
public class LocalizationText : MonoBehaviour   
{  
    // 1.在编辑器里赋值key,在start的时候会调用Localize
    // 2.在代码里赋值的调用SetKey, 也会去调用Localize
    // 3.在切换语言的时候, 在LocalizationManager里会找到所有的LocalizationText组件, 并调用Localize方法
    public string key = "";  
    void Start() {  
        Localize();
    }

    public void SetKey(string _key) {
        key = _key;
        Localize();
    }

    public void Localize() {
        GetComponent<Text>().text = LocalizationManager.Instance.GetValue(key);
    }
} 
