using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Keroosha.GraphQL.Web.Dto;
using Keroosha.GraphQL.Web.Models;
using Keroosha.GraphQL.Web.Models.Repositories;
using Microsoft.Extensions.Logging;

namespace Keroosha.GraphQL.Web.Auth
{
    public abstract class AuthManager<TUser> where TUser : IHaveId, IHavePasswordAuth
    {
        private readonly IPasswordAuthRepository<TUser> _repo;
        private readonly IRoleRepository _roleRepository;
        private readonly ITokenStorage _tokenStorage;

        protected AuthManager(
            ITokenStorage tokenStorage,
            IPasswordAuthRepository<TUser> repo,
            IRoleRepository roleRepository)
        {
            _tokenStorage = tokenStorage;
            _repo = repo;
            _roleRepository = roleRepository;
        }

        public (TUser user, string renewToken)? Auth(string token)
        {
            var loaded = _tokenStorage.LoadToken<AuthToken<TUser>>(token);
            if (loaded is null) return null;

            return (_repo.GetById(loaded.Value.data.UserId), loaded.Value.token);
        }

        public List<string> Roles(string token)
        {
            var loaded = _tokenStorage.LoadToken<AuthToken<TUser>>(token);
            return loaded is null ? new List<string>() : loaded.Value.data.Roles;
        }

        public Result<(TUser user, string token)> Login(string login, string password)
        {
            var user = _repo.FindByLogin(login);
            if (user == null)
                return ErrorCode.UserNotFound;
            if (!PasswordToolkit.CheckPassword(user.PasswordHash, password))
                return ErrorCode.InvalidPassword;
            var userRoles = _roleRepository.UserRolesByIds(user.Id)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role)
                .ToList();

            var token = CreateToken(user, userRoles);
            return (user, token);
        }

        protected string CreateToken(TUser user, List<string> roles) => _tokenStorage.CreateToken(new AuthToken<TUser>
        {
            UserId = user.Id,
            Roles = roles
        });

        private class AuthToken<T>
        {
            public int UserId { get; set; }
            public List<string> Roles { get; set; }
        }
    }

    public class UserAuthManager : AuthManager<User>
    {
        private readonly TimeSpan _confirmationCheckThreshold = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<int, DateTime> _confirmationCodeChecks = new();
        private readonly ILogger _logger;
        private readonly IUserRepository _repo;
        private readonly IRoleRepository _roles;

        public UserAuthManager(
            ITokenStorage tokenStorage,
            IUserRepository repo,
            IRoleRepository roles,
            ILogger<UserAuthManager> logger)
            : base(tokenStorage, repo, roles)
        {
            _repo = repo;
            _roles = roles;
            _logger = logger;
        }

        public Result<(User user, string token)> LoginByConfirmationCode(string code)
        {
            if (code == null)
                return ErrorCode.AccessDenied;
            var user = _repo.FindByConfirmCode(code);
            if (user == null || user.Confirmed)
                return ErrorCode.UserNotFound;

            var userRoles = _roles.UserRolesByIds(user.Id)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role)
                .ToList();
            return (user, CreateToken(user, userRoles));
        }

        public Result<UserProfileDto> Register(string email, string password)
        {
            email = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"[\w.-]+@.+\.[a-z]{2,3}"))
                return ErrorCode.InvalidEmail;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return ErrorCode.WeakPassword;
            return Result.Catch(() =>
            {
                var userId = _repo.Create(email, "Имя не установлено", PasswordToolkit.EncodeSshaPassword(password));
                var registeredUser = new UserProfileDto
                {
                    Id = userId,
                    Email = email,
                    Name = "Имя не установлено"
                };
                return registeredUser;
            }, ErrorCode.EmailIsAlreadyRegistered, _logger);
        }

        public Result<User> CanRequestConfirmationCode(string email, string password)
        {
            email = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"[\w.-]+@.+\.[a-z]{2,3}"))
                return ErrorCode.InvalidEmail;

            var existingUser = _repo.FindByLogin(email);
            if (existingUser == null)
                return ErrorCode.UserNotFound;
            if (!PasswordToolkit.CheckPassword(existingUser.PasswordHash, password))
                return ErrorCode.InvalidPassword;

            if (existingUser.Confirmed)
                return ErrorCode.UserAlreadyConfirmed;

            var now = DateTime.Now;
            if (_confirmationCodeChecks.TryGetValue(existingUser.Id, out var dateTime) &&
                now - dateTime <= _confirmationCheckThreshold)
                return ErrorCode.UserConfirmationEmailSentTooOften;

            _confirmationCodeChecks[existingUser.Id] = now;
            return Result.Create(existingUser);
        }

        public Result<User> CanResetPassword(string email)
        {
            email = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"[\w.-]+@.+\.[a-z]{2,3}"))
                return ErrorCode.InvalidEmail;

            var existingUser = _repo.FindByLogin(email);
            if (existingUser == null)
                return ErrorCode.UserNotFound;
            if (!existingUser.Confirmed)
                return ErrorCode.UserNotConfirmed;

            var now = DateTime.Now;
            if (_confirmationCodeChecks.TryGetValue(existingUser.Id, out var dateTime) &&
                now - dateTime <= _confirmationCheckThreshold)
                return ErrorCode.UserConfirmationEmailSentTooOften;

            _confirmationCodeChecks[existingUser.Id] = now;
            return Result.Create(existingUser);
        }

        public Result<(User User, string Token)> ChangePassword(string code, string password)
        {
            if (code == null)
                return ErrorCode.AccessDenied;
            var user = _repo.FindByConfirmCode(code);
            if (user is not { Confirmed: true })
                return ErrorCode.UserNotFound;
            user.PasswordHash = PasswordToolkit.EncodeSshaPassword(password);
            user.ConfirmationCode = null;
            _repo.Update(user);

            var userRoles = _roles.UserRolesByIds(user.Id)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role)
                .ToList();
            return (user, CreateToken(user, userRoles));
        }
    }
}