(function () {
    function onProgress(progressValue) {
        const circumference = 2 * Math.PI * 45;
        const offset = circumference - progressValue * circumference;
        window.ringProgress.style.strokeDashoffset = offset;
    }

    (async function () {
        const dataTypes = await window.utils.getDataTypes();
        const textureFormat = window.utils.getBestTextureFormat();

        console.log('[Resource Selection]');
        console.log('Available data types:', dataTypes);
        console.log('Texture Format:', textureFormat);

        const dataFile = window.utils.selectDataFile(dataTypes);
        
        if (!dataFile) {
            console.warn('[Resource Selection] No dataFile found for format:', textureFormat);
            console.warn('[Resource Selection] Available data files:', dataTypes || {});
        }

        console.log('Selected dataFile:', dataFile);

        function buildConfig() {
            const buildUrl = "Build";
            return {
                dataUrl: dataFile ? buildUrl + '/' + dataFile : null,
                frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
#if USE_THREADS
                workerUrl: buildUrl + "/{{{ WORKER_FILENAME }}}",
#endif
#if USE_WASM
                codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
#endif
#if SYMBOLS_FILENAME
                symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
#endif
                streamingAssetsUrl: "StreamingAssets",
                companyName: "{{{ COMPANY_NAME }}}",
                productName: "{{{ PRODUCT_NAME }}}",
                productVersion: "{{{ PRODUCT_VERSION }}}",
                autoSyncPersistentDataPath: true
            };
        }

        const canvas = document.querySelector("#unity-canvas");
        const loaderUrl = "Build" + "/{{{ LOADER_FILENAME }}}";
        const config = buildConfig();
        if (window.utils && window.utils.isMobile()) {
            config.devicePixelRatio = 2;
            var meta = document.createElement('meta');
            meta.name = 'viewport';
            meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
            document.getElementsByTagName('head')[0].appendChild(meta);
        }
        const loadingScreen = document.querySelector("#loading-screen");
        const ringProgress = document.querySelector("#ring-progress");
        window.ringProgress = ringProgress;
        loadingScreen.style.display = "";
        const script = document.createElement("script");
        script.src = loaderUrl;
        script.onload = () => {
            createUnityInstance(canvas, config, onProgress)
                .then(instance => {
                    window.unityInstance = instance;
                    loadingScreen.style.display = "none";
                })
                .catch(err => alert(err));
        };
        document.body.appendChild(script);
    })();
})();