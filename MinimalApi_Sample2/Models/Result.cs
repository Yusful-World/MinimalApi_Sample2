namespace MinimalApi_Sample2.Models
{
    public class Result<T> where T : class
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static Result<T> SuccessResult(T data) => new() { Success = true, Data = data };
        public static Result<T> Failure(params string[] errors) => new() { Success = false, Errors = errors };
        public static Result<T> Failure(IEnumerable<string> errors) => new() { Success = false, Errors = errors };
    }
}
