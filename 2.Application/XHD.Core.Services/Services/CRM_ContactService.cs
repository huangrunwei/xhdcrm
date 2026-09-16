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
    internal class CRM_ContactService : BaseService<CRM_Contact>, ICRM_ContactService
    {
        public CRM_ContactService(ICRM_ContactRepository repository)
        {
            _irepository = repository;
        }
    }
}
