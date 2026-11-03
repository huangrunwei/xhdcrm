using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.Services
{
    internal class Crm_KnowledgeService : BaseService<Crm_Knowledge>, ICrm_KnowledgeService
    {
        public Crm_KnowledgeService(ICrm_KnowledgeRepository repository)
        {
            _irepository = repository;
        }
    }
}
