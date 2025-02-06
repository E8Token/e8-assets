(function () {
    function adjustDataFilePath(canvas, dataFile) {
        if (isASTCAvailable(canvas)) {
            const mobileDataFile = dataFile.replace(".data", "-mobile.data");
            const xhr = new XMLHttpRequest();
            xhr.open('HEAD', "Build" + mobileDataFile, false);
            try {
            xhr.send();
            if (xhr.status === 200) {
                dataFile = mobileDataFile;
            }
            } catch (e) {
            console.log("Mobile data file not found, using default");
            }
        }
        return dataFile;
    }

    function isASTCAvailable(canvas) {
        const webgl2 = canvas.getContext("webgl2");
        return webgl2 && webgl2.getExtension('WEBGL_compressed_texture_astc');
    }

    function buildConfig(dataFile) {
        const buildUrl = "Build";
        return {
            dataUrl: buildUrl + dataFile,
            frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
            codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
            #if MEMORY_FILENAME
                memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
            #endif
              #if SYMBOLS_FILENAME
                symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
            #endif
                streamingAssetsUrl: "StreamingAssets",
            companyName: "{{{ COMPANY_NAME }}}",
            productName: "{{{ PRODUCT_NAME }}}",
            productVersion: "{{{ PRODUCT_VERSION }}}",
        };
    }

    function isMobile() {
        return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    }

    function setupLoadingElements() {
        const loadingScreen = document.querySelector("#loading-screen");
        const loadingRing = document.querySelector("#loading-ring");
        const ringBackground = document.querySelector("#ring-background");
        const ringProgress = document.querySelector("#ring-progress");
        return { loadingScreen, loadingRing, ringBackground, ringProgress };
    }

    function handleFullscreenSupport() {
        for (const key of [
            'exitFullscreen', 'webkitExitFullscreen', 'webkitCancelFullScreen',
            'mozCancelFullScreen', 'msExitFullscreen'
        ]) {
            if (key in document) {
                return true;
            }
        }
        return false;
    }

    function startLoading(unityCanvas, loadingScreen, loaderUrl, config, onProgressCallback) {
        const script = document.createElement("script");
        script.src = loaderUrl;
        script.onload = () => {
            createUnityInstance(unityCanvas, config, onProgressCallback)
                .then(instance => {
                    window.unityInstance = instance;
                    loadingScreen.style.display = "none";
                })
                .catch(err => alert(err));
        };
        document.body.appendChild(script);
    }

    function onProgress(progressValue) {
        const circumference = 2 * Math.PI * 45;
        const offset = circumference - progressValue * circumference;

        window.ringProgress.style.strokeDashoffset = offset;
    }

    const canvas = document.querySelector("#unity-canvas");

    let dataFile = "/{{{ DATA_FILENAME }}}";
    dataFile = adjustDataFilePath(canvas, dataFile);

    const loaderUrl = "Build" + "/{{{ LOADER_FILENAME }}}";

    const config = buildConfig(dataFile);
    if (isMobile()) {
        config.devicePixelRatio = 2;

        var meta = document.createElement('meta');
        meta.name = 'viewport';
        meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
        document.getElementsByTagName('head')[0].appendChild(meta);
    }

    const { loadingScreen, loadingRing, ringBackground, ringProgress } = setupLoadingElements();
    window.ringProgress = ringProgress;

    loadingScreen.style.display = "";

    startLoading(canvas, loadingScreen, loaderUrl, config, onProgress);

})();