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
            var employee = await _fsql.Select<hr_employee>().Where(a => a.id == emp_id).FirstAsync();
            if (employee == null || string.IsNullOrWhiteSpace(employee.role_id))
            {
                return new List<string>();
            }

            //根据角色查找已授权的目录/按钮
            List<string> menulist = await _fsql.Select<Sys_authority>()
                .Where(a => a.Role_id == employee.role_id)
                .ToListAsync(a => a.Auth_id);

            return menulist;
        }
    }
}
