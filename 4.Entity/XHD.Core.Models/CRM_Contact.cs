
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

    [JsonObject(MemberSerialization.OptIn)]
    public partial class CRM_Contact
    {
        /// <summary>
        /// 联系人ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 联系人地址
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_add { get; set; } = string.Empty;

        /// <summary>
        /// 联系人生日
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string C_birthday { get; set; }

        /// <summary>
        /// 联系人部门
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_department { get; set; } = string.Empty;

        /// <summary>
        /// 联系人邮箱
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_email { get; set; } = string.Empty;

        /// <summary>
        /// 联系人微信
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_weichat { get; set; } = string.Empty;

        /// <summary>
        /// 联系人爱好
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_hobby { get; set; } = string.Empty;

        /// <summary>
        /// 联系人手机
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_mob { get; set; } = string.Empty;

        /// <summary>
        /// 联系人姓名
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_name { get; set; } = string.Empty;

        /// <summary>
        /// 联系人职位
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_position { get; set; } = string.Empty;

        /// <summary>
        /// 联系人QQ
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_QQ { get; set; } = string.Empty;

        /// <summary>
        /// 联系人备注
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string C_remarks { get; set; } = string.Empty;

        /// <summary>
        /// 联系人性别
        /// </summary>
        [JsonProperty]
        public int? C_sex { get; set; }

        /// <summary>
        /// 联系人电话
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string C_tel { get; set; } = string.Empty;

        /// <summary>
        /// 联系人创建人
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string create_id { get; set; } = string.Empty;

        /// <summary>
        /// 联系人创建时间
        /// </summary>
        [JsonProperty]
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 联系人所属客户id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string customer_id { get; set; } = string.Empty;

        /// <summary>
        /// 联系人所属客户
        /// </summary>
        [JsonProperty]
        public CRM_Customer customer { get; set; }

        /// <summary>
        /// 联系人创建人
        /// </summary>
        [JsonProperty]
        public hr_employee Creater { get; set; }

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
