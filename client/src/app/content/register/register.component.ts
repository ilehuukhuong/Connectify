import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, NgForm, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';
// import { ReCaptchaV3Service } from 'ng-recaptcha';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit{
  @Output() cancelRegister = new EventEmitter()
  registerForm: FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;
  captchaResponse: string | null = null;

  constructor(private accountService: AccountService, private toastr: ToastrService, 
    private fb: FormBuilder, private router: Router, private membersService: MembersService) { }
    genders: any[] = [];

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() -18);
    this.accountService.getGender().subscribe(
      response => {
        this.genders = response;
      },
      error => {
        console.log(error); // Xử lý lỗi tùy ý
      }
    );
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      firstname: ['', Validators.required],
      lastname: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      genderId: [],
      email: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', [Validators.required, 
        Validators.minLength(4)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }
  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {notMatching: true}
    }
  }
  register() {
    const dob = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    const values = {...this.registerForm.value, dateOfBirth: dob, captcha: this.captchaResponse};
    this.accountService.register(values).subscribe({
      next: () => {
        window.location.href = '/';
        this.membersService.refreshMembers(); 
      },
      error: error => {
        this.validationErrors = error
        this.toastr.error(error);
      } 
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
  
  private getDateOnly(dob: string | undefined) {
    if (!dob) return;
    let theDob = new Date(dob);
    return new Date(theDob.setMinutes(theDob.getMinutes()-theDob.getTimezoneOffset()))
      .toISOString().slice(0,10);
  }

  
  resolved(captchaResponse: string) {
    this.captchaResponse = captchaResponse;
    console.log(`Resolved captcha with response: ${captchaResponse}`);
  }
}


  

