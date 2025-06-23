// Enhanced Orientation Handler with better UX
document.addEventListener('DOMContentLoaded', () => {
  const version = document.body.dataset.version || 'standard';
  const isLiteVersion = version === 'lite';
  const isDebugVersion = version === 'debug';
  const requiredOrientation = isLiteVersion ? 'portrait' : 'landscape';
  
  let orientationWarning = null;
  let orientationHint = null;
  let hideTimer = null;

  function getOrientation() {
    const width = window.innerWidth;
    const height = window.innerHeight;
    return height > width ? 'portrait' : 'landscape';
  }

  function isCorrectOrientation() {
    return requiredOrientation === getOrientation();
  }

  function showOrientationWarning() {
    hideOrientationHint(); // Hide hint when showing warning
    
    if (isLiteVersion) {
      // For lite version, just show a hint
      showOrientationHint();
      return;
    }
    
    // For standard/debug versions, show full warning
    if (!orientationWarning) {
      orientationWarning = document.getElementById('orientation-warning');
      if (!orientationWarning) {
        createOrientationWarning();
      }
    }
    
    if (orientationWarning) {
      orientationWarning.style.display = 'flex';
      orientationWarning.setAttribute('aria-hidden', 'false');
    }
  }

  function hideOrientationWarning() {
    if (orientationWarning) {
      orientationWarning.style.display = 'none';
      orientationWarning.setAttribute('aria-hidden', 'true');
    }
  }

  function showOrientationHint() {
    if (!orientationHint) {
      orientationHint = document.getElementById('orientation-hint');
    }
    
    if (orientationHint) {
      orientationHint.style.display = 'block';
      
      // Auto-hide hint after 3 seconds
      if (hideTimer) clearTimeout(hideTimer);
      hideTimer = setTimeout(() => {
        hideOrientationHint();
      }, 3000);
    }
  }

  function hideOrientationHint() {
    if (orientationHint) {
      orientationHint.style.display = 'none';
    }
    if (hideTimer) {
      clearTimeout(hideTimer);
      hideTimer = null;
    }
  }

  function createOrientationWarning() {
    const warning = document.createElement('div');
    warning.id = 'orientation-warning';
    warning.className = 'orientation-warning';
    warning.setAttribute('aria-hidden', 'true');
    
    const content = document.createElement('div');
    content.className = 'orientation-content';
    
    const icon = document.createElement('div');
    icon.className = 'orientation-icon';
    icon.textContent = '📱';
    
    const title = document.createElement('h3');
    title.textContent = 'Please Rotate Your Device';
    
    const description = document.createElement('p');
    description.textContent = `For the best experience, please use ${requiredOrientation} orientation.`;
    
    const demo = document.createElement('div');
    demo.className = 'orientation-demo';
    
    const phoneIcon = document.createElement('div');
    phoneIcon.className = `phone-icon ${requiredOrientation === 'landscape' ? 'landscape-icon' : ''}`;
    
    demo.appendChild(phoneIcon);
    content.appendChild(icon);
    content.appendChild(title);
    content.appendChild(description);
    content.appendChild(demo);
    warning.appendChild(content);
    
    document.body.appendChild(warning);
    orientationWarning = warning;
  }

  function handleOrientationChange() {
    // Small delay to ensure dimensions are updated
    setTimeout(() => {
      if (isCorrectOrientation()) {
        hideOrientationWarning();
        hideOrientationHint();
      } else {
        showOrientationWarning();
      }
    }, 100);
  }

  // Initial check
  if (!isCorrectOrientation()) {
    showOrientationWarning();
  }

  // Event listeners
  window.addEventListener('orientationchange', handleOrientationChange);
  window.addEventListener('resize', handleOrientationChange);
  
  // For debug version, add manual orientation toggle
  if (isDebugVersion) {
    window.toggleOrientationWarning = function() {
      if (orientationWarning && orientationWarning.style.display === 'flex') {
        hideOrientationWarning();
      } else {
        showOrientationWarning();
      }
    };
  }
  
  // Expose functions for Unity integration
  window.ShowOrientationWarning = showOrientationWarning;
  window.HideOrientationWarning = hideOrientationWarning;
  window.GetCurrentOrientation = getOrientation;
  window.IsCorrectOrientation = isCorrectOrientation;
});
