using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XHD.Core.Common
{
    /// <summary>
    /// 人民币金额大写转换
    /// </summary>
    public static class MoneyToChinese
    {
        private static readonly string[] Digits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static readonly string[] Units = { "", "拾", "佰", "仟" };
        private static readonly string[] GroupUnits = { "", "万", "亿", "兆" };

        /// <summary>
        /// 金额转人民币大写，例如 1234.56 -> 壹仟贰佰叁拾肆元伍角陆分
        /// </summary>
        public static string ToChinese(decimal amount)
        {
            var negative = amount < 0;
            amount = Math.Abs(amount);

            var integerPart = (long)Math.Floor(amount);
            var decimalPart = (int)Math.Round((amount - integerPart) * 100, MidpointRounding.AwayFromZero);
            if (decimalPart >= 100)
            {
                integerPart += 1;
                decimalPart = 0;
            }

            var result = new StringBuilder();
            if (negative) result.Append("负");
            result.Append(ConvertInteger(integerPart)).Append("元");

            if (decimalPart == 0)
            {
                result.Append("整");
                return result.ToString();
            }

            var jiao = decimalPart / 10;
            var fen = decimalPart % 10;

            if (jiao > 0)
            {
                result.Append(Digits[jiao]).Append("角");
            }
            else if (fen > 0)
            {
                result.Append("零");
            }

            if (fen > 0)
            {
                result.Append(Digits[fen]).Append("分");
            }
            else
            {
                result.Append("整");
            }

            return result.ToString();
        }

        private static string ConvertInteger(long number)
        {
            if (number == 0) return "零";

            var groups = new List<int>();
            while (number > 0)
            {
                groups.Add((int)(number % 10000));
                number /= 10000;
            }

            var result = new StringBuilder();
            for (var i = groups.Count - 1; i >= 0; i--)
            {
                var group = groups[i];
                if (group == 0)
                {
                    // 中间整万/整亿段补零
                    if (result.Length > 0 && groups.Take(i).Any(a => a > 0))
                    {
                        if (!result.ToString().EndsWith("零")) result.Append("零");
                    }
                    continue;
                }

                if (result.Length > 0 && group < 1000)
                {
                    if (!result.ToString().EndsWith("零")) result.Append("零");
                }

                result.Append(ConvertGroup(group)).Append(GroupUnits[i]);
            }

            return result.ToString().TrimEnd('零');
        }

        private static string ConvertGroup(int group)
        {
            var result = new StringBuilder();
            var pendingZero = false;

            for (var i = 3; i >= 0; i--)
            {
                var divisor = (int)Math.Pow(10, i);
                var digit = (group / divisor) % 10;

                if (digit == 0)
                {
                    if (result.Length > 0) pendingZero = true;
                }
                else
                {
                    if (pendingZero)
                    {
                        result.Append("零");
                        pendingZero = false;
                    }
                    result.Append(Digits[digit]).Append(Units[i]);
                }
            }

            return result.ToString();
        }
    }
}
