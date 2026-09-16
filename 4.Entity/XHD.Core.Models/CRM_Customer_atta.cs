
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
	public partial class CRM_Customer_atta {
		/// <summary>
		/// 附件id
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 客户id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string cus_id { get; set; } = string.Empty;

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
		/// 文件名
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string file_name { get; set; } = string.Empty;

		/// <summary>
		/// 文件大小
		/// </summary>
		[JsonProperty]
		public int? file_size { get; set; }

		/// <summary>
		/// 文件路径
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string real_name { get; set; } = string.Empty;

		/// <summary>
		/// 创建人
		/// </summary>
		[JsonProperty]
		public hr_employee creater { get; set; }

		/// <summary>
		/// 客户
		/// </summary>
        [JsonProperty]
		public CRM_Customer Customer { get; set; }

    }

}
