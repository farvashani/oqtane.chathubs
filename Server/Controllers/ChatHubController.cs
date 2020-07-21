using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.StreamHubs.Models;
using Oqtane.StreamHubs.Repository;

namespace Oqtane.StreamHubs.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ChatHubController : Controller
    {
        private readonly IChatHubRepository _Blogs;
        private readonly ILogManager _logger;

        public ChatHubController(IChatHubRepository Blogs, ILogManager logger)
        {
            _Blogs = Blogs;
            _logger = logger;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<ChatHub> Get(string moduleid)
        {
            return _Blogs.GetBlogs(int.Parse(moduleid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public ChatHub Get(int id)
        {
            return _Blogs.GetBlog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public ChatHub Post([FromBody] ChatHub Blog)
        {
            if (ModelState.IsValid)
            {
                Blog = _Blogs.AddBlog(Blog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Blog Added {ChatHub}", Blog);
            }
            return Blog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public ChatHub Put(int id, [FromBody] ChatHub Blog)
        {
            if (ModelState.IsValid)
            {
                Blog = _Blogs.UpdateBlog(Blog);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Blog Updated {ChatHub}", Blog);
            }
            return Blog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _Blogs.DeleteBlog(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Blog Deleted {ChatHubId}", id);
        }
    }
}
