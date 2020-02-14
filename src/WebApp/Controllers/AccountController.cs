using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Management;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class AccountController : ControllerBase
    {
        private readonly ManagementContext _context;

        public AccountController(ManagementContext context)
            => _context = context ?? throw new System.ArgumentNullException(nameof(context));

        [HttpGet]
        public IAsyncEnumerable<Account> GetAsync() => _context.Accounts.AsAsyncEnumerable();
    }
}