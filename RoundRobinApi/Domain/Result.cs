using Newtonsoft.Json.Linq;

namespace RoundRobinApi.Domain;

public class Result<T>
{
    public bool IsOk { get; set; }
    public T Value { get; set; }

    public static Result<T> Fail()
    {
        return new Result<T>
        {
            IsOk = false
        };
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            IsOk = true,
            Value = value
        };
    }
}
