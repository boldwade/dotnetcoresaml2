import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';


@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(public auth: AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // If token is present, send with request
    const token = this.auth.getToken();

    if (token) {
      const fixedToken = token.replace('"', '').replace('"', '');
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${fixedToken}`
        }
      });
    }

    return next.handle(request);

  }

}
