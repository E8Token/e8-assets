var webHLSPlayer = {
    InitializePlayer: function () {
        console.log("Initialization HLS Player...");
        if (!window.hlsVideoElement) {
            window.hlsVideoElement = document.createElement('video');
            window.hlsVideoElement.style.display = 'none';
            document.body.appendChild(window.hlsVideoElement);
        }
    },

    FetchStreamList: function (callbackObject, callbackFunction) {
        console.log("Fetching stream list from the server...");
        callbackObject = UTF8ToString(callbackObject)
        callbackFunction = UTF8ToString(callbackFunction)

        // Выполняем запрос к серверу для получения списка стримов
        fetch('/stream/list')
            .then(response => response.json())
            .then(data => {
                console.log("Received stream list from server:", data);
                // Передаем полученный список стримов обратно в Unity
                var jsonList = JSON.stringify(data);
                if (window.unityInstance) {
                    window.unityInstance.SendMessage(callbackObject, callbackFunction, jsonList);
                } else {
                    console.error("UnityLoader is undefined. Can't send stream list.");
                }
            })
            .catch(error => {
                console.error("Error fetching stream list:", error);
            });
    },

    SetUrl: function (url) {
        url = UTF8ToString(url);

        console.log("HLS Player set url: " + url);

        if (window.hlsVideoElement) {
            if (window.hls) {
                window.hls.destroy();
            }

            window.hls = new Hls();
            window.hls.loadSource(url);
            window.hls.attachMedia(window.hlsVideoElement);
            window.hls.on(Hls.Events.MANIFEST_PARSED, () => {
                console.log("HLS Manifest loaded.");
            });
            window.hls.on(Hls.Events.ERROR, (event, data) => {
                console.error("HLS.js error", data);
            });
        }
    },
    Play: function () {
        window.hlsVideoElement.currentTime = window.hlsVideoElement.duration - 0.1;
        window.hlsVideoElement.play().catch(console.error);
    },
    Pause: function () {
        if (window.hlsVideoElement) {
            window.hlsVideoElement.pause();
        }
    },
    SetHLSVolume: function (volume) {
        if (window.hlsVideoElement) {
            window.hlsVideoElement.volume = Math.max(0, Math.min(volume, 1));
        }
    }
}
mergeInto(LibraryManager.library, webHLSPlayer);