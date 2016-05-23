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

        public string Message { get; }

        public int Code { get; }

        public T Value { get; }

        public Exception Exception { get; }
    }
}