using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XHD.Core.Common;
using XHD.Core.Models;

namespace XHD.Core.View.Controllers
{
    public sealed class McpStatusRequest
    {
        public string orderId { get; set; } = string.Empty;
        public string statusId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 供 MCP 转发调用的 API 扩展。所有方法都复用 APIController 的 checkToken 和现有权限模型。
    /// </summary>
    public partial class APIController
    {
        private async Task<bool> McpHasMenu(string authId)
        {
            return await _dBAuthService.GetAuth(employee.id, authId);
        }

        private async Task<(bool full, List<string> empIds)> McpDataScope()
        {
            var role = await _dBAuthService.GetDataAuth(employee.id);
            return (role.authtype == 4, role.empList ?? new List<string>());
        }

        private static int McpPage(int page) => Math.Max(page, 1);
        private static int McpLimit(int limit) => Math.Clamp(limit <= 0 ? 20 : limit, 1, 100);

        private static string McpSuccess(object data)
        {
            return JsonConvert.SerializeObject(new { code = 0, msg = "", data });
        }

        public async Task<string> McpOverview()
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();

            var canCustomer = await McpHasMenu("CRM_Customer");
            var canOrder = await McpHasMenu("Sale_Menu_order");
            var canContract = await McpHasMenu("Sale_Contract");
            var canFinance = await McpHasMenu("Finance_Receive");
            var canPlan = await McpHasMenu("Sale_Order_PayPlan");

            var (full, empIds) = await McpDataScope();
            var allCustomers = await _fsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var customerMap = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var customers = canCustomer
                ? allCustomers.Where(a => full || empIds.Contains(a.emp_id) || a.isPrivate == 1).ToList()
                : new List<CRM_Customer>();

            var allOrders = await _fsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync();
            var scopedOrders = allOrders
                .Where(a => customerMap.TryGetValue(a.customer_id, out var c) && (full || empIds.Contains(c.emp_id)))
                .ToList();
            var orders = canOrder ? scopedOrders : new List<Sale_order>();

            var allContracts = await _fsql.Select<Sale_contract>().Where(a => a.isDelete != 1).ToListAsync();
            var contracts = canContract
                ? allContracts.Where(a => customerMap.TryGetValue(a.customer_id, out var c) && (full || empIds.Contains(c.emp_id))).ToList()
                : new List<Sale_contract>();

