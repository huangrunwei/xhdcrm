using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.Repository
{
    public class Sys_MenuRepository : BaseRepository<Sys_Menu>, ISys_MenuRepository
    {
        public Sys_MenuRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        public async Task<List<string>> GetMenuByEmpID(string emp_id)
        {
            //根据emp_id查找
            List<string> rolelist = await _fsql.Select<hr_employee>().Where(a => a.id == emp_id).ToListAsync(a => a.id);

            //根据roleid查找目录
            List<string> menulist = await _fsql.Select<Sys_authority>().Where(a => rolelist.Contains(a.Role_id) && a.Auth_type == 2).ToListAsync(a => a.Auth_id);

            return menulist;
        }
    }
}
