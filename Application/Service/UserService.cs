using Application.DTO;
using Application.ServiceResponse;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.AppConfig;
using Application.Utils;
using AutoMapper;
namespace Application.Service
{
    public class UserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly AppConfiguration _config;
        private readonly IMapper _mapper;
        public UserService(UnitOfWork unitOfWork, AppConfiguration config, IMapper mapper) { 
            _unitOfWork = unitOfWork;
            _config = config;
            _mapper = mapper;
        }
        public async Task<LoginToken<string>> LoginAsync(LoginUserDTO userObject)
        {
            var response = new LoginToken<string>();
            try
            {
                //var passHash = HashPass.HashWithSHA256(userObject.Password);
                var userLogin =
                    await _unitOfWork.UserRepository.GetUserByEmailAddressAndPassword(userObject.Email, userObject.Password);
                if (userLogin == null)
                {
                    response.Success = false;
                    response.Message = "Invalid username or password";
                    return response;
                }

                if (userLogin.Token != null && !userLogin.IsConfirm)
                {
                    System.Console.WriteLine(userLogin.Token + userLogin.IsConfirm);
                    response.Success = false;
                    response.Message = "Please confirm via link in your mail";
                    return response;
                }

                var auth = userLogin.Role;
                var userId = userLogin.Id;
                var token = userLogin.GenerateJsonWebToken(_config, _config.JWTSection.SecretKey, DateTime.Now);
                response.Success = true;
                response.Message = "Login successfully";
                response.DataToken = token;
                response.Role = auth;               
            }
            catch (DbException ex)
            {
                response.Success = false;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { ex.Message };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error";
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return response;
        }
    }
}
