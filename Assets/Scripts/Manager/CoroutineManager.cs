

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class CoroutineManager
{
    private class InnerCoroutine : MonoBehaviour { }
    private static InnerCoroutine coroutine;

    static CoroutineManager()
    {
        if (coroutine == null)
        {
            coroutine = new GameObject("CoroutineManager").AddComponent<InnerCoroutine>();
            GameObject.DontDestroyOnLoad(coroutine);
        }

    }

    public static Coroutine StartCoroutineTask(IEnumerator routine)
    {
        return coroutine.StartCoroutine(routine);
    }

    private static IEnumerator StartInnerCoroutine(IEnumerator routine, Action<object> callback)
    {
        yield return StartCoroutineTask(routine);
        callback(routine.Current);
    }


    public static void StartCoroutineTask(IEnumerator routine, Action<object> callback)
    {
        StartCoroutineTask(StartInnerCoroutine(routine, callback));
    }



    public static void StopCoroutineTask(IEnumerator routine)
    {
        coroutine.StopCoroutine(routine);
    }

    public static void StopAllCoroutineTask()
    {
        coroutine.StopAllCoroutines();
    }


}