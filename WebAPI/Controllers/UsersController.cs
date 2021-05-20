using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.ViewModels;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        readonly ApplicationContext database;
        public UsersController(ApplicationContext context)
        {
            database = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> Get()
        {
            //Ищем пользователей в БД
            var users = await database.Users.ToListAsync();
            if (users.Count == 0)
                return NotFound();

            //Преобразуем и отправляем
            var result = new List<UserViewModel>();
            foreach (User user in users)
            {
                result.Add(new UserViewModel(user));
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            //Ищем пользователя в БД
            var user = await database.Users.FirstOrDefaultAsync(x=>x.ID==id);
            if (user == null)
                return NotFound();

            //Удаляем пользователя и отправляем Ок
            database.Users.Remove(user);
            await database.SaveChangesAsync();
            return Ok();
        }
    }
}
