// ─── Funciones de descarga movidas a file-download.js ────────────────────────

window.printElement = function(elementId) {
    const el = document.getElementById(elementId);
    if (!el) {
        console.error("No se encontró el elemento a imprimir: " + elementId);
        return;
    }

    // Copiar estilos de la página principal para mantener el diseño
    let stylesHtml = '';
    const styleNodes = document.querySelectorAll('link[rel="stylesheet"], style');
    styleNodes.forEach(node => {
        stylesHtml += node.outerHTML;
    });

    const win = window.open('', '_blank', 'width=900,height=800');
    if (!win) {
        console.error("El navegador bloqueó la ventana emergente.");
        return;
    }

    win.document.write('<!DOCTYPE html><html><head><title>Imprimir Comprobante</title>');
    win.document.write(stylesHtml);
    win.document.write('</head><body class="vp-printing">');
    win.document.write(el.outerHTML);
    win.document.write('</body></html>');
    win.document.close();

    // Esperar a que carguen los estilos antes de imprimir
    win.setTimeout(() => {
        win.focus();
        win.print();
    }, 500); 
};
