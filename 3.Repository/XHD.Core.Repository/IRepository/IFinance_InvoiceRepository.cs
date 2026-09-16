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
    public interface IFinance_InvoiceRepository: IXHDBaseRepository<Finance_Invoice>
    {
        Task<JArray> ReportYear(Expression<Func<Finance_Invoice, bool>> expWhere);

        Task<JArray> ReportYearSum(Expression<Func<Finance_Invoice, bool>> expWhere);

        Task<JArray> ReportInvoiceType(Expression<Func<Finance_Invoice, bool>> expWhere);
    }
}
