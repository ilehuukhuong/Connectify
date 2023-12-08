import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { LocationDto } from '../_models/locationDto';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class LocationService{
  private apiLocationKey = environment.apiLocationKey;
  private  apiLocationUrl=environment.apiLocationUrl;
  baseUrl = environment.apiUrl;
  locationDto: LocationDto = { latitude: 0, longitude: 0, locationName:'' };
  requestingLocation = false;
  private locationPermissionGranted = false;
  locationName!: string;
  

  constructor(private http: HttpClient, private toastr: ToastrService, private router: Router) { }

  getCityInfoByCoords(latitude: number, longitude: number) {
    const url = `${this.apiLocationUrl}?q=${latitude}+${longitude}&key=${this.apiLocationKey}`;
    return this.http.get(url);
  }
  getLocation(locationDto: LocationDto): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}users/update-location`, locationDto);
  }
  checkLocation(): void {
    if ('geolocation' in navigator) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const latitude = position.coords.latitude;
          const longitude = position.coords.longitude;
          this.getCityInfoByCoords(latitude, longitude)
          .subscribe((data: any) => {
            if (data.results.length > 0) {
              this.locationName = data.results[0].components.city || data.results[0].components.city_district;
              this.updateLocation(latitude, longitude, this.locationName);
            } else {
              this.locationName = 'City not found';
              this.toastr.error(this.locationName);
            } 
            
          }, error => {
            this.locationName = 'Error occurred';
            this.toastr.error(this.locationName);
          });         
          this.locationPermissionGranted = true;
        },
        (error) => {
          this.toastr.error('Failed to get location. Please manually share your location.', 'Error');
          this.router.navigateByUrl('/');
          this.locationPermissionGranted = false;
        }
      );
    } else {
      this.toastr.error('Your browser does not support Geolocation API. Please manually share your location.', 'Error');
      this.router.navigateByUrl('/');
      this.locationPermissionGranted = false;
    }
  }
  updateLocation(latitude: number, longitude: number, locationName: string): void {
    const locationDto = { latitude: latitude, longitude: longitude, locationName: locationName };
    this.getLocation(locationDto).subscribe(
      (next) => {
        this.router.navigateByUrl('/');
      },
      (error) => {
        this.router.navigateByUrl('/');
      }
    );
  }
  
  isLocationPermissionGranted(): boolean {
    return this.locationPermissionGranted;
  }

  getCurrentLocation(): Observable<{ latitude: number, longitude: number }> {
    let locationSubject = new Subject<{ latitude: number, longitude: number }>();

    if ('geolocation' in navigator) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          locationSubject.next({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
          });
        },
        (error) => {
          this.toastr.error('Failed to get location. Please manually share your location.', 'Error');
          locationSubject.error('Failed to get location');
        }
      );
    } else {
      this.toastr.error('Your browser does not support Geolocation API. Please manually share your location.', 'Error');
      locationSubject.error('Geolocation API not supported');
    }

    return locationSubject.asObservable();
  }
}