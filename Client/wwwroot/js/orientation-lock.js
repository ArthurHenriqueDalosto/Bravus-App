window.getOrientation = () => {
    if (window.matchMedia("(orientation: portrait)").matches)
        return "portrait";
    else
        return "landscape";
};

window.tryLockLandscape = async () => {
    if (screen.orientation && screen.orientation.lock) {
        try {
            await screen.orientation.lock('landscape');
            return true;
        } catch (err) {
            console.warn('Falha ao travar orientação:', err);
            return false;
        }
    }
    return false;
};

window.requestFullScreen = () => {
    const elem = document.documentElement;

    if (elem.requestFullscreen) {
        elem.requestFullscreen();
    } else if (elem.webkitRequestFullscreen) { /* Safari */
        elem.webkitRequestFullscreen();
    } else if (elem.msRequestFullscreen) { /* IE11 */
        elem.msRequestFullscreen();
    }
};