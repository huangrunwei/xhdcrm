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
    public interface IDBAuthService
    {
        Task<int> GetAuthType(string emp_id);

        Task<bool> GetAuth(string emp_id, string auth_id);

        Task<XHDRoleData> GetDataAuth(string emp_id);
    }
}
