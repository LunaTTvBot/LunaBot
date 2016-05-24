using System;

namespace IBot.Facades
{
    public class Result<T>
    {
        public Result(string message, int code, T value, Exception exception)
        {
            Message = message;
            Code = code;
            Value = value;
            Exception = exception;
        }

        public Result(string message, int code, T value)
        {
            Message = message;
            Code = code;
            Value = value;
            Exception = null;
        }

        public Result()
        {
            Message = "Unexcpected Error";
            Code = 400;
            Value = default(T);
            Exception = null;
        }

        public bool Success => Code == 0;
        public string Message { get; }
        public int Code { get; }
        public T Value { get; }
        public Exception Exception { get; }
    }
}