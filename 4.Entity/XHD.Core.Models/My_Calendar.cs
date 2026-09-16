using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// /日程
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class My_Calendar
    {
        /// <summary>
        /// 日程ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 员工ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string emp_id { get; set; } = string.Empty;

        /// <summary>
        /// 日程标题
        /// </summary>
        [JsonProperty, Column(StringLength = 100)]
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 日程描述
        /// </summary>
        [JsonProperty, Column(StringLength = 500)]
        public string? description { get; set; } = string.Empty;

        /// <summary>
        /// 日程开始时间
        [JsonProperty, Column(StringLength = 50)]
        public string startDate { get; set; } = string.Empty;

        /// <summary>
        /// 日程开始时间
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string startTime { get; set; } = string.Empty;

        /// <summary>
        /// 日程结束时间
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string endDate { get; set; } = string.Empty;

        /// <summary>
        /// 日程结束时间
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string endTime { get; set; } = string.Empty;


        // 用于查询的 datetime 字段（自动根据 StartDate+StartTime 计算）
        [JsonProperty, Column(DbType = "datetime")]
        public DateTime? StartDateTime { get; set; }

        // 用于查询的 datetime 字段（自动根据 EndDate+EndTime 计算）
        [JsonProperty, Column(DbType = "datetime")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 日程颜色
        /// </summary>
        [JsonProperty]
        public int? color { get; set; }

        /// <summary>
        /// 是否全天
        /// </summary>
        [JsonProperty]
        public int? allDay { get; set; }
    }
}
