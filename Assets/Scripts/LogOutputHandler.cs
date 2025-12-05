using System;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

public class LogOutputHandler : MonoBehaviour
{
    [Header("File settings")]
    public string fileName = "unity_logs.csv";
    public bool usePersistentDataPath = true;
    public bool appendToFile = true;

    [Header("Buffer / performance")]
    [Tooltip("Seconds between forced flushes to disk")]
    public float flushIntervalSeconds = 5f;

    // Internal
    private string fullPath;
    private StreamWriter writer;
    private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
    private float timeSinceLastFlush = 0f;
    private bool headerWritten = false;

    void OnEnable()
    {
        Application.logMessageReceivedThreaded += HandleLog; // receives logs from other threads too
        OpenFile();
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
        FlushAndClose();
    }

    void Start()
    {
        // nothing extra
    }

    void Update()
    {
        // Drain queue and write lines on main thread (thread-safe)
        int wrote = 0;
        while (queue.TryDequeue(out string line))
        {
            try
            {
                writer.WriteLine(line);
                wrote++;
            }
            catch (Exception e)
            {
                Debug.LogError("LogToCsv write error: " + e);
            }
        }

        timeSinceLastFlush += Time.unscaledDeltaTime;
        if (timeSinceLastFlush >= flushIntervalSeconds && wrote > 0)
        {
            writer.Flush();
            timeSinceLastFlush = 0f;
        }
    }

    void OnApplicationQuit()
    {
        FlushAndClose();
    }

    private void OpenFile()
    {
        string basePath = usePersistentDataPath ? Application.persistentDataPath : Application.dataPath;
        fullPath = Path.Combine(basePath, fileName);

        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            bool fileExists = File.Exists(fullPath);

            writer = new StreamWriter(fullPath, appendToFile, Encoding.UTF8)
            {
                AutoFlush = false // we control flushing
            };

            // Write header if not appending or file doesn't exist
            if (!appendToFile || !fileExists)
            {
                string header = "timestamp,logType,message,stackTrace,threaded";
                writer.WriteLine(header);
                writer.Flush();
            }

            Debug.Log($"[LogToCsv] Logging to: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError("[LogToCsv] Failed to open file for writing: " + e);
            writer = null;
        }
    }

    private void FlushAndClose()
    {
        if (writer == null) return;

        // Flush any remaining queued lines (try for a short loop)
        int attempts = 0;
        while (queue.TryDequeue(out string line) && attempts < 10000)
        {
            try { writer.WriteLine(line); } catch { break; }
            attempts++;
        }

        try
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LogToCsv] Error while closing writer: " + e);
        }
        finally
        {
            writer = null;
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Called from any thread
        string timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 UTC
        string logType = type.ToString();
        string threaded = System.Threading.Thread.CurrentThread.IsThreadPoolThread || !System.Threading.Thread.CurrentThread.IsBackground ? "1" : "0";
        string messageEsc = EscapeCsv(logString);
        string stackEsc = EscapeCsv(stackTrace);

        string line = $"{timestamp},{logType},{messageEsc},{stackEsc},{threaded}";
        queue.Enqueue(line);
    }

    // Escape CSV fields: wrap in quotes and double any internal quotes.
    private static string EscapeCsv(string field)
    {
        if (field == null) return "\"\"";
        bool needsQuotes = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
        string escaped = field.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}

//#if UNITY_EDITOR
//    [MenuItem("Tools/LogToCsv/Open Last Log File")]
//    private static void OpenLogFileMenu()
//    {
//        // Try to find an active LogToCsv in scene
//        LogToCsv found = FindObjectOfType<LogToCsv>();
//        if (found == null)
//        {
//            Debug.LogWarning("No LogToCsv instance found in scene. Attach LogToCsv to a GameObject first.");
//            return;
//        }

//        string path = found.fullPath;
//        if (File.Exists(path))
//        {
//            EditorUtility.RevealInFinder(path);
//        }
//        else
//        {
//            Debug.LogWarning("Log file not found: " + path);
//        }
//    }
//#endif
//}
