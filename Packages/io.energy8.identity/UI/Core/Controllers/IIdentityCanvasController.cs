using System;
using Energy8.Identity.UI.Core.Management;

namespace Energy8.Identity.UI.Core.Controllers
{
    /// <summary>
    /// Interface for extensible Identity UI controllers.
    /// </summary>
    public interface IIdentityCanvasController
    {
        bool IsOpen { get; }
        IViewManager ViewManager { get; }
        UnityEngine.Canvas Canvas { get; }
        event Action<bool> OnOpenStateChanged;
        void SetOpenState(bool isOpen);
        IViewManager GetViewManager();
        void SetCanvasEnabled(bool enabled);
        void SetActive(bool active);
    }
}