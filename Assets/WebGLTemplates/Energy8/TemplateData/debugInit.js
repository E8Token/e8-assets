// Debug initialization: device info + eruda (mobile + telegram mini app) + service worker
document.addEventListener('DOMContentLoaded', () => {
    if (window.TextureOptimizer) {
        const optimizer = new window.TextureOptimizer();
        console.log('Device capabilities:', optimizer.getDeviceInfo());
    }
    
    // Проверяем, нужно ли загружать eruda (мобильные устройства ИЛИ Telegram mini app)
    const isMobile = window.utils && window.utils.isMobile && window.utils.isMobile();
    const isTelegramMiniApp = window.Telegram && window.Telegram.WebApp && window.Telegram.WebApp.initData;
    
    if (isMobile || isTelegramMiniApp) {
        var script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/eruda';
        script.onload = function() { eruda.init(); };
        document.body.appendChild(script);
    }
    
    // Регистрируем Service Worker
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('./ServiceWorker.js')
            .then(registration => {
                console.log('Service Worker registered successfully:', registration.scope);
            })
            .catch(error => {
                console.log('Service Worker registration failed:', error);
            });
    }
});
