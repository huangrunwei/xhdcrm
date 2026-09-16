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
    public class Sys_Param_ProvincesRepository : BaseRepository<Sys_Param_Provinces>, ISys_Param_ProvincesRepository
    {
        public Sys_Param_ProvincesRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// ¸üÐÂ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Sys_Param_Provinces model)
        {
            var result = await _fsql.Update<Sys_Param_Provinces>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.Provinces_type,a.create_id, a.create_time })
                .ExecuteAffrowsAsync();

            return result;
        }
    }
}
