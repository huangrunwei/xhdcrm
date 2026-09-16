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
    public class Finance_InvoiceRepository : BaseRepository<Finance_Invoice>, IFinance_InvoiceRepository
    {
        public Finance_InvoiceRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Finance_Invoice model)
        {
            var result = await _fsql.Update<Finance_Invoice>()
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
        public async new Task<XHDData<Finance_Invoice>> GridAsync(Expression<Func<Finance_Invoice, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Finance_Invoice>()
                .LeftJoin(a => a.InvoiceType.id == a.invoice_type_id && a.InvoiceType.params_type == "invoice_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Finance_Invoice> result = new XHDData<Finance_Invoice>()
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
        public async new Task<XHDData<Finance_Invoice>> GridAsync(Expression<Func<Finance_Invoice, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Finance_Invoice>()
                .LeftJoin(a => a.InvoiceType.id == a.invoice_type_id && a.InvoiceType.params_type == "invoice_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<Finance_Invoice> result = new XHDData<Finance_Invoice>()
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
        public async new Task<List<Finance_Invoice>> GridAsync(Expression<Func<Finance_Invoice, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Invoice>()
                .LeftJoin(a => a.InvoiceType.id == a.invoice_type_id && a.InvoiceType.params_type == "invoice_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
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
        public async new Task<List<Finance_Invoice>> GridAsync(Expression<Func<Finance_Invoice, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Finance_Invoice>()
                .LeftJoin(a => a.InvoiceType.id == a.invoice_type_id && a.InvoiceType.params_type == "invoice_type")
                .LeftJoin(a => a.Order.id == a.order_id)
                .LeftJoin(a => a.Order.customer.id == a.Order.customer_id)
                .LeftJoin(a => a.employee.id == a.emp_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }

        public async Task<JArray> ReportYear(Expression<Func<Finance_Invoice, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Invoice>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.invoice_date.Value.ToString("MM") })
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

        public async Task<JArray> ReportYearSum(Expression<Func<Finance_Invoice, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Invoice>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.invoice_date.Value.ToString("MM") })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Sum(a.Value.invoice_amount) });

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

        public async Task<JArray> ReportInvoiceType(Expression<Func<Finance_Invoice, bool>> expWhere)
        {
            var data = await _fsql.Select<Finance_Invoice>()
                .LeftJoin(a => a.InvoiceType.id == a.invoice_type_id && a.InvoiceType.params_type == "invoice_type")
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.InvoiceType.params_name })
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
