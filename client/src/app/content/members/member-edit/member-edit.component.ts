import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { InterestsDto } from 'src/app/common/_models/interestsDto';
import { LookingForsDto } from 'src/app/common/_models/lookingForsDto';
import { Member } from 'src/app/common/_models/member';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';


@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  updateIntroForm!: FormGroup;
  modalRef!: BsModalRef;
  isVisible = false;
  member!: Member;
  user: User | null = null;
  lookingFors: LookingForsDto[] = [];
  searchlookingForm!: FormGroup;
  searchinterestForm!: FormGroup;
  bsModalRef!: BsModalRef;
  interests: InterestsDto[] = []
  oldPassword!: string;
  newPassword!: string;
  confirmPassword!: string;

  constructor(private accountService: AccountService,
    private memberService: MembersService,
    private toastr: ToastrService,
    private router: Router,
    private fb: FormBuilder,
    private modalService: BsModalService,
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
    this.updateIntroForm = this.fb.group({
      introduction: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => {
        this.member = member
        this.isVisible = member.isVisible;
        this.lookingFors = member.lookingFors;
        this.interests = member.interests;
      }
    })
  }
  setVisibility(status: boolean) {
    this.isVisible = status;
    if (status) {
      this.memberService.setVisible().subscribe({
        next: () => {
          this.toastr.success('Set visibility successfully')
          this.loadMember();
        },
        error: error => console.log('Failed to update visibility: ', error)
      });
    } else {
      this.memberService.setInvisible().subscribe({
        next: () => {
          this.toastr.success('Set invisibility successfully')
          this.loadMember();
        },
        error: error => console.log('Failed to update visibility: ', error)
      });
    }
  }
  deleteAccount(): void {
    this.memberService.deleteAccount().subscribe(() => {
      this.accountService.logout();
      this.router.navigate(['/']);
      this.toastr.success('Account is disable!')
    });
  }
  updateIntroduction() {
    if (!this.member) {
      return;
    }
    const introductionText = this.updateIntroForm.value.introduction;
    if (introductionText.length > 200) { 
      this.toastr.error('Your introduction is too long. Please limit it to 200 characters.');
      return;
    }
    if (this.updateIntroForm.valid && this.updateIntroForm.dirty) {
      this.member.introduction = this.updateIntroForm.value.introduction;
    }
    this.memberService.updateUserIntroduction(this.member).subscribe({
      next: () => {
        this.toastr.success('Introduction updated successfully');
        this.updateIntroForm.reset(this.updateIntroForm.value);
        this.modalRef.hide();
      },
      error: error => {
        if (error.statusCode === 500) {
          this.toastr.error('There was an error updating your introduction. Please try again later.');
        }
      }
    })
  }
  openModal(template: TemplateRef<any>) {
    if (!this.member) {
      return;
    }
    this.updateIntroForm.setValue({
      introduction: this.member.introduction
    });
    this.modalRef = this.modalService.show(template);
  }
  reloadData() {
    this.loadMember();
  }
  openEditModal(template: TemplateRef<any>) {
     if (!this.member) {
      return;
    }
    this.updateIntroForm.setValue({
      introduction: this.member.introduction
    });
    this.modalRef = this.modalService.show(template);
  }
  deleteInterest(id: number) {
    this.memberService.deleteInterest(id).subscribe(response => {
      this.toastr.success('Interest removed successfully');
      this.loadMember();
    }, error => {
      this.toastr.error(error);
    });
  }
  deleteLookingFor(id: number) {
    this.memberService.deleteLookingFor(id).subscribe(response => {
      this.toastr.success('Looking for removed successfully!!');
      this.loadMember();
    }, error => {
      this.toastr.error(error);
    });
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
  openModalAccount(template: TemplateRef<any>) {
    
    this.modalRef = this.modalService.show(template);
  }
}
