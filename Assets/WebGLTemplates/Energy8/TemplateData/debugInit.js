// Debug initialization: device info + eruda (only for mobile)
document.addEventListener('DOMContentLoaded', () => {
    if (window.TextureOptimizer) {
        const optimizer = new window.TextureOptimizer();
        console.log('Device capabilities:', optimizer.getDeviceInfo());
    }
    // Загружаем eruda только на мобильных устройствах
    if (window.utils && window.utils.isMobile && window.utils.isMobile()) {
        var script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/eruda';
        script.onload = function() { eruda.init(); };
        document.body.appendChild(script);
    }
});
