// wwwroot/updateManager.js
// تابع نمایش پیام تأیید آپدیت
window.showUpdateConfirmation = function (message) {
    return new Promise((resolve) => {
        // استفاده از SweetAlert2 برای پیام زیباتر (اختیاری)
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'آپدیت جدید',
                text: message,
                icon: 'info',
                showCancelButton: true,
                confirmButtonText: 'رفرش کن',
                cancelButtonText: 'بعداً',
                background: '#f8f9fa',
                confirmButtonColor: '#3085d6'
            }).then((result) => {
                resolve(result.isConfirmed);
            });
        } else {
            // استفاده از confirm پیشفرض مرورگر
            resolve(confirm(message));
        }
    });
};

// تابع بررسی آپدیت به صورت دوره‌ای
function startUpdateChecker(checkInterval = 300000) {
    return;
    setInterval(async () => {
        try {
            // وقتی کاربر به تب برمی‌گردد چک کن
            if (!document.hidden) {
                const response = await fetch('/version.txt?t=' + Date.now());
                const latestVersion = await response.text();
                const currentVersion = '1.0.2'; // باید با برنامه sync شود

                if (latestVersion.trim() !== currentVersion.trim()) {
                    await window.showUpdateConfirmation(
                        `نسخه جدید ${latestVersion} موجود است. برای دریافت آپدیت صفحه را رفرش کنید.`
                    );
                }
            }
        } catch (error) {
            console.log('Error in periodic update check:', error);
        }
    }, checkInterval);
}

// Ensure it's globally accessible
window.startUpdateChecker = startUpdateChecker;

// مدیریت pending update هنگام بازگشت تب
// Make handler async so we can await forceUpdate
document.addEventListener('visibilitychange', async function () {
    if (!document.hidden) {
        // وقتی کاربر به تب برمی‌گردد، چک کن آپدیت باشد
        const pendingUpdate = localStorage.getItem('pending-update');
        if (pendingUpdate) {
            try {
                const confirmed = await window.showUpdateConfirmation(
                    `نسخه جدید ${pendingUpdate} منتظر نصب است. می‌خواهید الآن نصب کنید؟`
                );
                if (confirmed) {
                    localStorage.removeItem('pending-update');
                    // Use forceUpdate if available to clear caches and unregister service worker
                    if (typeof window.forceUpdate === 'function') {
                        try {
                            await window.forceUpdate();
                        } catch (err) {
                            console.log('forceUpdate failed, falling back to reload', err);
                            location.reload();
                        }
                    } else {
                        location.reload();
                    }
                }
            } catch (err) {
                console.log('Error handling pending update confirmation:', err);
            }
        }
    }
});


// تابع پاکسازی کش‌ها
window.clearCaches = async function () {
    try {
        if (typeof caches !== 'undefined') {
            const cacheNames = await caches.keys();
            await Promise.all(
                cacheNames.map(cacheName => caches.delete(cacheName))
            );
            console.log('All caches cleared');
        } else {
            console.log('Caching not supported in this environment');
        }
    } catch (error) {
        console.log('Error clearing caches:', error);
    }
};

// تابع ریلود اجباری: پاک کردن کش‌ها، unregister سرویس ورکر و پاکسازی localStorage مربوط به کش‌ها
window.forceUpdate = async function () {
    try {
        // 1. پاک کردن کش‌ها
        await window.clearCaches();

        // 2. unregister سرویس ورکر فعلی
        try {
            if ('serviceWorker' in navigator) {
                const registration = await navigator.serviceWorker.getRegistration();
                if (registration) {
                    await registration.unregister();
                    console.log('Service worker unregistered');
                }
            }
        } catch (swError) {
            console.log('Error unregistering service worker:', swError);
        }

        // 3. پاک کردن localStorage مربوط به کش
        try {
            const keys = Object.keys(localStorage);
            keys.forEach(key => {
                if (key && (key.toLowerCase().includes('blazor') || key.toLowerCase().includes('cache') || key.toLowerCase().includes('serviceworker') )) {
                    localStorage.removeItem(key);
                }
            });
            console.log('Relevant localStorage keys removed');
        } catch (lsError) {
            console.log('Error clearing localStorage keys:', lsError);
        }

        // 4. ریلود اجباری
        // location.reload(true) is deprecated; use reload() which will re-request resources
        window.location.reload(true);

    } catch (error) {
        console.log('Force update error:', error);
        window.location.reload(true);
    }
};