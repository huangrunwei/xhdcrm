
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
    /// 参数表
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Sys_Param {
        /// <summary>
        /// 参数id
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
        /// 参数名称
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string params_name { get; set; } = string.Empty;

        /// <summary>
        /// 参数排序
        /// </summary>
		[JsonProperty]
		public int? params_order { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string params_type { get; set; } = string.Empty;

        /// <summary>
        /// 参数类型
        /// </summary>
		[JsonProperty]
		public Sys_Param_Type ParamType { get; set; }

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
