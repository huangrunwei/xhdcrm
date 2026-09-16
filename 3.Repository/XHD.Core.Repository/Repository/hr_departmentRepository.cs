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
    public class hr_departmentRepository : BaseRepository<hr_department>, Ihr_departmentRepository
    {
        public hr_departmentRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
