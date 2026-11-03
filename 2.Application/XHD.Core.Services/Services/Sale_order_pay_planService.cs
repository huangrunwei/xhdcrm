using System;
using System.Linq;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.Services
{
    internal class Sale_order_pay_planService : BaseService<Sale_order_pay_plan>, ISale_order_pay_planService
    {
        private readonly IFreeSql _fsql;

        public Sale_order_pay_planService(ISale_order_pay_planRepository repository, IFreeSql freesql)
        {
            _irepository = repository;
            _fsql = freesql;
        }

        /// <summary>
        /// 根据实际收款记录重算计划的已收金额和状态
        /// </summary>
        public async Task RecalcAsync(string planId)
        {
            var plan = await _fsql.Select<Sale_order_pay_plan>().Where(a => a.id == planId).FirstAsync();
            if (plan == null)
            {
                return;
            }

            var received = await _fsql.Select<Finance_Receive>()
                .Where(a => a.pay_plan_id == planId)
                .SumAsync(a => a.Receive_amount);

            var planAmount = plan.plan_amount ?? 0m;
            int status;
            if (planAmount <= 0 || received >= planAmount)
            {
                status = 2;
            }
            else if (plan.plan_date.HasValue && plan.plan_date.Value.Date < DateTime.Today)
            {
                status = 3;
            }
            else if (received > 0)
            {
                status = 1;
            }
            else
            {
                status = 0;
            }

            await _fsql.Update<Sale_order_pay_plan>()
                .Set(a => a.received_amount == received)
                .Set(a => a.plan_status == status)
                .Where(a => a.id == planId)
                .ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 重算订单下所有收款计划，并刷新订单收款汇总
        /// </summary>
        public async Task RecalcByOrderAsync(string orderId)
        {
            var planIds = await _fsql.Select<Sale_order_pay_plan>()
                .Where(a => a.order_id == orderId)
                .ToListAsync(a => a.id);

            foreach (var planId in planIds)
            {
                await RecalcAsync(planId);
            }

            await RecalcOrderTotalsAsync(orderId);
        }

        /// <summary>
        /// 重算订单已收/未收金额
        /// </summary>
        public async Task RecalcOrderTotalsAsync(string orderId)
        {
            var receiveamount = await _fsql.Select<Finance_Receive>()
                .Where(a => a.order_id == orderId)
                .SumAsync(a => a.Receive_amount);

            await _fsql.Update<Sale_order>()
                .Set(a => a.receive_money == receiveamount)
                .Where(a => a.id == orderId)
                .ExecuteAffrowsAsync();

            await _fsql.Update<Sale_order>()
                .Set(a => a.arrears_money == a.total_amount - a.receive_money)
                .Where(a => a.id == orderId)
                .ExecuteAffrowsAsync();
        }
    }
}
