using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 文件处理器
    /// </summary>
    public class FileHandler : ILogHandler
    {
        private readonly string filePath;
        private readonly object fileLock = new object();

        public FileHandler()
        {
            string logDir = Path.Combine(Application.persistentDataPath, "Logs");
            filePath = Path.Combine(logDir, $"Log.txt");


            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        public void OnStart() 
        {
            //清理旧文件
            File.WriteAllText(filePath, string.Empty, Encoding.UTF8);
            Append(BuildHeaderText());
        }
        public void Write(LogData data)
        {
            Append(BuildLogText(data));
        }
        public void OnEnd()
        {
            Append(BuildExitText());
        }

        private void Append(string content)
        {
            //线程保护
            lock (fileLock)
            {
                File.AppendAllText(filePath, content, new UTF8Encoding(false));
            }
        }
        private string BuildHeaderText()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("============================================================");
            sb.AppendLine("EmptyFrame 日志文件");
            sb.AppendLine("============================================================");
            sb.AppendLine();

            sb.AppendLine($"产品名称      : {Application.productName}");
            sb.AppendLine($"产品版本      : {Application.version}");
            sb.AppendLine($"公司名称      : {Application.companyName}");
            sb.AppendLine($"Unity版本     : {Application.unityVersion}");
            sb.AppendLine($"运行平台      : {Application.platform}");
            sb.AppendLine();

            sb.AppendLine($"设备名称      : {SystemInfo.deviceName}");
            sb.AppendLine($"操作系统      : {SystemInfo.operatingSystem}");
            sb.AppendLine();

            sb.AppendLine($"处理器型号    : {SystemInfo.processorType}");
            sb.AppendLine($"处理器核心数  : {SystemInfo.processorCount}");
            sb.AppendLine();

            sb.AppendLine($"系统内存      : {SystemInfo.systemMemorySize} MB");
            sb.AppendLine();

            sb.AppendLine($"显卡型号      : {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"显存大小      : {SystemInfo.graphicsMemorySize} MB");
            sb.AppendLine();

            sb.AppendLine($"屏幕分辨率    : {Screen.currentResolution.width} × {Screen.currentResolution.height} @ {Screen.currentResolution.refreshRateRatio.value:F0}Hz");
            sb.AppendLine();

            sb.AppendLine("============================================================");
            sb.AppendLine("( •̀ ω •́ )✧ 应用程序已启动");
            sb.AppendLine($"时间 : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("============================================================");
            sb.AppendLine();

            return sb.ToString();
        }
        private string BuildExitText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("============================================================");
            sb.AppendLine("(￣▽￣)／ 应用程序已退出");
            sb.AppendLine($"时间 : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("============================================================");
            sb.AppendLine();
            return sb.ToString();
        }
        private string BuildLogText(LogData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[').Append((data.Time == default ? DateTime.Now : data.Time).ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");
            sb.Append('[').Append(data.Type).Append("] ");
            sb.AppendLine(data.Message);

            if (!string.IsNullOrEmpty(data.StackTrace))
            {
                sb.AppendLine(data.StackTrace);
            }

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
