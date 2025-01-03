namespace iCL.Modules.UserInteraction;

public static class UserInteractionServiceExt
{
    public static IUserInteraction UserInteractionInst => new UserInteractionService();
}