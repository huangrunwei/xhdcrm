using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using XHD.Core.Models;

namespace XHD.Core.View.Services
{
    public sealed class CustomerScoreResult
    {
        public int Score { get; set; }
        public string Label { get; set; } = "待激活客户";
        public string Color { get; set; } = "#ef4444";
    }

    /// <summary>
    /// 客户健康分统一计算入口。
    /// 评分规则：回款 40 分 + 复购 20 分 + 最近跟进 20 分 + 合同 10 分 + 无欠款 10 分。
    /// 订单、收款、合同、客户跟进变更后调用 RecalculateAsync，保证列表中的分数与详情页一致。
    /// </summary>
    public static class CustomerScoreService
    {
        public static async Task<CustomerScoreResult> RecalculateAsync(IFreeSql fsql, string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return new CustomerScoreResult();
            }

            var orders = await fsql.Select<Sale_order>()
                .Where(a => a.customer_id == customerId && a.isDelete != 1)
                .ToListAsync();
            var contracts = await fsql.Select<Sale_contract>()
                .Where(a => a.customer_id == customerId && a.isDelete != 1)
                .ToListAsync();
            var follows = await fsql.Select<CRM_follow>()
                .Where(a => a.customer_id == customerId && a.isDelete != 1)
                .ToListAsync();

            var orderIds = orders.Select(a => a.id).ToList();
            var receipts = orderIds.Count == 0
                ? new List<Finance_Receive>()
                : await fsql.Select<Finance_Receive>()
                    .Where(a => orderIds.Contains(a.order_id) && a.isDelete != 1)
                    .ToListAsync();

            var orderAmount = orders.Sum(a => a.total_amount ?? a.Order_amount ?? 0m);
            var received = receipts.Sum(a => a.Receive_amount ?? 0m);
            var arrears = Math.Max(orderAmount - received, 0m);
            var lastFollowTime = follows
                .Where(a => a.follow_time.HasValue)
                .Select(a => a.follow_time)
                .OrderByDescending(a => a)
                .FirstOrDefault();

            var result = Calculate(orders.Count, orderAmount, received, contracts.Count, lastFollowTime);
            await fsql.Update<CRM_Customer>()
                .Where(a => a.id == customerId)
                .Set(a => a.score, result.Score)
                .ExecuteAffrowsAsync();
            return result;
        }

        public static CustomerScoreResult Calculate(
            int orderCount,
            decimal orderAmount,
            decimal received,
            int contractCount,
            DateTime? lastFollowTime)
        {
            var payRate = orderAmount > 0 ? Math.Min(received / orderAmount, 1m) : 0m;
            var lastFollowDays = lastFollowTime.HasValue ? (DateTime.Today - lastFollowTime.Value.Date).Days : -1;
            var arrears = Math.Max(orderAmount - received, 0m);

            var score = 0;
            score += (int)Math.Round(payRate * 40m);
            score += orderCount >= 3 ? 20 : orderCount == 2 ? 15 : orderCount == 1 ? 10 : 0;
            score += lastFollowDays >= 0 && lastFollowDays <= 7 ? 20
                : lastFollowDays > 7 && lastFollowDays <= 30 ? 12
                : lastFollowDays > 30 && lastFollowDays <= 90 ? 5
                : 0;
            score += contractCount > 0 ? 10 : 0;
            score += orderCount > 0 && arrears <= 0 ? 10 : 0;
            score = Math.Clamp(score, 0, 100);

            return new CustomerScoreResult
            {
                Score = score,
                Label = score >= 80 ? "高价值客户" : score >= 60 ? "优质客户" : score >= 40 ? "潜力客户" : "待激活客户",
                Color = score >= 80 ? "#16a34a" : score >= 60 ? "#1677ff" : score >= 40 ? "#f59e0b" : "#ef4444"
            };
        }
    }
}
