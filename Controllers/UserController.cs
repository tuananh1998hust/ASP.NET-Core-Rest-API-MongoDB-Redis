using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using mvc.Models;
using mvc.Services;

namespace mvc.Controllers
{
    [Route("api/v1/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMongoCollection<User> _users;
        private readonly IDatabase _client;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
            _users = new DBService().Users;
            _client = new RedisService().RedisClient;
        }

        // GET /api/v1/User
        // Get all users
        [HttpGet]
        public ActionResult<Dictionary<string, List<User>>> Get()
        {
            try
            {
                List<User> users = _users.Find(u => true).ToList();

                Dictionary<string, List<User>> res = new Dictionary<string, List<User>>();
                res.Add("users", users);

                return res;
            }
            catch (Exception error)
            {
                System.Console.WriteLine("GET /api/v1/User Error: ", error.ToString());
                Dictionary<string, string> resErr = new Dictionary<string, string>();
                resErr.Add("error", "Bad Request");
                return BadRequest(resErr);
            }
        }

        // GET /api/v1/User/:id
        // Get User By ID
        [HttpGet("{id}")]
        public ActionResult<Dictionary<string, User>> GetByID(string id = "")
        {
            try
            {
                if (id == "")
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "Invalid params");
                    return BadRequest(error);
                }

                List<User> users = _users.Find(u => u.id == id).ToList();

                if (users.Count() <= 0)
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "User not found");
                    return BadRequest(error);
                }

                Dictionary<string, User> res = new Dictionary<string, User>();
                res.Add("data", users[0]);

                return res;
            }
            catch (Exception error)
            {
                System.Console.WriteLine("GET /api/v1/User/:id Error: ", error.ToString());
                Dictionary<string, string> resErr = new Dictionary<string, string>();
                resErr.Add("error", "Bad Request");
                return BadRequest(resErr);
            }
        }


        // POST /api/v1/User
        // Create new user
        [HttpPost]
        public ActionResult<Dictionary<string, User>> Post(string name = "", int age = 0, string phone = "")
        {
            try
            {
                if (name == "" || age == 0 || phone == "")
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "Invalid params");
                    return BadRequest(error);
                }

                List<User> users = _users.Find(u => u.phone == phone).ToList();

                if (users.Count() > 0)
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "User is exist");
                    return BadRequest(error);
                }

                User user = new User();
                user.name = name;
                user.age = age;
                user.phone = phone;
                user.id = ObjectId.GenerateNewId().ToString();

                // Save to Cache
                Dictionary<string, string> cacheData = new Dictionary<string, string>();
                cacheData.Add("type", "0");
                cacheData.Add("value", JsonConvert.SerializeObject(user));
                _client.ListRightPush("keyBulkWrite", JsonConvert.SerializeObject(cacheData));

                Dictionary<string, User> res = new Dictionary<string, User>();
                res.Add("user", user);

                return res;
            }
            catch (Exception error)
            {
                System.Console.WriteLine("POST /api/v1/User Error: ", error.ToString());
                Dictionary<string, string> resErr = new Dictionary<string, string>();
                resErr.Add("error", "Bad Request");
                return BadRequest(resErr);
            }
        }

        // PUT /api/v1/User
        // Update user
        [HttpPut("{id}")]
        public ActionResult<Dictionary<string, User>> Update(string id = "", string name = "", int age = 0, string phone = "")
        {
            try
            {
                if (id == "")
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "Invalid params");
                    return BadRequest(error);
                }

                List<User> users = _users.Find(u => u.id == id).ToList();

                if (users.Count() <= 0)
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "User not found");
                    return BadRequest(error);
                }

                User user = new User();
                user.id = users[0].id;
                if (name != "")
                {
                    user.name = name;
                }
                else
                {
                    user.name = users[0].name;
                }

                if (age != 0)
                {
                    user.age = age;
                }
                else
                {
                    user.age = users[0].age;
                }

                if (phone != "")
                {
                    user.phone = phone;
                }
                else
                {
                    user.phone = users[0].phone;
                }

                // Save To Cache
                Dictionary<string, string> cacheData = new Dictionary<string, string>();
                cacheData.Add("type", "1");
                cacheData.Add("value", JsonConvert.SerializeObject(user));

                _client.ListRightPush("keyBulkWrite", JsonConvert.SerializeObject(cacheData));

                Dictionary<string, User> res = new Dictionary<string, User>();
                res.Add("data", user);

                return res;
            }
            catch (Exception error)
            {
                System.Console.WriteLine("PUT /api/v1/User/:id Error: ", error.ToString());
                Dictionary<string, string> resErr = new Dictionary<string, string>();
                resErr.Add("error", "Bad Request");
                return BadRequest(resErr);
            }
        }

        // DELETE /api/v1/User
        // DELETE USER
        [HttpDelete("{id}")]
        public ActionResult<Dictionary<string, string>> Delete(string id = "")
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                List<User> users = _users.Find(u => u.id == id).ToList();

                if (users.Count() <= 0)
                {
                    Dictionary<string, string> error = new Dictionary<string, string>();
                    error.Add("error", "User not found");
                    return BadRequest(error);
                }

                _users.DeleteOne(u => u.id == id);

                return result;
            }
            catch (Exception error)
            {
                System.Console.WriteLine("DELETE /api/v1/User/:id Error: ", error.ToString());
                Dictionary<string, string> resErr = new Dictionary<string, string>();
                resErr.Add("error", "Bad Request");
                return BadRequest(resErr);
            }
        }
    }
}