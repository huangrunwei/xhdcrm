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
    public class Sale_order_pay_planRepository : BaseRepository<Sale_order_pay_plan>, ISale_order_pay_planRepository
    {
        public Sale_order_pay_planRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新（已收金额、状态由系统计算，不允许前端覆盖）
        /// </summary>
        public async new Task<int> UpdateAsync(Sale_order_pay_plan model)
        {
            var result = await _fsql.Update<Sale_order_pay_plan>()
                .SetSource(model)
                .IgnoreColumns(a => new
                {
                    a.create_id,
                    a.create_time,
                    a.received_amount,
                    a.plan_status,
                    a.isDelete,
                    a.Delete_time,
                    a.Delete_id
                })
                .ExecuteAffrowsAsync();

            return result;
        }

        public async new Task<XHDData<Sale_order_pay_plan>> GridAsync(Expression<Func<Sale_order_pay_plan, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sale_order_pay_plan>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Order.employee.id == a.Order.emp_id)
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Sale_order_pay_plan> result = new XHDData<Sale_order_pay_plan>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<XHDData<Sale_order_pay_plan>> GridAsync(Expression<Func<Sale_order_pay_plan, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sale_order_pay_plan>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Order.employee.id == a.Order.emp_id)
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Sale_order_pay_plan> result = new XHDData<Sale_order_pay_plan>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<List<Sale_order_pay_plan>> GridAsync(Expression<Func<Sale_order_pay_plan, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order_pay_plan>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Order.employee.id == a.Order.emp_id)
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .ToListAsync(true);

            return data;
        }

        public async new Task<List<Sale_order_pay_plan>> GridAsync(Expression<Func<Sale_order_pay_plan, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sale_order_pay_plan>()
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Order.employee.id == a.Order.emp_id)
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
