
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
	/// 销售合同附件
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sale_contract_atta {
		/// <summary>
		/// 主键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 销售合同id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string contract_id { get; set; } = string.Empty;

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
		/// 文件名
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string file_name { get; set; } = string.Empty;

        /// <summary>
		/// 文件大小
		/// </summary>
		[JsonProperty]
		public int? file_size { get; set; }

        /// <summary>
		/// 真实文件名
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string real_name { get; set; } = string.Empty;

        /// <summary>
		/// 创建人
		/// </summary>
		[JsonProperty]
		public hr_employee creater { get; set; }

	}

}
