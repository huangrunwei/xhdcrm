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
internal class Sale_order_detailsService : BaseService<Sale_order_details>, ISale_order_detailsService
    {
        public Sale_order_detailsService(ISale_order_detailsRepository repository)
        {
            _irepository = repository;
        }
    }
}