            var orderIds = scopedOrders.Select(a => a.id).ToHashSet();
            var receipts = canFinance
                ? (await _fsql.Select<Finance_Receive>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList()
                : new List<Finance_Receive>();
            var invoices = canFinance
                ? (await _fsql.Select<Finance_Invoice>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList()
                : new List<Finance_Invoice>();
            var plans = canPlan
                ? (await _fsql.Select<Sale_order_pay_plan>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList()
                : new List<Sale_order_pay_plan>();

            var orderAmount = scopedOrders.Sum(a => a.total_amount ?? a.Order_amount ?? 0m);
            var received = receipts.Sum(a => a.Receive_amount ?? 0m);
            var pendingPlans = plans.Where(a => (a.plan_status ?? 0) != 2).ToList();
            var authType = (await _dBAuthService.GetDataAuth(employee.id)).authtype;

            return McpSuccess(new
            {
                generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                scope = new { empId = employee.id, name = employee.name, dataAuth = authType },
                customerCount = customers.Count,
                orderCount = orders.Count,
                contractCount = contracts.Count,
                orderAmount = canOrder ? orderAmount : 0m,
                receivedAmount = received,
                invoiceAmount = invoices.Sum(a => a.invoice_amount ?? 0m),
                arrears = Math.Max(orderAmount - received, 0m),
                pendingPlanCount = pendingPlans.Count,
                overduePlanCount = pendingPlans.Count(a => a.plan_date.HasValue && a.plan_date.Value.Date < DateTime.Today),
                collectionRate = orderAmount > 0 ? Math.Round(received / orderAmount * 100m, 1) : 0m
            }).ToString();
        }

        public async Task<string> McpSearch(string keyword, int limit = 5)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (string.IsNullOrWhiteSpace(keyword)) return XHDResult.Error("keyword 不能为空。").ToString();

            var canCustomer = await McpHasMenu("CRM_Customer");
            var canOrder = await McpHasMenu("Sale_Menu_order");
            var canContract = await McpHasMenu("Sale_Contract");
            var (full, empIds) = await McpDataScope();
            var take = McpLimit(limit);

            var allCustomers = await _fsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var customerMap = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var customerHits = new List<object>();
            if (canCustomer)
            {
                customerHits.AddRange(allCustomers
                    .Where(a => (full || empIds.Contains(a.emp_id) || a.isPrivate == 1)
                                && ((a.cus_name ?? "").Contains(keyword) || (a.cus_tel ?? "").Contains(keyword) || (a.sn ?? "").Contains(keyword)))
                    .Take(take)
                    .Select(a => new { type = "customer", id = a.id, title = a.cus_name, subtitle = a.cus_tel, extra = a.sn }));
            }

            var orderHits = new List<object>();
            if (canOrder)
            {
                orderHits.AddRange((await _fsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync())
                    .Where(a => customerMap.TryGetValue(a.customer_id, out var c) && (full || empIds.Contains(c.emp_id))
                                && ((a.sn ?? "").Contains(keyword) || customerMap[a.customer_id].cus_name.Contains(keyword)))
                    .Take(take)
                    .Select(a => new { type = "order", id = a.id, title = a.sn, subtitle = customerMap[a.customer_id].cus_name, extra = (a.total_amount ?? a.Order_amount ?? 0m).ToString("N2") }));
            }

            var contractHits = new List<object>();
            if (canContract)
            {
                contractHits.AddRange((await _fsql.Select<Sale_contract>().Where(a => a.isDelete != 1).ToListAsync())
                    .Where(a => customerMap.TryGetValue(a.customer_id, out var c) && (full || empIds.Contains(c.emp_id))
                                && ((a.Contract_name ?? "").Contains(keyword) || (a.sn ?? "").Contains(keyword)))
                    .Take(take)
                    .Select(a => new { type = "contract", id = a.id, title = a.Contract_name, subtitle = a.sn, extra = (a.Contract_amount ?? 0m).ToString("N2") }));
            }

            return McpSuccess(new { keyword, customers = customerHits, orders = orderHits, contracts = contractHits });
        }

        public async Task<string> CustomerInfo(string id)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (!await McpHasMenu("CRM_Customer")) return XHDResult.Error("无客户菜单权限。").ToString();

            var (full, empIds) = await McpDataScope();
            var customer = await _fsql.Select<CRM_Customer>().Where(a => a.id == id && a.isDelete != 1).FirstAsync();
            if (customer == null || (!full && !empIds.Contains(customer.emp_id) && customer.isPrivate != 1))
            {
                return XHDResult.Error("客户不存在或无权访问。").ToString();
            }

            var canFollow = await McpHasMenu("CRM_Follow");
            var canOrder = await McpHasMenu("Sale_Menu_order");
            var canContract = await McpHasMenu("Sale_Contract");
            var contacts = await _fsql.Select<CRM_Contact>().Where(a => a.customer_id == id && a.isDelete != 1).ToListAsync();
            var follows = canFollow
                ? await _fsql.Select<CRM_follow>().Where(a => a.customer_id == id && a.isDelete != 1).OrderByDescending(a => a.follow_time).Limit(20).ToListAsync()
                : new List<CRM_follow>();
            var orders = canOrder
                ? await _fsql.Select<Sale_order>().Where(a => a.customer_id == id && a.isDelete != 1).OrderByDescending(a => a.Order_date).Limit(20).ToListAsync()
                : new List<Sale_order>();
            var contracts = canContract
                ? await _fsql.Select<Sale_contract>().Where(a => a.customer_id == id && a.isDelete != 1).OrderByDescending(a => a.Sign_date).Limit(20).ToListAsync()
                : new List<Sale_contract>();

            return McpSuccess(new
            {
                customer = new
                {
                    id = customer.id,
                    sn = customer.sn,
                    name = customer.cus_name,
                    ownerId = customer.emp_id,
                    levelId = customer.cus_level_id,
                    industryId = customer.cus_industry_id,
                    phone = customer.cus_tel,
                    address = customer.cus_add,
                    website = customer.cus_website,
                    isPrivate = customer.isPrivate == 1,
                    lastFollow = customer.lastfollow?.ToString("yyyy-MM-dd HH:mm"),
                    createTime = customer.create_time?.ToString("yyyy-MM-dd HH:mm"),
                    remarks = customer.Remarks
                },
                contacts = contacts.Select(a => new { id = a.id, name = a.C_name, position = a.C_position, phone = a.C_mob, tel = a.C_tel, email = a.C_email, wechat = a.C_weichat }).ToList(),
                follows = follows.Select(a => new { id = a.id, content = a.follow_content, employeeId = a.employee_id, time = a.follow_time?.ToString("yyyy-MM-dd HH:mm") }).ToList(),
                orders = orders.Select(a => new { id = a.id, sn = a.sn, amount = a.total_amount ?? a.Order_amount ?? 0m, statusId = a.Order_status_id, date = a.Order_date?.ToString("yyyy-MM-dd") }).ToList(),
                contracts = contracts.Select(a => new { id = a.id, sn = a.sn, name = a.Contract_name, amount = a.Contract_amount ?? 0m, start = a.Start_date?.ToString("yyyy-MM-dd"), end = a.End_date?.ToString("yyyy-MM-dd") }).ToList()
            }).ToString();
        }

