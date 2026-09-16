
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
	/// 产品表
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Product {

		/// <summary>
		/// 产品id
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

		/// <summary>
		/// 折扣
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? agio { get; set; }

		/// <summary>
		/// 类别id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string category_id { get; set; } = string.Empty;

		/// <summary>
		/// 成本
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? cost { get; set; }

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
		/// 价格
		/// </summary>
		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? price { get; set; }

		/// <summary>
		/// 产品名字
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string product_name { get; set; } = string.Empty;

		/// <summary>
		/// 备注
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string remarks { get; set; } = string.Empty;

		/// <summary>
		/// 规格
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string specifications { get; set; } = string.Empty;

		/// <summary>
		/// 状态
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string status { get; set; } = string.Empty;

		/// <summary>
		/// 单位
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string unit { get; set; } = string.Empty;

		/// <summary>
		/// 类别
		/// </summary>
		[JsonProperty]
		public Product_category Category { get; set; }

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
