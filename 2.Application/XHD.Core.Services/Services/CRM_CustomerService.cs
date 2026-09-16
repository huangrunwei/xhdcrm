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
    internal class CRM_CustomerService : BaseService<CRM_Customer>, ICRM_CustomerService
    {
        ICRM_CustomerRepository _irepositoryBase;

        public CRM_CustomerService(ICRM_CustomerRepository repository)
        {
            _irepository = repository;
            _irepositoryBase = repository;
        }

        public async Task<JArray> ReportYear(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            return await _irepositoryBase.ReportYear(expWhere);
        }

        public async Task<JArray> ReportIndustry(Expression<Func<CRM_Customer, bool>> expWhere)
        { 
            return  await _irepositoryBase.ReportIndustry(expWhere);
        }

        public async Task<JArray> ReportType(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            return await _irepositoryBase.ReportType(expWhere);
        }

        public async Task<JArray> ReportLevel(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            return await _irepositoryBase.ReportLevel(expWhere);
        }

        public async Task<JArray> ReportSource(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            return await _irepositoryBase.ReportSource(expWhere);
        }

        public async Task<JArray> ReportCity(Expression<Func<CRM_Customer, bool>> expWhere)
        {
            return await _irepositoryBase.ReportCity(expWhere);
        }

        public async Task<JArray> ReportProvinces(Expression<Func<CRM_Customer, bool>> expWhere)
        { 
            return await _irepositoryBase.ReportProvinces(expWhere);
        }

        public async Task<bool> LastFollow(string id)
        { 
            return await _irepositoryBase.LastFollow(id);
        }
    }
}
