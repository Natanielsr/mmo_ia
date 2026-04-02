export function isMobileDevice(): boolean {
    const hasTouchScreen = (
        'maxTouchPoints' in navigator && navigator.maxTouchPoints > 0
    ) || (
        'msMaxTouchPoints' in navigator && (navigator as any).msMaxTouchPoints > 0
    );

    const isMobileAgent = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    
    return hasTouchScreen && isMobileAgent;
}
