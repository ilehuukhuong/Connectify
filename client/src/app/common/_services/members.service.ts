import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject, map, of, take, tap } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { MemberUpdateDto } from '../_models/memberUpdateDto';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User | undefined;
  userParams: UserParams | undefined;
  private _reloadMember = new Subject<void>();
  reloadMember$ = this._reloadMember.asObservable();


  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.subscribe(user => {
      if (user) {
        this.user = user;
        this.userParams = new UserParams(user);
      } else {
        this.user = undefined;
        this.userParams = undefined;
      }
    });
  }
  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      this.userParams = new UserParams(this.user);
      return this.userParams;
    }
    return;
  }

  getMembers(userParams: UserParams) {
    const response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) return of(response);
    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    params = params.append('distance', userParams.distance);
    if (userParams.currentLatitude && userParams.currentLongitude != null) {
      params = params.append('currentLatitude', userParams.currentLatitude);
      params = params.append('currentLongitude', userParams.currentLongitude);
    }
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      })
    )
  
  }
  refreshMembers() {
    this.memberCache.clear();  // Clear the existing cache
    // Optionally, you could also call getMembers() here to repopulate the cache
  }

  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.userName === username);

    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }
  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'photos/set-main-photo/' + photoId, {}, { responseType: 'text' as 'json' });
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'photos/delete-photo/' + photoId, { responseType: 'text' as 'json' });
  }

  addLike(username: string) {
    return this.http.post(this.baseUrl + 'likes/' + username, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number, search: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);

    params = params.append('predicate', predicate);
    params = params.append('search', search);

    return getPaginatedResult<Member[]>(this.baseUrl + 'likes', params, this.http);
  }
  getRecommendedMembers(userParams: UserParams, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('distance', userParams.distance);
    params = params.append('similarity', userParams.similarity);
    if (userParams.currentLatitude && userParams.currentLongitude != null) {
      params = params.append('currentLatitude', userParams.currentLatitude);
      params = params.append('currentLongitude', userParams.currentLongitude);
    }
    return getPaginatedResult<Member[]>(this.baseUrl + 'users/recommended', params, this.http);
    
  }
  getGender() {
    return this.http.get<any>(`${this.baseUrl}gender`);
  }
  setVisible() {
    return this.http.put(this.baseUrl + 'users/visible', {});
  }
  setInvisible() {
    return this.http.put(this.baseUrl + 'users/invisible', {});
  }
  deleteAccount() {
    return this.http.put(this.baseUrl + 'users/delete-account', {}, { responseType: 'text' });
  }
  updateUserIntroduction(member: Member) {
    return this.http.put(this.baseUrl + 'updateintroduction', member, { responseType: 'text' as 'json' });
  }
  addLookingFor(id: number) {
    return this.http.post(this.baseUrl + 'users/add-lookingfor/' + id, {})
  }

  deleteLookingFor(id: number) {
    return this.http.delete(this.baseUrl + 'users/delete-lookingfor/' + id,);
  }
  addInterest(id: number) {
    return this.http.post(this.baseUrl + 'users/add-interest/' + id, {})
  }

  deleteInterest(id: number) {
    return this.http.delete(this.baseUrl + 'users/delete-interest/' + id,);
  }
  updateUser(memberUpdateDto: MemberUpdateDto): Observable<any> {
    return this.http.put(`${this.baseUrl}users/update-user`, memberUpdateDto);
  }

  getUserForUpdate(): Observable<MemberUpdateDto> {
    return this.http.get<MemberUpdateDto>(`${this.baseUrl}users/update-user`);
  }
}