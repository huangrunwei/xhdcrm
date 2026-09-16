using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;
using System.Linq;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XHD.Core.Repository
{
    public class CRM_CustomerRepository : BaseRepository<CRM_Customer>, ICRM_CustomerRepository
    {
        public CRM_CustomerRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(CRM_Customer model)
        {
            var result = await _fsql.Update<CRM_Customer>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.create_id, a.create_time, a.sn, a.isDelete, a.Delete_time,a.lastfollow })
                .ExecuteAffrowsAsync();

            return result;
        }

        /// <summary>
        /// 最后跟进
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> LastFollow(string id)
        {
            var result = await _fsql.Update<CRM_Customer>()
                .Set(a => a.lastfollow == DateTime.Now)
                .Where(a => a.id == id)
                .ExecuteAffrowsAsync();

            if (result == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async new Task<XHDData<CRM_Customer>> GridAsync(Expression<Func<CRM_Customer, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.cus_industry.id == a.cus_industry_id && a.cus_industry.params_type == "cus_industry")
                .LeftJoin(a => a.cus_type.id == a.cus_type_id && a.cus_type.params_type == "cus_type")
                .LeftJoin(a => a.cus_level.id == a.cus_level_id && a.cus_level.params_type == "cus_level")
                .LeftJoin(a => a.cus_source.id == a.cus_source_id && a.cus_source.params_type == "cus_source")
                .LeftJoin(a => a.Provinces.id == a.Provinces_id)
                .LeftJoin(a => a.City.id == a.City_id)
                .LeftJoin(a => a.Employee.id == a.emp_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .LeftJoin(a => a.Employee.department.id == a.Employee.dep_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Customer> result = new XHDData<CRM_Customer>()
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
        public async new Task<XHDData<CRM_Customer>> GridAsync(Expression<Func<CRM_Customer, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.cus_industry.id == a.cus_industry_id && a.cus_industry.params_type == "cus_industry")
                .LeftJoin(a => a.cus_type.id == a.cus_type_id && a.cus_type.params_type == "cus_type")
                .LeftJoin(a => a.cus_level.id == a.cus_level_id && a.cus_level.params_type == "cus_level")
                .LeftJoin(a => a.cus_source.id == a.cus_source_id && a.cus_source.params_type == "cus_source")
                .LeftJoin(a => a.Provinces.id == a.Provinces_id)
                .LeftJoin(a => a.City.id == a.City_id)
                .LeftJoin(a => a.Employee.id == a.emp_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .LeftJoin(a => a.Employee.department.id == a.Employee.dep_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_Customer> result = new XHDData<CRM_Customer>()
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
        public async new Task<List<CRM_Customer>> GridAsync(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.cus_industry.id == a.cus_industry_id && a.cus_industry.params_type == "cus_industry")
                .LeftJoin(a => a.cus_type.id == a.cus_type_id && a.cus_type.params_type == "cus_type")
                .LeftJoin(a => a.cus_level.id == a.cus_level_id && a.cus_level.params_type == "cus_level")
                .LeftJoin(a => a.cus_source.id == a.cus_source_id && a.cus_source.params_type == "cus_source")
                .LeftJoin(a => a.Provinces.id == a.Provinces_id)
                .LeftJoin(a => a.City.id == a.City_id)
                .LeftJoin(a => a.Employee.id == a.emp_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .LeftJoin(a => a.Employee.department.id == a.Employee.dep_id)
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
        public async new Task<List<CRM_Customer>> GridAsync(Expression<Func<CRM_Customer, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.cus_industry.id == a.cus_industry_id && a.cus_industry.params_type == "cus_industry")
                .LeftJoin(a => a.cus_type.id == a.cus_type_id && a.cus_type.params_type == "cus_type")
                .LeftJoin(a => a.cus_level.id == a.cus_level_id && a.cus_level.params_type == "cus_level")
                .LeftJoin(a => a.cus_source.id == a.cus_source_id && a.cus_source.params_type == "cus_source")
                .LeftJoin(a => a.Provinces.id == a.Provinces_id)
                .LeftJoin(a => a.City.id == a.City_id)
                .LeftJoin(a => a.Employee.id == a.emp_id)
                .LeftJoin(a => a.Creater.id == a.create_id)
                .LeftJoin(a => a.Employee.department.id == a.Employee.dep_id)
                    .Where(expWhere)
                    .OrderBy(OrderBy)
                    .ToListAsync(true);

            return data;
        }

        public async Task<JArray> ReportYear(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.create_time.Value.ToString("MM") })
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

        public async Task<JArray> ReportIndustry(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.cus_industry.params_name })
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

        public async Task<JArray> ReportType(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.cus_type.params_name })
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

        public async Task<JArray> ReportLevel(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.cus_level.params_name })
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

        public async Task<JArray> ReportSource(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.cus_source.params_name })
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

        public async Task<JArray> ReportProvinces(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.Provinces.id == a.Provinces_id)
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.Provinces.Provinces })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Count() });

            //return data.Select(a => (dynamic)a).ToList();
            JArray arr = new JArray();
            foreach (var item in data)
            {
                JObject obj = new JObject();

                if (string.IsNullOrWhiteSpace(item.xmonth))
                {
                    continue;
                }

                obj.Add("xmonth", item.xmonth);
                obj.Add("count", item.count);

                arr.Add(obj);
            }

            return arr;
        }

        public async Task<JArray> ReportCity(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_Customer>()
                .LeftJoin(a => a.City.id == a.City_id)
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.City.City })
                .ToListAsync(a => new { a.Key.xmonth, count = a.Count() });

            //return data.Select(a => (dynamic)a).ToList();
            JArray arr = new JArray();
            foreach (var item in data)
            {
                JObject obj = new JObject();

                if (string.IsNullOrWhiteSpace(item.xmonth))
                {
                    continue;
                }

                obj.Add("xmonth", item.xmonth);
                obj.Add("count", item.count);

                arr.Add(obj);
            }

            return arr;
        }
    }
}
