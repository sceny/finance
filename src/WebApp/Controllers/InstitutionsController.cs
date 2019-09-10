using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstitutionsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Institution> Get() => MockData.GetInstitutions();
    }
}
