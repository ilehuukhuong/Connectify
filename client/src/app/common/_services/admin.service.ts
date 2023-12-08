import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { Genders } from '../_models/gender';
import { LookingForsDto } from '../_models/lookingForsDto';
import { InterestsDto } from '../_models/interestsDto';
import { Term, Terms } from '../_models/term';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/'
      + username + '?roles=' + roles, {});
  }
  blockUser(username: string) {
    return this.http.put(this.baseUrl + 'admin/block/' + username, { responseType: 'text' as 'json' });
  }

  unblockUser(username: string) {
    return this.http.put(this.baseUrl + 'admin/unblock/' + username, { responseType: 'text' as 'json' });
  }
  getGenders() {
    return this.http.get<Genders[]>(this.baseUrl + 'gender');
  }
  createGender(gender: Genders) {
    return this.http.post<Genders>(this.baseUrl + 'gender', gender, { responseType: 'text' as 'json' });
  }

  updateGender(gender: Genders, id: number) {
    return this.http.put<Genders>(this.baseUrl + 'gender/' + id, gender, { responseType: 'text' as 'json' });
  }

  deleteGender(id: number) {
    return this.http.delete<Genders>(this.baseUrl + 'gender/' + id, { responseType: 'text' as 'json' });
  }
  getLookingFors(name?: string) {
    let url = this.baseUrl + 'lookingfor';
    if (name) {
      url += `/${name}`;
    }
    return this.http.get<LookingForsDto[]>(url);
  }
  updateLookingFor(lookingFor: LookingForsDto) {
    return this.http.put(this.baseUrl + 'lookingfor/' + lookingFor.id, lookingFor, { responseType: 'text' as 'json' });
  }

  deleteLookingFor(id: number) {
    return this.http.delete(this.baseUrl + 'lookingfor/' + id, { responseType: 'text' as 'json' });
  }
  createLookingFor(lookingFor: LookingForsDto) {
    return this.http.post(this.baseUrl + 'lookingfor', lookingFor, { responseType: 'text' as 'json' });
  }
  getInterest(name?: string) {
    let url = this.baseUrl + 'interest';
    if (name) {
      url += `/${name}`;
    }
    return this.http.get<InterestsDto[]>(url);
  }
  updateInterest(interest: InterestsDto) {
    return this.http.put(this.baseUrl + 'interest/' + interest.id, interest, { responseType: 'text' as 'json' });
  }

  deleteInterest(id: number) {
    return this.http.delete(this.baseUrl + 'interest/' + id, { responseType: 'text' as 'json' });
  }
  createInterest(interest: InterestsDto) {
    return this.http.post(this.baseUrl + 'interest', interest, { responseType: 'text' as 'json' });
  }
  getAllTermList(): Observable<Terms[]> {
    return this.http.get<Terms[]>(this.baseUrl + 'TermLists');
  }
  getAllTerm(listId: number): Observable<any> {
    return this.http.get<Term[]>(this.baseUrl + 'TermLists/' + listId).pipe();
  }
  createTermList(name: string, description: string) : Observable<Terms>  {
    const body = { name: name, description: description };
    return this.http.post<Terms>(this.baseUrl + 'TermLists', body, { responseType: 'text' as 'json' });
  }
  deleteTerm(listId: number, term: string) {
    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
      body: {
        term: term
      }
    };

    return this.http.delete(`${this.baseUrl}TermLists/DeleteTerm/${listId}`, options);
  }
  addTerm(listId: number, term: string) {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });
  
    const body = {
      term: term
    };
  
    return this.http.post(`${this.baseUrl}TermLists/AddTerm/${listId}`, body, { headers: headers });
  }
  

  deleteTermList(listId: number) {
    return this.http.delete(this.baseUrl + 'TermLists/' + listId, { responseType: 'text' as 'json' });
  }
  refereshIndex(listId: number) {
    return this.http.put(this.baseUrl + 'TermLists/' + listId, { responseType: 'text' as 'json' });
  }
}
