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
    public interface ICRM_followRepository: IXHDBaseRepository<CRM_follow>
    {
        Task<JArray> ReportYear(Expression<Func<CRM_follow, bool>> expWhere);
    }
}
