import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../modals/roles-modal/roles-modal.component';
import { AdminService } from 'src/app/common/_services/admin.service';
import { User } from 'src/app/common/_models/user';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  availableRoles = [
    'Admin',
    'Moderator',
    'Member'
  ]

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: users => {
        this.users = users;
      },
      error: (error) => {
        console.error('Error loading users', error);
      }
    });
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        username: user.username,
        availableRoles: this.availableRoles,
        selectedRoles: [...user.roles]
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        const selectedRoles = this.bsModalRef.content?.selectedRoles;
        if (!this.arrayEqual(selectedRoles!, user.roles)) {
          this.adminService.updateUserRoles(user.username, selectedRoles!).subscribe({
            next: roles => user.roles = roles
          })
        }
      }
    })
  }
  private arrayEqual(arr1: any[], arr2: any[]) {
    return JSON.stringify(arr1.sort()) === JSON.stringify(arr2.sort());
  }
  isAdmin(user: User) {
    return user.roles.includes('Admin');
  }
  toggleBlock(user: User) {
    if (user.isBlocked) {
      this.adminService.unblockUser(user.username).subscribe({
        next: () => {
          user.isBlocked = false;
        },
        error: (err) => {
          console.error(`Failed to unblock user ${user.username}`, err);
        }
      });
    } else {
      this.adminService.blockUser(user.username).subscribe({
        next: () => {
          user.isBlocked = true;
        },
        error: (err) => {
          console.error(`Failed to block user ${user.username}`, err);
        }
      });
    }
  }
}