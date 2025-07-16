using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        // get hold of refresh token from cookie
        var refreshToken = Request.Cookies["refreshToken"];

        // timer - every 5 min call this endpoint to refresh the token if the user is using the app
        // but if they log out - there shouldn't be a toast popup saying that they are unauthorized
        if (refreshToken == null) return NoContent();

        // check the token against the token in db
        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null) return Unauthorized();

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user); // store in db

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // = this cookie is NOT accessible from any kind of JS, including client app
            Secure = true, // = this cookie is only going to be sent over https => use https for dev
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // after 7 days this cookie gets removed from the browser
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions); // need to update cors policy
    }
}
