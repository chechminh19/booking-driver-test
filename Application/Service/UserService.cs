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
using Infrastructure.DTO;
using System.Net;
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
        public async Task<UserInfor> GetUserByIdAsync(long id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserInfor>(user);
        }
        public async Task<ApiResponse<string>> DeleteUserAsync(long id)
        {
            var response = new ApiResponse<string>();
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "User not found",
                        Data = null
                    };
                }
                if (user.Role == "Driver")
                {
                    var driver = await _unitOfWork.DriverRepository.GetIdUserFromDriver(user.Id);
                    if (driver != null)
                    {
                       await _unitOfWork.DriverRepository.Remove(driver);
                    }
                }
                else if (user.Role == "Customer")
                {
                    var customer = await _unitOfWork.CustomerRepository.GetIdUserFromCustomer(user.Id);
                    if (customer != null)
                    {
                        await _unitOfWork.CustomerRepository.Remove(customer);
                    }
                }

                await _unitOfWork.UserRepository.Remove(user);
                await _unitOfWork.SaveChangeAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "User deleted successfully";
            }
            catch (DbException e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Database error occurred.";
                response.Data = null;
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred.";
                response.Data = null;
            }
            return response;
        }

        public async Task<ApiResponse<UpdateUserDTO>> UpdateUserAsync(UpdateUserDTO updateUserDTO)
        {
            var response = new ApiResponse<UpdateUserDTO>();
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(updateUserDTO.Id);
                if (user == null)
                {
                    return new ApiResponse<UpdateUserDTO>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "User not found"
                    };
                }
                user.UserName = updateUserDTO.UserName;
                var existEmail = await _unitOfWork.UserRepository.CheckEmailAddressExisted(updateUserDTO.Email);
                if (existEmail)
                {
                    return new ApiResponse<UpdateUserDTO>
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "Email is already existed"
                    };
                }
                user.Email = updateUserDTO.Email;
                user.Password = updateUserDTO.Password;

                await _unitOfWork.UserRepository.UpdateUser(user);
                await _unitOfWork.SaveChangeAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Data = _mapper.Map<UpdateUserDTO>(user);
                response.Message = "User updated successfully";
            }
            catch (DbException e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Error occurred.";
                response.Data = null;
                response.ErrorMessage = new string(e.Message);
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Error occurred.";
                response.Data = null;
                response.ErrorMessage = new string(e.Message);
            }
            return response;
        }
        public async Task<ApiResponse<RegisterDTO>> RegisterAsync(RegisterDTO userObjectDTO)
        {
            var response = new ApiResponse<RegisterDTO>();
            try
            {
                var existEmail = await _unitOfWork.UserRepository.CheckEmailAddressExisted(userObjectDTO.Email);
                if (existEmail)
                {
                    return new ApiResponse<RegisterDTO>
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = "Email is already existed"
                    };
                }

                var userAccountRegister = _mapper.Map<User>(userObjectDTO);
                var currentCount = await _unitOfWork.UserRepository.GetMaxIdAsync();
                userAccountRegister.Id = currentCount + 1;
                userAccountRegister.Role = userObjectDTO.Role;
                userAccountRegister.UserName = userObjectDTO.FullName;
                //Create Token
                userAccountRegister.Token = Guid.NewGuid().ToString();
                await _unitOfWork.UserRepository.AddAsync(userAccountRegister);

                var confirmationLink = $"https://localhost:7128/confirm?token={userAccountRegister.Token}";
                
                if (userObjectDTO.Role == "Driver")
                {
                    var driverId = await _unitOfWork.DriverRepository.GetMaxIdAsync() + 1;
                    var driver = new Driver
                    {
                        Id = driverId + 1,
                        CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                        UserId = userAccountRegister.Id
                    };
                    userAccountRegister.Drivers.Add(driver);
                    await _unitOfWork.DriverRepository.AddAsync(driver);
                }
                else if (userObjectDTO.Role == "Customer")
                {
                    var customerId = await _unitOfWork.CustomerRepository.GetMaxIdAsync() + 1;
                    var customer = new Customer
                    {
                        Id = customerId + 1,
                        CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                        UserId = userAccountRegister.Id
                    };
                    userAccountRegister.Customers.Add(customer);
                    await _unitOfWork.CustomerRepository.AddAsync(customer);
                }
                else
                {
                    return new ApiResponse<RegisterDTO>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Invalid role"
                    };

                }

                //SendMail
                var emailSend = await SendMail.SendConfirmationEmail(userObjectDTO.Email, confirmationLink);
                if (!emailSend)
                {
                    return new ApiResponse<RegisterDTO>
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Message = "Error when sending mail"
                    };
                }

                var accountRegistedDTO = _mapper.Map<RegisterDTO>(userAccountRegister);
                response.StatusCode = HttpStatusCode.OK;
                response.Data = accountRegistedDTO;
                response.Message = "Register successfully.";
            }
            catch (DbException e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Database error occurred.";
                response.Data = null;
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Error occurred.";
                response.Data = null;
            }
            return response;
        }
        public async Task<ApiResponse<string>> LoginAsync(LoginUserDTO userObject)
        {
            var response = new ApiResponse<string>();
            try
            {
                var userLogin =
                    await _unitOfWork.UserRepository.GetUserByEmailAddressAndPassword(userObject.Email, userObject.Password);
                if (userLogin == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Invalid username or password"
                    };
                   
                }

                if (userLogin.Token != null && !userLogin.IsConfirm)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Message = "Please confirm via link in your mail"
                    };
                }

                var auth = userLogin.Role;
                var userId = userLogin.Id;
                var token = userLogin.GenerateJsonWebToken(_config, _config.JWTSection.SecretKey, DateTime.Now);
                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Login successfully";
                response.Data = token;
                               
            }
            catch (DbException ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Database error occurred.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error",
                    Data = null
                };
            }

            return response;
        }
    }
}
