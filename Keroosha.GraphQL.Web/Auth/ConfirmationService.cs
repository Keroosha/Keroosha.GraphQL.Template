using System;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Config;
using Keroosha.GraphQL.Web.Managers;
using Keroosha.GraphQL.Web.Models.Repositories;
using Keroosha.GraphQL.Web.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Nito.AsyncEx;

namespace Keroosha.GraphQL.Web.Auth
{
    public interface IConfirmationService
    {
        public Task SendNewConfirmationCode(int userId, ConfirmationCodeType type);
        public Task ConfirmUser(string code);
    }

    public enum ConfirmationCodeType
    {
        AccountConfirm,
        ChangePassword
    }

    public interface IEmailConfirmationService : IConfirmationService
    {
    }

    public static class ConfirmationCodeExtensions
    {
        public static string GetPath(this ConfirmationCodeType type) => type switch
        {
            ConfirmationCodeType.AccountConfirm => "/confirm",
            ConfirmationCodeType.ChangePassword => "/change-password",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static string GetPhrase(this ConfirmationCodeType type) => type switch
        {
            ConfirmationCodeType.AccountConfirm => "Перейдите по ссылке ниже, чтобы подтвердить аккаунт:",
            ConfirmationCodeType.ChangePassword => "Для изменения пароля от аккаунта перейдите по ссылке ниже:",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public class EmailConfirmationService : IEmailConfirmationService, IDisposable
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly IUserRepository _userRepository;
        private readonly IRazorRenderer _razorRenderer;
        private readonly SmtpClient _smtpClient = new SmtpClient();
        private readonly EmailOptions _options;

        public EmailConfirmationService(
            IUserRepository userRepository,
            IRazorRenderer razorRenderer,
            IOptions<EmailOptions> options)
        {
            _userRepository = userRepository;
            _razorRenderer = razorRenderer;
            _options = options.Value;
        }

        private async Task Connect()
        {
            _smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await _smtpClient.ConnectAsync(_options.SMTP.Host, _options.SMTP.Port);
            if (!_options.SMTP.Auth) return;

            _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            await _smtpClient.AuthenticateAsync(_options.SMTP.Username, _options.SMTP.Password);
        }

        public async Task SendNewConfirmationCode(int userId, ConfirmationCodeType type)
        {
            using (await _asyncLock.LockAsync())
            {
                await Connect();
                var user = _userRepository.GetById(userId);
                var confirmCode = Guid.NewGuid().ToString().Replace("-", "");
                var model = new EmailConfirmTemplateViewModel
                {
                    Caption = type.GetPhrase(),
                    ConfirmationsCode = confirmCode,
                    RedirectUrl = $"{_options.BaseUrl.TrimEnd('/')}{type.GetPath()}"
                };

                var text = await _razorRenderer.RenderViewToStringAsync("/Views/EmailConfirmTemplate.cshtml", model);
                await _smtpClient.SendAsync(new MimeMessage
                {
                    From =
                    {
                        new MailboxAddress(_options.Name, _options.FromEmail)
                    },
                    To =
                    {
                        new MailboxAddress(user.Name, user.Email)
                    },
                    Subject = "Подтверждение регистрации",
                    Body = new BodyBuilder
                    {
                        HtmlBody = text,
                        TextBody = type.GetPhrase() +
                                   Environment.NewLine +
                                   $"{model.RedirectUrl}?code={model.ConfirmationsCode}"
                    }.ToMessageBody()
                });

                await _smtpClient.DisconnectAsync(false);
                user.ConfirmationCode = confirmCode;
                _userRepository.Update(user);
            }
        }

        public async Task ConfirmUser(string code)
        {
            var user = _userRepository.FindByConfirmCode(code);
            if (user is null) throw new ArgumentException("Code mismatch");

            user.Confirmed = true;
            user.ConfirmationCode = null;
            _userRepository.Update(user);
            await Task.Yield();
        }

        public void Dispose()
        {
            _smtpClient.Disconnect(true);
            _smtpClient.Dispose();
        }
    }

    public class MockConfirmationService : IEmailConfirmationService
    {
        private readonly ILogger<MockConfirmationService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly EmailOptions _options;

        public MockConfirmationService(
            ILogger<MockConfirmationService> logger,
            IUserRepository userRepository,
            IOptions<EmailOptions> options)
        {
            _logger = logger;
            _userRepository = userRepository;
            _options = options.Value;
        }

        public async Task SendNewConfirmationCode(int userId, ConfirmationCodeType type)
        {
            var user = _userRepository.GetById(userId);
            var confirmCode = Guid.NewGuid().ToString().Replace("-", "");
            user.ConfirmationCode = confirmCode;
            _userRepository.Update(user);
            _logger.LogError($"{type.GetPhrase()} {_options.BaseUrl.TrimEnd('/')}{type.GetPath()}?code={confirmCode}");
            await Task.Yield();
        }

        public async Task ConfirmUser(string code)
        {
            var user = _userRepository.FindByConfirmCode(code);
            if (user is null) throw new ArgumentException("Code mismatch");

            user.Confirmed = true;
            user.ConfirmationCode = null;
            _userRepository.Update(user);
            await Task.Yield();
        }
    }
}