﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Anoroc_User_Management.Interfaces;
using Anoroc_User_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nancy.Json;
using Newtonsoft.Json;

namespace Anoroc_User_Management.Controllers
{
    //TODO: 
    //change return types of endpoints to RESTful return types
    [Route("[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly string XamarinKey;
        IUserManagementService UserManagementService;
        public UserManagementController(IUserManagementService userManagementService, IConfiguration configurationManager)
        {
            XamarinKey = configurationManager["XamarinKey"];
            UserManagementService = userManagementService;
        }

        [HttpPost("DownloadUserData")]
        public IActionResult DownloadUserData([FromBody] Token token)
        {
            if(UserManagementService.ValidateUserToken(token.access_token))
            {
                if (UserManagementService.SendData(token.access_token))
                    return Ok("Email Sent.");
                else
                    return Ok("Failed to send.");
            }
            else
            {
                return Unauthorized("Unauthorized");
            }
        }

        [HttpPost("CarrierStatus")]
        public IActionResult CarrierStatus([FromBody] Token token_object)
        {
            if (UserManagementService.ValidateUserToken(token_object.access_token))
            {
                UserManagementService.UpdateCarrierStatus(token_object.access_token, token_object.Object_To_Server);
                var returnString = token_object.Object_To_Server + "";
                return Ok(returnString);
            }
            else
            {
                return Unauthorized("Unauthroized accessed");
            }

        }

        [HttpPost("UploadProfilePhoto")]
        public IActionResult UploadProfilePhoto([FromBody] Token token)
        {
            if (UserManagementService.ValidateUserToken(token.access_token))
            {
                try
                {
                    var image = token.Object_To_Server;
                    UserManagementService.SaveProfileImage(token.access_token, image);
                    return Ok("Ok");
                }
                catch(Exception)
                {
                    return BadRequest("Invalid request");
                }
            }
            else
                return Unauthorized("Invalid token");
        }

        [HttpPost("GetUserProfilePicture")]
        public IActionResult GetUserProfilePicture([FromBody] Token token)
        {
            if(UserManagementService.ValidateUserToken(token.access_token))
            {
                return Ok(UserManagementService.GetProfileImage(token.access_token));
            }
            else
                return Unauthorized("Invalid request");
        }

        [HttpPost("FirebaseToken")]
        public IActionResult FirebaseToken([FromBody] Token token_object)
        {
            if (UserManagementService.ValidateUserToken(token_object.access_token))
            {
                UserManagementService.InsertFirebaseToken(token_object.access_token, token_object.Object_To_Server);

                var returnString = token_object.Object_To_Server + "";
                return Ok(returnString);

            }
            else
            {
                return Unauthorized("Unauthroized accessed");
            }
        }

        [HttpPost("UserIncidents")]
        public IActionResult UserIncidents([FromBody] Token token)
        {
            try
            {
                if(UserManagementService.ValidateUserToken(token.access_token))
                {
                    return Ok(UserManagementService.GetUserIncidents(token.access_token).ToString());
                }
                else
                {
                    return Unauthorized("Unauthorized");
                }
            }
            catch (Exception)
            {
                return BadRequest("Invalid request");
            }
        }
        [HttpPost("GetEmailNotificatoin")]
        public IActionResult GetEmailNotificatoin([FromBody] Token token)
        {
            if(UserManagementService.ValidateUserToken(token.access_token))
            {
                try
                {
                    var result = UserManagementService.GetEmailNotificationSettings(token.access_token);
                    return Ok(result.ToString());
                }
                catch(Exception)
                {
                    return BadRequest();
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("SetEmailNotification")]
        public IActionResult SetEmailNotification([FromBody] Token token)
        {
            try
            {
                if(UserManagementService.ValidateUserToken(token.access_token))
                {
                    var value = Convert.ToBoolean(token.Object_To_Server);
                    var result = UserManagementService.SetEmailNotificationSettings(token.access_token, value);
                    return Ok(result.ToString());
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch(FormatException)
            {
                return BadRequest();
            }
        }

        [HttpPost("ToggleAnonmity")]
        public IActionResult ToggleAnonmity([FromBody] Token token)
        {
            try
            { 
                if (UserManagementService.ValidateUserToken(token.access_token))
                {
                    var value = Convert.ToBoolean(token.Object_To_Server);
                    var result = UserManagementService.ToggleUserAnonomity(token.access_token, value);
                    if (result)
                        return Ok("Anonymous");
                    else
                        return Ok("Not Anonymous");
                }
                else
                {
                    return Unauthorized("Unauthorized.");
                }
            }
            catch(FormatException)
            {
                return BadRequest();
            }
        }

        [HttpPost("CompletelyDeleteUser")]
        public IActionResult CompletelyDeleteUser([FromBody] Token token)
        {
            if(UserManagementService.ValidateUserToken(token.access_token))
            {
                UserManagementService.CompletelyDeleteUser(token.access_token);
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("UserLoggedIn")]
        public IActionResult UserLoggedIn([FromBody] Token token)
        {
            try
            {
                if (Request.Headers.ContainsKey("X-XamarinKey"))
                {
                    if (UserManagementService.CheckXamarinKey(Request.Headers["X-XamarinKey"]))
                    {
                        User user = JsonConvert.DeserializeObject<User>(token.Object_To_Server);
                        var userToken = UserManagementService.UserAccessToken(user.Email);
                        if (userToken != null)
                        {
                            return Ok(userToken);
                        }
                        else
                        {
                            string custom_token = UserManagementService.addNewUser(user);
                            return Ok(custom_token);
                        }
                    }
                    else
                    {
                        return Unauthorized("Unauthorized");
                    }
                }
                else
                {
                    return Unauthorized("Unauthorized");
                }
            }
            catch(Exception e)
            {
                return BadRequest("Invalid object");
            }
        }
    }
}