        public async Task<string> OrderInfo(string id)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (!await McpHasMenu("Sale_Menu_order")) return XHDResult.Error("无订单菜单权限。").ToString();

            var (full, empIds) = await McpDataScope();
            var order = await _fsql.Select<Sale_order>().Where(a => a.id == id && a.isDelete != 1).FirstAsync();
            if (order == null) return XHDResult.Error("订单不存在。").ToString();
            var customer = await _fsql.Select<CRM_Customer>().Where(a => a.id == order.customer_id).FirstAsync();
            if (customer == null || (!full && !empIds.Contains(customer.emp_id))) return XHDResult.Error("无权访问该订单。").ToString();

            var canPlan = await McpHasMenu("Sale_Order_PayPlan");
            var canReceipt = await McpHasMenu("Finance_Receive");
            var canInvoice = await McpHasMenu("Finance_Invoice");
            var canFollow = await McpHasMenu("Sale_Order_Follow");
            var details = await _fsql.Select<Sale_order_details>().Where(a => a.order_id == id).ToListAsync();
            var plans = canPlan ? await _fsql.Select<Sale_order_pay_plan>().Where(a => a.order_id == id && a.isDelete != 1).OrderBy(a => a.plan_date).ToListAsync() : new List<Sale_order_pay_plan>();
            var receipts = canReceipt ? await _fsql.Select<Finance_Receive>().Where(a => a.order_id == id && a.isDelete != 1).OrderByDescending(a => a.Receive_date).ToListAsync() : new List<Finance_Receive>();
            var invoices = canInvoice ? await _fsql.Select<Finance_Invoice>().Where(a => a.order_id == id && a.isDelete != 1).OrderByDescending(a => a.invoice_date).ToListAsync() : new List<Finance_Invoice>();
            var follows = canFollow ? await _fsql.Select<Sale_order_follow>().Where(a => a.order_id == id && a.isDelete != 1).OrderByDescending(a => a.follow_time).ToListAsync() : new List<Sale_order_follow>();
            var received = receipts.Sum(a => a.Receive_amount ?? 0m);
            var amount = order.total_amount ?? order.Order_amount ?? 0m;

            return McpSuccess(new
            {
                order = new
                {
                    id = order.id,
                    sn = order.sn,
                    customerId = order.customer_id,
                    customer = customer.cus_name,
                    ownerId = order.emp_id,
                    statusId = order.Order_status_id,
                    payTypeId = order.pay_type_id,
                    amount,
                    received,
                    arrears = Math.Max(amount - received, 0m),
                    invoiceAmount = invoices.Sum(a => a.invoice_amount ?? 0m),
                    date = order.Order_date?.ToString("yyyy-MM-dd"),
                    remarks = order.Remarks
                },
                details = details.Select(a => new { productId = a.product_id, quantity = a.quantity ?? 0, price = a.price ?? 0m, amount = a.amount ?? 0m }).ToList(),
                payPlans = plans.Select(a => new { id = a.id, planNo = a.plan_no, name = a.plan_name, amount = a.plan_amount ?? 0m, received = a.received_amount ?? 0m, date = a.plan_date?.ToString("yyyy-MM-dd"), status = a.plan_status }).ToList(),
                receipts = receipts.Select(a => new { id = a.id, num = a.Receive_num, amount = a.Receive_amount ?? 0m, date = a.Receive_date?.ToString("yyyy-MM-dd"), payTypeId = a.Pay_type_id }).ToList(),
                invoices = invoices.Select(a => new { id = a.id, num = a.invoice_num, amount = a.invoice_amount ?? 0m, date = a.invoice_date?.ToString("yyyy-MM-dd"), typeId = a.invoice_type_id }).ToList(),
                orderFollows = follows.Select(a => new { id = a.id, serviceType = a.service_type, status = a.service_status, content = a.follow_content, solution = a.solution, time = a.follow_time?.ToString("yyyy-MM-dd HH:mm"), nextTime = a.next_time?.ToString("yyyy-MM-dd HH:mm") }).ToList()
            }).ToString();
        }

