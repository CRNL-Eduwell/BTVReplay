using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CielaSpike;

public class LoadingManager : MonoBehaviour
{
    #region Properties
    private static LoadingManager m_Instance;

    [SerializeField] private Canvas m_Canvas;
    [SerializeField] GameObject m_LoadingCirclePrefab;
    #endregion

    #region Private Methods
    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    #region Public Methods
    public static LoadingCircle Open()
    {
        GameObject loadingCircleGameObject = Instantiate(m_Instance.m_LoadingCirclePrefab, m_Instance.m_Canvas.transform);
        LoadingCircle loadingCircle = loadingCircleGameObject.GetComponent<LoadingCircle>();
        return loadingCircle;
    }
    public static void Load(IEnumerator action, GenericEvent<float, string> onChangeProgress, Action<TaskState> callBack = null)
    {
        m_Instance.StartCoroutine(c_Load(action, onChangeProgress, callBack));
    }
    public static IEnumerator c_Load(IEnumerator action, GenericEvent<float, string> onChangeProgress, Action<TaskState> callBack = null)
    {
        LoadingCircle loadingCircle = Open();
        UnityAction<float, string> progressHandler = (progress, message) => loadingCircle.Set(progress, message);
        onChangeProgress.AddListener(progressHandler);
        yield return m_Instance.StartCoroutineAsync(action, out Task task);
        switch (task.State)
        {
            case TaskState.Done:
                yield return new WaitForSeconds(0.2f);
                break;
            case TaskState.Error:
                Debug.LogError("LoadingManager: a background loading task failed.");
                if (task.Exception != null)
                {
                    Debug.LogException(task.Exception);
                    ApplicationState.displayMessage("Loading failed", "NOK", task.Exception.Message);
                }
                break;
        }
        onChangeProgress.RemoveListener(progressHandler);
        loadingCircle.Close();
        if (callBack != null) callBack.Invoke(task.State);
    }
    #endregion
}