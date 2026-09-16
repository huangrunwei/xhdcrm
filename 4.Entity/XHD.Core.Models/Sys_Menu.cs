
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
	/// 菜单
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Menu {
		/// <summary>
		/// id
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// app_id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string App_id { get; set; } = string.Empty;

		[JsonProperty, Column(StringLength = 50)]
		public string Menu_icon { get; set; } = string.Empty;

		[JsonProperty, Column(StringLength = 255)]
		public string Menu_name { get; set; } = string.Empty;

		[JsonProperty]
		public int? Menu_order { get; set; }

		[JsonProperty, Column(StringLength = 50)]
		public string Menu_type { get; set; } = string.Empty;

		[JsonProperty, Column(StringLength = 255)]
		public string Menu_url { get; set; } = string.Empty;

		[JsonProperty, Column(StringLength = 50)]
		public string parentid { get; set; } = string.Empty;

	}

}
