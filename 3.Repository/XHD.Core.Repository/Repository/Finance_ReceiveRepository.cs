using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;
using Newtonsoft.Json.Linq;

namespace XHD.Core.Repository
{
    public class Finance_ReceiveRepository : BaseRepository<Finance_Receive>, IFinance_ReceiveRepository
    {
        public Finance_ReceiveRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Finance_Receive model)
        {
            var result = await _fsql.Update<Finance_Receive>()
                .SetSource(model)
                .IgnoreColumns(
                    a => new
                    {
                        a.create_id,
                        a.create_time
                    })
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
        public async new Task<XHDData<Finance_Receive>> GridAsync(Expression<Func<Finance_Receive, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Finance_Receive>()
                .LeftJoin(a => a.PayType.id == a.Pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Payee.id == a.Payee_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Finance_Receive> result = new XHDData<Finance_Receive>()
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
        public async new Task<XHDData<Finance_Receive>> GridAsync(Expression<Func<Finance_Receive, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Finance_Receive>()
                .LeftJoin(a => a.PayType.id == a.Pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Payee.id == a.Payee_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Finance_Receive> result = new XHDData<Finance_Receive>()
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
        public async new Task<List<Finance_Receive>> GridAsync(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Receive>()
                .LeftJoin(a => a.PayType.id == a.Pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Payee.id == a.Payee_id)
                .LeftJoin(a => a.creater.id == a.create_id)
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
        public async new Task<List<Finance_Receive>> GridAsync(Expression<Func<Finance_Receive, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Finance_Receive>()
                .LeftJoin(a => a.PayType.id == a.Pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.Payee.id == a.Payee_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }

        public async Task<JArray> ReportYear(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Receive>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.Receive_date.Value.ToString("MM") })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Count() });

            //return data.Select(a => (dynamic)a).ToList();
            JArray arr = new JArray();
            foreach (var item in data)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", item.xmonth);
                obj.Add("count", item.count);

                arr.Add(obj);
            }

            return arr;
        }

        public async Task<JArray> ReportYearSum(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Receive>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.Receive_date.Value.ToString("MM") })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Sum(a.Value.Receive_amount) });

            //return data.Select(a => (dynamic)a).ToList();
            JArray arr = new JArray();
            foreach (var item in data)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", item.xmonth);
                obj.Add("count", item.count);

                arr.Add(obj);
            }

            return arr;
        }

        public async Task<JArray> ReportPayType(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Receive>()
                .LeftJoin(a => a.PayType.id == a.Pay_type_id && a.PayType.params_type == "pay_type")
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.PayType.params_name })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Count() });

            //return data.Select(a => (dynamic)a).ToList();
            JArray arr = new JArray();
            foreach (var item in data)
            {
                JObject obj = new JObject();

                if (string.IsNullOrWhiteSpace(item.xmonth))
                {
                    obj.Add("xmonth", "未分类");
                }
                else
                {
                    obj.Add("xmonth", item.xmonth);
                }


                obj.Add("count", item.count);

                arr.Add(obj);
            }

            return arr;
        }
    }
}
