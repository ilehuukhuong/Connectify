import { Component } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from 'src/app/common/_services/account.service';


@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email: string = '';
  message: string = '';

  constructor(private accountService: AccountService, private toastr: ToastrService) { }

  async onSubmit() {
    this.message = ''; // Reset the message before making a new request
    try {
      await this.accountService.forgotPassword(this.email, window.location.hostname);
      this.message = 'Password reset link sent to your email.';
      this.toastr.success(this.message, 'Success'); // Show success message
    } catch (error: any) {
    }
  }
}
