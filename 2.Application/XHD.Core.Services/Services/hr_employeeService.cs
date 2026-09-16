using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
    internal class hr_employeeService : BaseService<hr_employee>, Ihr_employeeService
    {
        public hr_employeeService(Ihr_employeeRepository repository)
        {
            _irepository = repository;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public new async Task<XHDData<hr_employee>> Grid(Expression<Func<hr_employee, bool>> expWhere, int Page, int Limit, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await Grid(expWhere, Page, Limit);
            }

            expWhere = expWhere.And(a => a.uid != "admin");

            var result = await _irepository.GridAsync(expWhere, Page, Limit, OrderBy);

            var data = new XHDData<hr_employee>()
            {
                data = result.data,
                count = result.count
            };

            return data;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public new async Task<XHDData<hr_employee>> Grid(Expression<Func<hr_employee, bool>> expWhere, int Page, int Limit)
        {
            expWhere = expWhere.And(a => a.uid != "admin");

            var result = await _irepository.GridAsync(expWhere, Page, Limit);

            var data = new XHDData<hr_employee>()
            {
                data = result.data,
                count = result.count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public new async Task<XHDData<hr_employee>> Grid(Expression<Func<hr_employee, bool>> expWhere)
        {
            expWhere = expWhere.And(a => a.uid != "admin");

            var result = await _irepository.GridAsync(expWhere);

            var data = new XHDData<hr_employee>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public new async Task<XHDData<hr_employee>> Grid(Expression<Func<hr_employee, bool>> expWhere, string OrderBy)
        {
            expWhere = expWhere.And(a => a.uid != "admin");

            var result = await _irepository.GridAsync(expWhere, OrderBy);

            var data = new XHDData<hr_employee>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }

        public async Task<JObject> Login(hr_employee model)
        {
            Expression<Func<hr_employee, bool>> expression = a => a.uid == model.uid && a.pwd == Common.DEncrypt.MD5Comm.MD5Hash(model.pwd);
            //Expression<Func<hr_employee, bool>> expression = a => a.uid == model.uid && a.pwd.ToLower() == model.pwd.ToLower();

            var result = await _irepository.GridAsync(expression);

            if (result.Count == 0)
            {
                return XHDResult.Error("用户名或密码错误！");
            }

            if (result[0].canlogin == 0 && model.id != "admin")
            {
                return XHDResult.Error("此用户限制登录！");
            }

            var json = JsonConvert.SerializeObject(result[0], new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            JObject obj = JObject.Parse(json);

            return XHDResult.Success(obj);
        }
    }
}
