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
    internal class Sale_orderService : BaseService<Sale_order>, ISale_orderService
    {
        protected ISale_orderRepository repositorySelf;
        public Sale_orderService(ISale_orderRepository repository)
        {
            _irepository = repository;
            repositorySelf = repository;
        }

        /// <summary>
        /// ¸üÐÂÓà¶î
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> UpdateArrearsMoney(string id)
        {
            Expression<Func<Sale_order, Sale_order>> expData = a => new Sale_order
            {
                arrears_money = a.total_amount - a.receive_money
            };

            Expression<Func<Sale_order, bool>> expID = a => a.id == id;

            return await _irepository.UpdateAsync(expData, expID);
        }

        public void UpdateOrderInvoice(string order_id)
        {
            repositorySelf.UpdateOrderInvoice(order_id);
        }

        public void UpdateOrderReceive(string order_id)
        {
            repositorySelf.UpdateOrderReceive(order_id);
        }

        public async Task<JArray> ReportYear(Expression<Func<Sale_order, bool>> expWhere)
        {
            return await repositorySelf.ReportYear(expWhere);
        }

        public async Task<JArray> ReportStatus(Expression<Func<Sale_order, bool>> expWhere)
        {
            return await repositorySelf.ReportStatus(expWhere);
        }

        public async Task<JArray> ReportPayType(Expression<Func<Sale_order, bool>> expWhere)
        {
            return await repositorySelf.ReportPayType(expWhere);
        }

        public async Task<JArray> ReportYearSum(Expression<Func<Sale_order, bool>> expWhere)
        {
            return await repositorySelf.ReportYearSum(expWhere);
        }
    }
}
