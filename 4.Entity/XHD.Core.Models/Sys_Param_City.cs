
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
    /// 城市
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Param_City {
        /// <summary>
        /// id
        /// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
        /// 城市
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string City { get; set; } = string.Empty;

        /// <summary>
        /// 城市排序
        /// </summary>
		[JsonProperty]
		public int? City_order { get; set; }

        /// <summary>
        /// 城市类型
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string City_type { get; set; } = string.Empty;

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
        /// 省份id
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Provinces_id { get; set; } = string.Empty;

        /// <summary>
        /// 省份
        /// </summary>
		[JsonProperty]
		public Sys_Param_Provinces Provinces { get; set; }

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
