import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Toast, ToastrService } from 'ngx-toastr';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';

@Component({
  selector: 'app-disable-account',
  templateUrl: './account-disable-account.component.html',
  styleUrls: ['./account-disable-account.component.css']
})
export class AccountDisableAccountComponent {
  constructor(private memberService: MembersService, private accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  disableAccount(): void {
    const confirmation = window.confirm("Are you sure you want to deactivate your account?");
    if (confirmation) {
      this.memberService.deleteAccount().subscribe(() => {
        this.accountService.logout();
        this.router.navigate(['/']);
        this.toastr.success('Account has been disabled!');
      });
    } else {
      this.toastr.info('The action has been cancelled');
    }
  }
}
