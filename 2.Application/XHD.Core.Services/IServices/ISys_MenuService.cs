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
    public interface ISys_MenuService:IBaseService<Sys_Menu>
    {
        Task<JArray> Tree();

        Task<JArray> Tree(Expression<Func<Sys_Menu, bool>> expWhere,bool isChildren);

        Task<JArray> Combo();

        Task<List<string>> GetMenuByEmpID(string emp_id);
    }
}
