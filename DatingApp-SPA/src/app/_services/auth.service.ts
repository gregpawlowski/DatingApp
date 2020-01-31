import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject('../../assets/user.png');
  currentPhotoUrl$ = this.photoUrl.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post(`${this.baseUrl}login`, model)
      .pipe(map((res: any) => {
        const user = res;
        if (user) {
          localStorage.setItem('token', user.token);
          localStorage.setItem('user', JSON.stringify(user.user));
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          this.currentUser = user.user;
          this.changeMemberPhoto(this.currentUser.photoUrl);
        }
      }));
  }

  register(user: User) {
    return this.http.post(`${this.baseUrl}register`, user);
  }

  // Check if user is logged in
  loggedIn() {
    const token = localStorage.getItem('token');
    // If token is expired return true
    // Change to return false if token is expired, otherwise if returns true user is logged in.
    return !this.jwtHelper.isTokenExpired(token);
  }

  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }
}
