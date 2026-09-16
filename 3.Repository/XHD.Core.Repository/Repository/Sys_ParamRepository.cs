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
    public class Sys_ParamRepository : BaseRepository<Sys_Param>, ISys_ParamRepository
    {
        public Sys_ParamRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async new Task<XHDData<Sys_Param>> GridAsync(Expression<Func<Sys_Param, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sys_Param>()
                .LeftJoin(a => a.ParamType.id == a.params_type)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<Sys_Param> result = new XHDData<Sys_Param>()
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
        public async new Task<XHDData<Sys_Param>> GridAsync(Expression<Func<Sys_Param, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sys_Param>()
                .LeftJoin(a => a.ParamType.id == a.params_type)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<Sys_Param> result = new XHDData<Sys_Param>()
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
        public async new Task<List<Sys_Param>> GridAsync(Expression<Func<Sys_Param, bool>> expWhere)
        {
            var data = await _fsql.Select<Sys_Param>()
                .LeftJoin(a => a.ParamType.id == a.params_type)
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
        public async new Task<List<Sys_Param>> GridAsync(Expression<Func<Sys_Param, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sys_Param>()
                .LeftJoin(a => a.ParamType.id == a.params_type)
                    .Where(expWhere)
                    .OrderBy(OrderBy)
                    .ToListAsync();

            return data;
        }
    }
}
