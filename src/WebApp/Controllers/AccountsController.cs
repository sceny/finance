using System.Collections.Generic;
using System.Linq;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (account == null)
                return BadRequest("The account must have a value.");
            MockData.AddAccount(account);
            return Ok(account);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Account account)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (account == null)
                return BadRequest("The account must have a value.");
            if (!MockData.TryUpdateAccount(id, account))
                return NotFound();
            return Ok(account);
        }
    }
}
