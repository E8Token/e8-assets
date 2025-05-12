/**
 * Energy8 JS Plugin Tools - Device Module
 * Модуль для получения информации об устройстве и браузере пользователя
 * Namespace: Energy8.JSPluginTools.Device
 */

mergeInto(LibraryManager.library, {
  __JS_Device_Init: function() {
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools [Device]: Cannot initialize - core not initialized");
        return;
      }
      
      // Инициализация и публичный API для Device
      Energy8JSPluginTools.Device = {
        // Константа для префикса сообщений
        CHANNEL_PREFIX: 'device.',
        
        // Получить User-Agent строку браузера
        getUserAgent: function() {
          return navigator.userAgent;
        },
        
        // Определить информацию о браузере
        getBrowserInfo: function() {
          var browserInfo = {
            Name: 'Unknown',
            Version: 'Unknown',
            Engine: 'Unknown',
            CookiesEnabled: navigator.cookieEnabled,
            JavaScriptEnabled: true
          };
          
          var userAgent = navigator.userAgent;
          
          // Определение браузера
          if (/Firefox\/([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Firefox';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'Gecko';
          } else if (/MSIE ([0-9.]+)/.test(userAgent) || /Trident\/.*rv:([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Internet Explorer';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'Trident';
          } else if (/Chrome\/([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Chrome';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'Blink';
          } else if (/Safari\/([0-9.]+)/.test(userAgent) && /Version\/([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Safari';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'WebKit';
          } else if (/Edge\/([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Edge';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'EdgeHTML';
          } else if (/OPR\/([0-9.]+)/.test(userAgent)) {
            browserInfo.Name = 'Opera';
            browserInfo.Version = RegExp.$1;
            browserInfo.Engine = 'Blink';
          }
          
          return browserInfo;
        },
        
        // Определить информацию об ОС
        getOSInfo: function() {
          var osInfo = {
            Name: 'Unknown',
            Version: 'Unknown',
            Architecture: 'Unknown',
            IsMobile: this.isMobileDevice()
          };
          
          var userAgent = navigator.userAgent;
          
          // Определение ОС
          if (/Windows NT ([0-9.]+)/.test(userAgent)) {
            osInfo.Name = 'Windows';
            var versionMap = {
              '10.0': '10',
              '6.3': '8.1',
              '6.2': '8',
              '6.1': '7',
              '6.0': 'Vista',
              '5.2': 'XP',
              '5.1': 'XP',
              '5.0': '2000'
            };
            osInfo.Version = versionMap[RegExp.$1] || RegExp.$1;
          } else if (/Macintosh.+Mac OS X ([0-9._]+)/.test(userAgent)) {
            osInfo.Name = 'macOS';
            osInfo.Version = RegExp.$1.replace(/_/g, '.');
          } else if (/Android ([0-9.]+)/.test(userAgent)) {
            osInfo.Name = 'Android';
            osInfo.Version = RegExp.$1;
          } else if (/iPhone|iPad|iPod/.test(userAgent) && /OS ([0-9._]+)/.test(userAgent)) {
            osInfo.Name = 'iOS';
            osInfo.Version = RegExp.$1.replace(/_/g, '.');
          } else if (/Linux/.test(userAgent)) {
            osInfo.Name = 'Linux';
          }
          
          // Определение архитектуры
          if (/x86_64|x86-64|Win64|x64;|amd64|AMD64|WOW64|x64_64/.test(userAgent)) {
            osInfo.Architecture = 'x64';
          } else if (/i386|i686|x86|WIn32/.test(userAgent)) {
            osInfo.Architecture = 'x86';
          } else if (/arm|ARM|ARMv\d/.test(userAgent)) {
            osInfo.Architecture = 'ARM';
          }
          
          return osInfo;
        },
        
        // Получить информацию об экране устройства
        getScreenInfo: function() {
          var screenInfo = {
            Width: window.screen.width,
            Height: window.screen.height,
            PixelRatio: window.devicePixelRatio || 1,
            ColorDepth: window.screen.colorDepth || 24,
            TouchScreen: 'ontouchstart' in window || navigator.maxTouchPoints > 0,
            RefreshRate: 60 // Значение по умолчанию, точное значение не всегда доступно
          };
          
          // Попытка получить частоту обновления экрана в современных браузерах
          if (window.screen.width && 'getScreenDetails' in window) {
            try {
              var fps = window.screen.width.refreshRate;
              if (fps > 0) {
                screenInfo.RefreshRate = Math.round(fps);
              }
            } catch (e) {
              // Ignore errors
            }
          }
          
          return screenInfo;
        },
        
        // Проверить, является ли устройство мобильным
        isMobileDevice: function() {
          var userAgent = navigator.userAgent || navigator.vendor || window.opera;
          
          // Регулярные выражения для проверки мобильных устройств
          var mobileRegex = /(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i;
          var tabletRegex = /android|ipad|playbook|silk/i;
          
          return mobileRegex.test(userAgent) || 
            (tabletRegex.test(userAgent) && navigator.maxTouchPoints > 0);
        },
        
        // Получить предпочитаемый язык в браузере пользователя
        getLanguage: function() {
          return navigator.language || navigator.userLanguage || 'en-US';
        },
        
        // Получить информацию о часовом поясе
        getTimeZone: function() {
          var timezone = {
            Id: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
            DisplayName: '',
            OffsetMinutes: -(new Date().getTimezoneOffset()),
            DaylightSavingTime: this._isDST(new Date())
          };
          
          try {
            // Попытка получить отображаемое имя часового пояза
            var dateFormatter = new Intl.DateTimeFormat(this.getLanguage(), {
              timeZone: timezone.Id,
              timeZoneName: 'long'
            });
            
            var parts = dateFormatter.formatToParts(new Date());
            for (var i = 0; i < parts.length; i++) {
              if (parts[i].type === 'timeZoneName') {
                timezone.DisplayName = parts[i].value;
                break;
              }
            }
          } catch (e) {
            timezone.DisplayName = timezone.Id;
          }
          
          return timezone;
        },
        
        // Проверка, действует ли сейчас летнее время
        _isDST: function(date) {
          var jan = new Date(date.getFullYear(), 0, 1);
          var jul = new Date(date.getFullYear(), 6, 1);
          return date.getTimezoneOffset() < Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
        },
        
        // Получить информацию об аппаратном обеспечении устройства
        getHardwareInfo: function() {
          var hardwareInfo = {
            ProcessorCores: navigator.hardwareConcurrency || 1,
            DeviceMemory: navigator.deviceMemory || 0,
            GpuRenderer: 'Unknown',
            GpuVendor: 'Unknown',
            HasBattery: false,
            BatteryLevel: -1
          };
          
          // Попытка получить информацию о GPU, если доступно
          try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            if (gl) {
              var debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
              if (debugInfo) {
                hardwareInfo.GpuVendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL) || 'Unknown';
                hardwareInfo.GpuRenderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) || 'Unknown';
              }
            }
          } catch (e) {
            // Ignore errors
          }
          
          // Попытка получить информацию о батарее, если API доступно
          if ('getBattery' in navigator) {
            navigator.getBattery().then(function(battery) {
              hardwareInfo.HasBattery = true;
              hardwareInfo.BatteryLevel = battery.level * 100;
            });
          }
          
          return hardwareInfo;
        },
        
        // Инициализация обработчиков сообщений для этого модуля
        _registerHandlers: function() {
          const self = this;
          
          // Регистрация обработчиков для каждого типа запроса
          if (Energy8JSPluginTools.Communication) {
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getUserAgent',
              () => self.getUserAgent()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getBrowserInfo',
              () => self.getBrowserInfo()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getOSInfo',
              () => self.getOSInfo()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getScreenInfo',
              () => self.getScreenInfo()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'isMobileDevice',
              () => self.isMobileDevice()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getLanguage',
              () => self.getLanguage()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getTimeZone',
              () => self.getTimeZone()
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getHardwareInfo',
              () => self.getHardwareInfo()
            );
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log("Energy8JSPluginTools [Device]: Registered all channel handlers");
            }
          } else {
            console.error("Energy8JSPluginTools [Device]: Communication module not available");
          }
        }
      };
      
      // Регистрация обработчиков событий
      Energy8JSPluginTools.Device._registerHandlers();
      
      console.log("Energy8JSPluginTools [Device]: Module initialized");
      
      // Добавим информацию о доступных командах в консоль
      if (Energy8JSPluginTools._internal.debugMode) {
        console.info(
          "Energy8JSPluginTools.Device module available.\n" +
          "Try: Energy8JSPluginTools.Device.getBrowserInfo() to get browser info\n" +
          "     Energy8JSPluginTools.Device.getOSInfo() to get OS info\n" +
          "     Energy8JSPluginTools.Device.isMobileDevice() to check if device is mobile"
        );
      }
      
    } catch (e) {
      console.error("Energy8JSPluginTools [Device]: Error initializing module:", e);
    }
    
    return 0;
  }
});