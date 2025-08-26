using UnityEngine;

public class ApplicationBoot : MonoBehaviour
{
    [SerializeField] private bool runInBackground = true;
    [SerializeField] private int targetFps = 60;
    [SerializeField] private int vSync = 0; // 0=Off(����), 1=��

    void Awake()
    {
        Application.runInBackground = runInBackground;
        QualitySettings.vSyncCount = vSync;
        Application.targetFrameRate = targetFps;
    }
}
