
window.clickOutsideHandlers = {};
window.registerClickOutsideHandler = function (componentInstance, uniqueId) {
    //document.addEventListener('click', function (event) {
    //    const dropdown = document.querySelector(`#${uniqueId}`);
    //    if (dropdown && !dropdown.contains(event.target)) {
    //        componentInstance.invokeMethodAsync('CloseDropdownjs');
    //    }
    //});
    // اگر قبلاً وجود داره، اول unregister کنیم
    if (window.clickOutsideHandlers[uniqueId]) {
        document.removeEventListener('click', window.clickOutsideHandlers[uniqueId]);
    }

    const handler = function (event) {
        const dropdown = document.querySelector(`#${uniqueId}`);
        if (dropdown && !dropdown.contains(event.target)) {
            componentInstance.invokeMethodAsync('CloseDropdownjs');
        }
    };

    window.clickOutsideHandlers[uniqueId] = handler;
    document.addEventListener('click', handler);
};
window.unregisterClickOutsideHandler = function (uniqueId) {
    const handler = window.clickOutsideHandlers[uniqueId];
    if (handler) {
        document.removeEventListener('click', handler);
        delete window.clickOutsideHandlers[uniqueId];
    }
};


window.downloadFileFromUrl = (url, fileName) => {
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};
function printLargeForm(elementId) {
    const printContent = document.getElementById(elementId);
    const originalContent = document.body.innerHTML;

    // Clone the form for printing
    const clonedContent = printContent.cloneNode(true);
    document.body.innerHTML = "";
    document.body.appendChild(clonedContent);

    // Adjust the styles for printing
    const style = document.createElement('style');
    style.innerHTML = `
        @media print {
            body {
                margin: 0;
                padding: 0;
            }
            #${elementId} {
                width: 100%;
                overflow: visible !important;
                page-break-inside: avoid;
            }
            #${elementId} * {
                overflow: visible !important;
                page-break-inside: avoid;
            }
            table {
                width: 100%;
                border-collapse: collapse;
            }
            table, th, td {
                border: 1px solid black;
            }
            tr {
                page-break-inside: avoid;
                break-inside: avoid;
            }
            .RadzenDataGrid {
                margin-bottom: 0;
            }
            .RadzenDataGrid tbody tr:last-child {
                border-bottom: none;
            }
            /* Prevent page breaks between grids */
            .RadzenDataGrid:not(:last-child) {
                margin-bottom: 1rem;
                page-break-after: always;
            }
        }
    `;
    document.head.appendChild(style);

    // Trigger print
    window.print();

    // Restore original content after printing
    document.body.innerHTML = originalContent;
}
function printDiv(divId) {
    var printContents = document.getElementById(divId);
    if (!printContents) {
        console.error(`Element with id ${divId} not found.`);
        return;
    }

    var originalContents = document.body.innerHTML;

    // Clone the form for printing
    var clonedContent = printContents.cloneNode(true);
    document.body.innerHTML = "";
    document.body.appendChild(clonedContent);

    // Adjust the styles for printing
    var style = document.createElement('style');
    style.innerHTML = `
        @page {
            size: A5 landscape;
            margin: 10mm;
        }
        @media print {
            body {
                margin: 0;
                padding: 0;
            }
            #${divId} {
                width: 100%;
                overflow: visible !important;
                page-break-inside: avoid;
            }
            #${divId} * {
                overflow: visible !important;
                page-break-inside: avoid;
            }
            table {
                width: 100%;
                border-collapse: collapse;
            }
            table, th, td {
                border: 1px solid black;
            }
            tr {
                page-break-inside: avoid;
                break-inside: avoid;
            }
        }
    `;
    document.head.appendChild(style);

    // Trigger print
    window.print();

    // Restore original content after printing
    document.body.innerHTML = originalContents;
}
window.getElementRect = (element) => {
    const rect = element.getBoundingClientRect();
    return {
        Top: rect.top + window.scrollY,
        Left: rect.left + window.scrollX,
        Width: rect.width,
        Height: rect.height
    };
};
window.getWindowWidth = () => {
    return window.innerWidth;
};
window.resizeListener = {
    callbacks: [],
    init: function () {
        if (!this.initialized) {
            window.addEventListener("resize", () => {
                this.callbacks.forEach(cb => cb(window.innerWidth));
            });
            this.initialized = true;
        }
    },
    register: function (dotNetRef) {
        this.init();
        this.callbacks.push((width) => {
            dotNetRef.invokeMethodAsync("OnResize", width);
        });
    }
};
window.scrollToHighlighted = function () {
    const el = document.getElementById('highlighted-letter');
    const container = document.getElementById('scroll-container');

    if (el && container) {
        const elRect = el.getBoundingClientRect();
        const containerRect = container.getBoundingClientRect();

        const scrollLeft = container.scrollLeft + (elRect.left - containerRect.left) - (container.clientWidth / 2) + (el.clientWidth / 2);

        const verticalOffset = elRect.top - containerRect.top;
        const scrollTop = container.scrollTop + verticalOffset - (container.clientHeight / 2) + (el.clientHeight / 2);

        console.log("scrollLeft : " + scrollLeft + " scrollTop " + scrollTop);
        console.log(elRect.top + " " + el.clientHeight);
        console.log(containerRect.top + " " + container.clientHeight + " " + container.scrollTop);

        container.scrollTo({
            top: scrollTop,
            left: scrollLeft,
            behavior: 'smooth'
        });
    }
};
window.ButtonClicker = {
    Click: function (id) {
        document.getElementById(id)?.click();
    }
};

