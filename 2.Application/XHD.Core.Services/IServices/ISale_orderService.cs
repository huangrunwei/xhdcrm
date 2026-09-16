using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace XHD.Core.IServices
{
    public interface ISale_orderService:IBaseService<Sale_order>
    {
        Task<int> UpdateArrearsMoney(string id);

        void UpdateOrderInvoice(string order_id);

        void UpdateOrderReceive(string order_id);

        Task<JArray> ReportYear(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportStatus(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportPayType(Expression<Func<Sale_order, bool>> expWhere);

        Task<JArray> ReportYearSum(Expression<Func<Sale_order, bool>> expWhere);
    }
}
