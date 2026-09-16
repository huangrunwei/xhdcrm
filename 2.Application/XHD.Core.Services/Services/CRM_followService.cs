using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace XHD.Core.Services
{
internal class CRM_followService : BaseService<CRM_follow>, ICRM_followService
    {
        ICRM_followRepository followRepository;
        public CRM_followService(ICRM_followRepository repository)
        {
            _irepository = repository;
            followRepository=repository;
        }

        public async Task<JArray> ReportYear(Expression<Func<CRM_follow, bool>> expWhere)
        { 
            return await followRepository.ReportYear(expWhere);
        }
    }
}
