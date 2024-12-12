const warningsQueue = {};

const warningModal = document.getElementById('warning-modal');
const warningMessage = document.getElementById('warning-message');
const warningButtons = document.getElementById('warning-buttons');

function showWarning(name, message, buttons = []) {
    if (warningModal.style.display === 'flex') {
        const currentWarning = warningModal.getAttribute('data-current-warning');
        if (currentWarning != name)
            warningsQueue[name] = { message, buttons };
        return;
    }

    warningMessage.textContent = message;
    warningButtons.innerHTML = '';

    buttons.forEach(button => {
        const btn = document.createElement('button');
        btn.textContent = button.label;
        btn.onclick = button.onClick;
        warningButtons.appendChild(btn);
    });

    warningModal.style.display = 'flex';
    warningModal.setAttribute('aria-hidden', 'false');
    warningModal.setAttribute('data-current-warning', name);
}

function hideWarning(name) {
    const currentWarning = warningModal.getAttribute('data-current-warning');
    if (currentWarning === name) {
        warningModal.style.display = 'none';
        warningModal.setAttribute('aria-hidden', 'true');

        const nextWarningName = Object.keys(warningsQueue)[0];
        if (nextWarningName) {
            const nextWarning = warningsQueue[nextWarningName];
            showWarning(nextWarningName, nextWarning.message, nextWarning.buttons);
            delete warningsQueue[nextWarningName];
        }
    } else {
        delete warningsQueue[name];
    }
}

window.showWarning = showWarning;
window.hideWarning = hideWarning;