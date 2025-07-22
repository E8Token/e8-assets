// Enhanced Progress Manager for Standard and Debug versions
class ProgressManager {
    constructor() {
        this.startTime = Date.now();
        this.currentStage = 'Initializing';
        this.totalFiles = 0;
        this.loadedFiles = 0;
        this.totalBytes = 0;
        this.loadedBytes = 0;
        this.speeds = [];
        this.lastUpdate = Date.now();
        
        this.elements = {
            progressContainer: document.getElementById('progress-container'),
            progressFill: document.getElementById('progress-fill'),
            progressPercentage: document.getElementById('progress-percentage'),
            progressCurrentFile: document.getElementById('progress-current-file'),
            progressSpeed: document.getElementById('progress-speed'),
            progressTitle: document.getElementById('progress-title'),
            loadingStage: document.getElementById('loading-stage'),
            loadingPercentage: document.getElementById('loading-percentage')
        };
        
        // Debug version specific elements
        if (document.body.dataset.version === 'debug') {
            this.debugElements = {
                progressFiles: document.getElementById('progress-files'),
                progressSize: document.getElementById('progress-size'),
                progressEta: document.getElementById('progress-eta'),
                loadingCurrentFile: document.getElementById('loading-current-file'),
                loadingSpeed: document.getElementById('loading-speed'),
                loadingMemory: document.getElementById('loading-memory'),
                loadingElapsed: document.getElementById('loading-elapsed')
            };
        }
    }
    
    updateProgress(progress, stage = null, currentFile = null) {
        const percentage = Math.round(progress * 100);
        const now = Date.now();
        
        // Update basic progress
        if (this.elements.progressFill) {
            this.elements.progressFill.style.width = `${percentage}%`;
        }
        
        if (this.elements.progressPercentage) {
            this.elements.progressPercentage.textContent = `${percentage}%`;
        }
        
        if (this.elements.loadingPercentage) {
            this.elements.loadingPercentage.textContent = `${percentage}%`;
        }
        
        // Update stage
        if (stage && this.elements.loadingStage) {
            this.currentStage = stage;
            this.elements.loadingStage.textContent = stage;
        }
        
        // Update current file
        if (currentFile && this.elements.progressCurrentFile) {
            this.elements.progressCurrentFile.textContent = this.truncateFileName(currentFile);
        }
        
        // Calculate speed
        const timeDiff = now - this.lastUpdate;
        if (timeDiff > 100) { // Update speed every 100ms
            const progressDiff = progress - (this.lastProgress || 0);
            const speed = progressDiff / (timeDiff / 1000); // progress per second
            this.speeds.push(speed);
            if (this.speeds.length > 10) this.speeds.shift();
            
            const avgSpeed = this.speeds.reduce((a, b) => a + b, 0) / this.speeds.length;
            this.updateSpeed(avgSpeed, progress);
            
            this.lastProgress = progress;
            this.lastUpdate = now;
        }
        
        // Debug version updates
        if (this.debugElements) {
            this.updateDebugInfo(progress, currentFile);
        }
        
        // Update ring progress for SVG loading ring
        this.updateRingProgress(progress);
    }
    
    updateSpeed(speed, currentProgress) {
        if (!this.elements.progressSpeed) return;
        
        if (speed > 0) {
            const eta = (1 - currentProgress) / speed;
            const speedText = this.formatSpeed(speed);
            const etaText = this.formatTime(eta);
            
            this.elements.progressSpeed.textContent = `${speedText} • ETA: ${etaText}`;
            
            if (this.debugElements && this.debugElements.progressEta) {
                this.debugElements.progressEta.textContent = etaText;
            }
        }
    }
    
