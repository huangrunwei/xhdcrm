using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.Repository
{
    public class DBAuthRepository : IDBAuthRepository
    {
        protected IFreeSql _fsql;

        public DBAuthRepository(IFreeSql freeSql)
        {
            _fsql = freeSql;
        }

        /// <summary>
        /// 获取用户所有角色的数据权限最高级别
        /// </summary>
        /// <param name="emp_id">员工ID</param>
        /// <returns>0无，1本人，2本部，3本部及下级，4全部</returns>
        public async Task<int> GetAuthType(string emp_id)
        {
            // 1. 先获取当前员工的角色ID
            var roleId = await _fsql.Select<hr_employee>()
                .Where(a => a.id == emp_id)
                .FirstAsync(a => a.role_id);

            // 2. 根据角色ID查询角色数据权限，取最大值，无数据返回0
            if (roleId == null)
                return 0;

            var maxDataAuth = await _fsql.Select<Sys_role>()
                .Where(r => r.id == roleId)
                .MaxAsync(r => r.DataAuth);

            return maxDataAuth ?? 0;
        }

        /// <summary>
        /// 获取用户是否有指定目录/按钮权限
        /// </summary>
        public async Task<bool> GetAuth(string emp_id, string auth_id)
        {
            // 系统管理员直接放行
            if (string.Equals(emp_id, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 获取用户的角色ID（假设每个员工只有一个角色，若多角色需调整）
            var roleId = await _fsql.Select<hr_employee>().Where(e => e.id == emp_id).FirstAsync(e => e.role_id);
            if (roleId == null)
            {
                return false;
            }

            // 检查角色是否有该权限
            var hasAuth = await _fsql.Select<Sys_authority>()
                .Where(a => a.Auth_id == auth_id && a.Role_id == roleId)
                .AnyAsync();

            return hasAuth;
        }

        /// <summary>
        /// 根据用户获取数据权限，返回员工ID列表
        /// </summary>
        public async Task<XHDRoleData> GetDataAuth(string emp_id)
        {
            // 系统管理员：全部权限
            if (string.Equals(emp_id, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return new XHDRoleData { authtype = 4, empList = new List<string>() };
            }

            int authType = await GetAuthType(emp_id);

            switch (authType)
            {
                case 0: // 无权限
                    return new XHDRoleData { authtype = 0, empList = new List<string>() };

                case 1: // 本人
                    return new XHDRoleData { authtype = 1, empList = new List<string> { emp_id } };

                case 2: // 本部门
                    var depId = await _fsql.Select<hr_employee>().Where(e => e.id == emp_id).FirstAsync(e => e.dep_id);
                    if (string.IsNullOrEmpty(depId))
                    {
                        return new XHDRoleData { authtype = 2, empList = new List<string>() };
                    }
                    var sameDeptEmps = await _fsql.Select<hr_employee>().Where(e => e.dep_id == depId).ToListAsync(e => e.id);
                    return new XHDRoleData { authtype = 2, empList = sameDeptEmps };

                case 3: // 本部及下级
                    var employee = await _fsql.Select<hr_employee>().Where(e => e.id == emp_id).FirstAsync();
                    if (employee == null)
                    {
                        return new XHDRoleData { authtype = 3, empList = new List<string>() };
                    }
                    var allDepts = await _fsql.Select<hr_department>().ToListAsync();
                    var childDeptIds = GetAllChildDeptIds(allDepts, employee.dep_id);
                    childDeptIds.Add(employee.dep_id);
                    var allEmps = await _fsql.Select<hr_employee>().Where(e => childDeptIds.Contains(e.dep_id)).ToListAsync(e => e.id);
                    return new XHDRoleData { authtype = 3, empList = allEmps };

                case 4: // 全部
                    return new XHDRoleData { authtype = 4, empList = new List<string>() };

                default:
                    return new XHDRoleData { authtype = 0, empList = new List<string>() };
            }
        }

        /// <summary>
        /// 递归获取部门及其所有下级部门ID列表
        /// </summary>
        private List<string> GetAllChildDeptIds(List<hr_department> allDepts, string parentId)
        {
            var children = allDepts.Where(d => d.parentid == parentId).ToList();
            var result = new List<string>();
            foreach (var child in children)
            {
                result.Add(child.id);
                result.AddRange(GetAllChildDeptIds(allDepts, child.id));
            }
            return result;
        }
    }
}