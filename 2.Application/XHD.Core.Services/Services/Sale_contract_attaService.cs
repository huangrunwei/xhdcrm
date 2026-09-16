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
internal class Sale_contract_attaService : BaseService<Sale_contract_atta>, ISale_contract_attaService
    {
        public Sale_contract_attaService(ISale_contract_attaRepository repository)
        {
            _irepository = repository;
        }
    }
}
