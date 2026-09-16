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
    public class My_NoteRepository : BaseRepository<My_Note>, IMy_NoteRepository
    {
        public My_NoteRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(My_Note model)
        {
            var result = await _fsql.Update<My_Note>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.emp_id,a.Note_time })
                .ExecuteAffrowsAsync();

            return result;
        }
    }
}
