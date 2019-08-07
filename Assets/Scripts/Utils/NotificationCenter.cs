using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// 通知中心，继承MonoBehavior的单例，用来处理各模块之间的消息传递
/// 本质其实就是观察者模式，可以参照java用接口观察者模式
/// C#用委托替换了接口实现了方法的传递，将委托存在字典对应的key值中
/// </summary>
public class NotificationCenter : MonoBehaviour {
	#region 单例							
	//继承monobehavior的单例
	private static NotificationCenter center = null;
	public static NotificationCenter DefaultCenter () {
		if (center == null) {
			GameObject g = new GameObject ("NotificationCenter");				//若示例为空则创建一个物体并把该实例附加
			DontDestroyOnLoad(g);
			center = g.AddComponent <NotificationCenter> ();
		}
		return center;
	}
	#endregion
	#region 变量
	//字典选用Action<Notifiacation>类型，实现了对各种参数的转换，缺陷是只能传一个参数
	//不过对于unity大部分工程应该是够用的，如果个别项目需要可以将Action改为Func实现
	//返回值，或者加多个参数等等
	private Dictionary <string, Action <Notification>> notificationDic;		

	// 队列用来存需要执行的notification, 
	// 因为在子线程中调用可能出问题, 所以存起来放到主线程调用
	private Queue<Notification> notificationQueue;
	#endregion
	#region 方法
	void Awake () {
		notificationDic = new Dictionary<string, Action<Notification>> ();		//初始化字典变量
		notificationQueue = new Queue<Notification> ();
	}

	void Update() {
		while (notificationQueue != null && notificationQueue.Count > 0) {
			Notification notification = notificationQueue.Dequeue();
			if (notificationDic.ContainsKey (notification.name)) {				
				notificationDic [notification.name] (notification);
			}
		}
	}

	//添加观察者的方法
	public void AddObserver (string name, Action <Notification> action) {
		if (notificationDic.ContainsKey (name)) {
			notificationDic [name] += action;						//若字典存在该key则将对应的委托添加传来的委托
		} else {
			notificationDic.Add (name, action);						//不存在则添加该key和传来的委托
		}
	}
	//移除观察者的方法
	public void RemoveObserver (string name, Action <Notification> action) {
		if (notificationDic.ContainsKey (name)) {
			notificationDic [name] -= action;
			if (notificationDic [name] == null) {
				notificationDic.Remove (name);
			}
		}
	}
	//传递消息的方法
	public void PostNotification (string name) {
		PostNotification (name, null);
	}
	public void PostNotification (string name, object data) {
		PostNotification (new Notification (name, null));
	}
	//如果字典存在key值则运行对用的委托
	public void PostNotification (Notification notification) {
		// if (notificationDic.ContainsKey (notification.name)) {				
		// 	notificationDic [notification.name] (notification);
		// }
		notificationQueue.Enqueue(notification);
	}
	#endregion
}
public class Notification {
	public string name;												//存储委托存在字典中的名字
	// public Component sender;									//注册的用户组件
	public object data;												//存放的信息
	public Notification (string name) {
		this.name = name;
		this.data = null;
	}
	public Notification (string name, object data) {
		this.name = name;
		this.data = data;
	}
}
