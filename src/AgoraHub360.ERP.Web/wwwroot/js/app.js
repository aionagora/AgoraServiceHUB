// Mitiga rechazos de scripts externos (p.ej. Funding Choices / ads)
(function () {
  if (typeof window === 'undefined') return;

  window.addEventListener('unhandledrejection', function (event) {
    try {
      var reason = event && event.reason;

      // Caso observado: Promise rechazada con Set(4) { gdpr, cpra, offerwall, ad_blocking }
      if (reason instanceof Set && reason.size === 4) {
        event.preventDefault();
        console.warn('[safe-guard] Rechazo externo controlado (Set(4)):', Array.from(reason));
        return;
      }

      // Fallback: algunos runtimes serializan el motivo como texto
      if (typeof reason === 'string' && reason.indexOf('Set(4)') !== -1) {
        event.preventDefault();
        console.warn('[safe-guard] Rechazo externo controlado (Set(4) string).');
      }
    } catch {
      // no-op
    }
  });
})();
