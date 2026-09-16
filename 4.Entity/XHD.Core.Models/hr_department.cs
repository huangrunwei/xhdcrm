
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
	/// 部门表
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class hr_department {
		/// <summary>
		/// 部门id
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 创建人id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

        /// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

        /// <summary>
		/// 部门地址
		/// </summary>
		[JsonProperty]
		public string dep_add { get; set; } = string.Empty;

        /// <summary>
		/// 部门负责人
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_chief { get; set; } = string.Empty;

        /// <summary>
		/// 部门描述
		/// </summary>
		[JsonProperty]
		public string dep_descript { get; set; } = string.Empty;

        /// <summary>
		/// 部门邮箱
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_email { get; set; } = string.Empty;

        /// <summary>
		/// 部门传真
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_fax { get; set; } = string.Empty;

        /// <summary>
		/// 部门名称
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_name { get; set; } = string.Empty;

        /// <summary>
		/// 部门排序
		/// </summary>
		[JsonProperty]
		public int? dep_order { get; set; }

        /// <summary>
		/// 部门电话
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_tel { get; set; } = string.Empty;

        /// <summary>
		/// 部门类型
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_type { get; set; } = string.Empty;

        /// <summary>
		/// 上级部门id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string parentid { get; set; } = string.Empty;

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
