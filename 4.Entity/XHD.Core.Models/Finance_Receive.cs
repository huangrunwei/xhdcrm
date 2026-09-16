
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
    /// 收款
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Finance_Receive
    {
        /// <summary>
        /// 收款id
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

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
        /// 收款方式id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string Pay_type_id { get; set; } = string.Empty;

        /// <summary>
        /// 收款人id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string Payee_id { get; set; } = string.Empty;

        /// <summary>
        /// 订单id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string order_id { get; set; } = string.Empty;

        /// <summary>
        /// 收款金额
        /// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
        public decimal? Receive_amount { get; set; }

        /// <summary>
        /// 收款时间
        /// </summary>
        [JsonProperty]
        public DateTime? Receive_date { get; set; }

        /// <summary>
        /// 收款编号
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string Receive_num { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// 收款方式
        /// </summary>
        [JsonProperty]
        public Sys_Param PayType { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        [JsonProperty]
        public Sale_order Order { get; set; }

        /// <summary>
        /// 收款人
        /// </summary>
        [JsonProperty]
        public hr_employee Payee { get; set; }

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
