
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
	public partial class CRM_Customer {

		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

		/// <summary>
		/// 城市ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string City_id { get; set; } = string.Empty;

		/// <summary>
		/// 创建人ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string create_id { get; set; } = string.Empty;

		/// <summary>
		/// 创建时间
		/// </summary>
		[JsonProperty]
		public DateTime? create_time { get; set; }

		/// <summary>
		/// 地址
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string cus_add { get; set; } = string.Empty;

        /// <summary>
		/// 扩展字段
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string cus_extend { get; set; } = string.Empty;

        /// <summary>
		/// 传真
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string cus_fax { get; set; } = string.Empty;

        /// <summary>
		/// 行业ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string cus_industry_id { get; set; } = string.Empty;

        /// <summary>
		/// 等级ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string cus_level_id { get; set; } = string.Empty;

        /// <summary>
		/// 名称
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string cus_name { get; set; } = string.Empty;

        /// <summary>
		/// 来源ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string cus_source_id { get; set; } = string.Empty;

        /// <summary>
		/// 电话
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string cus_tel { get; set; } = string.Empty;

        /// <summary>
		/// 类型ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string cus_type_id { get; set; } = string.Empty;

		/// <summary>
		/// 网站
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string cus_website { get; set; } = string.Empty;

        /// <summary>
		/// 删除时间
		/// </summary>
		[JsonProperty]
		public DateTime? Delete_time { get; set; }

        /// <summary>
		/// 描述
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string DesCripe { get; set; } = string.Empty;

        /// <summary>
		/// 归属人ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string emp_id { get; set; } = string.Empty;

        /// <summary>
		/// 是否删除
		/// </summary>
		[JsonProperty]
		public int? isDelete { get; set; }

        /// <summary>
		/// 是否私有
		/// </summary>
		[JsonProperty]
		public int? isPrivate { get; set; }

        /// <summary>
		/// 最后跟进时间
		/// </summary>
		[JsonProperty]
		public DateTime? lastfollow { get; set; }

        /// <summary>
		/// 省份ID
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string Provinces_id { get; set; } = string.Empty;

        /// <summary>
		/// 备注
		/// </summary>
		[JsonProperty, Column(StringLength = -1)]
		public string Remarks { get; set; } = string.Empty;

        /// <summary>
		/// 唯一标识
		/// </summary>
		[JsonProperty, Column(StringLength = 250)]
		public string sn { get; set; } = string.Empty;

        /// <summary>
		/// 坐标（已放弃）
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string xy { get; set; } = string.Empty;

        /// <summary>
		/// 行业
		/// </summary>
		[JsonProperty]
		public Sys_Param cus_industry { get; set; }

        /// <summary>
		/// 类型
		/// </summary>
		[JsonProperty]
		public Sys_Param cus_type { get; set; }

        /// <summary>
		/// 等级
		/// </summary>
		[JsonProperty]
		public Sys_Param cus_level { get; set; }

        /// <summary>
		/// 来源
		/// </summary>
		[JsonProperty]
		public Sys_Param cus_source { get; set; }

        /// <summary>
		/// 省份
		/// </summary>
		[JsonProperty]
		public Sys_Param_Provinces Provinces { get; set; }

        /// <summary>
		/// 城市
		/// </summary>
		[JsonProperty]
		public Sys_Param_City City { get; set; }

        /// <summary>
		/// 客户归属人
		/// </summary>
		[JsonProperty]
		public hr_employee Employee { get; set; }

        /// <summary>
		/// 创建人
		/// </summary>
		[JsonProperty]
		public hr_employee Creater { get; set; }

		/// <summary>
		/// 坐标X
		/// </summary>
        [JsonProperty, Column(DbType = "decimal(18,10)")]
        public decimal? x { get; set; }

        /// <summary>
		/// 坐标Y
		/// </summary>
        [JsonProperty, Column(DbType = "decimal(18,10)")]
        public decimal? y { get; set; }

        /// <summary>
		/// 删除人ID
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
