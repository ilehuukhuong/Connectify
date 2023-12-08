import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from 'src/app/common/_services/account.service';
import { LocationService } from 'src/app/common/_services/location.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',

  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  model: any = {};

  constructor(public accountService: AccountService, private router: Router,
    private toastr: ToastrService, private locationService: LocationService) { }

  ngOnInit(): void {
  }

  login() {
    if (!this.model.username) {
      this.toastr.warning('Please enter username.', 'Notice');
      return;
    } else if (!this.model.password) {
      this.toastr.warning('Please enter password.', 'Notice');
      return;
    }
    this.accountService.login(this.model).subscribe({
      next: (_) => {
        this.router.navigateByUrl('/');
      },
      error: (error: any) => {
        console.log(error);
        // this.toastr.error(error.toString(), 'Error');
      }
    });
  }


}
