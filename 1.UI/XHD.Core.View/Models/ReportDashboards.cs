using System;
using System.Collections.Generic;
using XHD.Core.Models;

namespace XHD.Core.View.Models
{
    public class ReportOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }

    public class ReportKpi
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Tone { get; set; } = "blue";
    }

    public class ReportNamedValue
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Extra { get; set; } = string.Empty;
    }

    public class ReportTrendPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class ReportCustomerRow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int FollowCount { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime? LastFollow { get; set; }
        public DateTime? CreateTime { get; set; }
        public int SilentDays { get; set; }
    }

    public class ReportOrderRow
    {
        public string Id { get; set; } = string.Empty;
        public string Sn { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = "gray";
        public string PayType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Received { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ReportPlanRow
    {
        public string Id { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string OrderSn { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? PlanDate { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string TagClass { get; set; } = "gray";
    }

    public class ReportReceiveRow
    {
        public string Id { get; set; } = string.Empty;
        public string Num { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string OrderSn { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
        public string PayType { get; set; } = string.Empty;
        public string Payee { get; set; } = string.Empty;
    }

    public class ReportEmployeeRow
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public string Extra { get; set; } = string.Empty;
    }

    public class CustomerReportModel
    {
        public int Year { get; set; }
        public string EmpId { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public List<ReportOption> Employees { get; set; } = new List<ReportOption>();
        public List<ReportOption> Years { get; set; } = new List<ReportOption>();
        public List<ReportKpi> Kpis { get; set; } = new List<ReportKpi>();
        public List<ReportTrendPoint> Trend { get; set; } = new List<ReportTrendPoint>();
        public List<ReportNamedValue> Industry { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> Level { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> Type { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> Source { get; set; } = new List<ReportNamedValue>();
        public List<ReportCustomerRow> TopCustomers { get; set; } = new List<ReportCustomerRow>();
        public List<ReportCustomerRow> RecentCustomers { get; set; } = new List<ReportCustomerRow>();
        public List<ReportCustomerRow> SilentCustomers { get; set; } = new List<ReportCustomerRow>();
        public List<ReportEmployeeRow> EmployeeRanking { get; set; } = new List<ReportEmployeeRow>();
    }

    public class OrderReportModel
    {
        public int Year { get; set; }
        public string EmpId { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public List<ReportOption> Employees { get; set; } = new List<ReportOption>();
        public List<ReportOption> Years { get; set; } = new List<ReportOption>();
        public List<ReportKpi> Kpis { get; set; } = new List<ReportKpi>();
        public List<ReportTrendPoint> Trend { get; set; } = new List<ReportTrendPoint>();
        public List<ReportNamedValue> Status { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> PayTypes { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> Products { get; set; } = new List<ReportNamedValue>();
        public List<ReportEmployeeRow> EmployeeRanking { get; set; } = new List<ReportEmployeeRow>();
        public List<ReportOrderRow> RecentOrders { get; set; } = new List<ReportOrderRow>();
        public List<ReportCustomerRow> TopCustomers { get; set; } = new List<ReportCustomerRow>();
    }

    public class FinanceReportModel
    {
        public int Year { get; set; }
        public string EmpId { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public List<ReportOption> Employees { get; set; } = new List<ReportOption>();
        public List<ReportOption> Years { get; set; } = new List<ReportOption>();
        public List<ReportKpi> Kpis { get; set; } = new List<ReportKpi>();
        public List<ReportTrendPoint> Trend { get; set; } = new List<ReportTrendPoint>();
        public List<ReportNamedValue> PayTypes { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> InvoiceTypes { get; set; } = new List<ReportNamedValue>();
        public List<ReportNamedValue> Aging { get; set; } = new List<ReportNamedValue>();
        public List<ReportReceiveRow> RecentReceives { get; set; } = new List<ReportReceiveRow>();
        public List<ReportPlanRow> PendingPlans { get; set; } = new List<ReportPlanRow>();
        public List<ReportCustomerRow> TopCustomers { get; set; } = new List<ReportCustomerRow>();
    }

    public class ReportScope
    {
        public bool Full { get; set; }
        public HashSet<string> AllowedEmpIds { get; set; } = new HashSet<string>();
        public List<hr_employee> Employees { get; set; } = new List<hr_employee>();
    }
}
