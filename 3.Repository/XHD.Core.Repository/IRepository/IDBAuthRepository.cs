using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;

using FreeSql;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.IRepository
{
    public interface IDBAuthRepository
    {
        Task<int> GetAuthType(string emp_id);

        Task<bool> GetAuth(string emp_id, string auth_id);

        Task<XHDRoleData> GetDataAuth(string emp_id);
    }
}
