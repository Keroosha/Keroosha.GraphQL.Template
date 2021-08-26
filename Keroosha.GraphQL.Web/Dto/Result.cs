using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keroosha.GraphQL.Web.Dto
{
    public class Result<T> : Result
    {
        public Result(T result)
        {
            Success = true;
            Value = result;
        }

        private Result()
        {
        }

        [JsonProperty] public T Value { get; }

        public Result<TRes> Map<TRes>(Func<T, TRes> cb) => Success ? Create(cb(Value)) : Error;

        public Result<TRes> Map<TRes>(Func<T, Result<TRes>> cb) => Success ? cb(Value) : Error;

        public static implicit operator Result<T>(ErrorCode error) => new() { Error = error };

        public static implicit operator Result<T>(T value) => new(value);
    }

    public class Result
    {
        public bool Success { get; protected set; }

        public ErrorCode Error { get; protected set; }

        public static Result Succeeded { get; } = new() { Success = true };

        public static Result<T> Create<T>(T result) => new(result);

        public ErrorCode AsError() => Error;

        public static implicit operator Result(ErrorCode error) => new() { Error = error };

        public Result<TRes> Map<TRes>(Func<TRes> cb) => Success ? Create(cb()) : Error;

        public Result<TRes> Map<TRes>(Func<Result<TRes>> cb) => Success ? cb() : Error;

        public static Result Catch(Action cb, ErrorCode code, ILogger logger)
        {
            try
            {
                cb();
                return Succeeded;
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error");
                return code;
            }
        }

        public static async Task<Result> Catch(Func<Task> cb, ErrorCode code, ILogger logger)
        {
            try
            {
                await cb();
                return Succeeded;
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error");
                return code;
            }
        }

        public static Result<T> Catch<T>(Func<T> cb, ErrorCode code, ILogger logger)
        {
            try
            {
                return cb();
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error");
                return code;
            }
        }

        public static async Task<Result<T>> Catch<T>(Func<Task<T>> cb, ErrorCode code, ILogger logger)
        {
            try
            {
                return await cb();
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error");
                return code;
            }
        }

        public static Result<T> WhenNull<T>(T v, ErrorCode code) where T : class => v ?? (Result<T>)code;
    }

    public static class ResultExtensions
    {
        public static Result<T> WhenNull<T>(this T v, ErrorCode code) where T : class => Result.WhenNull(v, code);
    }

    public class ErrorCode
    {
        private ErrorCode(string code, string description)
        {
            Code = code;
            Description = description;
        }

        [JsonProperty] public string Code { get; }
        [JsonProperty] public string Description { get; }

        public override string ToString() => Code + ": " + Description;

        public static ErrorCode NotFound => D("Ничего не найдено!", nameof(NotFound));

        public static ErrorCode EmailIsAlreadyRegistered => D("Пользователь с таким email уже зарегистрирован.",
            nameof(EmailIsAlreadyRegistered));

        public static ErrorCode UserNotFound =>
            D("Пользователь с таким email не зарегистрирован.", nameof(UserNotFound));

        public static ErrorCode UserNotConfirmed =>
            D(
                "Пользователь не подтверждён. Пожалуйста, проверьте почту — на неё должна была прийти ссылка, по которой необходимо перейти для завершения процедуры регистрации.",
                nameof(UserNotConfirmed));

        public static ErrorCode UserAlreadyConfirmed => D("Пользователь уже подтверждён, Вы можете авторизоваться.",
            nameof(UserAlreadyConfirmed));

        public static ErrorCode UserConfirmationEmailSentTooOften =
            D(
                "Ссылка для подтверждения аккаунта была только что отправлена на вашу почту. Повторная отправка письма со ссылкой возможна спустя некоторое время.",
                nameof(UserConfirmationEmailSentTooOften));

        public static ErrorCode InvalidPassword => D("Неправильный пароль.", nameof(InvalidPassword));

        public static ErrorCode DatabaseError =>
            D("Внутренняя ошибка, свяжитесь с администратором.", nameof(DatabaseError));

        public static ErrorCode AccessDenied => D("Доступ запрещён.", nameof(AccessDenied));
        public static ErrorCode InvalidEmail => D("Некорректный адрес электронной почты.", nameof(InvalidEmail));
        public static ErrorCode WeakPassword => D("Слишком слабый пароль.", nameof(WeakPassword));

        private static ErrorCode D(string description, string code) => new(code, description);

        private static ErrorCode D([CallerMemberName] string code = null)
        {
            var sb = new StringBuilder();
            sb.Append(code[0]);
            foreach (var c in code.Skip(1))
                if (char.IsUpper(c))
                {
                    sb.Append(' ');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }

            return new ErrorCode(code, sb.ToString());
        }
    }
}