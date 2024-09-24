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
        public async Task<ServiceResponse<string>> DeleteUserAsync(long id)
        {
            var response = new ServiceResponse<string>();
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
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

                response.Success = true;
                response.Message = "User deleted successfully";
            }
            catch (DbException e)
            {
                response.Success = false;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { e.Message };
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error";
                response.ErrorMessages = new List<string> { e.Message };
            }
            return response;
        }

        public async Task<ServiceResponse<UpdateUserDTO>> UpdateUserAsync(UpdateUserDTO updateUserDTO)
        {
            var response = new ServiceResponse<UpdateUserDTO>();
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(updateUserDTO.Id);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }
                user.UserName = updateUserDTO.UserName;
                var existEmail = await _unitOfWork.UserRepository.CheckEmailAddressExisted(updateUserDTO.Email);
                if (existEmail)
                {
                    response.Success = false;
                    response.Message = "Email is already existed";
                    return response;
                }
                user.Email = updateUserDTO.Email;
                user.Password = updateUserDTO.Password;

                await _unitOfWork.UserRepository.UpdateUser(user);
                await _unitOfWork.SaveChangeAsync();

                response.Success = true;
                response.Data = _mapper.Map<UpdateUserDTO>(user);
                response.Message = "User updated successfully";
            }
            catch (DbException e)
            {
                response.Success = false;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { e.Message };
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error";
                response.ErrorMessages = new List<string> { e.Message };
            }
            return response;
        }
        public async Task<ServiceResponse<RegisterDTO>> RegisterAsync(RegisterDTO userObjectDTO)
        {
            var response = new ServiceResponse<RegisterDTO>();
            try
            {
                var existEmail = await _unitOfWork.UserRepository.CheckEmailAddressExisted(userObjectDTO.Email);
                if (existEmail)
                {
                    response.Success = false;
                    response.Message = "Email is already existed";
                    return response;
                }

                var userAccountRegister = _mapper.Map<User>(userObjectDTO);
                var currentCount = await _unitOfWork.UserRepository.GetCountAsync();
                userAccountRegister.Id = currentCount + 1;
                userAccountRegister.Role = userObjectDTO.Role;
                userAccountRegister.UserName = userObjectDTO.FullName;
                //Create Token
                userAccountRegister.Token = Guid.NewGuid().ToString();
                await _unitOfWork.UserRepository.AddAsync(userAccountRegister);

                var confirmationLink = $"https://localhost:7128/confirm?token={userAccountRegister.Token}";
                
                if (userObjectDTO.Role == "Driver")
                {
                    var driverId = await _unitOfWork.DriverRepository.GetCountAsync() + 1;
                    var driver = new Driver
                    {
                        Id = driverId,
                        CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                        UserId = userAccountRegister.Id
                    };
                    userAccountRegister.Drivers.Add(driver);
                    await _unitOfWork.DriverRepository.AddAsync(driver);
                }
                else if (userObjectDTO.Role == "Customer")
                {
                    var customerId = await _unitOfWork.CustomerRepository.GetCountAsync() + 1;
                    var customer = new Customer
                    {
                        Id = customerId,
                        CreateAt = BitConverter.GetBytes(DateTime.Now.ToBinary()),
                        UserId = userAccountRegister.Id
                    };
                    userAccountRegister.Customers.Add(customer);
                    await _unitOfWork.CustomerRepository.AddAsync(customer);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Invalid role";
                    return response;
                }
               
                //SendMail
                var emailSend = await SendMail.SendConfirmationEmail(userObjectDTO.Email, confirmationLink);
                if (!emailSend)
                {
                    response.Success = false;
                    response.Message = "Error when send mail";
                    return response;
                }

                var accountRegistedDTO = _mapper.Map<RegisterDTO>(userAccountRegister);
                response.Success = true;
                response.Data = accountRegistedDTO;
                response.Message = "Register successfully.";
            }
            catch (DbException e)
            {
                response.Success = false;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { e.Message };
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error";
                response.ErrorMessages = new List<string> { e.Message };
            }

            return response;
        }
        public async Task<LoginToken<string>> LoginAsync(LoginUserDTO userObject)
        {
            var response = new LoginToken<string>();
            try
            {
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
