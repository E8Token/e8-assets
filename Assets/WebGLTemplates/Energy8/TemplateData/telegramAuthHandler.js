function haveTgAuthResult() {
    var locationHash = '', re = /[#\?\&]tgAuthResult=([A-Za-z0-9\-_=]*)$/, match;
    try {
        locationHash = location.hash.toString();
        if (match = locationHash.match(re)) {
            location.hash = locationHash.replace(re, '');
            var data = match[1] || '';
            data = data.replace(/-/g, '+').replace(/_/g, '/');
            var pad = data.length % 4;
            if (pad > 1) {
                data += new Array(5 - pad).join('=');
            }
            return JSON.parse(window.atob(data));
        }
    } catch (e) { }
    return false;
}

var TelegramLogin = {
    popups: {},
    options: null,
    auth_callback: null,
    auth_fallback: null,

    init: function (options, auth_callback, auth_fallback) {
        TelegramLogin.options = options;
        TelegramLogin.auth_callback = auth_callback;
        TelegramLogin.auth_fallback = auth_fallback;

        // Check Telegram Web App data
        if (window.Telegram?.WebApp?.initData) {
            TelegramLogin.auth_callback(window.Telegram.WebApp.initDatah);
            return;
        }

        // Check URL auth result
        var auth_result = haveTgAuthResult();
        if (auth_result && auth_callback) {
            TelegramLogin.auth_callback(auth_result);
            return;
        }
    },

    auth: function () {
        return new Promise((resolve, reject) => {
            const bot_id = parseInt(TelegramLogin.options.bot_id);
            const width = 550;
            const height = 470;
            const left = Math.max(0, (screen.width - width) / 2) + (screen.availLeft | 0);
            const top = Math.max(0, (screen.height - height) / 2) + (screen.availTop | 0);

            const onAuthDone = (authData) => {
                if (!TelegramLogin.popups[bot_id]) return;
                if (TelegramLogin.popups[bot_id].authFinished) return;
                TelegramLogin.popups[bot_id].authFinished = true;
                window.removeEventListener('message', onMessage);
                if (authData) {
                    resolve(authData);
                } else {
                    reject(new Error('Authentication cancelled'));
                }
            };

            const onMessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    if (!TelegramLogin.popups[bot_id]) return;
                    if (event.source !== TelegramLogin.popups[bot_id].window) return;
                    if (data.event === 'auth_result') {
                        onAuthDone(data.result);
                    }
                } catch (e) {
                    console.error('Telegram auth message parsing error:', e);
                }
            };

            const checkClose = () => {
                if (!TelegramLogin.popups[bot_id]) return;
                if (!TelegramLogin.popups[bot_id].window || TelegramLogin.popups[bot_id].window.closed) {
                    onAuthDone(false);
                    return;
                }
                setTimeout(checkClose, 100);
            };

            const popup_url = 'https://oauth.telegram.org/auth?' +
                'bot_id=' + encodeURIComponent(bot_id) +
                '&origin=' + encodeURIComponent(location.origin) +
                '&return_to=' + encodeURIComponent(location.href) +
                (TelegramLogin.options.request_access ? '&request_access=' + encodeURIComponent(TelegramLogin.options.request_access) : '') +
                (TelegramLogin.options.lang ? '&lang=' + encodeURIComponent(TelegramLogin.options.lang) : '');

            const popup = window.open(
                popup_url,
                'telegram_oauth_bot' + bot_id,
                'width=' + width + ',height=' + height + ',left=' + left + ',top=' + top +
                ',status=0,location=0,menubar=0,toolbar=0'
            );

            TelegramLogin.popups[bot_id] = {
                window: popup,
                authFinished: false
            };

            if (popup) {
                window.addEventListener('message', onMessage);
                popup.focus();
                checkClose();
            }
        });
    }
};

window.initializeTelegramAuth = TelegramLogin.init;
window.signInWithTelegram = TelegramLogin.auth;
