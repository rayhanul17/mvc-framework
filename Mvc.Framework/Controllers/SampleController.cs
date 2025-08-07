using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mvc.framework.Models;
using mvc.framework.Services;

namespace mvc.framework.Controllers
{
    public class SampleController : GenericController<SampleEntity>
    {
        public SampleController(IGenericService<SampleEntity> service, ILogger<SampleController> logger) 
            : base(service, logger)
        {
        }

        public override async Task<IActionResult> CreateOrEdit(int? id)
        {
            ViewBag.Title = id.HasValue ? "Edit Sample Entity" : "Create Sample Entity";
            return await base.CreateOrEdit(id);
        }

        public override async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.Title = "Sample Entities";
            return await base.Index(pageNumber, pageSize);
        }
    }
}