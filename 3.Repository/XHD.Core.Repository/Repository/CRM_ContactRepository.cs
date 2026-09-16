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
    public class CRM_ContactRepository : BaseRepository<CRM_Contact>, ICRM_ContactRepository
    {
        public CRM_ContactRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(CRM_Contact model)
        {
            var result = await _fsql.Update<CRM_Contact>()
                .SetSource(model)
                .IgnoreColumns(a => new {  a.create_id, a.create_time })
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
        public async new Task<XHDData<CRM_Contact>> GridAsync(Expression<Func<CRM_Contact, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<CRM_Contact>()
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Contact> result = new XHDData<CRM_Contact>()
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
        public async new Task<XHDData<CRM_Contact>> GridAsync(Expression<Func<CRM_Contact, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<CRM_Contact>()
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Contact> result = new XHDData<CRM_Contact>()
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
        public async new Task<List<CRM_Contact>> GridAsync(Expression<Func<CRM_Contact, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Contact>()
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
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
        public async new Task<List<CRM_Contact>> GridAsync(Expression<Func<CRM_Contact, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<CRM_Contact>()
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                    .Where(expWhere)
                    .OrderBy(OrderBy)
                    .ToListAsync(true);

            return data;
        }
    }
}
