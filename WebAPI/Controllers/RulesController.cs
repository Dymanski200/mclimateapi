using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.DataModels;
using WebAPI.Models;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RulesController : Controller
    {
        readonly ApplicationContext database;
        public RulesController(ApplicationContext context)
        {
            database = context;
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(RuleDataModel model)
        {
            if (!await database.Devices.AnyAsync(x => x.ID == model.DeviceID))
                return NotFound();

            var rule = new Rule
            {
                DeviceID = model.DeviceID,
                Temperature = model.Temperature,
                Offset = model.Offset,
                Command = model.Command,
                Status = model.Status
            };

            database.Rules.Add(rule);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var rule = await database.Rules.FirstOrDefaultAsync(x=>x.ID==id);
            if (rule == null)
                return NotFound();

            database.Rules.Remove(rule);
            await database.SaveChangesAsync();
            return Ok();
        }

    }
}