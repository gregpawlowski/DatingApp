import { HttpInterceptor, HttpRequest, HttpEvent, HttpHandler, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(error => {
        // Error coming back from the API / HTTP Error not network error.
        if (error instanceof HttpErrorResponse) {
          // Since our API returns an empty messge for 401 errors,
          // it only returns 401 we need to check for that error as well and return some message.
          if (error.status === 401) {
            return throwError(error.statusText);
          }
          // Check if we have an Application-Error header that we created in the API.
          const applicationError = error.headers.get('Application-Error');
          if (applicationError) {
            console.error(applicationError);
            // REturn a new observable error with the application Error messge.
            return throwError(applicationError);
          }

          const serverError = error.error;

          // Check if it's a model state or any other error.
          // If a modelstate error it will be an objext.
          let modelStateErrors = '';
          if (serverError.errors && typeof serverError.errors === 'object') {
            for (const key in serverError.errors) {
              if (serverError.errors[key]) {
                modelStateErrors += serverError.errors[key] + '\n';
              }
            }
          }
          // Accomodate all type of server errors from the server.
          return throwError(modelStateErrors || serverError || 'Server Error');
        }
      })
    );
  }
}

// This will need to be provided in the providers in the app.
export const ErrorInterceptorProvider = {
  // Need to use HTTP_INTERCEPTORS token so that our interceptor is added to the array of angular interceptors
  provide: HTTP_INTERCEPTORS,
  // Which class to use, use the one above.
  useClass: ErrorInterceptor,
  // multi means we don't want to replace the existing array instead add to it.
  multi: true
};

