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
    public class Sys_authorityRepository : BaseRepository<Sys_authority>, ISys_authorityRepository
    {
        public Sys_authorityRepository(IFreeSql freesql)
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
        public async new Task<XHDData<Sys_authority>> GridAsync(Expression<Func<Sys_authority, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sys_authority>()
                .LeftJoin(a => a.Role.id == a.Role_id )
                
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sys_authority> result = new XHDData<Sys_authority>()
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
        public async new Task<XHDData<Sys_authority>> GridAsync(Expression<Func<Sys_authority, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sys_authority>()
                .LeftJoin(a => a.Role.id == a.Role_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sys_authority> result = new XHDData<Sys_authority>()
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
        public async new Task<List<Sys_authority>> GridAsync(Expression<Func<Sys_authority, bool>> expWhere)
        {
            var data = await _fsql.Select<Sys_authority>()
                .LeftJoin(a => a.Role.id == a.Role_id)
                .Where(expWhere)
                .ToListAsync(true);

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="OrderBy"></param>
        /// <returns></returns>
        public async new Task<List<Sys_authority>> GridAsync(Expression<Func<Sys_authority, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sys_authority>()
                .LeftJoin(a => a.Role.id == a.Role_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
