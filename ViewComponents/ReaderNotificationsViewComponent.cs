using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.ViewComponents
{
    public class ReaderNotificationsViewComponent : ViewComponent
    {
        private readonly IReaderNotificationService _readerNotificationService;

        public ReaderNotificationsViewComponent(IReaderNotificationService readerNotificationService)
        {
            _readerNotificationService = readerNotificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = await _readerNotificationService.BuildDropdownAsync(HttpContext.User);
            return View(vm);
        }
    }
}