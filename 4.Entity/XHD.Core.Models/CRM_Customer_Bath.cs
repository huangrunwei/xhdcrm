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
    public partial class CRM_Customer_Bath
    {
        /// <summary>
        /// 批量转移id
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

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
        /// 旧员工id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string old_emp_id { get; set; } = string.Empty;

        /// <summary>
        /// 新员工id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string new_emp_id { get; set; } = string.Empty;

        /// <summary>
        /// 条件
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string Condition { get; set; } = string.Empty;

        /// <summary>
        /// 转出数量
        /// </summary>
        [JsonProperty]
        public int? cus_count { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// 转出人
        /// </summary>
        [JsonProperty]
        public hr_employee OldEmp { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        [JsonProperty]
        public hr_employee NewEmp { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [JsonProperty]
        public hr_employee Creater { get; set; }

    }

}
