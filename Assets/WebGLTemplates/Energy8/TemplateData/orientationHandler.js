document.addEventListener('DOMContentLoaded', () => {
  const isLiteVersion = window.location.pathname.includes('/Lite');
  const requiredOrientation = isLiteVersion ? 'portrait' : 'landscape';

  function checkOrientation() {
    const width = window.innerWidth;
    const height = window.innerHeight;
    const isPortrait = height > width;
    return requiredOrientation === (isPortrait ? 'portrait' : 'landscape');
  }

  function showOrientationWarning() {
    const message = isLiteVersion
      ? 'Please rotate your device to portrait mode.'
      : 'Please rotate your device to landscape mode.';
    showWarning("OrientationWarning", message, []);
  }

  if (!checkOrientation()) {
    showOrientationWarning();
  }

  function handleOrientationChange() {
    if (checkOrientation()) {
      hideWarning("OrientationWarning");
    } else {
      showOrientationWarning();
    }
  }

  window.addEventListener('orientationchange', handleOrientationChange);
  window.addEventListener('resize', handleOrientationChange);
});
