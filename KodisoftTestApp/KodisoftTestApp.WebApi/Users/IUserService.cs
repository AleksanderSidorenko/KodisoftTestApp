using KodisoftTestApp.WebApi.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace TasksWebApi.Controllers.Users
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        protected IMongoDatabase _database;
        private IMongoCollection<User> _usersDb;
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { Id = Guid.Parse("66fc7461-674c-4bde-bc03-83100cbae41b"), Username = "johndoe", Password = "john" },
            new User { Id = Guid.Parse("7730142e-a70c-4e4c-80da-937920e0f47e"), Username = "janedoe", Password = "jane" }
        };

        private readonly AppSettings _appSettings;

        public UserService(IMongoDatabase database, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _usersDb = _database.GetCollection<User>("Users");
        }

        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            _usersDb.ReplaceOne(filter, user);

            // remove password before returning
            user.Password = null;

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            return _users.Select(x =>
            {
                x.Password = null;
                return x;
            });
        }
    }
}