window.downloadFile = async (url, filename) => {
  const response = await fetch(url, {
      method: "GET",
      credentials: "include"
  });

  if (!response.ok) {
      throw new Error(`Error descargando archivo: ${response.status} ${response.statusText}`);
  }

  const blob = await response.blob();
  const blobUrl = window.URL.createObjectURL(blob);

  const a = document.createElement("a");
  a.href = blobUrl;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);

  window.URL.revokeObjectURL(blobUrl);
};

window.downloadFileFromBase64 = (filename, contentType, base64) => {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    const url = URL.createObjectURL(blob);
    
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = filename;
    document.body.appendChild(anchorElement);
    anchorElement.click();
    document.body.removeChild(anchorElement);
    URL.revokeObjectURL(url);
};

window.downloadFileFromResponse = async (url, fallbackFilename) => {
    const response = await fetch(url, {
        method: "GET",
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error(`Error descargando archivo: ${response.status} ${response.statusText}`);
    }

    const disposition = response.headers.get("Content-Disposition") || response.headers.get("content-disposition");
    let filename = fallbackFilename;
    if (disposition && disposition.indexOf("filename=") !== -1) {
        const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
        const matches = filenameRegex.exec(disposition);
        if (matches != null && matches[1]) {
            filename = matches[1].replace(/['"]/g, '').trim();
        }
    }

    const blob = await response.blob();
    const blobUrl = window.URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.style.display = "none";
    a.href = blobUrl;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    window.URL.revokeObjectURL(blobUrl);
};

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
