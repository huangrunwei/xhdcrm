
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models {

	[JsonObject(MemberSerialization.OptIn)]
	public partial class CRM_follow {
		/// <summary>
		/// 主键
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

		/// <summary>
		/// 联系人id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string contact_id { get; set; } = string.Empty;

		/// <summary>
		/// 客户id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string customer_id { get; set; } = string.Empty;

        /// <summary>
		/// 跟进人id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string employee_id { get; set; } = string.Empty;

        /// <summary>
		/// 跟进目标id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string follow_aim_id { get; set; } = string.Empty;

        /// <summary>
		/// 跟进内容
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string follow_content { get; set; } = string.Empty;

        /// <summary>
		/// 跟进时间
		/// </summary>
		[JsonProperty]
		public DateTime? follow_time { get; set; }

		/// <summary>
		/// 跟进方式id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string follow_type_id { get; set; } = string.Empty;

		/// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public CRM_Customer customer { get; set; }

        /// <summary>
		/// 联系人
		/// </summary>
		[JsonProperty]
		public CRM_Contact contact { get; set; }

        /// <summary>
		/// 跟进目标
		/// </summary>
		[JsonProperty]
		public Sys_Param FollowAim { get; set; }

        /// <summary>
		/// 跟进方式
		/// </summary>
		[JsonProperty]
		public Sys_Param FollowType { get; set; }

        /// <summary>
		/// 跟进人
		/// </summary>
		[JsonProperty]
		public hr_employee employee { get; set; }

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
