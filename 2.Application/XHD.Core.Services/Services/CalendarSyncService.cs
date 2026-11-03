using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.Services
{
    /// <summary>
    /// 业务日程同步：把收款计划、合同到期自动写入“我的日程”，
    /// 并按成交人/我方签约人挂靠到对应员工的日历。
    /// </summary>
    public class CalendarSyncService : ICalendarSyncService
    {
        public const string SourceReceivePlan = "receive_plan";
        public const string SourceContract = "contract";

        private readonly IFreeSql _fsql;

        public CalendarSyncService(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        public async Task SyncReceivePlanAsync(string planId)
        {
            var plan = await _fsql.Select<Sale_order_pay_plan>().Where(a => a.id == planId).FirstAsync();
            if (plan == null || plan.isDelete == 1 || !plan.plan_date.HasValue)
            {
                await RemoveBySourceAsync(SourceReceivePlan, planId);
                return;
            }

            var order = await _fsql.Select<Sale_order>().Where(a => a.id == plan.order_id).FirstAsync();
            if (order == null || order.isDelete == 1)
            {
                await RemoveBySourceAsync(SourceReceivePlan, planId);
                return;
            }

            var customerName = string.Empty;
            if (!string.IsNullOrWhiteSpace(order.customer_id))
            {
                customerName = await _fsql.Select<CRM_Customer>()
                    .Where(a => a.id == order.customer_id)
                    .FirstAsync(a => a.cus_name) ?? string.Empty;
            }

            // 挂靠成交人员（订单业务员），没有则退回计划/订单创建人
            var empId = order.emp_id;
            if (string.IsNullOrWhiteSpace(empId)) empId = order.create_id;
            if (string.IsNullOrWhiteSpace(empId)) empId = plan.create_id;

            var planDate = plan.plan_date.Value.Date;
            var amount = plan.plan_amount ?? 0m;
            var received = plan.received_amount ?? 0m;
            var statusText = PlanStatusText(plan.plan_status);

            var title = $"收款计划｜{customerName}｜{plan.plan_name}";
            var desc = $"客户：{customerName}\n" +
                       $"订单号：{order.sn}\n" +
                       $"计划收款：¥{amount:N2}\n" +
                       $"已收金额：¥{received:N2}\n" +
                       $"状态：{statusText}\n" +
                       $"计划日期：{planDate:yyyy-MM-dd}";

            await UpsertAsync(SourceReceivePlan, planId, empId, title, desc, planDate, 3);
        }

        public async Task SyncContractAsync(string contractId)
        {
            var contract = await _fsql.Select<Sale_contract>().Where(a => a.id == contractId).FirstAsync();
            if (contract == null || contract.isDelete == 1 || !contract.End_date.HasValue)
            {
                await RemoveBySourceAsync(SourceContract, contractId);
                return;
            }

            var customerName = string.Empty;
            if (!string.IsNullOrWhiteSpace(contract.customer_id))
            {
                customerName = await _fsql.Select<CRM_Customer>()
                    .Where(a => a.id == contract.customer_id)
                    .FirstAsync(a => a.cus_name) ?? string.Empty;
            }

            // 挂靠我方签约人，没有则退回创建人
            var empId = contract.Our_Contractor_id;
            if (string.IsNullOrWhiteSpace(empId)) empId = contract.create_id;

            var endDate = contract.End_date.Value.Date;
            var days = (endDate - DateTime.Today).Days;
            var daysText = days > 0 ? $"还有 {days} 天到期" : (days == 0 ? "今天到期" : $"已逾期 {-days} 天");

            var title = $"合同到期｜{contract.Contract_name}｜{customerName}";
            var desc = $"客户：{customerName}\n" +
                       $"合同编号：{contract.sn}\n" +
                       $"合同金额：¥{(contract.Contract_amount ?? 0m):N2}\n" +
                       $"到期日期：{endDate:yyyy-MM-dd}（{daysText}）\n" +
                       $"我方签约：{await EmployeeNameAsync(contract.Our_Contractor_id)}";

            await UpsertAsync(SourceContract, contractId, empId, title, desc, endDate, 4);
        }

        public async Task RemoveBySourceAsync(string sourceType, string sourceId)
        {
            await _fsql.Delete<My_Calendar>()
                .Where(a => a.source_type == sourceType && a.source_id == sourceId)
                .ExecuteAffrowsAsync();
        }

        public async Task RemoveReceivePlansByOrderAsync(string orderId)
        {
            var planIds = await _fsql.Select<Sale_order_pay_plan>()
                .Where(a => a.order_id == orderId)
                .ToListAsync(a => a.id);

            foreach (var planId in planIds)
            {
                await RemoveBySourceAsync(SourceReceivePlan, planId);
            }
        }

        public async Task ResyncReceivePlansByOrderAsync(string orderId)
        {
            var planIds = await _fsql.Select<Sale_order_pay_plan>()
                .Where(a => a.order_id == orderId)
                .ToListAsync(a => a.id);

            foreach (var planId in planIds)
            {
                await SyncReceivePlanAsync(planId);
            }
        }

        private async Task<string> EmployeeNameAsync(string empId)
        {
            if (string.IsNullOrWhiteSpace(empId)) return string.Empty;
            return await _fsql.Select<hr_employee>()
                .Where(a => a.id == empId)
                .FirstAsync(a => a.name) ?? string.Empty;
        }

        private async Task UpsertAsync(string sourceType, string sourceId, string empId, string title, string desc, DateTime date, int color)
        {
            var calendar = await _fsql.Select<My_Calendar>()
                .Where(a => a.source_type == sourceType && a.source_id == sourceId)
                .FirstAsync();

            var dateText = date.ToString("yyyy-MM-dd");

            if (calendar == null)
            {
                calendar = new My_Calendar
                {
                    id = Guid.NewGuid().ToString(),
                    source_type = sourceType,
                    source_id = sourceId
                };
                Apply(calendar, empId, title, desc, date, dateText, color);
                await _fsql.Insert(calendar).ExecuteAffrowsAsync();
            }
            else
            {
                Apply(calendar, empId, title, desc, date, dateText, color);
                await _fsql.Update<My_Calendar>()
                    .SetSource(calendar)
                    .ExecuteAffrowsAsync();
            }
        }

        private static void Apply(My_Calendar calendar, string empId, string title, string desc, DateTime date, string dateText, int color)
        {
            calendar.emp_id = empId ?? string.Empty;
            calendar.title = Truncate(title, 100);
            calendar.description = Truncate(desc, 500);
            calendar.startDate = dateText;
            calendar.endDate = dateText;
            calendar.startTime = string.Empty;
            calendar.endTime = string.Empty;
            calendar.StartDateTime = date;
            calendar.EndDateTime = date.AddDays(1).AddSeconds(-1);
            calendar.allDay = 1;
            calendar.color = color;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string PlanStatusText(int? status)
        {
            switch (status)
            {
                case 2: return "已收清";
                case 3: return "已逾期";
                case 1: return "部分收款";
                default: return "待收款";
            }
        }
    }
}
