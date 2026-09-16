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
    public interface ICRM_CustomerService:IBaseService<CRM_Customer>
    {
        Task<JArray> ReportYear(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportIndustry(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportType(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportLevel(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportSource(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportCity(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<JArray> ReportProvinces(Expression<Func<CRM_Customer, bool>> expWhere);

        Task<bool> LastFollow(string id);
    }
}
