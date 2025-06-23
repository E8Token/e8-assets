// Performance Monitor for Standard and Debug versions
class PerformanceMonitor {
    constructor() {
        this.isEnabled = ['standard', 'debug'].includes(document.body.dataset.version);
        this.isDebugVersion = document.body.dataset.version === 'debug';
        this.monitor = null;
        this.updateInterval = null;
        this.isVisible = false;
        
        // Performance metrics
        this.frameCount = 0;
        this.lastTime = performance.now();
        this.fps = 0;
        this.frameTime = 0;
        this.memoryUsage = 0;
        this.gpuMemory = 0;
        this.drawCalls = 0;
        this.triangles = 0;
        
        if (this.isEnabled) {
            this.init();
        }
    }
    
    init() {
        this.createMonitor();
        this.setupEventListeners();
        
        // Auto-show for debug version
        if (this.isDebugVersion) {
            this.show();
        }
    }
    
    createMonitor() {
        const className = this.isDebugVersion ? 'performance-monitor-debug' : 'performance-monitor';
        
        this.monitor = document.createElement('div');
        this.monitor.id = 'performance-monitor';
        this.monitor.className = className;
        this.monitor.style.display = 'none';
        
        if (this.isDebugVersion) {
            this.createDebugMonitor();
        } else {
            this.createStandardMonitor();
        }
        
        document.body.appendChild(this.monitor);
    }
    
    createStandardMonitor() {
        this.monitor.innerHTML = `
            <div class="perf-item">
                <span>FPS:</span>
                <span id="fps-counter">--</span>
            </div>
            <div class="perf-item">
                <span>Memory:</span>
                <span id="memory-usage">--</span>
            </div>
        `;
    }
    
    createDebugMonitor() {
        this.monitor.innerHTML = `
            <div class="perf-header">Performance Monitor</div>
            <div class="perf-grid">
                <div class="perf-item">
                    <span class="perf-label">FPS:</span>
                    <span id="fps-counter" class="perf-value">--</span>
                </div>
                <div class="perf-item">
                    <span class="perf-label">Frame Time:</span>
                    <span id="frame-time" class="perf-value">--ms</span>
                </div>
                <div class="perf-item">
                    <span class="perf-label">Memory:</span>
                    <span id="memory-usage" class="perf-value">--MB</span>
                </div>
                <div class="perf-item">
                    <span class="perf-label">GPU Memory:</span>
                    <span id="gpu-memory" class="perf-value">--MB</span>
                </div>
                <div class="perf-item">
                    <span class="perf-label">Draw Calls:</span>
                    <span id="draw-calls" class="perf-value">--</span>
                </div>
                <div class="perf-item">
                    <span class="perf-label">Triangles:</span>
                    <span id="triangles" class="perf-value">--</span>
                </div>
            </div>
        `;
    }
    
    setupEventListeners() {
        // Toggle visibility with keyboard shortcut
        document.addEventListener('keydown', (event) => {
            if (event.ctrlKey && event.key === 'p') {
                event.preventDefault();
                this.toggle();
            }
        });
        
        // Listen for Unity performance data
        window.addEventListener('message', (event) => {
            if (event.data && event.data.type === 'unity-performance') {
                this.updateFromUnity(event.data);
            }
        });
    }
    
    show() {
        if (!this.monitor) return;
        
        this.monitor.style.display = 'block';
        this.isVisible = true;
        this.startMonitoring();
    }
    
    hide() {
        if (!this.monitor) return;
        
        this.monitor.style.display = 'none';
        this.isVisible = false;
        this.stopMonitoring();
    }
    
    toggle() {
        if (this.isVisible) {
            this.hide();
        } else {
            this.show();
        }
    }
    
    startMonitoring() {
        if (this.updateInterval) return;
        
        this.updateInterval = setInterval(() => {
            this.updateMetrics();
        }, 1000); // Update every second
        
        // Request animation frame for FPS calculation
        this.requestFrame();
    }
    
