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
internal class Finance_InvoiceService : BaseService<Finance_Invoice>, IFinance_InvoiceService
    {

        IFinance_InvoiceRepository _InvoiceRepository;
        public Finance_InvoiceService(IFinance_InvoiceRepository repository)
        {
            _irepository = repository;
            _InvoiceRepository= repository;
        }

        public async Task<JArray> ReportYear(Expression<Func<Finance_Invoice, bool>> expWhere)
        { 
            return await _InvoiceRepository.ReportYear(expWhere);
        }

        public async Task<JArray> ReportYearSum(Expression<Func<Finance_Invoice, bool>> expWhere)
        { 
            return await _InvoiceRepository.ReportYearSum(expWhere);
        }

        public async Task<JArray> ReportInvoiceType(Expression<Func<Finance_Invoice, bool>> expWhere)
        { 
            return await _InvoiceRepository.ReportInvoiceType(expWhere);
        }
    }
}
