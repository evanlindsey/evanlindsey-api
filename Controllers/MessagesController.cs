using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using EvanLindseyApi.Models;

namespace EvanLindseyApi.Controllers
{
    public class MessageData
    {
        public string User { get; set; }
        public string Text { get; set; }
    }

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly DataContext _context;

        public MessagesController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var list = new List<MessageData>();

            var messages = _context.Messages.Include(x => x.User).ToList();
            foreach (Message message in messages)
            {
                list.Add(
                    new MessageData
                    {
                        User = message.User.FirstName + " " + message.User.LastName,
                        Text = message.Text
                    }
                );
            }

            return Ok(list);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody] MessageData message)
        {
            string id = HttpContext.User.Claims.First().Value;

            _context.Messages.Add(
                new Message
                {
                    UserId = Convert.ToInt32(id),
                    Text = message.Text
                }
            );
            _context.SaveChanges();

            var user = _context.Users.SingleOrDefault(x => x.Id.ToString() == id);
            message.User = user.FirstName + " " + user.LastName;

            return Ok(message);
        }
    }
}
