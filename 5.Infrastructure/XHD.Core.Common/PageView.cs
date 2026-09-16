using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHD.Core.Common
{
    public class PageView<T>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 单页条数
        /// </summary>
        public int Limit { get; set; } = 15;

        /// <summary>
        /// 查询条件
        /// </summary>
        public T models { get; set; }
    }
}
