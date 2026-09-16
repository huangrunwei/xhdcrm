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
internal class Sys_ButtonService : BaseService<Sys_Button>, ISys_ButtonService
    {
        public Sys_ButtonService(ISys_ButtonRepository repository)
        {
            _irepository = repository;
        }
    }
}
