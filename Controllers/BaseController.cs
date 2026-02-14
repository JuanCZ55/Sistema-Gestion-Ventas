using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    public void Notify(string message, string type = "success")
    {
        TempData["ToastMessage"] = message;
        TempData["ToastType"] = type;
    }

    protected object ApiResponse(bool success, string message, object? data = null)
    {
        return new
        {
            success,
            message,
            data,
        };
    }
}
