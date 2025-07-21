using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class UpdateViewResult : ViewResult
    {
        /// <summary>
        /// Требуется ли обновление приложения
        /// </summary>
        public bool IsUpdateRequired { get; set; }

        public UpdateViewResult() { }
        public UpdateViewResult(bool isUpdateRequired)
        {
            IsUpdateRequired = isUpdateRequired;
        }
    }
}

