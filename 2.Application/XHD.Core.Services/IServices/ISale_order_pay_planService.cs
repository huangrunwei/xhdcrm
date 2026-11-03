using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.Models;

namespace XHD.Core.IServices
{
    public interface ISale_order_pay_planService : IBaseService<Sale_order_pay_plan>
    {
        /// <summary>
        /// 根据收款记录重算计划已收金额与状态
        /// </summary>
        Task RecalcAsync(string planId);

        /// <summary>
        /// 重算某订单下所有收款计划及订单收款汇总
        /// </summary>
        Task RecalcByOrderAsync(string orderId);

        /// <summary>
        /// 重算订单已收/未收金额
        /// </summary>
        Task RecalcOrderTotalsAsync(string orderId);
    }
}
