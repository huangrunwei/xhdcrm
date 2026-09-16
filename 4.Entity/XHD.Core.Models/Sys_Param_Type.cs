
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
	/// 参数类别
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Param_Type {
		/// <summary>
		/// 参数类别ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

		/// <summary>
		/// 创建人ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

		/// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

		/// <summary>
		/// 参数类别名称
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string params_name { get; set; } = string.Empty;

        /// <summary>
		/// 参数类别排序
		/// </summary>
		[JsonProperty]
		public int? params_order { get; set; }

	}

}
