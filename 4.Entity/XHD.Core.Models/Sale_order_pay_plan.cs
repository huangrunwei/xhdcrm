using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// 订单收款计划
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Sale_order_pay_plan
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 订单id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string order_id { get; set; } = string.Empty;

        /// <summary>
        /// 收款计划编号
        /// </summary>
        [JsonProperty, Column(StringLength = 100)]
        public string plan_no { get; set; } = string.Empty;

        /// <summary>
        /// 计划名称（期数/阶段）
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string plan_name { get; set; } = string.Empty;

        /// <summary>
        /// 计划收款金额
        /// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
        public decimal? plan_amount { get; set; }

        /// <summary>
        /// 已收金额
        /// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
        public decimal? received_amount { get; set; }

        /// <summary>
        /// 计划收款日期
        /// </summary>
        [JsonProperty]
        public DateTime? plan_date { get; set; }

        /// <summary>
        /// 收款方式id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string pay_type_id { get; set; } = string.Empty;

        /// <summary>
        /// 计划状态：0待收款 1部分收款 2已收清 3已逾期
        /// </summary>
        [JsonProperty]
        public int? plan_status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string remarks { get; set; } = string.Empty;

        /// <summary>
        /// 创建人id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string create_id { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty]
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        [JsonProperty]
        public Sale_order Order { get; set; }

        /// <summary>
        /// 收款方式
        /// </summary>
        [JsonProperty]
        public Sys_Param PayType { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty]
        public hr_employee creater { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [JsonProperty]
        public int? isDelete { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        [JsonProperty]
        public DateTime? Delete_time { get; set; }

        /// <summary>
        /// 删除人id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string Delete_id { get; set; } = string.Empty;

        /// <summary>
        /// 删除人
        /// </summary>
        [JsonProperty]
        public hr_employee Deleter { get; set; }
    }
}
