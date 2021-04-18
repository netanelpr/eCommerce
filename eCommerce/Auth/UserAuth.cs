﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; 
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using eCommerce.Common;

namespace eCommerce.Auth
{
    public class UserAuth  : IUserAuth
    {
        private static readonly UserAuth Instance = new UserAuth(new ConcurrentRegisteredUserRepo());

        private readonly JWTAuth _jwtAuth;

        private Mutex _hashMutex;
        private readonly SHA256 _sha256;

        private ConcurrentIdGenerator _concurrentIdGenerator;
        
        private IRegisteredUserRepo _userRepo;

        private UserAuth(IRegisteredUserRepo repo)
        {
            _jwtAuth = new JWTAuth("keykeykeykeykeyekeykey");
            // TODO get the initialze id value from DB
            _concurrentIdGenerator = new ConcurrentIdGenerator(0);

            _hashMutex = new Mutex();
            _sha256 = SHA256.Create();
            
            _userRepo = repo;
            InitOneAdmin();
        }

        private void InitOneAdmin()
        {
            Register("Admin", "Admin");
            User adminUser = _userRepo.GetUserOrNull("Admin");
            if (adminUser == null)
            {
                throw new Exception("There isnt at least one admin in the system");
            }

            adminUser.AddRole(AuthUserRole.Admin);
        }

        public static UserAuth GetInstance()
        {
            return Instance;
        }
        
        public static UserAuth CreateInstanceForTests(IRegisteredUserRepo userRepo)
        {
            return new UserAuth(userRepo);
        }
        
        public string Connect()
        {
            string guestUsername = GenerateGuestUsername();
            string token = GenerateToken(new AuthData(guestUsername, RoleToString(AuthUserRole.Guest)));
            return token;
        }

        public void Disconnect(string token)
        {
        }

        public Result Register(string username, string password)
        {
            Result policyCheck = RegistrationsPolicy(username, password);
            if (policyCheck.IsFailure)
            {
                return policyCheck;
            }

            if (IsRegistered(username))
            {
                return Result.Fail("Username already taken");
            }
            
            User newUser = new User(username, HashPassword(password));
            newUser.AddRole(AuthUserRole.Member);

            if (!_userRepo.Add(newUser))
            {
                return Result.Fail("Username already taken");
            }
            return Result.Ok();
        }

        public Result<string> Login(string username, string password, AuthUserRole role)
        {
            if (role.Equals(AuthUserRole.Guest))
            {
                return Result.Fail<string>("Invalid role in log in");
            }
            
            User user = _userRepo.GetUserOrNull(username);
            Result canLogInRes = CanLogIn(user, password, role);
            if (canLogInRes.IsFailure)
            {
                return Result.Fail<string>(canLogInRes.Error);
            }

            string token = GenerateToken(new AuthData(user.Username, RoleToString(role)));
            return Result.Ok(token);
        }
        
        public Result Logout(string token)
        {
            Result<AuthData> authData = GetData(token);
            if (authData.IsFailure)
            {
                return Result.Fail<string>(authData.Error);
            }

            return Result.Ok();
        }

        public bool IsRegistered(string username)
        {
            return _userRepo.GetUserOrNull(username) != null;
        }

        public bool IsValidToken(string token)
        {
            return GetData(token).IsSuccess;
        }
        
        public Result<AuthData> GetData(string token)
        {
            var claims = _jwtAuth.GetClaimsFromToken(token);
            if (claims == null)
            {
                return Result.Fail<AuthData>("Token is not valid");
            }
            
            AuthData data = new AuthData(null, null);
            if (!FillAuthData(claims, data) || !data.AllDataIsNotNull())
            {
                return Result.Fail<AuthData>("Token have missing or redundant data");
            }

            return Result.Ok(data);
        }

        public string RoleToString(AuthUserRole role)
        {
            return role.ToString();
        }

        // ========== Private methods ========== //
        
        
        /// <summary>
        /// Fill the AuthData from claims
        /// </summary>
        /// <param name="claims">The claims</param>
        /// <param name="data">The auth data to fill</param>
        /// <returns>True if all the claims are valid</returns>
        private bool FillAuthData(IEnumerable<Claim> claims, AuthData data)
        {
            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case AuthClaimTypes.Username:
                    {
                        data.Username = claim.Value;
                        break;
                    }
                    case AuthClaimTypes.Role:
                    {
                        data.Role = claim.Value;
                        break;
                    }
                    default:
                        // TODO check the other expected types
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Hash the password.
        /// <para>Precondition: password is not empty.</para>
        /// </summary>
        /// <param name="password">The password</param>
        /// <returns>The password hash</returns>
        private byte[] HashPassword(string password)
        {
            byte[] hashedPassword = null;
            _hashMutex.WaitOne();
            hashedPassword = _sha256.ComputeHash(Encoding.Default.GetBytes(password));
            _hashMutex.ReleaseMutex();
            return hashedPassword;
        }
        
        private string GenerateToken(AuthData authData)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(AuthClaimTypes.Username, authData.Username),
                new Claim(AuthClaimTypes.Role, authData.Role)
            };
            
            return _jwtAuth.GenerateToken(claims.ToArray());
        }

        private long GetAndIncrementGuestId()
        {
            return _concurrentIdGenerator.MoveNext();
        }
        
        private string GenerateGuestUsername()
        {
            return $"_{RoleToString(AuthUserRole.Guest)}{GetAndIncrementGuestId():D}";
        }

        private Result RegistrationsPolicy(string username, string password)
        {
            String errMessage = null;
            if (string.IsNullOrEmpty(username))
            {
                errMessage = $"{errMessage}\nUsername cant be empty";
            }
            else if(username.StartsWith("_Guest"))
            {
                errMessage = $"{errMessage}\nUsername not valid";
            }

            if (string.IsNullOrEmpty(password))
            {
                errMessage = $"{errMessage}\nPassword not valid";
            }

            return errMessage == null ? Result.Ok() : Result.Fail(errMessage);
        }

        private Result CanLogIn(User user, string password, AuthUserRole role)
        {
            if (user == null || !user.HashedPassword.SequenceEqual(HashPassword(password)) || !user.HasRole(role))
            {
                return Result.Fail("Invalid username or password or role");
            }

            return Result.Ok();
        }
    }
}