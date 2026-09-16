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
    public class JobsRepository : BaseRepository<Jobs>, IJobsRepository
    {
        public JobsRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }


        /// <summary>
        /// ¸üÐÂ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Jobs model)
        {
            var result = await _fsql.Update<Jobs>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.create_id, a.create_time })
                .ExecuteAffrowsAsync();

            return result;
        }
    }
}