    updateDebugInfo(progress, currentFile) {
        if (!this.debugElements) return;
        
        // Update files count
        if (this.debugElements.progressFiles) {
            this.debugElements.progressFiles.textContent = `${this.loadedFiles}/${this.totalFiles}`;
        }
        
        // Update size
        if (this.debugElements.progressSize) {
            const loadedMB = (this.loadedBytes / 1024 / 1024).toFixed(1);
            const totalMB = (this.totalBytes / 1024 / 1024).toFixed(1);
            this.debugElements.progressSize.textContent = `${loadedMB}/${totalMB} MB`;
        }
        
        // Update current file in debug loading
        if (currentFile && this.debugElements.loadingCurrentFile) {
            this.debugElements.loadingCurrentFile.textContent = this.truncateFileName(currentFile, 30);
        }
        
        // Update elapsed time
        if (this.debugElements.loadingElapsed) {
            const elapsed = (Date.now() - this.startTime) / 1000;
            this.debugElements.loadingElapsed.textContent = `${elapsed.toFixed(1)}s`;
        }
        
        // Update memory (if available)
        if (this.debugElements.loadingMemory && performance.memory) {
            const usedMB = (performance.memory.usedJSHeapSize / 1024 / 1024).toFixed(1);
            this.debugElements.loadingMemory.textContent = `${usedMB} MB`;
        }
    }
    
    updateRingProgress(progress) {
        const ringProgress = document.getElementById('ring-progress');
        if (ringProgress) {
            const circumference = 2 * Math.PI * 45; // radius = 45
            const offset = circumference - (progress * circumference);
            ringProgress.style.strokeDashoffset = offset;
        }
    }
    
    setStage(stage, title = null) {
        this.currentStage = stage;
        
        if (this.elements.loadingStage) {
            this.elements.loadingStage.textContent = stage;
        }
        
        if (title && this.elements.progressTitle) {
            this.elements.progressTitle.textContent = title;
        }
    }
    
    setFileInfo(totalFiles, totalBytes) {
        this.totalFiles = totalFiles;
        this.totalBytes = totalBytes;
    }
    
    incrementFile(fileName, fileSize) {
        this.loadedFiles++;
        this.loadedBytes += fileSize || 0;
        
        if (this.debugElements) {
            this.updateDebugInfo(this.lastProgress || 0, fileName);
        }
    }
    
    show() {
        if (this.elements.progressContainer) {
            this.elements.progressContainer.style.display = 'block';
        }
    }
    
    hide() {
        if (this.elements.progressContainer) {
            this.elements.progressContainer.style.display = 'none';
        }
    }
    
    truncateFileName(fileName, maxLength = 25) {
        if (!fileName || fileName.length <= maxLength) return fileName;
        
        const extension = fileName.split('.').pop();
        const nameWithoutExt = fileName.substring(0, fileName.lastIndexOf('.'));
        const truncatedName = nameWithoutExt.substring(0, maxLength - extension.length - 4) + '...';
        
        return `${truncatedName}.${extension}`;
    }
    
    formatSpeed(progressPerSecond) {
        const percentage = progressPerSecond * 100;
        if (percentage > 1) return `${percentage.toFixed(0)}%/s`;
        if (percentage > 0.1) return `${percentage.toFixed(1)}%/s`;
        return `${percentage.toFixed(2)}%/s`;
    }
    
    formatTime(seconds) {
        if (seconds < 60) return `${seconds.toFixed(0)}s`;
        if (seconds < 3600) return `${Math.floor(seconds / 60)}m ${seconds % 60 | 0}s`;
        return `${Math.floor(seconds / 3600)}h ${Math.floor((seconds % 3600) / 60)}m`;
    }
}

// Initialize progress manager
let progressManager;
document.addEventListener('DOMContentLoaded', () => {
    progressManager = new ProgressManager();
});

// Export for Unity integration
window.UpdateLoadingProgress = function(progress, stage, currentFile) {
    if (progressManager) {
        progressManager.updateProgress(progress, stage, currentFile);
    }
};

window.SetLoadingStage = function(stage, title) {
    if (progressManager) {
        progressManager.setStage(stage, title);
    }
};

window.ShowProgressBar = function() {
    if (progressManager) {
        progressManager.show();
    }
};

window.HideProgressBar = function() {
    if (progressManager) {
        progressManager.hide();
    }
};
