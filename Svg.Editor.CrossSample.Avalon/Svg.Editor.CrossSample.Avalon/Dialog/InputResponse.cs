namespace iCL.Modules.UserInteraction;

public class InputResponse
{
    public InputResponse()
    { }

    public InputResponse(bool ok, string? text)
    {
        Ok = ok;
        Text = text;
    }

    public bool Ok { get; set; }
    public string? Text { get; set; }
}