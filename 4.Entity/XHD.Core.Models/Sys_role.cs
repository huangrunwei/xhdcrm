
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
    /// 角色
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_role {
        /// <summary>
        /// 角色id
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
        /// 数据权限
        /// </summary>
		[JsonProperty]
		public int? DataAuth { get; set; }

        /// <summary>
        /// 公客权限
        /// </summary>
		[JsonProperty]
		public int? PublicAuth { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
		[JsonProperty]
		public string RoleDscript { get; set; } = string.Empty;

        /// <summary>
        /// <summary>
        /// 角色名称
        /// </summary>
        /// </summary>
		[JsonProperty]
		public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// 角色排序
        /// </summary>
		[JsonProperty]
		public int? RoleSort { get; set; }

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
