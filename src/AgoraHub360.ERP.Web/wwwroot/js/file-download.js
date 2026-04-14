// ─── Helpers internos ────────────────────────────────────────────────────────

function _triggerDownload(blobOrUrl, fileName) {
    const link = document.createElement("a");
    link.style.display = "none";
    link.download = fileName;

    if (typeof blobOrUrl === "string") {
        link.href = blobOrUrl;
    } else {
        link.href = URL.createObjectURL(blobOrUrl);
    }

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    if (typeof blobOrUrl !== "string") {
        URL.revokeObjectURL(link.href);
    }
}

function _getAuthHeaders() {
    const headers = {};
    try {
        const token = localStorage.getItem("agorahub360_auth_token");
        if (token) {
            headers["Authorization"] = "Bearer " + token;
        }
    } catch (_) { /* localStorage no disponible */ }
    return headers;
}

// ─── Descarga desde base64 (bytes ya en el cliente) ─────────────────────────

window.downloadFileFromBase64 = (fileName, contentType, base64Data) => {
    if (!fileName || !contentType || !base64Data) {
        throw new Error("Parámetros inválidos para downloadFileFromBase64.");
    }
    const byteChars = atob(base64Data);
    const byteNums = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNums[i] = byteChars.charCodeAt(i);
    }
    const blob = new Blob([new Uint8Array(byteNums)], { type: contentType });
    _triggerDownload(blob, fileName);
};

// ─── Descarga desde byte array (Uint8Array) ─────────────────────────────────

window.downloadFileFromBytes = (fileName, contentType, byteArray) => {
    if (!fileName || !contentType || !byteArray) {
        throw new Error("Parámetros inválidos para downloadFileFromBytes.");
    }
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    _triggerDownload(blob, fileName);
};

// ─── Descarga por URL (fetch con JWT) ────────────────────────────────────────

window.downloadFile = async (url, fileName) => {
    if (!url || !fileName) {
        throw new Error("Parámetros inválidos para downloadFile.");
    }
    const response = await fetch(url, {
        method: "GET",
        credentials: "include",
        headers: _getAuthHeaders()
    });
    if (!response.ok) {
        throw new Error("Error descargando archivo: " + response.status + " " + response.statusText);
    }
    const blob = await response.blob();
    _triggerDownload(blob, fileName);
};

// ─── Descarga por URL con nombre desde Content-Disposition ───────────────────

window.downloadFileFromResponse = async (url, fallbackFilename) => {
    if (!url) {
        throw new Error("Parámetros inválidos para downloadFileFromResponse.");
    }
    const response = await fetch(url, {
        method: "GET",
        credentials: "include",
        headers: _getAuthHeaders()
    });
    if (!response.ok) {
        throw new Error("Error descargando archivo: " + response.status + " " + response.statusText);
    }

    let filename = fallbackFilename || "descarga";
    const disposition = response.headers.get("Content-Disposition") || response.headers.get("content-disposition");
    if (disposition && disposition.indexOf("filename=") !== -1) {
        const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
        if (matches && matches[1]) {
            filename = matches[1].replace(/['"]/g, "").trim();
        }
    }
    const blob = await response.blob();
    _triggerDownload(blob, filename);
};
