using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace XHD.Core.Services
{
    internal class DBAuthService:IDBAuthService
    {
        protected IDBAuthRepository _irepository;
        public DBAuthService(IDBAuthRepository repository)
        {
            _irepository = repository;
        }

        /// <summary>
        /// 获取用户所有角色的数据权限最高级别
        /// </summary>
        /// <param name="emp_id"></param>
        /// <returns>0无，1本人，2本部，3本部及下级，4全部</returns>
        public async Task<int> GetAuthType(string emp_id)
        {
            return await _irepository.GetAuthType(emp_id);
        }

        /// <summary>
        /// 根据用户获取目录和按钮权限
        /// </summary>
        /// <param name="emp_id">用户id</param>
        /// <param name="auth_id">目录id</param>
        /// <returns></returns>
        public async Task<bool> GetAuth(string emp_id, string auth_id)
        {
            //系统管理员，绝对的权限
            if (emp_id.ToLower().Equals("admin"))
            {
                return true;
            }

            return await _irepository.GetAuth(emp_id, auth_id);
        }

        /// <summary>
        /// 根据用户获取数据权限，返回用户id列表
        /// 此处都返回emp_id，不返回dep_id，是因为防止查询过深
        /// 比如order，如果返回的是dep_id，则需要这样查询：Customr.employee.department.id==dep_id
        /// 如果返回的是emplist，则Customer.employee.id in (emplist),可以少查询一层
        /// </summary>
        /// <param name="emp_id">用户id</param>
        /// <returns></returns>
        public async Task<XHDRoleData> GetDataAuth(string emp_id)
        {
            //系统管理员，绝对的权限
            if (emp_id.ToLower().Equals("admin"))
            {
                //全部,返回一个标记即可，前端无需处理
                return new XHDRoleData { authtype = 4, empList = new List<string> { } };
            }

            return await _irepository.GetDataAuth(emp_id);
        }
    }
}
