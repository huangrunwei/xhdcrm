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
    public class Sys_Param_TypeRepository : BaseRepository<Sys_Param_Type>, ISys_Param_TypeRepository
    {
        public Sys_Param_TypeRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
