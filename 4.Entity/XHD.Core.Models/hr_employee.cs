
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
	/// 员工表
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public partial class hr_employee {
		/// <summary>
		/// 员工id
		/// </summary>
		[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
		public string id { get; set; } = string.Empty;

        /// <summary>
		/// 员工地址
		/// </summary>
		[JsonProperty, Column(StringLength = 500)]
        public string address { get; set; } = string.Empty;

        /// <summary>
		/// 员工生日
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string birthday { get; set; } = string.Empty;

        /// <summary>
		/// 是否可登录
		/// </summary>
		[JsonProperty]
		public int? canlogin { get; set; }

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
		/// 默认城市
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string default_city { get; set; } = string.Empty;

        /// <summary>
		/// 部门id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string dep_id { get; set; } = string.Empty;

        /// <summary>
		/// 员工学历
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string education { get; set; } = string.Empty;

		/// <summary>
		/// 员工邮箱
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string email { get; set; } = string.Empty;

        /// <summary>
		/// 入职时间
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string EntryDate { get; set; } = string.Empty;

        /// <summary>
		/// 身份证号码
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string idcard { get; set; } = string.Empty;

        /// <summary>
		/// 员工姓名
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string name { get; set; } = string.Empty;

        /// <summary>
		/// 职位id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string position_id { get; set; } = string.Empty;

        /// <summary>
		/// 员工岗位id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string post_id { get; set; } = string.Empty;

        /// <summary>
		/// 员工专业
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string professional { get; set; } = string.Empty;

        /// <summary>
		/// 员工密码
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string pwd { get; set; } = string.Empty;

        /// <summary>
		/// 员工备注
		/// </summary>
		[JsonProperty]
		public string remarks { get; set; } = string.Empty;

        /// <summary>
		/// 员工学历
		/// </summary>
		[JsonProperty]
		public string schools { get; set; } = string.Empty;

        /// <summary>
		/// 员工性别
		/// </summary>
		[JsonProperty]
		public int? sex { get; set; }

        /// <summary>
		/// 排序
		/// </summary>
		[JsonProperty]
		public int? sort { get; set; }

        /// <summary>
		/// 员工状态
		/// </summary>
		[JsonProperty]
		public int? status { get; set; }

        /// <summary>
		/// 员工电话
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string tel { get; set; } = string.Empty;

        /// <summary>
		/// 员工uid
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string uid { get; set; } = string.Empty;

		/// <summary>
		/// 角色id
		/// </summary>
		[JsonProperty, Column(StringLength = 50)]
		public string role_id { get; set; } = string.Empty;

        /// <summary>
		/// 部门
		/// </summary>
		[JsonProperty]
		public hr_department department { get; set; }

        /// <summary>
		/// 职位
		/// </summary>
		[JsonProperty]
		public hr_position position { get; set; }

        /// <summary>
		/// 角色
		/// </summary>
		[JsonProperty]
		public Sys_role Role { get; set; }

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
