using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mvc.framework.Models;
using mvc.framework.Services;

namespace mvc.framework.Controllers
{
    public class BaseController<T> : Controller where T : BaseEntity
    {
        protected readonly BaseService<T> _service;
        public BaseController(BaseService<T> service)
        {
            _service = service;
        }
    }
}