window.scrollToBottom = function (el) { el.scrollTop = el.scrollHeight; };
//function printElementById(id) {
//    const elem = document.getElementById(id);
//    if (!elem) return;

//    // ساخت پنجره جدید برای چاپ
//    const printWindow = window.open('', '_blank');

//    // استخراج تمام لینک‌ها و استایل‌ها
//    const links = [...document.querySelectorAll('link[rel="stylesheet"]')]
//        .map(link => `<link rel="stylesheet" href="${link.href}">`)
//        .join('');

//    const styles = [...document.querySelectorAll('style')]
//        .map(style => style.outerHTML)
//        .join('');

//    // ساخت HTML کامل با head واقعی
//    const html = `
//      <html>
//        <head>
//          <meta charset="utf-8">
//          <title>Print</title>
//          ${links}
//          ${styles}
//          <style>
//            @page { margin: 10mm; }
//            body {
//                font-family: 'Vazir', sans-serif;
//                direction: rtl;
//                padding: 0;
//                margin: 0;
//            }
//          </style>
//        </head>
//        <body>
//          ${elem.outerHTML}
//        </body>
//      </html>
//    `;

//    printWindow.document.write(html);
//    printWindow.document.close();

//    // اطمینان از لود کامل استایل‌ها
//    printWindow.onload = function () {
//        printWindow.focus();
//        printWindow.print();
//        printWindow.close();
//    };
//}

function printElementById(id) {
    const elem = document.getElementById(id);
    if (!elem) return;

    const printWindow = window.open('', '_blank', 'width=800,height=600');

    // مهم: همه چیز sync انجام بشه تا مرورگر بلاک نکنه
    const links = [...document.querySelectorAll('link[rel="stylesheet"]')]
        .map(link => `<link rel="stylesheet" href="${link.href}">`)
        .join('');

    const styles = [...document.querySelectorAll('style')]
        .map(style => style.outerHTML)
        .join('');

    const html = `
      <html>
        <head>
          <title>Print</title>
          ${links}
          ${styles}
          <style>
            @page { margin: 10mm; }
            body {
              font-family: 'Vazir', sans-serif;
              direction: rtl;
              margin: 0;
              padding: 0;
            }
          </style>
        </head>
        <body>
          ${elem.outerHTML}
          <script>
            window.onload = function () {
              setTimeout(() => {
                window.print();
                window.close();
              }, 100);
            };
          </script>
        </body>
      </html>
    `;

    // write synchronously
    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
}
