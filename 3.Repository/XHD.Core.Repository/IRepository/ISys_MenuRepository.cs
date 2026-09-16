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
    public interface ISys_MenuRepository: IXHDBaseRepository<Sys_Menu>
    {
        Task<List<string>> GetMenuByEmpID(string emp_id);
    }
}
