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

// Функции для работы с текстурами и data файлами
window.utils.getBestTextureFormat = function() {
    const canvas = typeof OffscreenCanvas !== 'undefined' ? new OffscreenCanvas(1, 1) : document.createElement('canvas');
    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
    const isMobile = window.utils.isMobile();
    if (!isMobile) return 'dxt';
    if (gl && gl.getExtension('WEBGL_compressed_texture_astc')) return 'astc';
    return 'etc2';
};

window.utils.getTextureFormatPriority = function() {
    const canvas = typeof OffscreenCanvas !== 'undefined' ? new OffscreenCanvas(1, 1) : document.createElement('canvas');
    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
    const isMobile = window.utils.isMobile();

    if (!isMobile) {
        return ['dxt', 'etc2', 'astc'];
    } else {
        const supportsAstc = gl && gl.getExtension('WEBGL_compressed_texture_astc');
        if (supportsAstc) {
            return ['astc', 'etc2', 'dxt'];
        } else {
            return ['etc2', 'dxt', 'astc'];
        }
    }
};

window.utils.selectDataFile = function(dataTypes) {
    if (!dataTypes) return null;
    const formatPriority = window.utils.getTextureFormatPriority();
    for (const fmt of formatPriority) {
        const found = dataTypes[fmt];
        if (found) return found;
    }
    return null;
};

window.utils.getDataTypes = async function() {
    try {
        const response = await fetch('Build/data-types.json', { cache: 'no-cache' });
        return response.ok ? await response.json() : null;
    } catch (error) {
        console.warn('[Utils] Failed to load data-types.json:', error);
        return null;
    }
};
