using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.Services
{
    internal class Sale_order_followService : BaseService<Sale_order_follow>, ISale_order_followService
    {
        public Sale_order_followService(ISale_order_followRepository repository)
        {
            _irepository = repository;
        }
    }
}
