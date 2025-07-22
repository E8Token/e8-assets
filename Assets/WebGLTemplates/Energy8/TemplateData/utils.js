// Глобальная функция определения мобильного устройства
window.utils = window.utils || {};
window.utils.isMobile = function() {
    return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
};
window.utils.isIOS = function() {
    return /iPhone|iPad|iPod/i.test(navigator.userAgent);
};
window.utils.isAndroid = function() {
    return /Android/i.test(navigator.userAgent);
};
window.utils.getDeviceMemory = function() {
    return navigator.deviceMemory || 4;
};
window.utils.getHardwareConcurrency = function() {
    return navigator.hardwareConcurrency || 2;
};
window.utils.isLowEndDevice = function() {
    const memory = window.utils.getDeviceMemory();
    const cores = window.utils.getHardwareConcurrency();
    return memory < 3 || cores < 4;
};
window.utils.isMediumEndDevice = function() {
    const memory = window.utils.getDeviceMemory();
    const cores = window.utils.getHardwareConcurrency();
    return (memory >= 3 && memory < 6) || (cores >= 4 && cores < 8);
};
