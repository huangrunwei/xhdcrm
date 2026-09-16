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
    public class Sys_infoRepository : BaseRepository<Sys_info>, ISys_infoRepository
    {
        public Sys_infoRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
