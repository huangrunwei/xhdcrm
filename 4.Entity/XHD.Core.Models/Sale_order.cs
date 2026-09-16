
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models {
	/// <summary>
	/// 销售订单
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sale_order {
		/// <summary>
		/// 主键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

		/// <summary>
		/// 未开票金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? arrears_invoice { get; set; }

        /// <summary>
		/// 未收金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? arrears_money { get; set; }

        /// <summary>
		/// 创建人
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

        /// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

        /// <summary>
		/// 客户id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string customer_id { get; set; } = string.Empty;

        /// <summary>
		/// 优惠金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? discount_amount { get; set; }

        /// <summary>
		/// 业务员id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string emp_id { get; set; } = string.Empty;

        /// <summary>
		/// 开票金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? invoice_money { get; set; }

        /// <summary>
		/// 订单金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? Order_amount { get; set; }

        /// <summary>
		/// 订单日期
		/// </summary>
		[JsonProperty]
		public DateTime? Order_date { get; set; }

        /// <summary>
		/// 订单详情
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string Order_details { get; set; } = string.Empty;

        /// <summary>
		/// 订单状态id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Order_status_id { get; set; } = string.Empty;

        /// <summary>
		/// 付款方式id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string pay_type_id { get; set; } = string.Empty;

        /// <summary>
		/// 收款金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? receive_money { get; set; }

        /// <summary>
		/// 订单编号
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string sn { get; set; } = string.Empty;

        /// <summary>
		/// 备注
		/// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
		/// 合计金额
		/// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? total_amount { get; set; }

        /// <summary>
		/// 客户
		/// </summary>
		[JsonProperty]
		public CRM_Customer customer { get; set; }

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
		/// 订单状态
		/// </summary>
		[JsonProperty]
		public Sys_Param OrderStatus { get; set; }

		/// <summary>
		/// 付款方式
		/// </summary>
		[JsonProperty]
		public Sys_Param PayType { get; set; }

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
