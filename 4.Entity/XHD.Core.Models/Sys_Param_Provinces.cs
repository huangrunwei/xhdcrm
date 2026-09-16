
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
    /// 省份
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Param_Provinces {
        /// <summary>
        /// id
        /// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
        /// <summary>
        /// 创建人id
        /// </summary>
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string Provinces { get; set; } = string.Empty;

        /// <summary>
        /// 排序
		[JsonProperty]
		public int? Provinces_order { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Provinces_type { get; set; } = string.Empty;

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
