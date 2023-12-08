import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MessagesComponent } from './messages/messages.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { AuthGuard } from '../common/_guards/auth.guard';
import { MemberDetailedResolver } from '../common/_resolvers/member-detailed.resolver';
import { AdminGuard } from '../common/_guards/admin.guard';
import { CallComponent } from './call/call.component';
import { AccountSettingsComponent } from './account-settings/account-settings.component';
import { AccountChangePasswordComponent } from './account-settings/account-change-password/account-change-password.component';
import { AccountGeneralComponent } from './account-settings/account-general/account-general.component';
import { AccountSetvisibilityComponent } from './account-settings/account-setvisibility/account-setvisibility.component';
import { AccountDisableAccountComponent } from './account-settings/account-disable-account/account-disable-account.component';
import { EndCallComponent } from './call/end-call/end-call.component';
import { UserMessagesComponent } from './messages/user-messages/user-messages.component';
import { TermManagementComponent } from './admin/term-management/term-management.component';
import { TermComponent } from './admin/term-management/term/term.component';


const routes: Routes = [
  { path: '', component: HomeComponent, runGuardsAndResolvers: 'always' },
  { path: 'login', component: LoginComponent, runGuardsAndResolvers: 'always' },
  { path: 'register', component: RegisterComponent, runGuardsAndResolvers: 'always' },
  { path: 'forgot-password', component: ForgotPasswordComponent, runGuardsAndResolvers: 'always' },
  { path: 'reset-password/:token', component: ResetPasswordComponent, pathMatch: 'full' },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      { path: 'members/:username', component: MemberDetailComponent, resolve: { member: MemberDetailedResolver } },
      { path: 'member/edit', component: MemberEditComponent },
      { path: 'lists', component: ListsComponent },
      { path: 'messages', component: MessagesComponent },
      { path: 'admin', component: AdminPanelComponent, canActivate: [AdminGuard] },
      { path: 'call/:username', component: CallComponent, pathMatch: 'full', resolve: { member: MemberDetailedResolver } },
      { path: 'end-call/:username', component: EndCallComponent, pathMatch: 'full', resolve: { member: MemberDetailedResolver }  },
      { path: 'messages/:username', component: UserMessagesComponent, resolve: { member: MemberDetailedResolver } },
      { path: 'term/:Id', component: TermComponent, canActivate: [AdminGuard] },

    ]
  },
  {
    path: 'account-settings',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    component: AccountSettingsComponent,
    children: [
      { path: '', redirectTo: 'general', pathMatch: 'full' },
      { path: 'general', component: AccountGeneralComponent },
      { path: 'change-password', component: AccountChangePasswordComponent },
      { path: 'visibility', component: AccountSetvisibilityComponent },
      { path: 'disable-account', component: AccountDisableAccountComponent },
    ]
  },
  { path: 'not-found', component: NotFoundComponent },
  { path: 'server-error', component: ServerErrorComponent },
  { path: '**', component: NotFoundComponent, pathMatch: 'full' },
  { path: '**', component: MessagesComponent },

];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }