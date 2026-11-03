using System;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// 知识库文档
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Crm_Knowledge
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 分类：产品知识/销售话术/售后支持/制度流程/其他
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string category { get; set; } = string.Empty;

        /// <summary>
        /// 关键词（逗号分隔）
        /// </summary>
        [JsonProperty, Column(StringLength = 250)]
        public string keywords { get; set; } = string.Empty;

        /// <summary>
        /// 摘要
        /// </summary>
        [JsonProperty, Column(StringLength = 500)]
        public string summary { get; set; } = string.Empty;

        /// <summary>
        /// 正文（HTML）
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 浏览次数
        /// </summary>
        [JsonProperty]
        public int? view_count { get; set; }

        /// <summary>
        /// 是否置顶：0否 1是
        /// </summary>
        [JsonProperty]
        public int? is_top { get; set; }

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
        /// 更新时间
        /// </summary>
        [JsonProperty]
        public DateTime? update_time { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [JsonProperty]
        public hr_employee creater { get; set; }

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
