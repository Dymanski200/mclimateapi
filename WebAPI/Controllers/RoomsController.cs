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
    public class RoomsController : Controller
    {
        readonly ApplicationContext database;
        public RoomsController(ApplicationContext context)
        {
            database = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {
            //Ищем помещения в БД
            var rooms = await database.Rooms.ToListAsync();
            if (rooms.Count == 0)
                return NotFound();

            //Преобразуем и отправляем
            var result = new List<RoomViewModel>();
            foreach (Room room in rooms)
            {
                result.Add(new RoomViewModel(room));
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RoomViewModel>> Get(int id)
        {
            //Ищем помещение в БД
            var room = await database.Rooms.FirstOrDefaultAsync(x => x.ID == id);
            if (room == null)
                return NotFound();

            //Преобразуем и отправляем
            return Ok(new RoomViewModel(room));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(RoomDataModel model)
        {
            //Ищем помещение в БД
            if (await database.Rooms.AnyAsync(x => x.Name == model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Название уже используется");
                return BadRequest(ModelState);
            }

            //Создаём и пишем в БД помещение
            var room = new Room
            {
                Name = model.Name,
                Code = model.Code,
                Temperature = 0,
                Humidity = 0,
                TargetTemperature = 0,
                TargetHumidity = 0,
                PreviousUpdate = DateTime.Now
            };
            database.Rooms.Add(room);
            await database.SaveChangesAsync();

            //Отправляем Ок
            return Ok();
        }

        [HttpPost("{id}/Refresh")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Refresh(int id, ClimateDataModel model) 
        {
            //Ищем помещение
            var room = await database.Rooms.FirstOrDefaultAsync(x => x.ID == id);
            if (room == null)
                return NotFound();

            //Обновляем показатели
            room.Temperature = model.Temperature;
            room.Humidity = model.Humidity;

            //Инициализация списка комманд
            var commands = new List<string>();

            //Перебираем устройства
            var devices = await database.Devices.Where(x=>x.RoomID == id).ToListAsync();
            foreach (Device device in devices)
            { 
                var rules = await database.Rules.Where(x => x.DeviceID == device.ID).ToListAsync();
                foreach (Rule rule in rules)
                {
                    var offset = room.TargetTemperature - room.Temperature;
                    if (Math.Sign(rule.Offset) == -1)
                    {
                        if(Math.Abs(offset) < Math.Abs(rule.Offset))
                        {
                            device.Status = rule.Status;
                            database.Devices.Update(device);
                            commands.Add(rule.Command);
                        }
                    }
                    if (Math.Sign(rule.Offset) == 1)
                    {
                        if (Math.Abs(offset) > Math.Abs(rule.Offset))
                        {
                            device.Status = rule.Status;
                            database.Devices.Update(device);
                            commands.Add(rule.Command);
                        }
                    }
                }
            }

            //Сохраняем изменения
            database.Rooms.Update(room);
            await database.SaveChangesAsync();

            //Отправляем списком комманд
            return Ok(commands);

        }

        [HttpGet("{id}/Changes")]
        [Authorize]
        public async Task<ActionResult<ChangeViewModel>> GetChanges(int id)
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            //Ищем изменения в БД
            var changes = await database.Changes.Where(x => x.RoomID == id).ToListAsync();
            if (changes.Count == 0)
                return NotFound();

            //Преобразуем и отправляем
            var result = new List<ChangeViewModel>();
            foreach (Change change in changes)
            {
                result.Add(new ChangeViewModel(change));
            }
            return Ok(result);
        }

        [HttpGet("{id}/Devices")]
        [Authorize]
        public async Task<ActionResult<ChangeViewModel>> GetDevices(int id)
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            //Ищем устройства в БД
            var devices = await database.Devices.Where(x => x.RoomID == id).ToListAsync();
            if (devices.Count == 0)
                return NotFound();

            //Преобразуем и отправляем
            var result = new List<DeviceViewModel>();
            foreach (Device device in devices)
            {
                result.Add(new DeviceViewModel(device));
            }
            return Ok(result);
        }

        [HttpPost("{id}/Devices")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PostDevice(int id, DeviceDataModel model)
        { 

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            //Ищем помещение в БД
            var room = await database.Rooms.FirstOrDefaultAsync(x => x.ID == id);
            if (room == null)
                return NotFound();

            //Удаляем помещение и отправляем Ок
            database.Rooms.Remove(room);
            await database.SaveChangesAsync();
            return Ok();
        }
    }
}
