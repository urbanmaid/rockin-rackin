using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class VisitRepositoryButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        CacheElement();
    }

    private void OnEnable()
    {
        CacheElement();
        button.onClick.RemoveListener(VisitRepo);
        button.onClick.AddListener(VisitRepo);
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(VisitRepo);
        }
    }

    public void VisitRepo()
    {
        string url = "https://github.com/urbanmaid/rockin-rackin";
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            var processInfo = new System.Diagnostics.ProcessStartInfo(url);
            System.Diagnostics.Process.Start(processInfo);
        }
    }

    private void CacheElement()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }
}
