var webHLSPlayer = {
    InitializePlayer: function () {
        console.log("Initialization HLS Player...");
        if (!window.hlsVideoElement) {
            window.hlsVideoElement = document.createElement('video');
            window.hlsVideoElement.style.display = 'none';
            document.body.appendChild(window.hlsVideoElement);
        }
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
        if (window.hlsVideoElement) {
            window.hlsVideoElement.addEventListener('loadedmetadata', () => {
                window.hlsVideoElement.currentTime = window.hlsVideoElement.duration - 0.1;
                window.hlsVideoElement.play().catch(console.error);
            });
        }
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