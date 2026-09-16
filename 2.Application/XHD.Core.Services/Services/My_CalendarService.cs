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
    internal class My_CalendarService : BaseService<My_Calendar>, IMy_CalendarService
    {
        public My_CalendarService(IMy_CalendarRepository repository)
        {
            _irepository = repository;
        }
    }
}
