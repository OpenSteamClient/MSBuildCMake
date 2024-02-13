namespace CustomBuildTask;

[System.Serializable]
public class CompileException : System.Exception
{
    public CompileException() { }
    public CompileException(string message) : base(message) { }
    public CompileException(string message, System.Exception inner) : base(message, inner) { }
}