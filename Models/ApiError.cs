namespace RAG_Code_Base.Models;

public class ApiError
{
    private string _code="";
    private string _message="";

    public string Code
    {
        get => _code;
        set => _code = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Message
    {
        get => _message;
        set  => _message = value ?? throw new ArgumentNullException(nameof(value));
    }

    public  ApiError(string code, string message)
    {
        Code = code.Trim();
        Message = message.Trim();
    }
}