    stopMonitoring() {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
    }
    
    requestFrame() {
        if (!this.isVisible) return;
        
        requestAnimationFrame((timestamp) => {
            this.calculateFPS(timestamp);
            this.requestFrame();
        });
    }
    
    calculateFPS(timestamp) {
        this.frameCount++;
        const delta = timestamp - this.lastTime;
        
        if (delta >= 1000) { // Update FPS every second
            this.fps = Math.round((this.frameCount * 1000) / delta);
            this.frameTime = Math.round(delta / this.frameCount * 100) / 100;
            this.frameCount = 0;
            this.lastTime = timestamp;
        }
    }
    
    updateMetrics() {
        // Update FPS
        const fpsElement = document.getElementById('fps-counter');
        if (fpsElement) {
            fpsElement.textContent = this.fps.toString();
            
            // Color code FPS
            if (this.fps >= 60) {
                fpsElement.style.color = '#00ff00';
            } else if (this.fps >= 30) {
                fpsElement.style.color = '#ffff00';
            } else {
                fpsElement.style.color = '#ff0000';
            }
        }
        
        // Update memory usage
        if (performance.memory) {
            this.memoryUsage = Math.round(performance.memory.usedJSHeapSize / 1024 / 1024);
            const memoryElement = document.getElementById('memory-usage');
            if (memoryElement) {
                memoryElement.textContent = this.isDebugVersion ? 
                    `${this.memoryUsage}MB` : `${this.memoryUsage}MB`;
            }
        }
        
        // Update frame time (debug only)
        if (this.isDebugVersion) {
            const frameTimeElement = document.getElementById('frame-time');
            if (frameTimeElement) {
                frameTimeElement.textContent = `${this.frameTime}ms`;
            }
        }
    }
    
    updateFromUnity(data) {
        // Update Unity-specific metrics
        if (data.drawCalls !== undefined) {
            this.drawCalls = data.drawCalls;
            const element = document.getElementById('draw-calls');
            if (element) element.textContent = this.drawCalls.toString();
        }
        
        if (data.triangles !== undefined) {
            this.triangles = data.triangles;
            const element = document.getElementById('triangles');
            if (element) element.textContent = this.triangles.toLocaleString();
        }
        
        if (data.gpuMemory !== undefined) {
            this.gpuMemory = Math.round(data.gpuMemory / 1024 / 1024);
            const element = document.getElementById('gpu-memory');
            if (element) element.textContent = `${this.gpuMemory}MB`;
        }
        
        if (data.fps !== undefined) {
            this.fps = data.fps;
        }
    }
    
    getMetrics() {
        return {
            fps: this.fps,
            frameTime: this.frameTime,
            memoryUsage: this.memoryUsage,
            gpuMemory: this.gpuMemory,
            drawCalls: this.drawCalls,
            triangles: this.triangles
        };
    }
}

// Initialize performance monitor
let performanceMonitor;
document.addEventListener('DOMContentLoaded', () => {
    performanceMonitor = new PerformanceMonitor();
});

// Expose functions for Unity integration
window.ShowPerformanceMonitor = function() {
    if (performanceMonitor) {
        performanceMonitor.show();
    }
};

window.HidePerformanceMonitor = function() {
    if (performanceMonitor) {
        performanceMonitor.hide();
    }
};

window.TogglePerformanceMonitor = function() {
    if (performanceMonitor) {
        performanceMonitor.toggle();
    }
};

window.UpdateUnityPerformance = function(drawCalls, triangles, gpuMemory, unityFps) {
    if (performanceMonitor) {
        performanceMonitor.updateFromUnity({
            drawCalls: drawCalls,
            triangles: triangles,
            gpuMemory: gpuMemory,
            fps: unityFps
        });
    }
};

window.GetPerformanceMetrics = function() {
    if (performanceMonitor) {
        return performanceMonitor.getMetrics();
    }
    return null;
};
