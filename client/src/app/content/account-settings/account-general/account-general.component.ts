import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MemberUpdateDto } from 'src/app/common/_models/memberUpdateDto';
import { MembersService } from 'src/app/common/_services/members.service';


@Component({
  selector: 'app-account-general',
  templateUrl: './account-general.component.html',
  styleUrls: ['./account-general.component.css']
})
export class AccountGeneralComponent implements OnInit {
  memberUpdateInfo: MemberUpdateDto | undefined;
  @Output() interestAdded = new EventEmitter<void>();
  genders: any[] = []
  updatedSuccessfully: boolean = false;
  constructor(private membersService: MembersService, private toastr: ToastrService) { }
  ngOnInit(): void {
    this.loadMemberUpdateInfo();
    this.membersService.getGender().subscribe(
      response => {
        this.genders = response;
      },
      error => {
        console.log(error);
      }
    );
  }
  loadMemberUpdateInfo() {
    this.membersService.getUserForUpdate().subscribe(
      (response: MemberUpdateDto) => {
        this.memberUpdateInfo = response;
      },
      (error) => {
        console.log(error);
      }
    );
  }
  onUpdateUser() {
    if (this.memberUpdateInfo) {
      this.membersService.updateUser(this.memberUpdateInfo).subscribe(
        (response) => {
          // Successfully updated
          this.updatedSuccessfully = true;
          this.toastr.success('Update successfully!', response);
          this.interestAdded.emit();
        },
        (error) => {
          // Handling errors
          console.error('Error while updating member information:', error);
        }
      );
    }
  }

}