        public async Task<string> PayPlanList(string customerId, string orderId, int page = 1, int limit = 20)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (!await McpHasMenu("Sale_Order_PayPlan")) return XHDResult.Error("无收款计划菜单权限。").ToString();

            var (full, empIds) = await McpDataScope();
            var orders = await _fsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync();
            var customers = await _fsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var customerMap = customers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var orderMap = orders.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());

            var query = (await _fsql.Select<Sale_order_pay_plan>().Where(a => a.isDelete != 1).ToListAsync())
                .Where(a => orderMap.TryGetValue(a.order_id, out var o) && customerMap.TryGetValue(o.customer_id, out var c) && (full || empIds.Contains(c.emp_id)))
                .Where(a => string.IsNullOrWhiteSpace(orderId) || a.order_id == orderId)
                .Where(a => string.IsNullOrWhiteSpace(customerId) || orderMap[a.order_id].customer_id == customerId)
                .OrderBy(a => a.plan_date)
                .ToList();

            var total = query.Count;
            var items = query.Skip((McpPage(page) - 1) * McpLimit(limit)).Take(McpLimit(limit)).Select(a => new
            {
                id = a.id,
                planNo = a.plan_no,
                name = a.plan_name,
                orderId = a.order_id,
                orderSn = orderMap[a.order_id].sn,
                customer = customerMap[orderMap[a.order_id].customer_id].cus_name,
                amount = a.plan_amount ?? 0m,
                received = a.received_amount ?? 0m,
                remain = Math.Max((a.plan_amount ?? 0m) - (a.received_amount ?? 0m), 0m),
                date = a.plan_date?.ToString("yyyy-MM-dd"),
                status = a.plan_status
            }).ToList();

