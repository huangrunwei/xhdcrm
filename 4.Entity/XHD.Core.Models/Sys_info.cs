
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
	/// 系统信息
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_info {
		/// <summary>
		/// 键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string sys_key { get; set; } = string.Empty;

        /// <summary>
		/// 值
		/// </summary>
		[JsonProperty,Column(StringLength = -1)]
		public string sys_value { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty, Column(StringLength = 500)]
        public string sys_remark { get; set; } = string.Empty;
        

    }

}
