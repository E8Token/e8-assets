// Touch Helper for Lite version - provides touch interaction hints
class TouchHelper {
    constructor() {
        this.isEnabled = document.body.dataset.version === 'lite';
        this.touchHelper = null;
        this.lastInteraction = Date.now();
        this.interactionTimeout = null;
        this.isVisible = false;
        
        if (this.isEnabled) {
            this.init();
        }
    }
    
    init() {
        this.touchHelper = document.getElementById('touch-helper');
        
        if (!this.touchHelper) {
            this.createTouchHelper();
        }
        
        this.setupEventListeners();
        this.showInitialHint();
    }
    
    createTouchHelper() {
        const helper = document.createElement('div');
        helper.id = 'touch-helper';
        helper.className = 'touch-helper';
        helper.style.display = 'none';
        
        const instruction = document.createElement('div');
        instruction.className = 'touch-instruction';
        instruction.innerHTML = '<span>👆 Tap to interact</span>';
        
        helper.appendChild(instruction);
        document.body.appendChild(helper);
        
        this.touchHelper = helper;
    }
    
    setupEventListeners() {
        // Track user interactions
        const events = ['touchstart', 'touchend', 'click', 'keydown'];
        
        events.forEach(event => {
            document.addEventListener(event, () => {
                this.onUserInteraction();
            }, { passive: true });
        });
        
        // Show hint when Unity game is loaded and idle
        document.addEventListener('unity-loaded', () => {
            this.scheduleHint(3000); // Show hint after 3 seconds of inactivity
        });
        
        // Listen for Unity events that might need touch hints
        window.addEventListener('message', (event) => {
            if (event.data && event.data.type === 'unity-needs-interaction') {
                this.showHint(event.data.message || '👆 Tap to interact');
            }
        });
    }
    
    onUserInteraction() {
        this.lastInteraction = Date.now();
        this.hideHint();
        
        // Clear existing timeout
        if (this.interactionTimeout) {
            clearTimeout(this.interactionTimeout);
        }
        
        // Schedule next hint if idle for too long
        this.scheduleHint(10000); // 10 seconds of inactivity
    }
    
    scheduleHint(delay) {
        if (this.interactionTimeout) {
            clearTimeout(this.interactionTimeout);
        }
        
        this.interactionTimeout = setTimeout(() => {
            const timeSinceLastInteraction = Date.now() - this.lastInteraction;
            if (timeSinceLastInteraction >= delay) {
                this.showHint();
            }
        }, delay);
    }
    
    showHint(message = '👆 Tap to interact') {
        if (!this.touchHelper || this.isVisible) return;
        
        const instruction = this.touchHelper.querySelector('.touch-instruction span');
        if (instruction) {
            instruction.textContent = message;
        }
        
        this.touchHelper.style.display = 'block';
        this.isVisible = true;
        
        // Auto-hide after 4 seconds
        setTimeout(() => {
            this.hideHint();
        }, 4000);
    }
    
    hideHint() {
        if (!this.touchHelper || !this.isVisible) return;
        
        this.touchHelper.style.display = 'none';
        this.isVisible = false;
    }
    
    showInitialHint() {
        // Show initial hint after page load
        setTimeout(() => {
            if (Date.now() - this.lastInteraction > 2000) {
                this.showHint('👆 Tap anywhere to start');
            }
        }, 2000);
    }
    
    updateHintMessage(message) {
        const instruction = this.touchHelper?.querySelector('.touch-instruction span');
        if (instruction) {
            instruction.textContent = message;
        }
    }
    
    setHintPosition(position) {
        if (!this.touchHelper) return;
        
        // Reset position classes
        this.touchHelper.classList.remove('hint-top', 'hint-bottom', 'hint-left', 'hint-right');
        
        switch (position) {
            case 'top':
                this.touchHelper.classList.add('hint-top');
                break;
            case 'bottom':
                this.touchHelper.classList.add('hint-bottom');
                break;
            case 'left':
                this.touchHelper.classList.add('hint-left');
                break;
            case 'right':
                this.touchHelper.classList.add('hint-right');
                break;
        }
    }
}

// Initialize touch helper when DOM is ready
let touchHelper;
document.addEventListener('DOMContentLoaded', () => {
    touchHelper = new TouchHelper();
});

// Expose functions for Unity integration
window.ShowTouchHint = function(message, duration) {
    if (touchHelper) {
        touchHelper.showHint(message);
        if (duration) {
            setTimeout(() => touchHelper.hideHint(), duration);
        }
    }
};

window.HideTouchHint = function() {
    if (touchHelper) {
        touchHelper.hideHint();
    }
};

window.UpdateTouchHint = function(message) {
    if (touchHelper) {
        touchHelper.updateHintMessage(message);
    }
};

window.SetTouchHintPosition = function(position) {
    if (touchHelper) {
        touchHelper.setHintPosition(position);
    }
};
