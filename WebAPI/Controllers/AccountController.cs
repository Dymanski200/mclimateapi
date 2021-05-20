using Microsoft.AspNetCore.Authorization;
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
    public class AccountController : Controller
    {
        readonly ApplicationContext database;
        public AccountController(ApplicationContext context)
        {
            database = context;
        }

        [HttpPost("Registration")]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(RegistrationDataModel model)
        {
            //Приводим email
            model.Email = model.Email.ToLower().Replace(" ",String.Empty);

            //Проверяем наличие пользователя с таким email в БД 
            if (await database.Users.AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует");
                return BadRequest(ModelState);
            }

            //Проверяем сходство паролей
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли не совпадают");
                return BadRequest(ModelState);
            }

            //Создаём код активации
            var code = CodeGenerator.Generate(6);

            //Создаём объект пользователя
            var user = new User
            {
                Email = model.Email,
                Password = HashGenerator.Generate(model.Password),
                Surname = model.Surname,
                Name = model.Name,
                Patronymic = model.Patronymic,
                ActivationStatus = false,
                ConfirmationCode = HashGenerator.Generate(code),
                RegistrationDate = DateTime.UtcNow,
                RefreshToken = string.Empty,
                Role = await database.Users.AnyAsync() ? "user" : "admin"
            };

            //Отправляем письмо
            try
            {
                await EmailService.SendEmailAsync(model.Email, "Подтверждение", $"Ваш код подтверждения: <strong>{code}</strong><br>Если Вы не запрашивали код, то просто проигнорируйте это сообщение.");
            }
            catch
            {
                return StatusCode(500);
            }

            //Добавляем пользователя в БД
            database.Users.Add(user);
            await database.SaveChangesAsync();

            //Отправляем Ок
            return Ok();
        }

        [HttpPost("Activation")]
        [AllowAnonymous]
        public async Task<ActionResult<TokensViewModel>> Activation(ActivationDataModel model)
        {
            //Приводим email
            model.Email = model.Email.ToLower().Replace(" ", String.Empty);

            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email не существует");
                return BadRequest(ModelState);
            }

            //Проверяем, подтверждён ли аккаунт
            if (user.ActivationStatus == true)
                return Forbid();

            //Проверяем совпадение хешей кодов подтверждения
            if (user.ConfirmationCode != HashGenerator.Generate(model.Code.ToUpper()))
            {
                ModelState.AddModelError(nameof(model.Code), "Неверный код");
                return BadRequest(ModelState);
            }

            //Подтверждаем
            user.ConfirmationCode = string.Empty;
            user.ActivationStatus = true;

            //Создаём claims с данными пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //Создаём новые токены
            var tokens = new TokensViewModel
            {
                AccessToken = TokenService.GenerateAccessToken(claims),
                RefreshToken = TokenService.GenerateRefreshToken()
            };

            //Обновляем пользователя в БД
            user.RefreshToken = tokens.RefreshToken;
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем токены
            return Ok(tokens);
        }

        [HttpPost("Recovery")]
        [AllowAnonymous]
        public async Task<ActionResult<TokensViewModel>> Recovery(RecoveryDataModel model)
        {
            //Приводим email
            model.Email = model.Email.ToLower().Replace(" ", String.Empty);

            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email не существует");
                return BadRequest(ModelState);
            }

            //Проверяем сходство хэшей кодов
            if (user.ConfirmationCode != HashGenerator.Generate(model.Code.ToUpper()))
            {
                ModelState.AddModelError(nameof(model.Code), "Неверный код");
                return BadRequest(ModelState);
            }

            //Проверяем сходство паролей
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли не совпадают");
                return BadRequest(ModelState);
            }

            //Создаём claims с данными пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //Создаём новые токены
            var tokens = new TokensViewModel
            {
                AccessToken = TokenService.GenerateAccessToken(claims),
                RefreshToken = TokenService.GenerateRefreshToken()
            };

            //Меняем пароль и токен обновления
            user.Password = HashGenerator.Generate(model.Password);
            user.RefreshToken = tokens.RefreshToken;

            //Обновляем пользователя в БД
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем токены
            return Ok(tokens);
        }

        [HttpPost("Code")]
        [AllowAnonymous]
        public async Task<IActionResult> Code(EmailDataModel model)
        {
            //Приводим email
            model.Email = model.Email.ToLower().Replace(" ", String.Empty);

            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email не существует");
                return BadRequest(ModelState);
            }

            //Создаём код подтверждения
            var code = CodeGenerator.Generate(6);

            //Отправляем письмо
            try
            {
                await EmailService.SendEmailAsync(model.Email, "Подтверждение", $"Ваш код подтверждения: <strong>{code}</strong><br>Если Вы не запрашивали код, то просто проигнорируйте это сообщение.");
            }
            catch
            {
                return StatusCode(500);
            }

            //Обновляем пользователя в БД
            user.ConfirmationCode = HashGenerator.Generate(code);
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем Ок
            return Ok();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokensViewModel>> Login(LoginDataModel model)
        {
            //Приводим email
            model.Email = model.Email.ToLower().Replace(" ", String.Empty);

            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email не существует");
                return BadRequest(ModelState);
            }

            //Проверяем сходство хэшей паролей
            if (user.Password != HashGenerator.Generate(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Неверный пароль");
                return BadRequest(ModelState);
            }

            //Проверяем, подтверждён ли аккаунт
            if (user.ActivationStatus == false)
                return Forbid();

            //Создаём claims с данными пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //Создаём новые токены
            var tokens = new TokensViewModel
            {
                AccessToken = TokenService.GenerateAccessToken(claims),
                RefreshToken = TokenService.GenerateRefreshToken()
            };

            //Обновляем пользователя в БД
            user.RefreshToken = tokens.RefreshToken;
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем токены
            return Ok(tokens);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokensViewModel>> Refresh(TokensDataModel model)
        {
            //Вытаскиваем email из токена
            var email = TokenService.GetPrincipalFromExpiredToken(model.AccessToken).Claims.FirstOrDefault(x=>x.Type==ClaimTypes.Email).Value;

            //Ищем пользователя в БД
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized();

            //Сравниваем токены
            if (user.RefreshToken != model.RefreshToken)
                return Unauthorized();

            //Создаём claims с данными пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //Создаём новые токены
            var tokens = new TokensViewModel
            {
                AccessToken = TokenService.GenerateAccessToken(claims),
                RefreshToken = TokenService.GenerateRefreshToken()
            };

            //Обновляем пользователя в БД
            user.RefreshToken = tokens.RefreshToken;
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем токены
            return Ok(tokens);
        }

        [HttpHead("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            //Ищем пользователя с таким email в БД 
            var user = await database.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirst(ClaimTypes.Email).Value);
            if (user == null)
                return Unauthorized();

            //Чистим токен обновления
            user.RefreshToken = string.Empty;

            //Обновляем пользователя в БД
            database.Users.Update(user);
            await database.SaveChangesAsync();

            //Отправляем Ок
            return Ok();
        }
    }
}
