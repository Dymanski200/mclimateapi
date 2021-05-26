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
    public class DevicesController : Controller
    {
        readonly ApplicationContext database;
        public DevicesController(ApplicationContext context)
        {
            database = context;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(DeviceDataModel model)
        {
            if (!await database.Rooms.AnyAsync(x => x.ID == model.RoomID))
                return NotFound();

            var device = new Device
            {
                RoomID = model.RoomID,
                Name = model.Name,
                Status = "Добавлен",
            };
            database.Devices.Add(device);
            await database.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id) 
        {
            var device = await database.Devices.FirstOrDefaultAsync(x=>x.ID==id);
            if (device == null)
                return NotFound();

            database.Devices.Remove(device);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/Rules")]
        public async Task<ActionResult<IEnumerable<RuleViewModel>>> GetRules(int id)
        {
            var rules = await database.Rules.Where(x => x.DeviceID == id).ToListAsync();
            if (rules.Count == 0)
                return NotFound();

            return Ok(rules);
        }
    }
}