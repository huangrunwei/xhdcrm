
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
	/// 系统日志
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_log {
		/// <summary>
		/// 日志ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 日志时间
		/// </summary>
		[JsonProperty]
		public DateTime? EventDate { get; set; }

        /// <summary>
		/// 日志ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string EventID { get; set; } = string.Empty;

        /// <summary>
        /// 客户id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string cus_id { get; set; } = string.Empty;

        /// <summary>
		/// 日志标题
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string EventTitle { get; set; } = string.Empty;

        /// <summary>
		/// 日志类型
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string EventType { get; set; } = string.Empty;

        /// <summary>
		/// IP地址
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string IPStreet { get; set; } = string.Empty;

        /// <summary>
		/// 日志内容
		/// </summary>
		[JsonProperty,Column(StringLength = -1)]
		public string Log_Content { get; set; } = string.Empty;

        /// <summary>
		/// 用户ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string UserID { get; set; } = string.Empty;

        /// <summary>
		/// 用户名称
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// 关联客户
		/// </summary>
        [JsonProperty]
		public CRM_Customer Customer { get; set; }

	}

}
