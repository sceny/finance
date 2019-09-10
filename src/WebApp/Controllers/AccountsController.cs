using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {

        [HttpGet]
        public IEnumerable<Account> Get() => MockData.GetAccounts();
    }
}
