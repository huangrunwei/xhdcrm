
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// 权限表
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Sys_authority
    {
        /// <summary>
        /// 权限ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string App_id { get; set; } = string.Empty;

        /// <summary>
        /// 功能ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string Auth_id { get; set; } = string.Empty;

        /// <summary>
		/// 权限类型
		/// </summary>
		[JsonProperty]
        public int? Auth_type { get; set; }

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
		/// 角色ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
        public string Role_id { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        [JsonProperty]
        public Sys_role Role { get; set; }

    }

}
