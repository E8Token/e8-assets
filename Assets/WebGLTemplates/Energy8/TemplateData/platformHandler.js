document.addEventListener('DOMContentLoaded', () => {
    const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    const isLiteVersion = window.location.pathname.includes('/Lite');

    if ((isMobile && !isLiteVersion) || (!isMobile && isLiteVersion)) {
        const message = isMobile
            ? 'You are using the main version. Would you like to switch to the Lite version?'
            : 'You are using the Lite version. Would you like to switch to the main version?';

        const buttons = [
            {
                label: isMobile ? 'Switch to Lite' : 'Switch to Main Version',
                onClick: () => {
                    const newUrl = !isMobile ? window.location.href.replace('/Lite', '') : window.location.href + '/Lite';
                    window.location.href = newUrl;
                }
            },
            {
                label: 'Stay here',
                onClick: () => {
                    hideWarning("PlatformWarning");
                }
            }
        ];

        showWarning("PlatformWarning", message, buttons);
    }
})