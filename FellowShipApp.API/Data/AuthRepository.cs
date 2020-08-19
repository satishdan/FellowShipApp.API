using FellowShipApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FellowShipApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext context;

        public AuthRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == username);

            if(user == null)
            {
                return null;
            }
            else
            {
                if(!VerifyPassword(password, user.PasswordHash, user.PasswordKey))
                {
                    return null;
                }

            }

            return user;

        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordKey)
        {
            using (var hmac = new HMACSHA512(passwordKey))
            {
                //passwordKey = hmac.Key;
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for(int i=0; i<computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) { return false; }
                }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordKey;
            CreatePasswordHash(password, out passwordHash, out passwordKey);
            user.PasswordHash = passwordHash;
            user.PasswordKey = passwordKey;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordKey)
        {
            using (var hmac =  new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            var users = context.Users;
            if( await users.AnyAsync(c=> c.UserName== username))
            {
                return true;
            }

            return false;
        }
    }
}