            return McpSuccess(new { total, page = McpPage(page), limit = McpLimit(limit), items }).ToString();
        }

        public async Task<string> OrderFollowList(string orderId, int page = 1, int limit = 20)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (!await McpHasMenu("Sale_Order_Follow")) return XHDResult.Error("无订单售后菜单权限。").ToString();

            var (full, empIds) = await McpDataScope();
            var orders = await _fsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync();
            var customers = await _fsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var customerMap = customers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var orderMap = orders.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());

            var query = (await _fsql.Select<Sale_order_follow>().Where(a => a.isDelete != 1).ToListAsync())
                .Where(a => orderMap.TryGetValue(a.order_id, out var o) && customerMap.TryGetValue(o.customer_id, out var c) && (full || empIds.Contains(c.emp_id)))
                .Where(a => string.IsNullOrWhiteSpace(orderId) || a.order_id == orderId)
                .OrderByDescending(a => a.follow_time)
                .ToList();

            var total = query.Count;
            var items = query.Skip((McpPage(page) - 1) * McpLimit(limit)).Take(McpLimit(limit)).Select(a => new
            {
                id = a.id,
                orderId = a.order_id,
                orderSn = orderMap[a.order_id].sn,
                serviceType = a.service_type,
                status = a.service_status,
                content = a.follow_content,
                solution = a.solution,
                time = a.follow_time?.ToString("yyyy-MM-dd HH:mm"),
                nextTime = a.next_time?.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            return McpSuccess(new { total, page = McpPage(page), limit = McpLimit(limit), items }).ToString();
        }

        public async Task<string> FinanceSummary(int? year)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (!await McpHasMenu("Finance_Receive")) return XHDResult.Error("无财务菜单权限。").ToString();

            var (full, empIds) = await McpDataScope();
            var y = year ?? DateTime.Now.Year;
            var customers = await _fsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var customerMap = customers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var orders = (await _fsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync())
                .Where(a => customerMap.TryGetValue(a.customer_id, out var c) && (full || empIds.Contains(c.emp_id)))
                .ToList();
            var orderIds = orders.Select(a => a.id).ToHashSet();
            var receipts = (await _fsql.Select<Finance_Receive>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList();
            var invoices = (await _fsql.Select<Finance_Invoice>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList();
            var plans = (await _fsql.Select<Sale_order_pay_plan>().Where(a => a.isDelete != 1).ToListAsync()).Where(a => orderIds.Contains(a.order_id)).ToList();
            var yearReceipts = receipts.Where(a => a.Receive_date.HasValue && a.Receive_date.Value.Year == y).ToList();
            var yearInvoices = invoices.Where(a => a.invoice_date.HasValue && a.invoice_date.Value.Year == y).ToList();
            var receivedByOrder = receipts.GroupBy(a => a.order_id).ToDictionary(g => g.Key, g => g.Sum(a => a.Receive_amount ?? 0m));
            var orderAmount = orders.Sum(a => a.total_amount ?? a.Order_amount ?? 0m);
            var outstanding = orders.Sum(a => Math.Max((a.total_amount ?? a.Order_amount ?? 0m) - receivedByOrder.GetValueOrDefault(a.id), 0m));
            var pendingPlans = plans.Where(a => (a.plan_status ?? 0) != 2).ToList();

            return McpSuccess(new
            {
                year = y,
                receivedAmount = yearReceipts.Sum(a => a.Receive_amount ?? 0m),
                invoiceAmount = yearInvoices.Sum(a => a.invoice_amount ?? 0m),
                outstanding,
                collectionRate = orderAmount > 0 ? Math.Round(receipts.Sum(a => a.Receive_amount ?? 0m) / orderAmount * 100m, 1) : 0m,
                pendingPlanCount = pendingPlans.Count,
                overduePlanCount = pendingPlans.Count(a => a.plan_date.HasValue && a.plan_date.Value.Date < DateTime.Today),
                pendingPlanAmount = pendingPlans.Sum(a => a.plan_amount ?? 0m)
            }).ToString();
        }

        [HttpPost]
        public async Task<string> UpdateOrderStatus([FromBody] McpStatusRequest model)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (model == null || string.IsNullOrWhiteSpace(model.orderId) || string.IsNullOrWhiteSpace(model.statusId))
                return XHDResult.Error("orderId 和 statusId 不能为空。").ToString();
            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Order|edit")) return XHDResult.Error("无权限！").ToString();
            if (!await CanAccessOrderAsync(model.orderId)) return XHDResult.Error("无权操作该订单。").ToString();

            var exists = await _fsql.Select<Sys_Param>().Where(a => a.id == model.statusId && a.params_type == "order_status").AnyAsync();
            if (!exists) return XHDResult.Error("无效的订单状态。").ToString();

            await _fsql.Update<Sale_order>().Where(a => a.id == model.orderId).Set(a => a.Order_status_id, model.statusId).ExecuteAffrowsAsync();
            return XHDResult.Success().ToString();
        }

        [HttpPost]
        public async Task<string> PayPlanSave([FromBody] Sale_order_pay_plan model)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (model == null || string.IsNullOrWhiteSpace(model.order_id)) return XHDResult.Error("请选择订单。").ToString();
            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Order_PayPlan|add")) return XHDResult.Error("无权限！").ToString();
            if (!await CanAccessOrderAsync(model.order_id)) return XHDResult.Error("无权操作该订单。").ToString();
            if ((model.plan_amount ?? 0m) <= 0) return XHDResult.Error("计划金额必须大于 0。").ToString();

            model.id = UUIDNext.Uuid.NewSequential().ToString();
            model.plan_no = $"SKJH-{DateTime.Now:yyyyMMddHHmmss}-{model.id.Split('-')[2]}";
            model.received_amount = 0;
            model.plan_status = 0;
            model.create_id = employee.id;
            model.create_time = DateTime.Now;
            model.isDelete = 0;

            var result = await _fsql.Insert(model).ExecuteAffrowsAsync();
            return result > 0 ? XHDResult.Success(model.id).ToString() : XHDResult.Error("操作失败。").ToString();
        }

        [HttpPost]
        public async Task<string> OrderFollowSave([FromBody] Sale_order_follow model)
        {
            var auth = checkToken();
            if (auth.Value<int>("code") != 0) return auth.ToString();
            if (model == null || string.IsNullOrWhiteSpace(model.order_id)) return XHDResult.Error("请选择订单。").ToString();
            if (string.IsNullOrWhiteSpace(model.service_type)) return XHDResult.Error("售后类型不能为空。").ToString();
            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Order_Follow|add")) return XHDResult.Error("无权限！").ToString();
            if (!await CanAccessOrderAsync(model.order_id)) return XHDResult.Error("无权操作该订单。").ToString();

            var order = await _fsql.Select<Sale_order>().Where(a => a.id == model.order_id).FirstAsync();
            model.id = UUIDNext.Uuid.NewSequential().ToString();
            model.customer_id = order?.customer_id ?? "";
            model.employee_id = employee.id;
            model.service_status = model.service_status ?? 0;
            model.follow_time = model.follow_time ?? DateTime.Now;
            model.create_time = DateTime.Now;
            model.isDelete = 0;

            var result = await _fsql.Insert(model).ExecuteAffrowsAsync();
            return result > 0 ? XHDResult.Success(model.id).ToString() : XHDResult.Error("操作失败。").ToString();
        }
    }
}
