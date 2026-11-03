using System;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// 订单售后跟进
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Sale_order_follow
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty, Column(StringLength = 50, IsPrimary = true)]
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 订单id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string order_id { get; set; } = string.Empty;

        /// <summary>
        /// 客户id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string customer_id { get; set; } = string.Empty;

        /// <summary>
        /// 跟进人id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string employee_id { get; set; } = string.Empty;

        /// <summary>
        /// 售后类型：咨询答疑/安装调试/故障报修/投诉处理/回访关怀/续费增购/其他
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string service_type { get; set; } = string.Empty;

        /// <summary>
        /// 处理状态：0待处理 1处理中 2已解决 3已关闭
        /// </summary>
        [JsonProperty]
        public int? service_status { get; set; }

        /// <summary>
        /// 跟进方式id
        /// </summary>
        [JsonProperty, Column(StringLength = 50)]
        public string follow_type_id { get; set; } = string.Empty;

        /// <summary>
        /// 跟进内容
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string follow_content { get; set; } = string.Empty;

        /// <summary>
        /// 处理结果
        /// </summary>
        [JsonProperty, Column(StringLength = -1)]
        public string solution { get; set; } = string.Empty;

        /// <summary>
        /// 跟进时间
        /// </summary>
        [JsonProperty]
        public DateTime? follow_time { get; set; }

        /// <summary>
        /// 下次跟进时间
        /// </summary>
        [JsonProperty]
        public DateTime? next_time { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty]
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        [JsonProperty]
        public Sale_order Order { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        [JsonProperty]
        public CRM_Customer customer { get; set; }

        /// <summary>
        /// 跟进人
        /// </summary>
        [JsonProperty]
        public hr_employee employee { get; set; }

        /// <summary>
        /// 跟进方式
        /// </summary>
        [JsonProperty]
        public Sys_Param FollowType { get; set; }

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
