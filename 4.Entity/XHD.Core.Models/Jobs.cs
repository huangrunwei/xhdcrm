
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
	/// 任务表，未启用
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Jobs {
		/// <summary>
		/// 任务ID
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
		/// 客户ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string customer_id { get; set; } = string.Empty;

        /// <summary>
		/// 执行时间
		/// </summary>
		[JsonProperty]
		public DateTime? executive_time { get; set; }
				
        /// <summary>
		/// 任务内容
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string job_content { get; set; } = string.Empty;

		/// <summary>
		/// 任务标题
		/// </summary>
		[JsonProperty, Column(StringLength = 200)]
		public string job_title { get; set; } = string.Empty;

        /// <summary>
		/// 优先级
		/// </summary>
        [JsonProperty]
        public int? priority { get; set; } 

        /// <summary>
		/// 状态
		/// </summary>
		[JsonProperty]
		public int? status { get; set; }

	}

}
