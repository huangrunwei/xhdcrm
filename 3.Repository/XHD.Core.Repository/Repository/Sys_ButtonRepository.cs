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
    public class Sys_ButtonRepository : BaseRepository<Sys_Button>, ISys_ButtonRepository
    {
        public Sys_ButtonRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
