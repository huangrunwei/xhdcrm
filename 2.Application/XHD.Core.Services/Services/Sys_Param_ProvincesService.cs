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
internal class Sys_Param_ProvincesService : BaseService<Sys_Param_Provinces>, ISys_Param_ProvincesService
    {
        public Sys_Param_ProvincesService(ISys_Param_ProvincesRepository repository)
        {
            _irepository = repository;
        }
    }
}
