import { HttpInterceptorFn } from '@angular/common/http';

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
  return next(req.clone({ setHeaders: { 'X-Correlation-ID': correlationId } }));
};
