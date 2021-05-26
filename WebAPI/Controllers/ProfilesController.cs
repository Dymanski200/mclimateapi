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
    public class ProfilesController : Controller
    {
        readonly ApplicationContext database;
        public ProfilesController(ApplicationContext context)
        {
            database = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(ProfileDataModel model)
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            if (await database.Profiles.AnyAsync(x => x.UserID == user.ID && x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Название уже используется");
                return BadRequest(ModelState);
            }

            var profile = new Profile
            {
                UserID = user.ID,
                Name = model.Name,
                TargetTemperature = model.TargetTemperature,
                TargetHumidity = model.TargetHumidity
            };

            database.Profiles.Add(profile);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Post(int id) 
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            var profile = await database.Profiles.FirstOrDefaultAsync(x=>x.ID==id);
            if (profile == null)
                return NotFound();

            if (profile.UserID != user.ID)
                return Forbid();

            database.Remove(profile);
            await database.SaveChangesAsync();

            return Ok();
        }
    }
}