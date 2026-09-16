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
    public interface ICRM_followService:IBaseService<CRM_follow>
    {
        Task<JArray> ReportYear(Expression<Func<CRM_follow, bool>> expWhere);
    }
}
