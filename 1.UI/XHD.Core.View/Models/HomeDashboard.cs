using System;
using System.Collections.Generic;

namespace XHD.Core.View.Models
{
    /// <summary>
    /// 首页仪表盘数据
    /// </summary>
    public class HomeDashboard
    {
        public string UserName { get; set; } = string.Empty;
        public string Greeting { get; set; } = string.Empty;
        public string TodayText { get; set; } = string.Empty;

        /// <summary>
        /// 是否系统管理员（管理员拥有全部菜单和全部数据）
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 当前用户已授权的菜单ID
        /// </summary>
        public List<string> AllowedMenus { get; set; } = new List<string>();

        /// <summary>
        /// 判断当前用户是否拥有指定菜单权限
        /// </summary>
        public bool HasMenu(string menuId)
        {
            return IsAdmin || AllowedMenus.Contains(menuId);
        }

        public int CustomerCount { get; set; }
        public int FollowCount { get; set; }
        public int OrderCount { get; set; }
        public int ContractCount { get; set; }
        public int ReceiveCount { get; set; }

        public decimal OrderAmount { get; set; }
        public decimal ReceiveAmount { get; set; }
        public decimal Arrears { get; set; }
        public decimal ContractAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PayRate { get; set; }

        public int MonthNewCustomers { get; set; }
        public int MonthFollows { get; set; }
        public decimal MonthOrderAmount { get; set; }
        public decimal MonthReceiveAmount { get; set; }
        public int PendingPlanCount { get; set; }

        public List<string> TrendMonths { get; set; } = new List<string>();
        public List<decimal> TrendOrderAmount { get; set; } = new List<decimal>();
        public List<decimal> TrendReceiveAmount { get; set; } = new List<decimal>();

        public List<NameValueItem> OrderStatuses { get; set; } = new List<NameValueItem>();
        public List<NameValueItem> Industries { get; set; } = new List<NameValueItem>();
        public List<RecentOrderItem> RecentOrders { get; set; } = new List<RecentOrderItem>();
        public List<RecentFollowItem> RecentFollows { get; set; } = new List<RecentFollowItem>();
        public List<ReminderItem> Reminders { get; set; } = new List<ReminderItem>();
    }

    public class NameValueItem
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percent { get; set; }
    }

    public class RecentOrderItem
    {
        public string Id { get; set; } = string.Empty;
        public string Sn { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class RecentFollowItem
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Employee { get; set; } = string.Empty;
        public DateTime? Time { get; set; }
    }

    public class ReminderItem
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string TagClass { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Link { get; set; } = string.Empty;
        public DateTime SortDate { get; set; }
    }
}
