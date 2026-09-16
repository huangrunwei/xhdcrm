
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
	public partial class hr_position {
        /// <summary>
        /// 职位id
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
        /// 职位等级
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string position_level { get; set; } = string.Empty;

        /// <summary>
        /// 职位名称
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string position_name { get; set; } = string.Empty;

        /// <summary>
        /// 职位排序
        /// </summary>
		[JsonProperty]
		public int? position_order { get; set; }

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
