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
    public interface ISys_logService:IBaseService<Sys_log>
    {

        Task<JObject> LogType();

        Task<int> UpdateLog(Sys_log models);

        Task<int> DeleteLog(Sys_log models);

        Task<int> LoginLog(string emp_id, string emp_name, string ip);
    }
}
