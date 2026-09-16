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
    public class CRM_followRepository : BaseRepository<CRM_follow>, ICRM_followRepository
    {
        public CRM_followRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(CRM_follow model)
        {
            var result = await _fsql.Update<CRM_follow>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.employee_id,a.follow_time,a.customer_id })
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
        public async new Task<XHDData<CRM_follow>> GridAsync(Expression<Func<CRM_follow, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<CRM_follow>()
                .LeftJoin(a => a.FollowAim.id == a.follow_aim_id && a.FollowAim.params_type == "follow_aim")
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .LeftJoin(a => a.customer.id == a.customer_id )
                .LeftJoin(a => a.contact.id == a.contact_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_follow> result = new XHDData<CRM_follow>()
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
        public async new Task<XHDData<CRM_follow>> GridAsync(Expression<Func<CRM_follow, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<CRM_follow>()
                .LeftJoin(a => a.FollowAim.id == a.follow_aim_id && a.FollowAim.params_type == "follow_aim")
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.contact.id == a.contact_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            //构建返回数据
            XHDData<CRM_follow> result = new XHDData<CRM_follow>()
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
        public async new Task<List<CRM_follow>> GridAsync(Expression<Func<CRM_follow, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_follow>()
                .LeftJoin(a => a.FollowAim.id == a.follow_aim_id && a.FollowAim.params_type == "follow_aim")
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.contact.id == a.contact_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
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
        public async new Task<List<CRM_follow>> GridAsync(Expression<Func<CRM_follow, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<CRM_follow>()
                .LeftJoin(a => a.FollowAim.id == a.follow_aim_id && a.FollowAim.params_type == "follow_aim")
                .LeftJoin(a => a.FollowType.id == a.follow_type_id && a.FollowType.params_type == "follow_type")
                .LeftJoin(a => a.customer.id == a.customer_id)
                .LeftJoin(a => a.contact.id == a.contact_id)
                .LeftJoin(a => a.employee.id == a.employee_id)
                .LeftJoin(a => a.employee.department.id == a.employee.dep_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }

        public async Task<JArray> ReportYear(Expression<Func<CRM_follow, bool>> expWhere)
        {
            var data = await _fsql.Select<CRM_follow>()
                .Where(expWhere)
                .GroupBy(a => new { xmonth = a.follow_time.Value.ToString("MM") })
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
    }
}
