using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Core
{
    public class DebugLogger : Singleton<DebugLogger>
    {
        [SerializeField] private TextMeshProUGUI debugAreaText;
        [SerializeField] private int maxLines = 50;

        private int _currentLineCount = 0;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        private void OnEnable()
        {
            LogInternal($"{this.GetType().Name} enabled", "white");
        }

        public void LogInfo(string message)
        {
            LogInternal(message, "green");
        }

        public void LogError(string message)
        {
            LogInternal(message, "red");
        }

        public void LogWarning(string message)
        {
            LogInternal(message, "yellow");
        }

        private void LogInternal(string message, string color)
        {
            if (_currentLineCount >= maxLines)
            {
                ClearLines();
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"<color=\"{color}\">{timestamp} {message}</color>");

            debugAreaText.text = _logBuilder.ToString();
            _currentLineCount++;
        }

        private void ClearLines()
        {
            _logBuilder.Clear();
            debugAreaText.text = string.Empty;
            _currentLineCount = 0;
        }
    }
}