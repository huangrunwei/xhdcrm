using DataType = FreeSql.DataType;

namespace XHD.Core.Common
{
    /// <summary>
    /// 多数据库
    /// </summary>
    public class MultiDb
    {
        /// <summary>
        /// 数据库命名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataType Type { get; set; }

        /// <summary>
        /// 数据库字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DbConfig
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataType Type { get; set; } = DataType.SqlServer;

        /// <summary>
        /// 数据库字符串
        /// </summary>
        public string ConnectionString { get; set; } 

        /// <summary>
        /// 多数据库
        /// </summary>
        public MultiDb[] Dbs { get; set; }
    }
}
