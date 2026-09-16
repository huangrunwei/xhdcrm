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
    internal class CRM_Customer_BathService : BaseService<CRM_Customer_Bath>, ICRM_Customer_BathService
    {
        public CRM_Customer_BathService(ICRM_Customer_BathRepository repository)
        {
            _irepository = repository;
        }
    }
}
