using UnityEngine;
using UniRx;
using Pathfinding;
using System;

public class ScanManager : MonoBehaviour
{
    private static ScanManager _instance;
    public static ScanManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ScanManager");
                _instance = go.AddComponent<ScanManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private IDisposable scanSubscription;
    private float minimumTimeBetweenScans = 0.1f; // Adjust as needed
    private float lastScanTime;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Schedules a scan of the A* pathfinding grid after a specified delay
    /// </summary>
    /// <param name="scanDelay">The delay in seconds before the scan is performed</param>
    /// <param name="requesterId">Identifier for the requesting object</param>
    public void ScheduleScan(float scanDelay, string requesterId)
    {
        if (scanDelay < 0)
        {
            Debug.LogError($"Scan delay cannot be negative. Requested by: {requesterId}");
            return;
        }

        float timeSinceLastScan = Time.time - lastScanTime;
        if (timeSinceLastScan < minimumTimeBetweenScans)
        {
            scanDelay = Mathf.Max(scanDelay, minimumTimeBetweenScans - timeSinceLastScan);
        }

        scanSubscription?.Dispose();
        scanSubscription = Observable
            .Timer(TimeSpan.FromSeconds(scanDelay))
            .Subscribe(_ =>
            {
                if (AstarPath.active != null)
                {
                    Debug.Log($"Performing scan requested by: {requesterId}");
                    AstarPath.active.Scan();
                    lastScanTime = Time.time;
                }
                else
                {
                    Debug.LogWarning($"AstarPath.active is null. Cannot perform scan requested by: {requesterId}");
                }
            })
            .AddTo(this);
    }

    private void OnDestroy()
    {
        scanSubscription?.Dispose();
    }
}