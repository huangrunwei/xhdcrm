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
    public class Sale_orderRepository : BaseRepository<Sale_order>, ISale_orderRepository
    {
        public Sale_orderRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Sale_order model)
        {
            var result = await _fsql.Update<Sale_order>()
                .SetSource(model)
                .IgnoreColumns(
                    a => new
                    {
                        a.create_id,
                        a.create_time,
                        a.sn,
                        a.receive_money,
                        a.arrears_money,
                        a.invoice_money,
                        a.arrears_invoice
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
        public async new Task<XHDData<Sale_order>> GridAsync(Expression<Func<Sale_order, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.OrderStatus.id == a.Order_status_id && a.OrderStatus.params_type == "order_status")
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sale_order> result = new XHDData<Sale_order>()
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
        public async new Task<XHDData<Sale_order>> GridAsync(Expression<Func<Sale_order, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.OrderStatus.id == a.Order_status_id && a.OrderStatus.params_type == "order_status")
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Sale_order> result = new XHDData<Sale_order>()
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
        public async new Task<List<Sale_order>> GridAsync(Expression<Func<Sale_order, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.OrderStatus.id == a.Order_status_id && a.OrderStatus.params_type == "order_status")
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
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
        public async new Task<List<Sale_order>> GridAsync(Expression<Func<Sale_order, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.OrderStatus.id == a.Order_status_id && a.OrderStatus.params_type == "order_status")
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }

        /// <summary>
        /// 更新订单发票
        /// </summary>
        /// <param name="order_id"></param>
        /// <returns></returns>
        public async void UpdateOrderInvoice(string order_id)
        {
            //更新订单发票总额
            var invoiceamount = await _fsql.Select<Finance_Invoice>().Where(a => a.order_id == order_id).SumAsync(a => a.invoice_amount);
            await _fsql.Update<Sale_order>().Set(a => a.invoice_money == invoiceamount).Where(a => a.id == order_id).ExecuteAffrowsAsync();

            //更新订单发票余额
            await _fsql.Update<Sale_order>().Set(a => a.arrears_invoice == a.total_amount - a.invoice_money).Where(a => a.id == order_id).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 更新订单收款
        /// </summary>
        /// <param name="order_id"></param>
        /// <returns></returns>
        public async void UpdateOrderReceive(string order_id)
        {
            //更新订单收款总额
            var receiveamount = await _fsql.Select<Finance_Receive>().Where(a => a.order_id == order_id).SumAsync(a => a.Receive_amount);
            await _fsql.Update<Sale_order>().Set(a => a.receive_money == receiveamount).Where(a => a.id == order_id).ExecuteAffrowsAsync();

            //更新订单收款余额
            await _fsql.Update<Sale_order>().Set(a => a.arrears_money == a.total_amount - a.receive_money).Where(a => a.id == order_id).ExecuteAffrowsAsync();
        }

        public async Task<JArray> ReportYear(Expression<Func<Sale_order, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.Order_date.Value.ToString("MM") })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Count()});

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

        public async Task<JArray> ReportYearSum(Expression<Func<Sale_order, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order>()                
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.Order_date.Value.ToString("MM") })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Sum(a.Value.Order_amount) });

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

        public async Task<JArray> ReportStatus(Expression<Func<Sale_order, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.OrderStatus.id == a.Order_status_id && a.OrderStatus.params_type == "order_status")
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.OrderStatus.params_name })
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

        public async Task<JArray> ReportPayType(Expression<Func<Sale_order, bool>> expWhere)
        {
            var data = await _fsql.Select<Sale_order>()
                .LeftJoin(a => a.PayType.id == a.pay_type_id && a.PayType.params_type == "pay_type")
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
