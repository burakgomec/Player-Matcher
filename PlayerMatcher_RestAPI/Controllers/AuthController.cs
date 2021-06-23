﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlayerMatcher_RestAPI.Model;
using System.Linq;

namespace PlayerMatcher_RestAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static Dictionary<Guid, string> tokens = new Dictionary<Guid, string>();

        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> SignUp([FromBody] Account acc)
        {
            if (ReferenceEquals(acc.email, null) || ReferenceEquals(acc.password, null) || ReferenceEquals(acc.username, null) || acc.password.Length < 6)
                   return BadRequest();

            var uuid = Guid.NewGuid();
            var account = new Account(uuid, acc.email, acc.password, acc.username);
            string dbFeedback = DatabaseOperations.shared.SaveAccountToDB(account);

            if (dbFeedback.Equals("false")) 
                return Problem(title: "Girdiginiz bilgiler veri tabanında yer almaktadır, lutfen bilgilerinizi kontrol ediniz");                    

            if(dbFeedback.Equals("error"))
                return Problem(title: "Kullanici hesabiniz yaratılırken bir hata meydana geldi");

            //Kullanıcı sisteme kayıt oldu ise kullanıcıya bir oyuncu hesabı oluşturulup veri tabanına kayıt ediliyor
            bool feedback = DatabaseOperations.shared.SavePlayerToDB(account);

            if(!feedback)
            {
                return Problem(title: "Oyuncu hesabınız yaratılırken bir hata meydana geldi");
            }

            string token = DatabaseOperations.shared.Encypting(acc.username);
            tokens.Add(acc.id, token);

            return Ok(new { title = "Hesap basariyla olusturuldu" });
        }

        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> SignIn([FromBody] Account acc)
        {
            if (ReferenceEquals(acc.email, null) || ReferenceEquals(acc.password, null) || ReferenceEquals(acc.username, null) || acc.password.Length < 6)
                return BadRequest();

            Account account = new Account(Guid.NewGuid(), acc.email,acc.password,acc.username);
            if (DatabaseOperations.shared.CheckAccountFromDB(account))
            {
                return Ok(new { title = "Basari ile giris yaptiniz" });
            }
            else
            {
                return Problem(title: "Girdiginiz bilgiler hatalidir"); 
            }
        }

        [HttpGet("match")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<bool> MatchToPlayer()
        {
            return true;
        }

        [HttpPut("update")] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<bool> UpdateUserInfo([FromBody] Account acc, string userName)
        {
            if (ReferenceEquals(acc.email, null) || ReferenceEquals(acc.password, null) || ReferenceEquals(acc.username, null) || acc.password.Length < 6)
                return BadRequest();

            Account account = new Account(Guid.NewGuid(), acc.email, acc.password, acc.username);

            if (DatabaseOperations.shared.CheckAccountFromDB(account))
            {
                account.username = userName;
                if (DatabaseOperations.shared.UpdateUsername(account))
                {
                    return Ok( new { title = " Kullanıcı adı güncelleme işlemi başarılı" } );
                }
            }
            else
            {
                return Problem(title: "Girdiginiz bilgiler hatalidir");
            }

            return true;
        }



        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<string> LogOut([FromBody]string username, string token)   
        {
            

            if (ReferenceEquals(username, null))
                return BadRequest();
  
            var player = DatabaseOperations.shared.FindPlayer(username);

            if (ReferenceEquals(player, null))
                return NotFound();

            if (!tokens.Any(x => x.Key == player.id && x.Value == token))
                return Unauthorized();



                player.status = false;
            var control = DatabaseOperations.shared.UpdatePlayerStats(player);

            if (!control)
                return Problem(title: "Çıkış yapılırken bir hata meydana geldi");

            //player'ın id si ile eşleşen Guid-string'i siler;
            tokens.Remove(player.id);

            return Ok();
        }

        [HttpGet("level")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<bool> IncreaseUserLevel()
        {
            return true;
        }


        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<bool> DeleteUser()
        {
            return true;
        }


    }
}

