
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
	/// 销售订单详情
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sale_order_details {
		/// <summary>
		/// 单价
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? price { get; set; }

        /// <summary>
		/// 金额
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? amount { get; set; }

        /// <summary>
		/// 订单id
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string order_id { get; set; } = string.Empty;

        /// <summary>
		/// 产品id
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string product_id { get; set; } = string.Empty;

        /// <summary>
		/// 数量
		/// </summary>
		[JsonProperty]
		public int? quantity { get; set; }

        /// <summary>
		/// 产品
		/// </summary>
		[JsonProperty]
		public Product Product { get; set; }

	}

}
