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
    public class Sys_logRepository : BaseRepository<Sys_log>, ISys_logRepository
    {
        public Sys_logRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        public async Task<List<string>> Logtype()
        {
            var data = await _fsql.Select<Sys_log>().Distinct().ToListAsync(a => a.EventType);

            //var data1 = await _fsql.Select<Sys_log>().GroupBy(a => a.EventDate.Value.ToString("yyyyMM")).ToListAsync(b=>b.Value.ToString());

            return data;
        }
    }
}
