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
internal class Sys_authorityService : BaseService<Sys_authority>, ISys_authorityService
    {
        public Sys_authorityService(ISys_authorityRepository repository)
        {
            _irepository = repository;
        }
    }
}
