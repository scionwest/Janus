using Janus.SampleApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Janus.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController(AppDbContext context) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public AppDbContext Context { get; }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entities = await this.Context.Users.Include(user => user.Tasks).ToArrayAsync();
            return base.Ok(entities);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var entity = await this.Context.Users.FirstAsync(user => user.Id == id);
            return base.Ok(entity);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserEntity entity)
        {
            entity.Id = Guid.NewGuid();
            this.Context.Users.Add(entity);
            await this.Context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.Id);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await this.Context.Users.FirstAsync(user => user.Id == id);
            this.Context.Users.Remove(entity);
            await this.Context.SaveChangesAsync();

            return base.Ok();
        }
    }
}
