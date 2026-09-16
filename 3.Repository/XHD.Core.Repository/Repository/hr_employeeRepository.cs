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
    public class hr_employeeRepository : BaseRepository<hr_employee>, Ihr_employeeRepository
    {
        public hr_employeeRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(hr_employee model)
        {
            var result = await _fsql.Update<hr_employee>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.default_city, a.create_id, a.create_time, a.pwd })
                .ExecuteAffrowsAsync();

            return result;
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async new Task<XHDData<hr_employee>> GridAsync(Expression<Func<hr_employee, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<hr_employee>()
                .LeftJoin(a => a.position.id == a.position_id)
                .LeftJoin(a => a.department.id == a.dep_id)
                .LeftJoin(a => a.Role.id == a.role_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<hr_employee> result = new XHDData<hr_employee>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async new Task<XHDData<hr_employee>> GridAsync(Expression<Func<hr_employee, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<hr_employee>()
                .LeftJoin(a => a.position.id == a.position_id)
                .LeftJoin(a => a.department.id == a.dep_id)
                .LeftJoin(a => a.Role.id == a.role_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<hr_employee> result = new XHDData<hr_employee>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async new Task<List<hr_employee>> GridAsync(Expression<Func<hr_employee, bool>> expWhere)
        {
            var data = await _fsql.Select<hr_employee>()
                .LeftJoin(a => a.position.id == a.position_id)
                .LeftJoin(a => a.department.id == a.dep_id)
                .LeftJoin(a => a.Role.id == a.role_id)
                .Where(expWhere)
                .ToListAsync();

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="OrderBy"></param>
        /// <returns></returns>
        public async new Task<List<hr_employee>> GridAsync(Expression<Func<hr_employee, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<hr_employee>()
                .LeftJoin(a => a.position.id == a.position_id)
                .LeftJoin(a => a.department.id == a.dep_id)
                .LeftJoin(a => a.Role.id == a.role_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync();

            return data;
        }
    }
}
