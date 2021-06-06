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

            var salt = HashGenerator.GetSalt(32);

            //Создаём и пишем в БД помещение
            var room = new Room
            {
                Name = model.Name,
                Code = HashGenerator.Generate(model.Code,salt),
                Salt = salt,
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
        public async Task<IActionResult> Refresh(int id, ClimateDataModel model)
        {
            //Ищем помещение
            var room = await database.Rooms.FirstOrDefaultAsync(x => x.ID == id);
            if (room == null)
                return NotFound();

            if (room.Code != HashGenerator.Generate(model.Code, room.Salt))
            {
                ModelState.AddModelError(nameof(model.Code), "Неверный код");
                return BadRequest(ModelState);
            }

            //Обновляем показатели

            if (room.Temperature != model.Temperature) 
            {
                room.Temperature = model.Temperature;
                database.Changes.Add(new Change 
                {
                    RoomID = id,
                    Message = $"Изменение температуры: {model.Temperature}°C",
                    Date = DateTime.Now
                });
            }

            if (room.Humidity != model.Humidity)
            {
                room.Humidity = model.Humidity;
                database.Changes.Add(new Change
                {
                    RoomID = id,
                    Message = $"Изменение влажности: {model.Humidity}%",
                    Date = DateTime.Now
                });
            }
            room.PreviousUpdate = DateTime.Now;


            //Инициализация списка комманд
            var commands = new List<string>();

            //Перебираем устройства
            var devices = await database.Devices.Where(x => x.RoomID == id).ToListAsync();
            foreach (Device device in devices)
            {
                var rules = await database.Rules.Where(x => x.DeviceID == device.ID).ToListAsync();
                foreach (Rule rule in rules)
                {
                    if (rule.Temperature)
                    {
                        var offset = room.Temperature - room.TargetTemperature;
                        if ((rule.Offset == 0) && (offset == 0))
                            commands.Add(rule.Command);
                        if ((rule.Offset > 0) && (offset > 0) && (Math.Abs(offset) > Math.Abs(rule.Offset)))
                            commands.Add(rule.Command);
                        if ((rule.Offset < 0) && (offset < 0) && (Math.Abs(offset) > Math.Abs(rule.Offset)))
                            commands.Add(rule.Command);
                    }
                    if (!rule.Temperature)
                    {
                        var offset = room.Humidity - room.TargetHumidity;
                        if ((rule.Offset == 0) && (offset == 0))
                            commands.Add(rule.Command);
                        if ((rule.Offset > 0) && (offset > 0) && (Math.Abs(offset) > Math.Abs(rule.Offset)))
                            commands.Add(rule.Command);
                        if ((rule.Offset < 0) && (offset < 0) && (Math.Abs(offset) > Math.Abs(rule.Offset)))
                            commands.Add(rule.Command);
                    }
                }
            }

            //Сохраняем изменения
            database.Rooms.Update(room);
            await database.SaveChangesAsync();

            var changes = await database.Changes.Where(x => x.RoomID == room.ID).OrderBy(x => x.Date).ToListAsync();
            var removes = new List<Change>();

            while (changes.Count > ServerConfiguration.HistorySize)
            {
                removes.Add(changes[0]);
                changes.Remove(changes[0]);
            }

            database.Changes.RemoveRange(removes);
            await database.SaveChangesAsync();

            //Отправляем списком комманд
            return Ok(commands);

        }

        [HttpPost("{id}/Targets")]
        [Authorize]
        public async Task<ActionResult> Targets(int id, TargetsDataModel model)
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            //Ищем помещение
            var room = await database.Rooms.FirstOrDefaultAsync(x => x.ID == id);
            if (room == null)
                return NotFound();

            if (room.Code != HashGenerator.Generate(model.Code, room.Salt))
            {
                ModelState.AddModelError(nameof(model.Code), "Неверный код");
                return BadRequest(ModelState);
            }

            if (model.TargetTemperature != room.TargetTemperature) 
            {
                room.TargetTemperature = model.TargetTemperature;
                database.Changes.Add(new Change
                {
                    RoomID = id,
                    Message = $"{user.Surname} {user.Name} {user.Patronymic} установил(а) целевую температуру на {model.TargetTemperature}°C",
                    Date = DateTime.Now
                });
            }

            if (model.TargetHumidity != room.TargetHumidity)
            {
                room.TargetHumidity = model.TargetHumidity;
                database.Changes.Add(new Change
                {
                    RoomID = id,
                    Message = $"{user.Surname} {user.Name} {user.Patronymic} установил(а) целевую влажность на {model.TargetHumidity}%",
                    Date = DateTime.Now
                });
            }

            database.Rooms.Update(room);
            await database.SaveChangesAsync();

            var changes = await database.Changes.Where(x => x.RoomID == room.ID).OrderBy(x=>x.Date).ToListAsync();
            var removes = new List<Change>();

            while (changes.Count > ServerConfiguration.HistorySize)
            {
                removes.Add(changes[0]);
                changes.Remove(changes[0]);
            }

            database.Changes.RemoveRange(removes);
            await database.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/Changes")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ChangeViewModel>>> GetChanges(int id)
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            //Ищем изменения в БД
            var changes = await database.Changes.Where(x => x.RoomID == id).OrderByDescending(x=>x.Date).ToListAsync();
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
        public async Task<ActionResult<IEnumerable<DeviceViewModel>>> GetDevices(int id)
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
