using System;

namespace OnceMi.AspNetCore.OSS
{
    internal static class TimeUtil
    {
        /// <summary>
        /// 将时间转换成unix时间戳
        /// </summary>
        /// <param name="time">本地时间</param>
        /// <returns>返回单位秒</returns>
        public static long DateTimeToUnixTimeStamp(DateTime time)
        {
            var dto = new DateTimeOffset(time);
            return dto.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 将时间转换成unix时间戳
        /// </summary>
        /// <param name="time">本地时间</param>
        /// <returns>返回单位秒</returns>
        public static long DateTimeToUnixTimeStamp(string timeStr)
        {
            if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr, out var time))
            {
                var dto = new DateTimeOffset(time);
                return dto.ToUnixTimeSeconds();
            }
            throw new ArgumentException("Input string is not time format.");
        }

        /// <summary>
        /// 将unix时间戳转换成时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(long timeStamp)
        {
            if (timeStamp > 4102415999)
            {
                var temp = timeStamp.ToString();
                if (temp.Length >= 10)
                {
                    temp = temp.Substring(0, 10);
                }
                timeStamp = long.Parse(temp);
            }
            var dto = DateTimeOffset.FromUnixTimeSeconds(timeStamp);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        /// 返回Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static long Timestamp() => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        /// <summary>
        /// 返回当前时间
        /// 默认格式 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <returns></returns>
        public static string Date(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return DateTime.Now.ToString(format);
            }
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #region 返回每月的第一天和最后一天

        /// <summary>
        /// 返回每月的第一天和最后一天
        /// </summary>
        /// <param name="month"></param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay"></param>
        public static void ReturnDateFormat(int month, out string firstDay, out string lastDay)
        {
            if (month > 12 || month < 1) month = 12;
            var date = new DateTime(DateTime.Now.Year, month, 1);
            var first = date.Date.AddDays(1 - date.Day);
            var last = first.AddMonths(1).AddMilliseconds(-1);
            firstDay = $"{first:yyyy-MM-dd}";
            lastDay = $"{last:yyyy-MM-dd}";
        }

        #endregion

        #region 返回时间差

        public static string DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
            string dateDiff = null;
            try
            {
                //TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
                //TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
                //TimeSpan ts = ts1.Subtract(ts2).Duration();
                var ts = DateTime2 - DateTime1;
                if (ts.Days >= 1)
                {
                    dateDiff = DateTime1.Month + "月" + DateTime1.Day + "日";
                }
                else
                {
                    if (ts.Hours > 1)
                    {
                        dateDiff = ts.Hours + "小时前";
                    }
                    else
                    {
                        dateDiff = ts.Minutes + "分钟前";
                    }
                }
            }
            catch { }
            return dateDiff;
        }

        /// <summary>
        /// 获得两个日期的间隔
        /// </summary>
        /// <param name="DateTime1">日期一。</param>
        /// <param name="DateTime2">日期二。</param>
        /// <returns>日期间隔TimeSpan。</returns>
        public static TimeSpan DateDiff2(DateTime DateTime1, DateTime DateTime2)
        {
            var ts1 = new TimeSpan(DateTime1.Ticks);
            var ts2 = new TimeSpan(DateTime2.Ticks);
            var ts = ts1.Subtract(ts2).Duration();
            return ts;
        }

        #endregion
    }
}