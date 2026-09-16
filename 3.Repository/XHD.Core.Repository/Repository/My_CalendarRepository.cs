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
    public class My_CalendarRepository : BaseRepository<My_Calendar>, IMy_CalendarRepository
    {
        public My_CalendarRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(My_Calendar model)
        {
            var result = await _fsql.Update<My_Calendar>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.emp_id })
                .ExecuteAffrowsAsync();

            return result;
        }
    }
}
