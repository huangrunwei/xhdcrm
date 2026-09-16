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
    /// <summary>
    /// 便签
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class My_Note
    {
        /// <summary>
        /// 便签ID
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string emp_id { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty]
        public DateTime? Note_time { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty, Column(StringLength = 500)]
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 颜色
        /// </summary>
        [JsonProperty, Column(StringLength = 10)]
        public string color { get; set; } = string.Empty;

        /// <summary>
        /// 上坐标
        [JsonProperty]
        public int? top { get; set; }

        /// <summary>
        /// 左坐标
        /// </summary>
        [JsonProperty]
        public int? left { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        [JsonProperty]
        public int? zIndex { get; set; }

    }
}
