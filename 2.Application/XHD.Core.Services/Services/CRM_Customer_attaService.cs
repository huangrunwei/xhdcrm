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
    internal class CRM_Customer_attaService : BaseService<CRM_Customer_atta>, ICRM_Customer_attaService
    {
        public CRM_Customer_attaService(ICRM_Customer_attaRepository repository)
        {
            _irepository = repository;
        }
    }
}
