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
        [HttpPost]
        public IActionResult Post([FromBody]Account account)
        {
            if (!ModelState.IsValid || account == null)
                return BadRequest();
            MockData.AddAccount(account);
            return Ok(account);
        }

        [HttpPut]
        public IActionResult Put([FromBody]Account account)
        {
            if (!ModelState.IsValid || account == null)
                return BadRequest();
            if (!MockData.TryUpdateAccount(account))
                return NotFound();
            return Ok(account);
        }
    }
}
