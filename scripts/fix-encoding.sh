#!/bin/bash
# =============================================================================
# fix-encoding.sh - Convierte archivos ISO-8859-1 a UTF-8
# AgoraHub360 ERP - Corrección de codificación de caracteres
# =============================================================================

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC_DIR="$REPO_ROOT/src"
CONVERTED=0
SKIPPED=0
ERRORS=0

echo "======================================================"
echo " AgoraHub360 ERP - Corrección de Encoding"
echo " Directorio: $SRC_DIR"
echo "======================================================"
echo ""

find "$SRC_DIR" -type f \( -name "*.cs" -o -name "*.razor" \) | sort | while read -r f; do
    encoding=$(file -i "$f" 2>/dev/null | grep -o 'charset=[^;,[:space:]]*' | cut -d= -f2 | tr '[:upper:]' '[:lower:]')

    if [[ "$encoding" == "iso-8859-1" || "$encoding" == "iso-8859" || "$encoding" == "unknown-8bit" ]]; then
        tmp="$f.utf8.tmp"
        if iconv -f ISO-8859-1 -t UTF-8 "$f" -o "$tmp" 2>/dev/null; then
            mv "$tmp" "$f"
            echo "  [OK] Convertido: ${f#$REPO_ROOT/}"
            CONVERTED=$((CONVERTED + 1))
        else
            rm -f "$tmp"
            echo "  [ERROR] Falló conversión: ${f#$REPO_ROOT/}"
            ERRORS=$((ERRORS + 1))
        fi
    else
        SKIPPED=$((SKIPPED + 1))
    fi
done

echo ""
echo "======================================================"
echo " Resumen:"
echo "   Convertidos : $CONVERTED"
echo "   Omitidos    : $SKIPPED (ya UTF-8 o ASCII)"
echo "   Errores     : $ERRORS"
echo "======================================================"
