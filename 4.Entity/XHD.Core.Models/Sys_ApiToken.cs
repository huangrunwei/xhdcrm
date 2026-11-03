using System;
using FreeSql.DataAnnotations;

namespace XHD.Core.Models
{
    /// <summary>
    /// 用户创建的长期 API / MCP 访问令牌（仅保存哈希，不保存明文）
    /// </summary>
    public class Sys_ApiToken
    {
        [Column(StringLength = 50)]
        public string id { get; set; } = string.Empty;

        [Column(StringLength = 50)]
        public string emp_id { get; set; } = string.Empty;

        [Column(StringLength = 100)]
        public string token_name { get; set; } = string.Empty;

        [Column(StringLength = 64)]
        public string token_hash { get; set; } = string.Empty;

        [Column(StringLength = 30)]
        public string token_prefix { get; set; } = string.Empty;

        [Column(StringLength = 12)]
        public string token_suffix { get; set; } = string.Empty;

        [Column(StringLength = 500)]
        public string remarks { get; set; } = string.Empty;

        public DateTime? expires_at { get; set; }

        public DateTime? last_used_at { get; set; }

        public int? is_revoked { get; set; }

        [Column(StringLength = 50)]
        public string create_id { get; set; } = string.Empty;

        public DateTime? create_time { get; set; }
    }
}
