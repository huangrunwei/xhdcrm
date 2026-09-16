
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
	/// 销售合同
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sale_contract {
		/// <summary>
		/// 主键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 销售合同金额
		/// </summary>
        [JsonProperty, Column(DbType = "decimal(18,2)")]
        public decimal? Contract_amount { get; set; }

        /// <summary>
		/// 销售合同名称
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string Contract_name { get; set; } = string.Empty;

        /// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

        /// <summary>
		/// 创建人
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

        /// <summary>
		/// 客户联系人
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string Customer_Contractor { get; set; } = string.Empty;

        /// <summary>
		/// 客户id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string customer_id { get; set; } = string.Empty;

        /// <summary>
		/// 结束时间
		/// </summary>
		[JsonProperty]
		public DateTime? End_date { get; set; }

        /// <summary>
		/// 主内容
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string Main_Content { get; set; } = string.Empty;

        /// <summary>
		///我方签约人id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Our_Contractor_id { get; set; } = string.Empty;

        /// <summary>
		/// 付款周期
		/// </summary>
		[JsonProperty]
		public int? Pay_cycle { get; set; }

        /// <summary>
		/// 备注
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string Remarks { get; set; } = string.Empty;

        /// <summary>
		/// 编号
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string sn { get; set; } = string.Empty;

        /// <summary>
		/// 签订时间
		/// </summary>
		[JsonProperty]
		public DateTime? Sign_date { get; set; }

        /// <summary>
		/// 开始时间
		/// </summary>
		[JsonProperty]
		public DateTime? Start_date { get; set; }

        /// <summary>
		/// 客户
		/// </summary>
		[JsonProperty]
		public CRM_Customer customer { get; set; }

        /// <summary>
		/// 创建人
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
