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
    public interface IFinance_InvoiceService:IBaseService<Finance_Invoice>
    {
        Task<JArray> ReportYear(Expression<Func<Finance_Invoice, bool>> expWhere);

        Task<JArray> ReportYearSum(Expression<Func<Finance_Invoice, bool>> expWhere);

        Task<JArray> ReportInvoiceType(Expression<Func<Finance_Invoice, bool>> expWhere);
    }
}
