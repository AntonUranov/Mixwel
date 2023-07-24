namespace Mixwel
{
    public readonly struct Result
    {
        public bool IsFailure => !IsSuccess;
        public bool IsSuccess { get; init; }
        public string Error { get; init; }

        public static Result<T> Fail<T>(string message) => new() { IsSuccess = false, Error = message, Value = default };
        public static Result<T> Ok<T>(T value) => new() { Value = value, IsSuccess = true, Error = string.Empty };
        public static Result Ok() => new() { IsSuccess = true, Error = string.Empty };
    }

    public readonly struct Result<T>
    {
        public bool IsFailure => !IsSuccess;
        public bool IsSuccess { get; init; }
        public string Error { get; init; }
        public T? Value { get; init; }
    }
}
