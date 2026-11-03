using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.Repository
{
    public class Sale_order_followRepository : BaseRepository<Sale_order_follow>, ISale_order_followRepository
    {
        public Sale_order_followRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public async new Task<int> UpdateAsync(Sale_order_follow model)
        {
            var result = await _fsql.Update<Sale_order_follow>()
                .SetSource(model)
                .IgnoreColumns(a => new
                {
                    a.create_time,
                    a.isDelete,
                    a.Delete_time,
                    a.Delete_id
                })
                .ExecuteAffrowsAsync();

            return result;
        }

        public async new Task<XHDData<Sale_order_follow>> GridAsync(Expression<Func<Sale_order_follow, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sale_order_follow>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .Where(expWhere)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Sale_order_follow> result = new XHDData<Sale_order_follow>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<XHDData<Sale_order_follow>> GridAsync(Expression<Func<Sale_order_follow, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sale_order_follow>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Sale_order_follow> result = new XHDData<Sale_order_follow>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<List<Sale_order_follow>> GridAsync(Expression<Func<Sale_order_follow, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order_follow>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .Where(expWhere)
                .ToListAsync(true);

            return data;
        }

        public async new Task<List<Sale_order_follow>> GridAsync(Expression<Func<Sale_order_follow, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sale_order_follow>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
