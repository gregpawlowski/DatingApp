import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/User';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { PaginationConfig } from 'ngx-bootstrap';

// Temporarily send headers this way
// WIll use interceptro later.
// const httpOptions = {
//   headers: new HttpHeaders({
//     Authorization: 'Bearer ' + localStorage.getItem('token')
//   })
// };

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(page?, itemsPerPage?, userParams?): Observable<PaginatedResult<User[]>> {

    let params = new HttpParams();

    // Pass in optional parametes as part of the request
    if (page !== null && itemsPerPage !== null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    // Get response instead of JSON so we can get access to the full response including the headers.
    return this.http.get<User[]>(this.baseUrl + 'users', {
      observe: 'response',
      params
    })
    .pipe(
      map(res => {
        const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

        paginatedResult.result = res.body;
        if (res.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(res.headers.get('Pagination'));
        }

        return paginatedResult;
      })
    );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain', {});
  }

  deletePhoto(userID: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userID + '/photos/' + id);
  }
}
