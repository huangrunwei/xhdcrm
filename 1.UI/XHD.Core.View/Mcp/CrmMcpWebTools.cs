using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace XHD.Core.View.Mcp
{
    public sealed class McpOrderDetail
    {
        public string product_id { get; set; } = "";
        public int quantity { get; set; } = 1;
        public decimal price { get; set; }
        public decimal amount { get; set; }
    }

    /// <summary>
    /// 集成在 XHD.Core.View 里的 MCP 工具。工具只做参数转发，业务逻辑全部在 APIController 中，
    /// 因此 Web 和 API 仍然共用同一套数据权限、菜单权限和按钮权限。
    /// </summary>
    [McpServerToolType]
    public sealed class CrmMcpWebTools
    {
        private readonly McpApiForwarder _api;

        public CrmMcpWebTools(McpApiForwarder api)
        {
            _api = api;
        }

        [McpServerTool, Description("读取当前用户权限范围内的 CRM 经营概览。")]
        public Task<string> crm_overview()
            => _api.GetAsync("/API/McpOverview");

        [McpServerTool, Description("按关键词同时搜索客户、订单和合同，适合先定位业务对象 ID。")]
        public Task<string> crm_search(string keyword, int limit = 5)
            => _api.GetAsync("/API/McpSearch", ("keyword", keyword), ("limit", limit));

        [McpServerTool, Description("分页查询客户，支持姓名/电话/客户编号关键词。")]
        public Task<string> crm_list_customers(string keyword = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/CustomerList", ("serchtxt", keyword), ("page", page), ("limit", limit));

        [McpServerTool, Description("读取客户详情，包括联系人、最近跟进、订单和合同。请传 customerId。")]
        public Task<string> crm_get_customer(string customerId)
            => _api.GetAsync("/API/CustomerInfo", ("id", customerId));

        [McpServerTool, Description("分页查询订单，返回金额、回款和状态。支持订单号/客户名称关键词。")]
        public Task<string> crm_list_orders(string keyword = null, string customerId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/OrderList", ("serchtxt", keyword), ("customer_id", customerId), ("page", page), ("limit", limit));

        [McpServerTool, Description("读取订单详情，包括产品明细、收款计划、收款、发票和售后记录。请传 orderId。")]
        public Task<string> crm_get_order(string orderId)
            => _api.GetAsync("/API/OrderInfo", ("id", orderId));

        [McpServerTool, Description("分页查询合同，支持合同名称/合同号/客户名称关键词。")]
        public Task<string> crm_list_contracts(string keyword = null, string customerId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/ContractList", ("serchtxt", keyword), ("customer_id", customerId), ("page", page), ("limit", limit));

        [McpServerTool, Description("查询客户跟进记录。")]
        public Task<string> crm_list_customer_follows(string customerId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/FollowList", ("customer_id", customerId), ("page", page), ("limit", limit));

        [McpServerTool, Description("查询订单售后/回访记录。")]
        public Task<string> crm_list_order_follows(string orderId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/OrderFollowList", ("order_id", orderId), ("page", page), ("limit", limit));

        [McpServerTool, Description("查询订单收款计划。")]
        public Task<string> crm_list_pay_plans(string customerId = null, string orderId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/PayPlanList", ("customer_id", customerId), ("order_id", orderId), ("page", page), ("limit", limit));

        [McpServerTool, Description("查询实际收款流水。")]
        public Task<string> crm_list_receipts(string customerId = null, string orderId = null, int page = 1, int limit = 20)
            => _api.GetAsync("/API/ReceiveList", ("customer_id", customerId), ("order_id", orderId), ("page", page), ("limit", limit));

        [McpServerTool, Description("读取指定年度的回款、开票、应收余额和收款计划汇总。")]
        public Task<string> crm_finance_summary(int? year = null)
            => _api.GetAsync("/API/FinanceSummary", ("year", year));

        [McpServerTool, Description("读取可用于筛选的员工列表。")]
        public Task<string> crm_list_employees(string keyword = null)
            => _api.GetAsync("/API/EmployeeList", ("serchtxt", keyword));

        [McpServerTool, Description("读取客户级别、行业、来源、订单状态、支付方式等数据字典。")]
        public Task<string> crm_list_params(string type = null)
            => _api.GetAsync("/API/paramsCombo", ("type", type));

        [McpServerTool, Description("返回 CRM MCP 支持的主要实体和字段说明。")]
        public Task<string> crm_schema()
        {
            var schema = new
            {
                product = "小黄豆 CRM",
                entities = new
                {
                    customer = new { table = "CRM_Customer", keyFields = new[] { "id", "sn", "cus_name", "cus_tel", "emp_id", "cus_level_id", "cus_industry_id", "lastfollow" } },
                    order = new { table = "Sale_order", keyFields = new[] { "id", "sn", "customer_id", "emp_id", "Order_date", "Order_amount", "total_amount", "Order_status_id", "pay_type_id" } },
                    contract = new { table = "Sale_contract", keyFields = new[] { "id", "sn", "customer_id", "Contract_name", "Contract_amount", "Start_date", "End_date" } },
                    follow = new { table = "CRM_follow", keyFields = new[] { "id", "customer_id", "employee_id", "follow_content", "follow_time" } },
                    orderFollow = new { table = "Sale_order_follow", keyFields = new[] { "id", "order_id", "service_type", "service_status", "follow_content", "solution", "follow_time" } },
                    payPlan = new { table = "Sale_order_pay_plan", keyFields = new[] { "id", "order_id", "plan_no", "plan_name", "plan_amount", "received_amount", "plan_date", "plan_status" } },
                    receipt = new { table = "Finance_Receive", keyFields = new[] { "id", "order_id", "Receive_num", "Receive_amount", "Receive_date", "Pay_type_id" } }
                },
                notes = new[]
                {
                    "所有查询都会按当前 Bearer Token 所属用户的数据权限过滤。",
                    "写入工具还会校验 Web 系统中的按钮权限。",
                    "日期使用 yyyy-MM-dd 或 yyyy-MM-dd HH:mm:ss。"
                }
            };
            return Task.FromResult(JsonConvert.SerializeObject(schema));
        }

        [McpServerTool, Description("新增客户。")]
        public Task<string> crm_create_customer(
            string cusName,
            string phone = null,
            string address = null,
            string remarks = null)
        {
            return _api.PostJsonAsync("/API/CustomerSave", new
            {
                id = "",
                cus_name = cusName,
                cus_tel = phone,
                cus_add = address,
                Remarks = remarks
            });
        }

        [McpServerTool, Description("修改客户基础信息。")]
        public Task<string> crm_update_customer(
            string customerId,
            string cusName = null,
            string phone = null,
            string address = null,
            string remarks = null)
        {
            return _api.PostJsonAsync("/API/CustomerSave", new
            {
                id = customerId,
                cus_name = cusName,
                cus_tel = phone,
                cus_add = address,
                Remarks = remarks
            });
        }

        [McpServerTool, Description("新增客户跟进记录。")]
        public Task<string> crm_create_customer_follow(
            string customerId,
            string content,
            string followTypeId = "P_FOLLOW_TEL",
            string followAimId = "P_AIM_VISIT")
        {
            return _api.PostJsonAsync("/API/FollowSave", new
            {
                customer_id = customerId,
                follow_content = content,
                follow_type_id = followTypeId,
                follow_aim_id = followAimId
            });
        }

        [McpServerTool, Description("新增订单及产品明细。details 可传 JSON 数组，例如 [{\"product_id\":\"PROD001\",\"quantity\":1,\"price\":5000,\"amount\":5000}]。")]
        public Task<string> crm_create_order(
            string customerId,
            decimal amount,
            decimal discount = 0,
            string orderDate = null,
            string statusId = "P_ORDER_UNPAID",
            string payTypeId = "P_PAY_BANK",
            string remarks = null,
            List<McpOrderDetail> details = null)
        {
            var detailJson = JsonConvert.SerializeObject(details ?? new List<McpOrderDetail>());
            return _api.PostJsonAsync("/API/OrderSave", new
            {
                id = "",
                customer_id = customerId,
                Order_amount = amount,
                discount_amount = discount,
                total_amount = Math.Max(amount - discount, 0m),
                Order_date = string.IsNullOrWhiteSpace(orderDate) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : orderDate,
                Order_status_id = statusId,
                pay_type_id = payTypeId,
                Remarks = remarks,
                Order_details = detailJson
            });
        }

        [McpServerTool, Description("更新订单状态。statusId 必须是订单状态字典 ID。")]
        public Task<string> crm_update_order_status(string orderId, string statusId)
        {
            return _api.PostJsonAsync("/API/UpdateOrderStatus", new { orderId, statusId });
        }

        [McpServerTool, Description("新增合同。")]
        public Task<string> crm_create_contract(
            string customerId,
            string contractName,
            decimal amount,
            string signDate = null,
            string startDate = null,
            string endDate = null,
            string remarks = null)
        {
            return _api.PostJsonAsync("/API/ContractSave", new
            {
                id = "",
                customer_id = customerId,
                Contract_name = contractName,
                Contract_amount = amount,
                Sign_date = string.IsNullOrWhiteSpace(signDate) ? DateTime.Now.ToString("yyyy-MM-dd") : signDate,
                Start_date = startDate,
                End_date = endDate,
                Remarks = remarks
            });
        }

        [McpServerTool, Description("为订单新增收款计划。")]
        public Task<string> crm_create_pay_plan(
            string orderId,
            string planName,
            decimal amount,
            string planDate = null,
            string payTypeId = "P_PAY_BANK",
            string remarks = null)
        {
            return _api.PostJsonAsync("/API/PayPlanSave", new
            {
                order_id = orderId,
                plan_name = planName,
                plan_amount = amount,
                received_amount = 0,
                plan_date = string.IsNullOrWhiteSpace(planDate) ? DateTime.Now.ToString("yyyy-MM-dd") : planDate,
                pay_type_id = payTypeId,
                plan_status = 0,
                remarks = remarks
            });
        }

        [McpServerTool, Description("为订单登记实际收款。")]
        public Task<string> crm_register_receive(
            string orderId,
            decimal amount,
            string planId = null,
            string receiveDate = null,
            string payTypeId = "P_PAY_BANK",
            string remarks = null)
        {
            return _api.PostJsonAsync("/API/ReceiveSave", new
            {
                id = "",
                order_id = orderId,
                pay_plan_id = planId,
                Receive_amount = amount,
                Receive_date = string.IsNullOrWhiteSpace(receiveDate) ? DateTime.Now.ToString("yyyy-MM-dd") : receiveDate,
                Pay_type_id = payTypeId,
                Remarks = remarks
            });
        }

        [McpServerTool, Description("新增订单售后/回访记录。")]
        public Task<string> crm_create_order_follow(
            string orderId,
            string serviceType,
            string content,
            string solution = null,
            int serviceStatus = 0,
            string followTime = null,
            string nextTime = null)
        {
            return _api.PostJsonAsync("/API/OrderFollowSave", new
            {
                order_id = orderId,
                service_type = serviceType,
                service_status = serviceStatus,
                follow_content = content,
                solution = solution,
                follow_time = string.IsNullOrWhiteSpace(followTime) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : followTime,
                next_time = nextTime
            });
        }
    }
}
