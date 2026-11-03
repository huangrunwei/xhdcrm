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
    public class Crm_KnowledgeRepository : BaseRepository<Crm_Knowledge>, ICrm_KnowledgeRepository
    {
        public Crm_KnowledgeRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        public async new Task<int> UpdateAsync(Crm_Knowledge model)
        {
            var result = await _fsql.Update<Crm_Knowledge>()
                .SetSource(model)
                .IgnoreColumns(a => new
                {
                    a.create_id,
                    a.create_time,
                    a.view_count,
                    a.isDelete,
                    a.Delete_time,
                    a.Delete_id
                })
                .ExecuteAffrowsAsync();

            return result;
        }

        public async new Task<XHDData<Crm_Knowledge>> GridAsync(Expression<Func<Crm_Knowledge, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Crm_Knowledge>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Crm_Knowledge> result = new XHDData<Crm_Knowledge>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<XHDData<Crm_Knowledge>> GridAsync(Expression<Func<Crm_Knowledge, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Crm_Knowledge>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync(true);

            XHDData<Crm_Knowledge> result = new XHDData<Crm_Knowledge>()
            {
                data = data,
                count = total
            };

            return result;
        }

        public async new Task<List<Crm_Knowledge>> GridAsync(Expression<Func<Crm_Knowledge, bool>> expWhere)
        {
            var data = await _fsql.Select<Crm_Knowledge>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .ToListAsync(true);

            return data;
        }

        public async new Task<List<Crm_Knowledge>> GridAsync(Expression<Func<Crm_Knowledge, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Crm_Knowledge>()
                .LeftJoin(a => a.creater.id == a.create_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync(true);

            return data;
        }
    }
}
