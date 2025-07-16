import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds, {withCredentials: true}).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      })
    );
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds, {withCredentials: true}).pipe(
      tap((user) => {
        if (user) {
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      })
    );
  }

  refreshToken() {
    return this.http.post<User>(this.baseUrl + 'account/refresh-token', {}, {withCredentials: true});
  }

  startTokenRefreshInterval() {
    setInterval(() => {
      this.http.post<User>(this.baseUrl + 'account/refresh-token', {}, {withCredentials: true})
        .subscribe({
          next: user => {
            this.setCurrentUser(user) // this will give a new token
          },
          error: () => {
            this.logout()
          }
      })
    }, 5 * 60 * 1000) // 5 min
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user);
    this.currentUser.set(user);
    this.likesService.getLikeIds();
  }

  logout() {
    localStorage.removeItem('filters');
    this.likesService.clearLikeIds();
    this.currentUser.set(null);
  }

  private getRolesFromToken(user: User): string[] {
    // payload with roles is in the 2nd part of token - encoded, but not encrypted
    const payload = user.token.split('.')[1];
    const decoded = atob(payload);
    const jsonPayload = JSON.parse(decoded);

    // ensure it's always an array regardless of whether the user is in one role or more than one roles
    return Array.isArray(jsonPayload.role) ? jsonPayload.role : [jsonPayload.role]
  }
}
