using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models
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

