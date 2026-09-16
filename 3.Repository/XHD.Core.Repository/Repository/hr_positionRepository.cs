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
    public class hr_positionRepository : BaseRepository<hr_position>, Ihr_positionRepository
    {
        public hr_positionRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
