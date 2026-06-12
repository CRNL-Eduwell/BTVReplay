using System;
using System.Threading.Tasks;
using UnityEngine;

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

    /// <summary>
    /// Shows a loading circle while the task runs and reports its failure to the user. The
    /// IProgress handed to the action marshals progress reports back to the main thread, so the
    /// action may Report() from inside a Task.Run worker.
    /// </summary>
    public static async void Load(Func<IProgress<(float progress, string message)>, Task> action)
    {
        LoadingCircle loadingCircle = Open();
        // Progress posts its reports asynchronously, so a late report can arrive after the
        // circle was closed and destroyed - hence the null check (Unity fake-null).
        Progress<(float progress, string message)> progress = new Progress<(float progress, string message)>(report =>
        {
            if (loadingCircle != null) loadingCircle.Set(report.progress, report.message);
        });
        try
        {
            await action(progress);
            await Task.Delay(200); // let the user see the finished state, like the old WaitForSeconds did
        }
        catch (Exception ex)
        {
            Debug.LogError("LoadingManager: a background loading task failed.");
            Debug.LogException(ex);
            ApplicationState.displayMessage("Loading failed", "NOK", ex.Message);
        }
        finally
        {
            loadingCircle.Close();
        }
    }
    #endregion
}
