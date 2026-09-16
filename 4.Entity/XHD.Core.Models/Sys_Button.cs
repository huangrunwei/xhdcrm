
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
	/// 按钮
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Button {
		/// <summary>
		/// 按钮ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 按钮处理函数
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Btn_handler { get; set; } = string.Empty;

        /// <summary>
		/// 按钮图标
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Btn_icon { get; set; } = string.Empty;

        /// <summary>
		/// 按钮名称
		/// </summary>
		[JsonProperty]
		public string Btn_name { get; set; } = string.Empty;

        /// <summary>
		/// 按钮排序
		/// </summary>
		[JsonProperty]
		public int? Btn_order { get; set; }

        /// <summary>
		/// 按钮类型
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Btn_type { get; set; } = string.Empty;

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
		/// 菜单ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Menu_id { get; set; } = string.Empty;

	}

}
