
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
    /// 产品分类
    /// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Product_category {
        /// <summary>
        /// 分类id
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

        // <summary>
        /// 父级id
        /// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string parentid { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string category_name { get; set; } = string.Empty;

        /// <summary>
        /// 分类图标
        /// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string product_icon { get; set; } = string.Empty;

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
