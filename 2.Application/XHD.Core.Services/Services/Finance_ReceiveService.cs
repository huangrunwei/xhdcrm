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
    internal class Finance_ReceiveService : BaseService<Finance_Receive>, IFinance_ReceiveService
    {
        IFinance_ReceiveRepository irepositorySelf;
        public Finance_ReceiveService(IFinance_ReceiveRepository repository)
        {
            _irepository = repository;
            irepositorySelf = repository;
        }

        public async Task<JArray> ReportYear(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            return await irepositorySelf.ReportYear(expWhere);
        }

        public async Task<JArray> ReportPayType(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            return await irepositorySelf.ReportPayType(expWhere);
        }

        public async Task<JArray> ReportYearSum(Expression<Func<Finance_Receive, bool>> expWhere)
        {
            return await irepositorySelf.ReportYearSum(expWhere);
        }
    }
}
