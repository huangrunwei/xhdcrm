
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

    [JsonObject(MemberSerialization.OptIn)]
    public partial class Finance_Invoice
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// <summary>
        /// 创建人id
        /// </summary>
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string create_id { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty]
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 业务员id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string emp_id { get; set; } = string.Empty;

        /// <summary>
        /// 开票金额
        /// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
        public decimal? invoice_amount { get; set; }

        /// <summary>
        /// 开票内容
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string invoice_content { get; set; } = string.Empty;

        /// <summary>
        /// 开票日期
        /// </summary>
        [JsonProperty]
        public DateTime? invoice_date { get; set; }

        /// <summary>
        /// 开票编号
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string invoice_num { get; set; } = string.Empty;

        /// <summary>
        /// 开票类型id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string invoice_type_id { get; set; } = string.Empty;

        /// <summary>
        /// 订单id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string order_id { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string remarks { get; set; } = string.Empty;

        /// <summary>
        /// 订单
        /// </summary>
        [JsonProperty]
        public Sale_order Order { get; set; }

        /// <summary>
        /// 开票类型
        /// </summary>
        [JsonProperty]
        public Sys_Param InvoiceType { get; set; } 

        /// <summary>
        /// 业务员
        /// </summary>
        [JsonProperty]
        public hr_employee employee { get; set; }

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
