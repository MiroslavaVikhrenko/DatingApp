import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../services/account-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);

  const user = accountService.currentUser(); // making a copy of signal => loosing reactivity on the copy

  // request is immutable => need to create a clone of it to modify
  if (user) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${user.token}`
      }
    })
  }
  return next(req);
};
