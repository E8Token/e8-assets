(function () {
    function onProgress(progressValue) {
        const circumference = 2 * Math.PI * 45;
        const offset = circumference - progressValue * circumference;
        window.ringProgress.style.strokeDashoffset = offset;
    }

    (async function () {
        async function getResourcesConfig() {
            const response = await fetch('Build/Build.json', { cache: 'no-cache' });
            return response.ok ? await response.json() : null;
        }

        function getBestCompression() {
            let supportsBrotli = false;
            let supportsGzip = true;
            try {
                const request = new Request('data:text/plain,', { headers: { 'Accept-Encoding': 'br' } });
                supportsBrotli = request.headers.get('Accept-Encoding').includes('br');
            } catch (e) {
                supportsBrotli = 'CompressionStream' in window;
            }
            return supportsBrotli ? 'br' : (supportsGzip ? 'gz' : 'none');
        }

        function getBestTextureFormat() {
            const canvas = document.createElement('canvas');
            const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
            const isMobile = window.utils && window.utils.isMobile && window.utils.isMobile();
            if (!isMobile) return 'dxt';
            if (gl && gl.getExtension('WEBGL_compressed_texture_astc')) return 'astc';
            return 'etc2';
        }

        function selectResource(resources, type, options = {}) {
            let list = resources[type];
            if (!list) return null;
            if (type === 'data') {
                // Фолбеки по формату: сначала запрошенный, потом dxt, astc, etc2
                const formats = [options.format, 'dxt', 'astc', 'etc2'];
                let found = null;
                for (const fmt of formats) {
                    found = list.find(f => f.format === fmt && f.compression === options.compression)?.filename;
                    if (!found && options.compression === 'br') {
                        found = list.find(f => f.format === fmt && f.compression === 'gz')?.filename;
                    }
                    if (!found) {
                        found = list.find(f => f.format === fmt && f.compression === 'none')?.filename;
                    }
                    if (found) break;
                }
                return found;
            }
            let found = list.find(f => f.compression === options.compression)?.filename;
            if (!found && options.compression === 'br') {
                found = list.find(f => f.compression === 'gz')?.filename;
            }
            if (!found) {
                found = list.find(f => f.compression === 'none')?.filename;
            }
            return found;
        }

        // Загружаем ресурсы
        const resources = await getResourcesConfig();
        const compression = getBestCompression();
        const textureFormat = getBestTextureFormat();

        // Выбираем файлы
        console.log('[Resource Selection]');
        console.log('Available resources:', resources);
        console.log('Compression:', compression);
        console.log('Texture Format:', textureFormat);

        const dataFile = selectResource(resources, 'data', { format: textureFormat, compression });
        if (!dataFile) {
            const dataList = resources?.data || [];
            console.warn('[Resource Selection] No dataFile found for format:', textureFormat, 'compression:', compression);
            console.warn('[Resource Selection] Available data files:', dataList.map(f => ({ format: f.format, compression: f.compression, filename: f.filename })));
        }
        const frameworkFile = selectResource(resources, 'framework', { compression });
        if (!frameworkFile) {
            const frameworkList = resources?.framework || [];
            console.warn('[Resource Selection] No frameworkFile found for compression:', compression);
            console.warn('[Resource Selection] Available framework files:', frameworkList.map(f => ({ compression: f.compression, filename: f.filename })));
        }
        const wasmFile = selectResource(resources, 'wasm', { compression });
        if (!wasmFile) {
            const wasmList = resources?.wasm || [];
            console.warn('[Resource Selection] No wasmFile found for compression:', compression);
            console.warn('[Resource Selection] Available wasm files:', wasmList.map(f => ({ compression: f.compression, filename: f.filename })));
        }
        const symbolsFile = selectResource(resources, 'symbols', { compression });
        if (!symbolsFile) {
            const symbolsList = resources?.symbols || [];
            console.warn('[Resource Selection] No symbolsFile found for compression:', compression);
            console.warn('[Resource Selection] Available symbols files:', symbolsList.map(f => ({ compression: f.compression, filename: f.filename })));
        }

        console.log('Selected dataFile:', dataFile);
        console.log('Selected frameworkFile:', frameworkFile);
        console.log('Selected wasmFile:', wasmFile);
        console.log('Selected symbolsFile:', symbolsFile);

        function buildConfig() {
            const buildUrl = "Build";
            return {
                dataUrl: dataFile ? buildUrl + '/' + dataFile : null,
                frameworkUrl: frameworkFile ? buildUrl + '/' + frameworkFile : null,
                codeUrl: wasmFile ? buildUrl + '/' + wasmFile : null,
                symbolsUrl: symbolsFile ? buildUrl + '/' + symbolsFile : undefined,
                streamingAssetsUrl: "StreamingAssets",
                companyName: "{{{ COMPANY_NAME }}}",
                productName: "{{{ PRODUCT_NAME }}}",
                productVersion: "{{{ PRODUCT_VERSION }}}",
            };
        }

        const canvas = document.querySelector("#unity-canvas");
        const loaderUrl = "Build" + "/{{{ LOADER_FILENAME }}}";
        const config = buildConfig();
        if (window.utils && window.utils.isMobile && window.utils.isMobile()) {
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