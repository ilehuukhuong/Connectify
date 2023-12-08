import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, AfterViewChecked, TemplateRef } from '@angular/core';
import { Options } from '@angular-slider/ngx-slider';
import { Member } from 'src/app/common/_models/member';
import { Pagination } from 'src/app/common/_models/pagination';
import { UserParams } from 'src/app/common/_models/userParams';
import { MembersService } from 'src/app/common/_services/members.service';
import { LocationService } from 'src/app/common/_services/location.service';
import { environment } from 'src/environments/environment';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
declare var H: any;

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit, AfterViewChecked {
  @ViewChild('filterModal') filterModalTemplate!: TemplateRef<any>;
  @ViewChild('mapContainer')
  public mapElement!: ElementRef;
  private currentMarker: any; 
  private platform: any;
  private map: any;
  bsModalRef!: BsModalRef;
  private apiMapKey =environment.apiMapKey;
  members: Member[] = [];
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  genders: any[] = [];
  ageSliderOptions: Options = {
    floor: 18,
    ceil: 99
  };
  distanceSliderOptions: Options = {
    floor: 1,
    ceil: 100
  };
  distanceSliderValue = 100;
  ageRangeSliderValue: number[] = [];
  member: any;
  modalRef!: BsModalRef;
  constructor(
    private memberService: MembersService,
    public locationService: LocationService,
    private modalService: BsModalService,
  ) {
    this.userParams = this.memberService.getUserParams();
    this.platform = new H.service.Platform({
      "apikey": this.apiMapKey
    });
    this.memberService.getGender().subscribe(
      (response) => {
        this.genders = response;
      },
      (error) => {
        console.log(error);
      }
    );
    this.ageRangeSliderValue = [this.userParams!.minAge, this.userParams!.maxAge];
    this.distanceSliderValue = this.userParams!.distance;
  
    this.locationService.checkLocation();
  }

    ngOnInit() {
      this.locationService.getCurrentLocation().subscribe(
        (coords) => {
          if (this.userParams) {
            this.userParams.currentLatitude = coords.latitude;
            this.userParams.currentLongitude = coords.longitude;
            this.loadMembers();
          }
        },
        (error) => {
          console.error('Error getting current location:', error);
        }
      );
  }
  ngAfterViewChecked() {
    if (!this.map && this.mapElement && this.mapElement.nativeElement) {
      this.loadMap();
    }
  }

  private loadMap(): void {
    if (this.mapElement && this.mapElement.nativeElement) {
      const defaultLayers = this.platform.createDefaultLayers();
      this.map = new H.Map(
        this.mapElement.nativeElement,
        defaultLayers.vector.normal.map,
        {
          zoom: 2,
          center: { lat: 10.762622, lng: 106.660172 }
        }
      );
      let mapEvents = new H.mapevents.MapEvents(this.map);

      let behavior = new H.mapevents.Behavior(mapEvents);
    
      let ui = H.ui.UI.createDefault(this.map, defaultLayers);
      this.map.addEventListener('tap', (evt: any) => {
        const coord = this.map.screenToGeo(evt.currentPointer.viewportX, evt.currentPointer.viewportY);
        console.log(`Clicked at ${Math.abs(coord.lat.toFixed(4))} ${Math.abs(coord.lng.toFixed(4))}`);
        if (this.userParams) {
          this.userParams.currentLatitude = coord.lat;
          this.userParams.currentLongitude = coord.lng;
          this.loadMembers();
        }
        if (this.currentMarker) {
          this.map.removeObject(this.currentMarker);
        }
        this.currentMarker = new H.map.Marker({ lat: coord.lat, lng: coord.lng });
        this.map.addObject(this.currentMarker);
      });
    }
  }

  loadMembers() {
    if (this.userParams) {
      this.memberService.setUserParams(this.userParams);
      this.memberService.getMembers(this.userParams).subscribe({
        next: (response) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
    }
  }
  resetFilters() {
    if (this.currentMarker) {
      this.map.removeObject(this.currentMarker);
      this.currentMarker = null;
    }
    this.map.setCenter({ lat: 10.762622, lng: 106.660172 });
    this.map.setZoom(4);
    this.userParams = this.memberService.resetUserParams();
    this.loadMembers();
  }
  pageChanged(event: any) {
    if (this.userParams && this.userParams?.pageNumber !== event.page) {
      this.userParams.pageNumber = event.page;
      this.memberService.setUserParams(this.userParams);
      this.loadMembers();
    }
  }

  openFilterModal() {
    this.modalRef = this.modalService.show(this.filterModalTemplate);
  }
}
