import { NgModule } from "@angular/core";
import { HasRoleDirective } from "../common/_directives/has-role.directive";
import { DatePickerComponent } from "../common/_forms/date-picker/date-picker.component";
import { TextInputComponent } from "../common/_forms/text-input/text-input.component";
import { AdminPanelComponent } from "./admin/admin-panel/admin-panel.component";
import { ConfirmDialogComponent } from "./admin/modals/confirm-dialog/confirm-dialog.component";
import { RolesModalComponent } from "./admin/modals/roles-modal/roles-modal.component";
import { UserManagementComponent } from "./admin/user-management/user-management.component";
import { NotFoundComponent } from "./errors/not-found/not-found.component";
import { ServerErrorComponent } from "./errors/server-error/server-error.component";
import { ForgotPasswordComponent } from "./forgot-password/forgot-password.component";
import { HomeComponent } from "./home/home.component";
import { ListsComponent } from "./lists/lists.component";
import { LoginComponent } from "./login/login.component";
import { MemberCardComponent } from "./members/member-card/member-card.component";
import { MemberDetailComponent } from "./members/member-detail/member-detail.component";
import { MemberEditComponent } from "./members/member-edit/member-edit.component";
import { MemberListComponent } from "./members/member-list/member-list.component";
import { PhotoEditorComponent } from "./members/photo-editor/photo-editor.component";
import { MessagesComponent } from "./messages/messages.component";
import { NavComponent } from "./nav/nav.component";
import { RegisterComponent } from "./register/register.component";
import { ResetPasswordComponent } from "./reset-password/reset-password.component";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app-routing.module";
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgxWebstorageModule } from "ngx-webstorage";
import { SharedModule } from "../common/_modules/shared.module";
import { LoadingInterceptor } from "../common/_interceptors/loading.interceptor";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from "./app.component";
import { NgxSliderModule } from "ngx-slider-v2";
import { RecaptchaModule, RECAPTCHA_SETTINGS, RecaptchaSettings, RecaptchaFormsModule } from 'ng-recaptcha';
import { GenderManagementComponent } from './admin/gender-management/gender-management.component';
import { ErrorInterceptor } from "../common/_interceptors/error.interceptor";
import { JwtInterceptor } from "../common/_interceptors/jwt.interceptor";
import { LookingforManagementComponent } from "./admin/lookingfor-management/lookingfor-management.component";
import { InterestManagementComponent } from "./admin/interest-management/interest-management.component";
import { EditLookingforComponent } from './members/edit-lookingfor/edit-lookingfor.component';
import { EditInterestComponent } from './members/edit-interest/edit-interest.component';
import { CallComponent } from './call/call.component';
import { AccountGeneralComponent } from "./account-settings/account-general/account-general.component";
import { AccountSettingsComponent } from "./account-settings/account-settings.component";
import { AccountChangePasswordComponent } from "./account-settings/account-change-password/account-change-password.component";
import { AccountSetvisibilityComponent } from './account-settings/account-setvisibility/account-setvisibility.component';
import { AccountDisableAccountComponent } from './account-settings/account-disable-account/account-disable-account.component';
import { EndCallComponent } from './call/end-call/end-call.component';
import { UserMessagesComponent } from './messages/user-messages/user-messages.component';
import { TermManagementComponent } from './admin/term-management/term-management.component';
import { TermComponent } from './admin/term-management/term/term.component';


@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    HomeComponent,
    RegisterComponent,
    MemberDetailComponent,
    ListsComponent,
    MessagesComponent,
    NotFoundComponent,
    ServerErrorComponent,
    MemberCardComponent,
    MemberEditComponent,
    PhotoEditorComponent,
    TextInputComponent,
    DatePickerComponent,
    AdminPanelComponent,
    HasRoleDirective,
    UserManagementComponent,
    RolesModalComponent,
    ConfirmDialogComponent,
    LoginComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    MemberListComponent,
    GenderManagementComponent,
    LookingforManagementComponent,
    InterestManagementComponent,
    EditLookingforComponent,
    EditInterestComponent,
    CallComponent,
    AccountSettingsComponent,
    AccountGeneralComponent,
    AccountChangePasswordComponent,
    AccountSetvisibilityComponent,
    AccountDisableAccountComponent,
    EndCallComponent,
    UserMessagesComponent,
    TermManagementComponent,
    TermComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    NgxWebstorageModule.forRoot(),
    NgxSliderModule,
    RecaptchaModule,
    RecaptchaFormsModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
    {provide: RECAPTCHA_SETTINGS,
    useValue: {
      siteKey: '6Leup4AnAAAAANNuiRoTeYEvTmuo6IpnTcJsoKKo',
    } as RecaptchaSettings,},

  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
