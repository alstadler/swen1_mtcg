using MTCG.Controllers;
using MTCG.Services;
using MTCG.DTOs;
using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MTCG.HTTP
{
    public class HTTPServer
    {
        private readonly HttpListener _listener;
        private readonly UserController _userController;
        public HTTPServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:10001/");
            _userController = new UserController(new UserService());
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server started on http://localhost:10001/");
            while (true)
            {
                var context = _listener.GetContext();
                Process(context);
            }
        }

        private void Process(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string responseString = "";

            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/users")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = reader.ReadToEnd();
                    var registrationData = JsonConvert.DeserializeObject<RegistrationRequest>(requestBody);
                    try
                    {
                        responseString = _userController.RegisterUser(registrationData.Username, registrationData.Password);
                        response.StatusCode = 201;  // HTTP 201 Created
                    }
                    catch (Exception ex)
                    {
                        responseString = ex.Message;
                        response.StatusCode = 409;  // HTTP 409 Conflict
                    }
                }
            }
            else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/sessions")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = reader.ReadToEnd();
                    var loginData = JsonConvert.DeserializeObject<LoginRequest>(requestBody);
                    try
                    {
                        responseString = _userController.Login(loginData.Username, loginData.Password);
                        response.StatusCode = 200;  // HTTP 200 OK
                    }
                    catch(Exception ex)
                    {
                        responseString = ex.Message;
                        response.StatusCode = 401;  // HTTP 401 Unauthorized
                    }
                }
            }

            response.ContentType = "application/json";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}