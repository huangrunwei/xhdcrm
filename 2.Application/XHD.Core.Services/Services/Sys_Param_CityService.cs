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
internal class Sys_Param_CityService : BaseService<Sys_Param_City>, ISys_Param_CityService
    {
        public Sys_Param_CityService(ISys_Param_CityRepository repository)
        {
            _irepository = repository;
        }
    }
}
