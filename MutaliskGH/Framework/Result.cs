using System;

namespace MutaliskGH.Framework
{
    internal sealed class Result<T>
    {
        private Result(bool isSuccess, T value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public bool IsSuccess { get; }

        public bool IsFailure
        {
            get { return !IsSuccess; }
        }

        public T Value { get; }

        public string ErrorMessage { get; }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, string.Empty);
        }

        public static Result<T> Failure(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("An error message is required for a failed result.", nameof(errorMessage));
            }

            return new Result<T>(false, default(T), errorMessage);
        }
    }
}
