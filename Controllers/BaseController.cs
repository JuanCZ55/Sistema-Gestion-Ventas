using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    public void Notify(string message, string type = "success")
    {
        TempData["ToastMessage"] = message;
        TempData["ToastType"] = type;
    }
}
