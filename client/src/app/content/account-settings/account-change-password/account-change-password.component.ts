import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';

@Component({
  selector: 'app-account-change-password',
  templateUrl: './account-change-password.component.html',
  styleUrls: ['./account-change-password.component.css']
})
export class AccountChangePasswordComponent {
  user: User | null = null;
  oldPassword!: string;
  newPassword!: string;
  confirmPassword!: string;


  constructor(private accountService: AccountService,
    private memberService: MembersService,
    private toastr: ToastrService,
    private router: Router,
    private fb: FormBuilder,
    private modalService: BsModalService,) {
      this.accountService.currentUser$.pipe(take(1)).subscribe({
        next: user => this.user = user
      })
     }

  changePassword(){
    if (this.newPassword !== this.confirmPassword) {
      this.toastr.error('Password and Confirm Password do not match.');
      return;
    }
    this.accountService.changePassword(this.oldPassword, this.newPassword).subscribe(
      response => {
        this.toastr.success('Change password successfully!!');
        window.location.reload();
      },
      error => {
        // Xử lý lỗi
        this.toastr.error('Invalid password format');
        console.error(error);
      }
    );
  }

}
