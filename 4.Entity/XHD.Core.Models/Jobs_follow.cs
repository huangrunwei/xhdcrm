
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
	/// 任务跟进，未启用
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Jobs_follow {
		/// <summary>
		/// 主键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

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
		/// 跟进内容
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string follow_content { get; set; } = string.Empty;

        /// <summary>
		/// 跟进状态
		/// </summary>
		[JsonProperty]
		public int? follow_status { get; set; }

        /// <summary>
		/// 任务ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string job_id { get; set; } = string.Empty;

	}

}
