using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;

using FreeSql;
using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace XHD.Core.IRepository
{
    public interface ISale_orderRepository: IXHDBaseRepository<Sale_order>
    {
        void UpdateOrderInvoice(string order_id);

        void UpdateOrderReceive(string order_id);

        Task<JArray> ReportYear(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportStatus(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportPayType(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportYearSum(Expression<Func<Sale_order, bool>> expWhere);
    }
}
