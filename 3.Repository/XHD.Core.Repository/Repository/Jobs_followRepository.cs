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
    public class Jobs_followRepository : BaseRepository<Jobs_follow>, IJobs_followRepository
    {
        public Jobs_followRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
