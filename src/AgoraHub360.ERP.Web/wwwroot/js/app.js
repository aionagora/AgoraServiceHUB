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
