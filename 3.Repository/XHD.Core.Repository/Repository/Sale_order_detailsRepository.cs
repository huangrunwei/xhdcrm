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
    public class Sale_order_detailsRepository : BaseRepository<Sale_order_details>, ISale_order_detailsRepository
    {
        public Sale_order_detailsRepository(IFreeSql freesql)
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
        public async new Task<XHDData<Sale_order_details>> GridAsync(Expression<Func<Sale_order_details, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sale_order_details>()                
                .LeftJoin(a => a.Product.id == a.product_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sale_order_details> result = new XHDData<Sale_order_details>()
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
        public async new Task<XHDData<Sale_order_details>> GridAsync(Expression<Func<Sale_order_details, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sale_order_details>()
                .LeftJoin(a => a.Product.id == a.product_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sale_order_details> result = new XHDData<Sale_order_details>()
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
        public async new Task<List<Sale_order_details>> GridAsync(Expression<Func<Sale_order_details, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order_details>()
                .LeftJoin(a => a.Product.id == a.product_id)
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
        public async new Task<List<Sale_order_details>> GridAsync(Expression<Func<Sale_order_details, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sale_order_details>()
                .LeftJoin(a => a.Product.id == a.product_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
