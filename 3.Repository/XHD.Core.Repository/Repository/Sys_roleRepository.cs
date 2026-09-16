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
    public class Sys_roleRepository : BaseRepository<Sys_role>, ISys_roleRepository
    {
        public Sys_roleRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
