namespace Claims.Services;

public abstract record ServiceResult
{
    public sealed record Success : ServiceResult;

    public sealed record NotFound : ServiceResult;

    public sealed record Invalid(Dictionary<string, string[]> Errors) : ServiceResult;
}

public abstract record ServiceResult<T>
{
    public sealed record Success(T Value) : ServiceResult<T>;

    public sealed record NotFound : ServiceResult<T>;

    public sealed record Invalid(Dictionary<string, string[]> Errors) : ServiceResult<T>;
}
