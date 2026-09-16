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
    public class CRM_Customer_attaRepository : BaseRepository<CRM_Customer_atta>, ICRM_Customer_attaRepository
    {
        public CRM_Customer_attaRepository(IFreeSql freesql)
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
        public async new Task<XHDData<CRM_Customer_atta>> GridAsync(Expression<Func<CRM_Customer_atta, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<CRM_Customer_atta>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .LeftJoin(a => a.Customer.id == a.cus_id)
                .LeftJoin(a => a.Customer.cus_industry.id == a.Customer.cus_industry_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Customer_atta> result = new XHDData<CRM_Customer_atta>()
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
        public async new Task<XHDData<CRM_Customer_atta>> GridAsync(Expression<Func<CRM_Customer_atta, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<CRM_Customer_atta>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .LeftJoin(a => a.Customer.id == a.cus_id)
                .LeftJoin(a => a.Customer.cus_industry.id == a.Customer.cus_industry_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Customer_atta> result = new XHDData<CRM_Customer_atta>()
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
        public async new Task<List<CRM_Customer_atta>> GridAsync(Expression<Func<CRM_Customer_atta, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer_atta>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .LeftJoin(a => a.Customer.id == a.cus_id)
                .LeftJoin(a => a.Customer.cus_industry.id == a.Customer.cus_industry_id)
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
        public async new Task<List<CRM_Customer_atta>> GridAsync(Expression<Func<CRM_Customer_atta, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<CRM_Customer_atta>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .LeftJoin(a => a.Customer.id == a.cus_id)
                .LeftJoin(a => a.Customer.cus_industry.id == a.Customer.cus_industry_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
