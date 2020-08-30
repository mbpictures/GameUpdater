using System;
using GameUpdater.Services.Download;

namespace GameUpdater.Services
{
    public static class Util
    {
        public static string FormatBytes(ref double value)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (value >= 1024 && order < sizes.Length - 1) {
                order++;
                value /= 1024;
            }

            return sizes[order];
        }

        public static string FormatDownloadSpeed(double value, bool isInfinite)
        {
            var order = FormatBytes(ref value);
            value = Math.Round(value);
            
            return isInfinite ? "Infinite" : $"{value} {order}/s";
        }

        public static double GetRealBytesPerSecondsFromValue(double value, double max)
        {
            return Math.Abs(value - max) < 0.001 ? ThrottledStream.Infinite : (-Math.Pow(Math.E, (-value + 1500) * 0.01) + 3270000) * 10;
        }
    }
}